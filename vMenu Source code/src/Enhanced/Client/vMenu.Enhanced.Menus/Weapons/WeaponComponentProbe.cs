using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Data.Weapons;

namespace vMenu.Enhanced.Menus.Weapons;

// Which components a weapon actually takes, asked of the game rather than declared anywhere. Probed
// on first open and kept, not built up front: the config lists a few hundred components and the menu
// holds a hundred weapons, so asking about every pair at startup would be tens of thousands of
// native calls for weapons nobody opens.
//
// Echo911: ox_inventory attachments (at_*) list several COMPONENT_* hashes; any one match is enough.
internal static class WeaponComponentProbe
{
    private static readonly string[] TypeOrder =
    [
        "flashlight",
        "muzzle",
        "barrel",
        "grip",
        "magazine",
        "sight",
        "skin",
    ];

    private static readonly Dictionary<uint, IReadOnlyList<WeaponComponentEntry>> Cache = [];

    internal static IReadOnlyList<WeaponComponentEntry> For(uint weaponHash)
    {
        if (Cache.TryGetValue(weaponHash, out var cached))
        {
            return cached;
        }

        var accepted = new List<WeaponComponentEntry>();

        foreach (var component in WeaponSync.Components)
        {
            if (ResolveNativeHash(weaponHash, component) is not null)
            {
                accepted.Add(component);
            }
        }

        accepted.Sort(CompareTypeThenName);

        Cache[weaponHash] = accepted;

        return accepted;
    }

    /// <summary>GTA component hash that fits this weapon for the ox/legacy attachment entry.</summary>
    internal static uint? ResolveNativeHash(uint weaponHash, WeaponComponentEntry component)
    {
        if (component.NativeComponents is { Count: > 0 })
        {
            foreach (var native in component.NativeComponents)
            {
                var hash = API.Hash(native);
                if (Native.DoesWeaponTakeWeaponComponent(weaponHash, hash))
                {
                    return hash;
                }
            }

            return null;
        }

        var fallback = API.Hash(component.SpawnName);
        return Native.DoesWeaponTakeWeaponComponent(weaponHash, fallback) ? fallback : null;
    }

    // Call when the component list is replaced, so a stale probe is not kept.
    internal static void Forget() => Cache.Clear();

    private static int CompareTypeThenName(WeaponComponentEntry left, WeaponComponentEntry right)
    {
        var typeCompare = TypeRank(left.Type).CompareTo(TypeRank(right.Type));
        if (typeCompare != 0)
        {
            return typeCompare;
        }

        return string.Compare(left.Label, right.Label, StringComparison.OrdinalIgnoreCase);
    }

    private static int TypeRank(string type)
    {
        if (string.IsNullOrWhiteSpace(type))
        {
            return TypeOrder.Length + 1;
        }

        for (var i = 0; i < TypeOrder.Length; i++)
        {
            if (type.Equals(TypeOrder[i], StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }

        return TypeOrder.Length;
    }
}
