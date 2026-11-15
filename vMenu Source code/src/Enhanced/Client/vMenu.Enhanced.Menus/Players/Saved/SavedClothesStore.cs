using vMenu.Enhanced.Menus.Saved;
using vMenu.Enhanced.Storage;

namespace vMenu.Enhanced.Menus.Players.Saved;

public static class SavedClothesStore
{
    public const string ClothesPrefix = "vmenu_clothes_";

    public const string CategoryPrefix = "vmenu_clothescategory_";

    #region Outfits

    public static List<SavedClothesEntry> All()
    {
        var outfits = new List<SavedClothesEntry>();

        foreach (var key in KvpStore.Keys(ClothesPrefix))
        {
            if (Read(key) is { } entry)
            {
                outfits.Add(entry);
            }
        }

        outfits.Sort(static (left, right) =>
            string.Compare(left.Clothes.Name, right.Clothes.Name, StringComparison.OrdinalIgnoreCase));

        return outfits;
    }

    public static SavedClothesEntry? Load(string name) => Read(ClothesKey(name));

    public static bool Exists(string name) => Load(name) is not null;

    public static SaveOutcome Save(SavedClothes clothes, bool replacing)
    {
        var key = ClothesKey(clothes.Name);

        if (Read(key) is not null && !replacing)
        {
            return SaveOutcome.NameTaken;
        }

        return KvpStore.TryWrite(key, KvpValueType.Json, SavedClothes.SchemaVersion, clothes)
            ? SaveOutcome.Saved
            : SaveOutcome.Refused;
    }

    public static void Delete(string name) => KvpStore.Delete(ClothesKey(name));

    public static bool Edit(SavedClothesEntry entry, string newName, string description)
    {
        if (entry.IsFromNewerBuild)
        {
            return false;
        }

        var oldName = entry.Clothes.Name;
        var renaming = !string.Equals(oldName, newName, StringComparison.Ordinal);

        if (renaming && Exists(newName))
        {
            return false;
        }

        var oldDescription = entry.Clothes.Description;

        entry.Clothes.Name = newName;
        entry.Clothes.Description = description;

        if (Save(entry.Clothes, replacing: !renaming) is not SaveOutcome.Saved)
        {
            entry.Clothes.Name = oldName;
            entry.Clothes.Description = oldDescription;

            return false;
        }

        if (renaming)
        {
            Delete(oldName);
        }

        return true;
    }

    public static SaveOutcome Duplicate(SavedClothesEntry entry, string newName) =>
        Save(
            new SavedClothes
            {
                Name = newName,
                Description = entry.Clothes.Description,
                Category = entry.Clothes.Category,
                Gender = entry.Clothes.Gender,
                Outfit = entry.Clothes.Outfit.Copy(),
            },
            replacing: false);

    public static bool MoveToCategory(SavedClothes clothes, string category)
    {
        clothes.Category = category;

        return Save(clothes, replacing: true) is SaveOutcome.Saved;
    }

    #endregion

    #region Categories

    public static List<SavedClothesCategory> Categories()
    {
        var categories = new List<SavedClothesCategory>();

        foreach (var key in KvpStore.Keys(CategoryPrefix))
        {
            if (KvpStore.TryRead<SavedClothesCategory>(
                    key, KvpValueType.Json, SavedClothes.SchemaVersion, out var category, out _)
                && category is not null)
            {
                categories.Add(category);
            }
        }

        categories.Sort(static (left, right) =>
            string.Compare(left.Name, right.Name, StringComparison.OrdinalIgnoreCase));

        return categories;
    }

    public static bool AddCategory(string name, string description)
    {
        if (HasCategory(name))
        {
            return false;
        }

        return KvpStore.TryWrite(
            CategoryPrefix + name,
            KvpValueType.Json,
            SavedClothes.SchemaVersion,
            new SavedClothesCategory { Name = name, Description = description });
    }

    public static bool HasCategory(string name) =>
        KvpStore.TryRead<SavedClothesCategory>(
            CategoryPrefix + name,
            KvpValueType.Json,
            SavedClothes.SchemaVersion,
            out _,
            out _);

    public static bool EditCategory(string oldName, string newName, string description)
    {
        var renaming = !string.Equals(oldName, newName, StringComparison.OrdinalIgnoreCase);

        if (renaming && HasCategory(newName))
        {
            return false;
        }

        KvpStore.Delete(CategoryPrefix + oldName);

        if (!KvpStore.TryWrite(
            CategoryPrefix + newName,
            KvpValueType.Json,
            SavedClothes.SchemaVersion,
            new SavedClothesCategory { Name = newName, Description = description }))
        {
            return false;
        }

        if (!renaming)
        {
            return true;
        }

        foreach (var entry in All())
        {
            if (!string.Equals(entry.Clothes.Category, oldName, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (entry.IsFromNewerBuild)
            {
                continue;
            }

            MoveToCategory(entry.Clothes, newName);
        }

        return true;
    }

    public static void DeleteCategory(string name)
    {
        KvpStore.Delete(CategoryPrefix + name);

        foreach (var entry in All())
        {
            if (!string.Equals(entry.Clothes.Category, name, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (entry.IsFromNewerBuild)
            {
                continue;
            }

            MoveToCategory(entry.Clothes, string.Empty);
        }
    }

    #endregion

    private static SavedClothesEntry? Read(string key)
    {
        if (!KvpStore.TryRead<SavedClothes>(
                key, KvpValueType.Json, SavedClothes.SchemaVersion, out var clothes, out var version)
            || clothes is null)
        {
            return null;
        }

        return new SavedClothesEntry(clothes, version);
    }

    private static string ClothesKey(string name) => ClothesPrefix + name;
}
