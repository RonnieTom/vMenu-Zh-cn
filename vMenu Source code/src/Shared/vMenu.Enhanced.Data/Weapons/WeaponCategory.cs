namespace vMenu.Enhanced.Data.Weapons;

public sealed class WeaponCategory
{
    /// <summary>Leaf title shown in the menu (ammo type, 投掷物, 近战, …).</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Echo911: top bucket — 原版武器 or 添加式武器. Empty keeps a flat menu.</summary>
    public string Group { get; set; } = string.Empty;

    public List<WeaponEntry> Weapons { get; set; } = [];

    /// <summary>Stable ACE key: "原版武器/ammo-9" or just Name when Group is empty.</summary>
    public string PermissionKey =>
        string.IsNullOrWhiteSpace(Group) ? Name : $"{Group}/{Name}";
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
