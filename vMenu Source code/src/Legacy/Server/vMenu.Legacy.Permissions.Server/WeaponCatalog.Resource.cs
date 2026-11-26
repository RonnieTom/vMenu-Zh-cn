using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.BrokenNatives.Server;
using vMenu.Enhanced.Data.Permissions;
using vMenu.Enhanced.Data.Permissions.Menus;
using vMenu.Enhanced.Data.Weapons;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Players.Server;
using vMenu.Enhanced.Serialization.Server;

using vMenu.Legacy.Hosting;

namespace vMenu.Enhanced.Permissions.Server;

public static partial class WeaponCatalog
{
    private static string _payload = "[]";

    public static void LoadAndRegister()
    {
        Load(API.LoadResourceFile(API.GetCurrentResourceName(), ConfigFile));
        _payload = ServerJson.Serialize(Categories);
        NativeFixer.EmitLocal("vMenu:oxInv:rebuildCatalog");
    }

    public static void RegisterEventHandlers()
    {
        Script.EventHandlers["vMenu:oxInv:catalogBuilt"] += new Action<string>(OnOxCatalogBuilt);
        Script.EventHandlers[WeaponEvents.Request] += new Action<Player>(OnRequested);
        NativeFixer.EmitLocal("vMenu:oxInv:rebuildCatalog");
    }

    private static void OnOxCatalogBuilt(string json)
    {
        if (string.IsNullOrWhiteSpace(json) || json == "[]")
        {
            Log.Warning("[Permissions] ox_inventory weapon catalog was empty; keeping the previous weapon list.");
            return;
        }

        if (!ServerJson.TryDeserialize<List<WeaponCategory>>(json, out var categories, out var error) || categories is null)
        {
            Log.Error($"[Permissions] ox_inventory weapon catalog could not be parsed: {error}");
            return;
        }

        ApplyOxCategories(categories);
        _payload = ServerJson.Serialize(Categories);

        Log.Info($"[Permissions] Loaded {Categories.Count} ox_inventory weapon categor(y/ies) (vanilla/addon × ammo/throw/melee).");

        foreach (var player in ConnectedPlayers.All())
        {
            Script.TriggerClientEvent(player.ServerId, WeaponEvents.Set, _payload, WeaponComponentCatalog.Payload);
        }
    }

    private static void ApplyOxCategories(List<WeaponCategory> categories)
    {
        Categories.Clear();

        var segments = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var claimedWeapons = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var category in categories)
        {
            if (category is null || string.IsNullOrWhiteSpace(category.Name) || category.Weapons is not { Count: > 0 })
            {
                continue;
            }

            category.Name = category.Name.Trim();
            category.Group = category.Group?.Trim() ?? string.Empty;

            var segment = CategoryName.ToWeaponCategorySegment(category.PermissionKey);

            if (segment.Length == 0 || !segments.Add(segment))
            {
                continue;
            }

            var kept = new List<WeaponEntry>();

            foreach (var weapon in category.Weapons)
            {
                if (weapon is null || string.IsNullOrWhiteSpace(weapon.SpawnName))
                {
                    continue;
                }

                var spawnName = weapon.SpawnName.Trim().ToLowerInvariant();

                if (spawnName == Unarmed || !PermissionPath.IsValidSegment(spawnName) || !claimedWeapons.Add(spawnName))
                {
                    continue;
                }

                kept.Add(new WeaponEntry
                {
                    SpawnName = spawnName,
                    Label = string.IsNullOrWhiteSpace(weapon.Label) ? spawnName : weapon.Label.Trim(),
                });
            }

            if (kept.Count == 0)
            {
                continue;
            }

            Categories.Add(new WeaponCategory
            {
                Name = category.Name,
                Group = category.Group,
                Weapons = kept,
            });

            var permission = WeaponCategories.ForCategory(segment);

            if (!PermissionRegistry.TryGet(permission, out _))
            {
                PermissionRegistry.RegisterDynamic(permission, "ox_inventory");
            }
        }
    }

    private static void OnRequested([FromSource] Player source) =>
        Script.TriggerClientEvent(source.ServerId(), WeaponEvents.Set, _payload, WeaponComponentCatalog.Payload);
}
