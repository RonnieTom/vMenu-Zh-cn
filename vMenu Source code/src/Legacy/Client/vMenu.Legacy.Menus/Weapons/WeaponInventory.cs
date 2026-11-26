using CitizenFX.Core.Native;

using vMenu.Enhanced.BrokenNatives;
using vMenu.Enhanced.Data.Weapons;
using vMenu.Enhanced.Permissions;

using WeaponOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.WeaponOptions;

using vMenu.Legacy.Hosting;

namespace vMenu.Enhanced.Menus.Weapons;

// The verbs live here rather than in the menu so the rows stay declarative, and so the permission
// check sits next to the thing it guards instead of only on the row that happens to offer it.
//
// Echo911: when ox_inventory is running, weapons and attachments are inventory items (WEAPON_*, at_*)
// rather than GiveWeaponToPed / GiveWeaponComponentToPed.
public static class WeaponInventory
{
    private const string Unarmed = "weapon_unarmed";

    // What the game hands out when it is not told a number.
    private const int FallbackAmmo = 250;

    public static bool Has(uint weaponHash)
    {
        if (OxInventoryWeapons.Available && TrySpawnName(weaponHash, out var spawn))
        {
            return OxInventoryWeapons.HasWeaponItem(spawn);
        }

        return NativeFixer.HasPedGotWeapon(API.PlayerPedId(), weaponHash, 0);
    }

    public static int TintCount(uint weaponHash) =>
        Math.Max(1, API.GetWeaponTintCount(weaponHash));

    public static int Tint(uint weaponHash) =>
        API.GetPedWeaponTintIndex(API.PlayerPedId(), weaponHash);

    public static void SetTint(uint weaponHash, int tint)
    {
        if (!ClientPermissions.IsAllowed(WeaponOptionsPermissions.Modify) || !Has(weaponHash))
        {
            return;
        }

        if (!NativeFixer.HasPedGotWeapon(API.PlayerPedId(), weaponHash, 0))
        {
            return;
        }

        API.SetPedWeaponTintIndex(API.PlayerPedId(), weaponHash, tint);
    }

    public static int MaxAmmo(uint weaponHash) =>
        NativeFixer.GetMaxAmmo(API.PlayerPedId(), weaponHash, out var ammo) ? ammo : FallbackAmmo;

    public static int Ammo(uint weaponHash) =>
        API.GetAmmoInPedWeapon(API.PlayerPedId(), weaponHash);

    // Gives the weapon full, or takes it away if it is already held.
    public static bool Toggle(uint weaponHash)
    {
        if (Has(weaponHash))
        {
            Remove(weaponHash);
            return false;
        }

        Give(weaponHash);
        return true;
    }

    public static void Give(uint weaponHash)
    {
        if (OxInventoryWeapons.Available && TrySpawnName(weaponHash, out var spawn))
        {
            OxInventoryWeapons.GiveWeapon(spawn, MaxAmmo(weaponHash));
            return;
        }

        API.GiveWeaponToPed(API.PlayerPedId(), weaponHash, MaxAmmo(weaponHash), false, true);
        Refill(weaponHash);
    }

    public static void Remove(uint weaponHash)
    {
        if (OxInventoryWeapons.Available && TrySpawnName(weaponHash, out var spawn))
        {
            OxInventoryWeapons.RemoveWeapon(spawn);
            return;
        }

        API.RemoveWeaponFromPed(API.PlayerPedId(), weaponHash);
    }

    // Does nothing when the weapon is not held, so nothing is given by accident.
    public static bool Refill(uint weaponHash)
    {
        if (!Has(weaponHash))
        {
            return false;
        }

        if (OxInventoryWeapons.Available && TrySpawnName(weaponHash, out var spawn))
        {
            OxInventoryWeapons.RefillWeapon(spawn, MaxAmmo(weaponHash));
            return true;
        }

        var ped = API.PlayerPedId();

        API.SetAmmoInClip(ped, weaponHash, API.GetMaxAmmoInClip(ped, weaponHash, false));
        NativeFixer.SetPedAmmo(ped, weaponHash, MaxAmmo(weaponHash), false);

        return true;
    }

    // Weapons this player is not allowed are skipped, whether they came with the game or with an addon.
    public static int GiveAll()
    {
        var allowed = Allowed().Select(entry => entry.SpawnName).ToList();

        if (OxInventoryWeapons.Available)
        {
            OxInventoryWeapons.GiveAll(allowed);
            return allowed.Count;
        }

        var given = 0;

        foreach (var spawn in allowed)
        {
            Give(Hashing.Joaat(spawn));
            given++;
        }

        // Left unarmed rather than holding whatever happened to be given last, which would otherwise be a
        // loaded weapon pointed at whoever is standing there.
        API.SetCurrentPedWeapon(API.PlayerPedId(), Hashing.Joaat(Unarmed), true);

        return given;
    }

    public static void RemoveAll()
    {
        if (OxInventoryWeapons.Available)
        {
            OxInventoryWeapons.RemoveAll(Listed().Select(entry => entry.SpawnName));
            return;
        }

        API.RemoveAllPedWeapons(API.PlayerPedId(), true);
    }

    public static int RefillAll()
    {
        var refilled = 0;

        foreach (var (weapon, _) in Listed())
        {
            if (Refill(Hashing.Joaat(weapon)))
            {
                refilled++;
            }
        }

        return refilled;
    }

    // Walks the same list RefillAll does, so the two cannot cover different weapons.
    public static int SetAllAmmo(int ammo)
    {
        if (OxInventoryWeapons.Available)
        {
            var changed = 0;

            foreach (var (weapon, _) in Listed())
            {
                if (!OxInventoryWeapons.HasWeaponItem(weapon))
                {
                    continue;
                }

                OxInventoryWeapons.RefillWeapon(weapon, Math.Min(ammo, MaxAmmo(Hashing.Joaat(weapon))));
                changed++;
            }

            return changed;
        }

        var ped = API.PlayerPedId();
        var nativeChanged = 0;

        foreach (var (weapon, _) in Listed())
        {
            var hash = Hashing.Joaat(weapon);

            if (!Has(hash))
            {
                continue;
            }

            NativeFixer.SetPedAmmo(ped, hash, Math.Min(ammo, MaxAmmo(hash)), false);

            nativeChanged++;
        }

        return nativeChanged;
    }

    public static bool HasComponent(uint weaponHash, WeaponComponentEntry component)
    {
        if (OxInventoryWeapons.Available && TrySpawnName(weaponHash, out var spawn))
        {
            return OxInventoryWeapons.HasFittedComponent(spawn, component.SpawnName);
        }

        var native = WeaponComponentProbe.ResolveNativeHash(weaponHash, component);
        return native is uint hash
            && API.HasPedGotWeaponComponent(API.PlayerPedId(), weaponHash, hash);
    }

    public static bool HasComponent(uint weaponHash, uint componentHash) =>
        API.HasPedGotWeaponComponent(API.PlayerPedId(), weaponHash, componentHash);

    public static void ToggleComponent(uint weaponHash, WeaponComponentEntry component)
    {
        if (!ClientPermissions.IsAllowed(WeaponOptionsPermissions.Modify) || !Has(weaponHash))
        {
            return;
        }

        if (OxInventoryWeapons.Available && TrySpawnName(weaponHash, out var spawn))
        {
            var fitted = OxInventoryWeapons.HasFittedComponent(spawn, component.SpawnName);
            OxInventoryWeapons.ToggleComponent(spawn, component.SpawnName, attach: !fitted);
            return;
        }

        var native = WeaponComponentProbe.ResolveNativeHash(weaponHash, component);
        if (native is not uint componentHash)
        {
            return;
        }

        var ped = API.PlayerPedId();

        if (HasComponent(weaponHash, componentHash))
        {
            API.RemoveWeaponComponentFromPed(ped, weaponHash, componentHash);
            return;
        }

        // Read back and re-applied around the swap: fitting a component resets the weapon's ammo, which
        // would quietly empty a gun the player had just filled.
        var reserve = API.GetAmmoInPedWeapon(ped, weaponHash);

        NativeFixer.GetAmmoInClip(ped, weaponHash, out var clip);

        API.GiveWeaponComponentToPed(ped, weaponHash, componentHash);

        API.SetAmmoInClip(ped, weaponHash, clip);
        NativeFixer.SetPedAmmo(ped, weaponHash, reserve, false);
    }

    public static void ToggleComponent(uint weaponHash, uint componentHash)
    {
        foreach (var component in WeaponComponentProbe.For(weaponHash))
        {
            if (WeaponComponentProbe.ResolveNativeHash(weaponHash, component) == componentHash)
            {
                ToggleComponent(weaponHash, component);
                return;
            }
        }
    }

    internal static IEnumerable<(string SpawnName, string Category)> Listed()
    {
        foreach (var category in WeaponSync.Categories)
        {
            foreach (var weapon in category.Weapons)
            {
                yield return (weapon.SpawnName, category.PermissionKey);
            }
        }
    }

    internal static IEnumerable<(string SpawnName, string Category)> Allowed()
    {
        if (!ClientPermissions.IsAllowed(WeaponOptionsPermissions.Spawn))
        {
            yield break;
        }

        foreach (var listed in Listed())
        {
            if (ClientWeaponPermissions.CanUseWeapon(listed.SpawnName, listed.Category)
                && CatalogValidity.IsValidWeapon(listed.SpawnName))
            {
                yield return listed;
            }
        }
    }

    private static bool TrySpawnName(uint weaponHash, out string spawnName)
    {
        foreach (var (spawn, _) in Listed())
        {
            if (Hashing.Joaat(spawn) == weaponHash)
            {
                spawnName = spawn;
                return true;
            }
        }

        spawnName = string.Empty;
        return false;
    }
}
