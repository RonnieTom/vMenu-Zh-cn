using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using CitizenFX.Core.Native;

using vMenu.Enhanced.Data.Weapons;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Serialization.Server;

namespace vMenu.Enhanced.Permissions.Server;

public static class WeaponComponentCatalog
{
    private const string ConfigFile = "config/weapon-components.json";

    private static readonly List<WeaponComponentEntry> Components = [];

    public static string Payload { get; private set; } = "[]";

    public static void Load()
    {
        Components.Clear();
        Payload = "[]";

        var contents = API.LoadResourceFile(API.GetCurrentResourceName(), ConfigFile);

        if (string.IsNullOrWhiteSpace(contents))
        {
            Log.Warning($"[Permissions] No {ConfigFile} found. No weapon offers any components.");
            return;
        }

        JToken document;

        try
        {
            document = ServerJson.Parse(contents);
        }
        catch (JsonException exception)
        {
            Log.Error($"[Permissions] {ConfigFile} could not be parsed, so no weapon offers any components: {exception.Message}");
            return;
        }

        if (document is not JObject root)
        {
            Log.Error($"[Permissions] {ConfigFile} has to hold a single object of component names, so no weapon offers any components.");
            return;
        }

        Read(root);

        Payload = ServerJson.Serialize(Components);

        if (Components.Count > 0)
        {
            Log.Debug($"[Permissions] Loaded {Components.Count} weapon component(s) from '{ConfigFile}'.");
        }
    }

    private static void Read(JObject root)
    {
        var claimed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var component in root.Properties())
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

            if (component.Value.Type == JTokenType.String)
            {
                var label = ((string?)component.Value)?.Trim();

                Components.Add(new WeaponComponentEntry
                {
                    SpawnName = spawnName,
                    Label = string.IsNullOrEmpty(label) ? spawnName : label,
                });
                continue;
            }

            if (component.Value is not JObject body)
            {
                Log.Warning($"[Permissions] Skipping component '{spawnName}': expected a label string or an object.");
                continue;
            }

            var labelText = spawnName;
            var type = string.Empty;
            var natives = new List<string>();

            if (body["label"]?.Type == JTokenType.String)
            {
                var read = ((string?)body["label"])?.Trim();
                if (!string.IsNullOrEmpty(read))
                {
                    labelText = read;
                }
            }

            if (body["type"]?.Type == JTokenType.String)
            {
                type = ((string?)body["type"])?.Trim() ?? string.Empty;
            }

            if (body["components"] is JArray list)
            {
                foreach (var entry in list)
                {
                    if (entry.Type != JTokenType.String)
                    {
                        continue;
                    }

                    var native = ((string?)entry)?.Trim();
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
