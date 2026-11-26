using CitizenFX.Core;

using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Localization;
using vMenu.Enhanced.Serialization;

using vMenu.Legacy.Hosting;

namespace vMenu.Enhanced.MenuFramework;

/// <summary>Echo911: text prompts go through ox_lib inputDialog instead of vMenu's NUI prompt page.</summary>
public static class UserInput
{
    private const string RequestEvent = "vMenu:oxInput";

    private const string ResultEvent = "vMenu:oxInputResult";

    // What the page falls back to when a prompt declares no limit, so both ends agree on the cap.
    private const int DefaultMaxLength = 255;

    // ox_lib select stays usable with a short list; larger lists stay as free text.
    private const int MaxSelectOptions = 80;

    private static bool _callbacksRegistered;
    private static bool _open;
    private static int _nextId;
    private static TaskCompletionSource<string[]?>? _pending;
    private static int _pendingId;

    /// <summary>What was typed, or null if the player cancelled.</summary>
    public static async Task<string?> GetTextAsync(
        MenuText title,
        int maxLength,
        string initialValue = "",
        IReadOnlyList<InputSuggestion>? suggestions = null,
        bool suggestWhenEmpty = false,
        MenuText description = default)
    {
        var answers = await GetTextAsync(
            new InputPrompt(title, maxLength, initialValue, suggestions, suggestWhenEmpty, description));

        return answers?[0];
    }

    /// <summary>Asks for several things in one ox_lib dialog, answering null if the player cancelled.</summary>
    public static async Task<string[]?> GetTextAsync(params InputPrompt[] prompts)
    {
        if (prompts.Length == 0 || _open)
        {
            return null;
        }

        EnsureCallbacks();

        _open = true;

        MenuButtonLock.Take();

        try
        {
            var requestId = ++_nextId;
            var pending = new TaskCompletionSource<string[]?>();

            _pendingId = requestId;
            _pending = pending;

            BaseScript.TriggerEvent(RequestEvent, ClientJson.Serialize(BuildRequest(requestId, prompts)));

            var answers = await pending.Task;

            if (answers is null)
            {
                return null;
            }

            if (answers.Length != prompts.Length)
            {
                Log.Error($"[Input] ox_lib returned {answers.Length} value(s) for {prompts.Length} prompt(s).");

                return null;
            }

            for (var index = 0; index < answers.Length; index++)
            {
                var max = prompts[index].MaxLength > 0 ? prompts[index].MaxLength : DefaultMaxLength;
                answers[index] = Capped(answers[index] ?? string.Empty, max);
            }

            return answers;
        }
        finally
        {
            _open = false;
            _pending = null;
            _pendingId = 0;

            MenuButtonLock.Release();
        }
    }

    private static void EnsureCallbacks()
    {
        if (_callbacksRegistered)
        {
            return;
        }

        _callbacksRegistered = true;

        Script.EventHandlers[ResultEvent] += new Action<string>(OnResult);
    }

    private static void OnResult(string json)
    {
        if (!ClientJson.TryDeserialize<ResultMessage>(json, out var result) || result is null)
        {
            Log.Error($"[Input] Ignoring a bad ox_lib input reply: {json}");

            return;
        }

        if (_pending is null || result.Id != _pendingId)
        {
            return;
        }

        _pending.TrySetResult(result.Values);
    }

    private static InputRequest BuildRequest(int id, InputPrompt[] prompts)
    {
        var localizer = Localizer.Current;
        var rows = new InputRow[prompts.Length];

        for (var index = 0; index < prompts.Length; index++)
        {
            var prompt = prompts[index];
            var label = prompt.Title.Resolve(localizer);
            var description = prompt.Description.Resolve(localizer);

            rows[index] = new InputRow
            {
                Label = label,
                Description = description,
                Default = prompt.InitialValue,
                Placeholder = localizer.Get(Loc.Framework.InputPlaceholder),
                Max = prompt.MaxLength > 0 ? prompt.MaxLength : DefaultMaxLength,
                Options = OptionsFor(prompt),
            };
        }

        return new InputRequest
        {
            Id = id,
            Heading = prompts.Length == 1
                ? prompts[0].Title.Resolve(localizer)
                : "vMenu",
            Rows = rows,
        };
    }

    private static SelectOption[]? OptionsFor(InputPrompt prompt)
    {
        if (!prompt.SuggestWhenEmpty
            || prompt.Suggestions is not { Count: > 0 and <= MaxSelectOptions } suggestions)
        {
            return null;
        }

        var options = new SelectOption[suggestions.Count];

        for (var index = 0; index < suggestions.Count; index++)
        {
            var suggestion = suggestions[index];

            options[index] = new SelectOption
            {
                Value = suggestion.Value,
                Label = string.IsNullOrWhiteSpace(suggestion.Label) ? suggestion.Value : suggestion.Label,
            };
        }

        return options;
    }

    private static string Capped(string text, int maxLength)
    {
        if (text.Length <= maxLength)
        {
            return text;
        }

        var end = maxLength;

        if (char.IsHighSurrogate(text[end - 1]))
        {
            end--;
        }

        Log.Warning($"[Input] An answer of {text.Length} characters was cut to the {maxLength} the prompt asked for.");

        return text[..end];
    }

    private sealed class InputRequest
    {
        public required int Id { get; init; }

        public required string Heading { get; init; }

        public required IReadOnlyList<InputRow> Rows { get; init; }
    }

    private sealed class InputRow
    {
        public required string Label { get; init; }

        public required string Description { get; init; }

        public required string Default { get; init; }

        public required string Placeholder { get; init; }

        public required int Max { get; init; }

        public SelectOption[]? Options { get; init; }
    }

    private sealed class SelectOption
    {
        public required string Value { get; init; }

        public required string Label { get; init; }
    }

    private sealed class ResultMessage
    {
        public int Id { get; init; }

        public string[]? Values { get; init; }
    }
}
