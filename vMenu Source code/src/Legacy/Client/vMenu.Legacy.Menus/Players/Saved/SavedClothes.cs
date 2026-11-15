using vMenu.Enhanced.Data.Appearance;

namespace vMenu.Enhanced.Menus.Players.Saved;

// Clothes + props only. Face, hair and tattoos are never part of this payload.
public sealed class SavedClothes
{
    public const int SchemaVersion = 1;

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    // "male", "female", or empty for any / non-freemode.
    public string Gender { get; set; } = string.Empty;

    public PedOutfit Outfit { get; set; } = new();
}

public sealed class SavedClothesEntry(SavedClothes clothes, int storedVersion)
{
    public SavedClothes Clothes { get; } = clothes;

    public int StoredVersion { get; } = storedVersion;

    public bool IsFromNewerBuild => StoredVersion > SavedClothes.SchemaVersion;
}

public sealed class SavedClothesCategory
{
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
}
