using System.Text.Json;

using CitizenFX.FiveM.Server;

using vMenu.Enhanced.Data.Weapons;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Serialization.Server;

namespace vMenu.Enhanced.Permissions.Server;

public static class WeaponComponentCatalog
{
    private const string ConfigFile = "config/weapon-components.json";

    private static readonly JsonDocumentOptions ParseOptions = new()
    {
        CommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    private static readonly List<WeaponComponentEntry> Components = [];

    public static string Payload { get; private set; } = "[]";

    public static void Load()
    {
        Components.Clear();
        Payload = "[]";

        var contents = Native.LoadResourceFile(Native.GetCurrentResourceName(), ConfigFile);

        if (string.IsNullOrWhiteSpace(contents))
        {
            Log.Warning($"[Permissions] No {ConfigFile} found. No weapon offers any components.");
            return;
        }

        JsonDocument document;

        try
        {
            document = JsonDocument.Parse(contents, ParseOptions);
        }
        catch (JsonException exception)
        {
            Log.Error($"[Permissions] {ConfigFile} could not be parsed, so no weapon offers any components: {exception.Message}");
            return;
        }

        using (document)
        {
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                Log.Error($"[Permissions] {ConfigFile} has to hold a single object of component names, so no weapon offers any components.");
                return;
            }

            Read(document.RootElement);
        }

        Payload = ServerJson.Serialize(Components);

        if (Components.Count > 0)
        {
            Log.Debug($"[Permissions] Loaded {Components.Count} weapon component(s) from '{ConfigFile}'.");
        }
    }

    private static void Read(JsonElement root)
    {
        var claimed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var component in root.EnumerateObject())
        {
            var spawnName = component.Name.Trim();

            if (spawnName.Length == 0)
            {
                continue;
            }

            if (!claimed.Add(spawnName))
            {
                Log.Warning($"[Permissions] '{spawnName}' is listed more than once in {ConfigFile}, so only the first one is used.");
                continue;
            }

            // Legacy flat map: "COMPONENT_X": "WCT_CLIP2"
            if (component.Value.ValueKind == JsonValueKind.String)
            {
                var label = component.Value.GetString()?.Trim();

                Components.Add(new WeaponComponentEntry
                {
                    SpawnName = spawnName,
                    Label = string.IsNullOrEmpty(label) ? spawnName : label,
                });
                continue;
            }

            // Echo911 / ox_inventory object:
            // "at_flashlight": { "label": "...", "type": "flashlight", "components": ["COMPONENT_..."] }
            if (component.Value.ValueKind != JsonValueKind.Object)
            {
                Log.Warning($"[Permissions] Skipping component '{spawnName}': expected a label string or an object.");
                continue;
            }

            var labelText = spawnName;
            var type = string.Empty;
            var natives = new List<string>();

            if (component.Value.TryGetProperty("label", out var labelProperty)
                && labelProperty.ValueKind == JsonValueKind.String)
            {
                var read = labelProperty.GetString()?.Trim();
                if (!string.IsNullOrEmpty(read))
                {
                    labelText = read;
                }
            }

            if (component.Value.TryGetProperty("type", out var typeProperty)
                && typeProperty.ValueKind == JsonValueKind.String)
            {
                type = typeProperty.GetString()?.Trim() ?? string.Empty;
            }

            if (component.Value.TryGetProperty("components", out var listProperty)
                && listProperty.ValueKind == JsonValueKind.Array)
            {
                foreach (var entry in listProperty.EnumerateArray())
                {
                    if (entry.ValueKind != JsonValueKind.String)
                    {
                        continue;
                    }

                    var native = entry.GetString()?.Trim();
                    if (!string.IsNullOrEmpty(native))
                    {
                        natives.Add(native);
                    }
                }
            }

            Components.Add(new WeaponComponentEntry
            {
                SpawnName = spawnName,
                Label = labelText,
                Type = type,
                NativeComponents = natives,
            });
        }
    }
}
