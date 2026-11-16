using System.Text.RegularExpressions;

using CitizenFX.FiveM.Client;

using vMenu.Enhanced.BrokenNatives;
using vMenu.Enhanced.Localization;
using vMenu.Enhanced.Serialization;

namespace vMenu.Enhanced.MenuFramework;

/// <summary>Echo911: short player messages go through ox_lib instead of vMenu's NUI toast stack.</summary>
public static class Notifications
{
    /// <summary>Long enough to read a sentence without hurrying, short enough not to sit in the way.</summary>
    public const int DefaultDurationMs = 8500;

    public const int SpawnDurationMs = 4000;

    private const int VisibilityCheckMs = 500;

    // Thirty seconds of waiting for a spawn, after which the message is shown regardless.
    private const int MaxVisibilityChecks = 60;

    // GTA feed codes (~g~, ~s~, ~n~, …) mean nothing to ox_lib; strip them to plain text.
    private static readonly Regex GtaCodes = new(@"~[a-zA-Z0-9]~", RegexOptions.Compiled);

    public static void Info(MenuText text, int durationMs = DefaultDurationMs) =>
        Show(NotificationStyle.Info, text, durationMs);

    public static void Success(MenuText text, int durationMs = DefaultDurationMs) =>
        Show(NotificationStyle.Success, text, durationMs);

    public static void Warning(MenuText text, int durationMs = DefaultDurationMs) =>
        Show(NotificationStyle.Warning, text, durationMs);

    public static void Error(MenuText text, int durationMs = DefaultDurationMs) =>
        Show(NotificationStyle.Error, text, durationMs);

    /// <summary>For anything raised during startup. Returns immediately when the player is already in,
    /// so a caller that runs both at startup and at runtime can use it unconditionally.</summary>
    public static async Task ShowWhenVisibleAsync(NotificationStyle style, MenuText text, int durationMs = DefaultDurationMs)
    {
        for (var attempt = 0; attempt < MaxVisibilityChecks; attempt++)
        {
            if (Native.NetworkIsSessionStarted() && !Native.IsScreenFadedOut())
            {
                break;
            }

            await API.Delay(VisibilityCheckMs);
        }

        Show(style, text, durationMs);
    }

    public static void Show(
        NotificationStyle style,
        MenuText text,
        int durationMs = DefaultDurationMs,
        string? source = null)
    {
        var message = StripGtaCodes(text.Resolve(Localizer.Current));

        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        var title = string.IsNullOrWhiteSpace(source) ? "vMenu" : StripGtaCodes(source);

        // Primitives only across EmitLocal: the Lua bridge decodes this JSON for lib.notify.
        NativeFixer.EmitLocal(
            "vMenu:oxNotify",
            ClientJson.Serialize(new OxNotifyMessage
            {
                Title = title,
                Description = message,
                Type = Name(style),
                Duration = durationMs,
                Position = "top-right",
            }));
    }

    private static string StripGtaCodes(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        return GtaCodes.Replace(text, string.Empty).Trim();
    }

    private sealed class OxNotifyMessage
    {
        public required string Title { get; init; }

        public required string Description { get; init; }

        public required string Type { get; init; }

        public required int Duration { get; init; }

        public required string Position { get; init; }
    }

    // ox_lib types: info | success | warning | error
    private static string Name(NotificationStyle style) => style switch
    {
        NotificationStyle.Success => "success",
        NotificationStyle.Warning => "warning",
        NotificationStyle.Error => "error",
        _ => "info",
    };
}
