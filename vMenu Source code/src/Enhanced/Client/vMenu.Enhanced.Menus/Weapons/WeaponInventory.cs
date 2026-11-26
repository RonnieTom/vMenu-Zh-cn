using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Data.Weapons;
using vMenu.Enhanced.Permissions;

using WeaponOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.WeaponOptions;

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

        return Native.HasPedGotWeapon(Native.PlayerPedId(), weaponHash, 0);
    }

    public static int TintCount(uint weaponHash) =>
        Math.Max(1, Native.GetWeaponTintCount(weaponHash));

    public static int Tint(uint weaponHash) =>
        Native.GetPedWeaponTintIndex(Native.PlayerPedId(), weaponHash);

    public static void SetTint(uint weaponHash, int tint)
    {
        if (!ClientPermissions.IsAllowed(WeaponOptionsPermissions.Modify) || !Has(weaponHash))
        {
            return;
        }

        // Tint stays a ped native; ox_inventory does not own MK2/standard tint indices.
        if (!Native.HasPedGotWeapon(Native.PlayerPedId(), weaponHash, 0))
        {
            return;
        }

        Native.SetPedWeaponTintIndex(Native.PlayerPedId(), weaponHash, tint);
    }

    public static int MaxAmmo(uint weaponHash) =>
        Native.GetMaxAmmo(Native.PlayerPedId(), weaponHash, out var ammo) ? ammo : FallbackAmmo;

    public static int Ammo(uint weaponHash) =>
        Native.GetAmmoInPedWeapon(Native.PlayerPedId(), weaponHash);

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

        Native.GiveWeaponToPed(Native.PlayerPedId(), weaponHash, MaxAmmo(weaponHash), false, true);
        Refill(weaponHash);
    }

    public static void Remove(uint weaponHash)
    {
        if (OxInventoryWeapons.Available && TrySpawnName(weaponHash, out var spawn))
        {
            OxInventoryWeapons.RemoveWeapon(spawn);
            return;
        }

        Native.RemoveWeaponFromPed(Native.PlayerPedId(), weaponHash);
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

        var ped = Native.PlayerPedId();

        Native.SetAmmoInClip(ped, weaponHash, Native.GetMaxAmmoInClip(ped, weaponHash, false));
        Native.SetPedAmmo(ped, weaponHash, MaxAmmo(weaponHash), false);

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
            Give(API.Hash(spawn));
            given++;
        }

        // Left unarmed rather than holding whatever happened to be given last, which would otherwise be a
        // loaded weapon pointed at whoever is standing there.
        Native.SetCurrentPedWeapon(Native.PlayerPedId(), API.Hash(Unarmed), true);

        return given;
    }

    public static void RemoveAll()
    {
        if (OxInventoryWeapons.Available)
        {
            OxInventoryWeapons.RemoveAll(Listed().Select(entry => entry.SpawnName));
            return;
        }

        Native.RemoveAllPedWeapons(Native.PlayerPedId(), true);
    }

    public static int RefillAll()
    {
        var refilled = 0;

        foreach (var (weapon, _) in Listed())
        {
            if (Refill(API.Hash(weapon)))
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

                OxInventoryWeapons.RefillWeapon(weapon, Math.Min(ammo, MaxAmmo(API.Hash(weapon))));
                changed++;
            }

            return changed;
        }

        var ped = Native.PlayerPedId();
        var nativeChanged = 0;

        foreach (var (weapon, _) in Listed())
        {
            var hash = API.Hash(weapon);

            if (!Has(hash))
            {
                continue;
            }

            Native.SetPedAmmo(ped, hash, Math.Min(ammo, MaxAmmo(hash)), false);

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
            && Native.HasPedGotWeaponComponent(Native.PlayerPedId(), weaponHash, hash);
    }

    public static bool HasComponent(uint weaponHash, uint componentHash) =>
        Native.HasPedGotWeaponComponent(Native.PlayerPedId(), weaponHash, componentHash);

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

        var ped = Native.PlayerPedId();

        if (HasComponent(weaponHash, componentHash))
        {
            Native.RemoveWeaponComponentFromPed(ped, weaponHash, componentHash);
            return;
        }

        // Read back and re-applied around the swap: fitting a component resets the weapon's ammo, which
        // would quietly empty a gun the player had just filled.
        var reserve = Native.GetAmmoInPedWeapon(ped, weaponHash);

        Native.GetAmmoInClip(ped, weaponHash, out var clip);

        Native.GiveWeaponComponentToPed(ped, weaponHash, componentHash);

        Native.SetAmmoInClip(ped, weaponHash, clip);
        Native.SetPedAmmo(ped, weaponHash, reserve, false);
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
                yield return (weapon.SpawnName, category.Name);
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
            if (API.Hash(spawn) == weaponHash)
            {
                spawnName = spawn;
                return true;
            }
        }

        spawnName = string.Empty;
        return false;
    }
}
