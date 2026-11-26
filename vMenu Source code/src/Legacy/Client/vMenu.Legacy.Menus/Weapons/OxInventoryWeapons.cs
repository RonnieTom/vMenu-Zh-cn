using CitizenFX.Core;

using vMenu.Enhanced.Serialization;

using vMenu.Legacy.Hosting;

namespace vMenu.Enhanced.Menus.Weapons;

/// <summary>Echo911: weapon menu uses ox_inventory items (WEAPON_*, at_*) instead of GiveWeapon natives.</summary>
public static class OxInventoryWeapons
{
    private static readonly Dictionary<string, int> Counts = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, HashSet<string>> Fitted = new(StringComparer.OrdinalIgnoreCase);

    private static bool _callbacksRegistered;
    private static bool _available;

    public static bool Available
    {
        get
        {
            EnsureCallbacks();
            return _available;
        }
    }

    public static string ToWeaponItem(string spawnName) =>
        string.IsNullOrWhiteSpace(spawnName) ? string.Empty : spawnName.Trim().ToUpperInvariant();

    public static void EnsureCallbacks()
    {
        if (_callbacksRegistered)
        {
            return;
        }

        _callbacksRegistered = true;
        Script.EventHandlers["vMenu:oxInv:sync"] += new Action<string>(OnSync);
    }

    public static void TrackCatalog()
    {
        EnsureCallbacks();

        var items = new List<string>();

        foreach (var (spawn, _) in WeaponInventory.Listed())
        {
            items.Add(ToWeaponItem(spawn));
        }

        foreach (var component in WeaponSync.Components)
        {
            if (!string.IsNullOrWhiteSpace(component.SpawnName))
            {
                items.Add(component.SpawnName);
            }
        }

        BaseScript.TriggerEvent("vMenu:oxInv:track", ClientJson.Serialize(items));
    }

    public static void RequestSync()
    {
        EnsureCallbacks();
        BaseScript.TriggerEvent("vMenu:oxInv:requestSync");
    }

    public static bool HasWeaponItem(string spawnName) =>
        Count(ToWeaponItem(spawnName)) > 0;

    public static bool HasFittedComponent(string spawnName, string componentItem)
    {
        var weaponItem = ToWeaponItem(spawnName);
        return Fitted.TryGetValue(weaponItem, out var set)
            && set.Contains(componentItem);
    }

    public static void GiveWeapon(string spawnName, int ammo, IReadOnlyList<string>? components = null)
    {
        var item = ToWeaponItem(spawnName);
        Counts[item] = Count(item) + 1;
        BaseScript.TriggerEvent(
            "vMenu:oxInv:giveWeapon",
            item,
            ammo,
            ClientJson.Serialize(components ?? Array.Empty<string>()));
    }

    public static void RemoveWeapon(string spawnName)
    {
        var item = ToWeaponItem(spawnName);
        Counts[item] = Math.Max(0, Count(item) - 1);
        BaseScript.TriggerEvent("vMenu:oxInv:removeWeapon", item);
    }

    public static void RefillWeapon(string spawnName, int ammo) =>
        BaseScript.TriggerEvent("vMenu:oxInv:refillWeapon", ToWeaponItem(spawnName), ammo);

    public static void ToggleComponent(string spawnName, string componentItem, bool attach) =>
        BaseScript.TriggerEvent(
            "vMenu:oxInv:toggleComponent",
            ToWeaponItem(spawnName),
            componentItem,
            attach);

    public static void GiveAll(IEnumerable<string> spawnNames)
    {
        var items = spawnNames.Select(ToWeaponItem).Where(static s => s.Length > 0).ToList();
        BaseScript.TriggerEvent("vMenu:oxInv:giveAllWeapons", ClientJson.Serialize(items));
    }

    public static void RemoveAll(IEnumerable<string> spawnNames)
    {
        var items = spawnNames.Select(ToWeaponItem).Where(static s => s.Length > 0).ToList();
        BaseScript.TriggerEvent("vMenu:oxInv:removeAllWeapons", ClientJson.Serialize(items));
    }

    private static int Count(string item) =>
        Counts.TryGetValue(item, out var count) ? count : 0;

    private static void OnSync(string payload)
    {
        if (string.IsNullOrWhiteSpace(payload)
            || !ClientJson.TryDeserialize<OxSyncMessage>(payload, out var message)
            || message is null)
        {
            _available = false;
            return;
        }

        _available = message.Available;
        Counts.Clear();
        Fitted.Clear();

        if (message.Items is not null)
        {
            foreach (var pair in message.Items)
            {
                Counts[pair.Key] = pair.Value;
            }
        }

        if (message.Fitted is not null)
        {
            foreach (var pair in message.Fitted)
            {
                Fitted[pair.Key] = new HashSet<string>(pair.Value ?? [], StringComparer.OrdinalIgnoreCase);
            }
        }
    }

    private sealed class OxSyncMessage
    {
        public bool Available { get; set; }

        public Dictionary<string, int>? Items { get; set; }

        public Dictionary<string, string[]?>? Fitted { get; set; }
    }
}
