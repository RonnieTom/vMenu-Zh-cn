namespace vMenu.Enhanced.Data.Weapons;

public sealed class WeaponCategory
{
    public string Name { get; set; } = string.Empty;

    public List<WeaponEntry> Weapons { get; set; } = [];
}

public sealed class WeaponEntry
{
    public string SpawnName { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;
}

public sealed class WeaponComponentEntry
{
    /// <summary>ox_inventory item name (e.g. at_flashlight), or a legacy COMPONENT_* spawn name.</summary>
    public string SpawnName { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    /// <summary>ox attachment type: flashlight, muzzle, grip, barrel, magazine, sight, skin.</summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>GTA COMPONENT_* hashes this ox item can apply. Empty means SpawnName itself is the native component.</summary>
    public List<string> NativeComponents { get; set; } = [];
}
