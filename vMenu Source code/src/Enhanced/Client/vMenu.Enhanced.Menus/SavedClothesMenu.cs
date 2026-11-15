using System.Globalization;
using CitizenFX.FiveM.Client;
using vMenu.Enhanced.Data.Appearance;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.Localization;
using vMenu.Enhanced.Menus.Players;
using vMenu.Enhanced.Menus.Players.Appearance;
using vMenu.Enhanced.Menus.Players.Saved;
using vMenu.Enhanced.Menus.Saved;
using SavedClothesPermissions = vMenu.Enhanced.Data.Permissions.Menus.SavedClothes;
namespace vMenu.Enhanced.Menus;
// Laid out exactly like the saved vehicles menu, because they are the same idea applied to a
// different thing and a player who has learned one should not have to learn the other.
[VMenu(
    TitleKey = Loc.SavedClothes.Title,
    SubtitleKey = Loc.SavedClothes.Subtitle,
    DescriptionKey = Loc.SavedClothes.LinkDescription,
    Permission = SavedClothesPermissions.Menu)]
public sealed class SavedClothesMenu : MenuDefinition
{
    private const int NameLength = 30;
    private const int DescriptionLength = 100;
    private MenuBuilder? _root;
    private DetachedMenu? _clothesMenu;
    private DetachedMenu? _detailMenu;
    // Empty is the uncategorised group, which is a real place rather than a missing one.
    private string _category = string.Empty;
    private SavedClothesEntry? _selected;
    protected override void Build(MenuBuilder menu)
    {
        _root = menu;
        _clothesMenu = menu.AddDetachedMenu(
            MenuText.From(CategoryTitle),
            MenuText.From(CategoryTitle),
            _ => { });
        _clothesMenu.Builder.OnOpened = _ => Fill(_clothesMenu, ClothesRows());
        _detailMenu = menu.AddDetachedMenu(
            MenuText.From(() => _selected?.Clothes.Name ?? string.Empty),
            MenuText.Key(Loc.SavedClothes.Title),
            _ => { });
        _detailMenu.Builder.OnOpened = _ => Fill(_detailMenu, DetailRows());
        menu.AddRange(RootRows());
        // The store changes from inside this menu, so the rows are rebuilt whenever the player comes back up
        // to this level.
        menu.OnOpened = _ => Refill(menu, RootRows());
    }
    #region Rows
    private IReadOnlyList<MenuEntry> RootRows()
    {
        var categories = SavedClothesStore.Categories();
        var rows = new List<MenuEntry>
        {
            new ButtonEntry
            {
                Text = MenuText.Key(Loc.SavedClothes.SaveCurrent),
                Description = MenuText.Key(Loc.SavedClothes.SaveCurrentDescription),
                Gate = SavedClothesPermissions.Save,
                OnSelectedAsync = _ => SaveCurrentAsync(),
            },
            new ButtonEntry
            {
                Text = MenuText.Key(Loc.SavedClothes.CreateCategory),
                Description = MenuText.Key(Loc.SavedClothes.CreateCategoryDescription),
                Gate = SavedClothesPermissions.Manage,
                OnSelectedAsync = _ => CreateCategoryAsync(),
            },
        };
        // Nothing to pick from until a category exists, and a list row with no options cannot do anything.
        if (categories.Count > 0)
        {
            rows.Add(EditCategoryRow(categories));
            rows.Add(DeleteCategoryRow(categories));
        }
        var outfits = SavedClothesStore.All();
        var groups = GroupNames(outfits);
        if (groups.Count == 0)
        {
            rows.Add(new ButtonEntry
            {
                Text = MenuText.Key(Loc.SavedClothes.NoOutfits),
                Description = MenuText.Key(Loc.SavedClothes.NoOutfitsDescription),
            });
            return rows;
        }
        foreach (var name in groups)
        {
            var group = name;
            var count = Count(outfits, group);
            rows.Add(new ButtonEntry
            {
                Text = group.Length == 0 ? MenuText.Key(Loc.SavedClothes.Uncategorised) : MenuText.Literal(group),
                Label = MenuText.Literal($"({count.ToString(CultureInfo.InvariantCulture)})"),
                Description = CategoryDescription(categories, group, count),
                OnSelected = _ =>
                {
                    _category = group;
                    _clothesMenu?.Open();
                },
            });
        }
        return rows;
    }
    private static MenuText CategoryDescription(List<SavedClothesCategory> categories, string group, int count)
    {
        foreach (var category in categories)
        {
            if (string.Equals(category.Name, group, StringComparison.OrdinalIgnoreCase)
                && category.Description.Length > 0)
            {
                return MenuText.Literal(category.Description);
            }
        }
        return MenuText.Key(
            Loc.SavedClothes.CategoryRowDescription,
            ("count", MenuText.Literal(count.ToString(CultureInfo.InvariantCulture))));
    }
    private ListEntry EditCategoryRow(List<SavedClothesCategory> categories)
    {
        var options = new List<MenuText>(categories.Count);
        foreach (var category in categories)
        {
            options.Add(MenuText.Literal(category.Name));
        }
        return new ListEntry
        {
            Text = MenuText.Key(Loc.SavedClothes.EditCategory),
            Description = MenuText.Key(Loc.SavedClothes.EditCategoryDescription),
            Options = options,
            Gate = SavedClothesPermissions.Manage,
            OnSelectedAsync = selected => EditCategoryAsync(categories, selected.SelectedIndex),
        };
    }
    private ConfirmListEntry DeleteCategoryRow(List<SavedClothesCategory> categories)
    {
        var options = new List<MenuText>(categories.Count);
        foreach (var category in categories)
        {
            options.Add(MenuText.Literal(category.Name));
        }
        var picked = 0;
        return new ConfirmListEntry
        {
            Text = MenuText.Key(Loc.SavedClothes.DeleteCategory),
            Description = MenuText.Key(Loc.SavedClothes.DeleteCategoryDescription),
            ConfirmationDescription = MenuText.Key(
                Loc.SavedClothes.DeleteCategoryConfirm,
                ("name", MenuText.From(() => NameAt(categories, picked)))),
            Options = options,
            Gate = SavedClothesPermissions.Manage,
            OnIndexChanged = changed => picked = changed.NewIndex,
            OnConfirmed = confirmed =>
            {
                var name = NameAt(categories, confirmed.SelectedIndex);
                if (name.Length == 0)
                {
                    return;
                }
                SavedClothesStore.DeleteCategory(name);
                Notifications.Success(MenuText.Key(Loc.SavedClothes.CategoryDeleted));
                RebuildEverything();
            },
        };
    }
    private IReadOnlyList<MenuEntry> ClothesRows()
    {
        var rows = new List<MenuEntry>();
        foreach (var entry in SavedClothesStore.All())
        {
            if (!string.Equals(GroupOf(entry), _category, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }
            rows.Add(ClothesRow(entry));
        }
        if (rows.Count == 0)
        {
            rows.Add(new ButtonEntry
            {
                Text = MenuText.Key(Loc.SavedClothes.NoOutfits),
                Description = MenuText.Key(Loc.SavedClothes.NoOutfitsDescription),
            });
        }
        return rows;
    }
    private ButtonEntry ClothesRow(SavedClothesEntry entry) => new()
    {
        Text = MenuText.Literal(entry.Clothes.Name),
        Label = entry.IsFromNewerBuild
            ? MenuText.Key(Loc.SavedClothes.NewerBuildLabel)
            : GenderLabel(entry.Clothes.Gender),
        Description = Describe(entry),
        OnSelected = _ =>
        {
            _selected = entry;
            _detailMenu?.Open();
        },
    };
    // Their own words win over ours: a description is only there because somebody typed it.
    private static MenuText Describe(SavedClothesEntry entry)
    {
        if (entry.IsFromNewerBuild)
        {
            return MenuText.Key(Loc.SavedClothes.NewerBuildDescription);
        }
        return entry.Clothes.Description.Length > 0
            ? MenuText.Literal(entry.Clothes.Description)
            : MenuText.Key(
                Loc.SavedClothes.OutfitRowDescription,
                ("gender", GenderLabel(entry.Clothes.Gender)));
    }
    private IReadOnlyList<MenuEntry> DetailRows()
    {
        if (_selected is not { } entry)
        {
            return
            [
                new ButtonEntry
                {
                    Text = MenuText.Key(Loc.SavedClothes.NoOutfits),
                    Description = MenuText.Key(Loc.SavedClothes.NoOutfitsDescription),
                },
            ];
        }
        var name = MenuText.Literal(entry.Clothes.Name);
        var rows = new List<MenuEntry>
        {
            new ButtonEntry
            {
                Text = MenuText.Key(Loc.SavedClothes.Wear),
                Description = MenuText.Key(Loc.SavedClothes.WearDescription),
                Gate = SavedClothesPermissions.Wear,
                OnSelectedAsync = _ => WearAsync(entry),
            },
            EditRow(entry),
            MoveRow(entry),
            new ButtonEntry
            {
                Text = MenuText.Key(Loc.SavedClothes.Duplicate),
                Description = MenuText.Key(Loc.SavedClothes.DuplicateDescription),
                Gate = SavedClothesPermissions.Save,
                OnSelectedAsync = _ => DuplicateAsync(entry),
            },
        };
        // Overwriting a save this build cannot fully read would silently drop whatever the newer version put
        // in it, so the row becomes an offer to save alongside it instead.
        if (entry.IsFromNewerBuild)
        {
            rows.Add(new ButtonEntry
            {
                Text = MenuText.Key(Loc.SavedClothes.SaveAsNew),
                Description = MenuText.Key(Loc.SavedClothes.SaveAsNewDescription),
                Gate = SavedClothesPermissions.Save,
                OnSelectedAsync = _ => SaveCurrentAsync(),
            });
        }
        else
        {
            rows.Add(new ConfirmButtonEntry
            {
                Text = MenuText.Key(Loc.SavedClothes.Replace),
                Description = MenuText.Key(Loc.SavedClothes.ReplaceDescription),
                ConfirmationDescription = MenuText.Key(Loc.SavedClothes.ReplaceConfirm, ("name", name)),
                Gate = SavedClothesPermissions.Manage,
                OnConfirmed = _ => Replace(entry),
            });
        }
        rows.Add(new ConfirmButtonEntry
        {
            Text = MenuText.Key(Loc.SavedClothes.Delete),
            Description = MenuText.Key(Loc.SavedClothes.DeleteDescription),
            ConfirmationDescription = MenuText.Key(Loc.SavedClothes.DeleteConfirm, ("name", name)),
            Gate = SavedClothesPermissions.Manage,
            OnConfirmed = _ => Delete(entry),
        });
        return rows;
    }
    // Editing rewrites the whole save, so a newer build's extra fields would be dropped on the way
    // through. Locked rather than hidden, so the reason is on screen.
    private ButtonEntry EditRow(SavedClothesEntry entry) => entry.IsFromNewerBuild
        ? new ButtonEntry
        {
            Text = MenuText.Key(Loc.SavedClothes.Edit),
            Description = MenuText.Key(Loc.SavedClothes.NewerBuildDescription),
            Gate = MenuGate.Never,
        }
        : new ButtonEntry
        {
            Text = MenuText.Key(Loc.SavedClothes.Edit),
            Description = MenuText.Key(Loc.SavedClothes.EditDescription),
            Gate = SavedClothesPermissions.Manage,
            OnSelectedAsync = _ => EditAsync(entry),
        };
    private ListEntry MoveRow(SavedClothesEntry entry)
    {
        var groups = GroupNames(SavedClothesStore.All());
        var options = new List<MenuText>(groups.Count);
        foreach (var group in groups)
        {
            options.Add(group.Length == 0 ? MenuText.Key(Loc.SavedClothes.Uncategorised) : MenuText.Literal(group));
        }
        return new ListEntry
        {
            Text = MenuText.Key(Loc.SavedClothes.MoveToCategory),
            Description = MenuText.Key(Loc.SavedClothes.MoveToCategoryDescription),
            Options = options,
            Gate = SavedClothesPermissions.Manage,
            ReadSelectedIndex = () => Math.Max(0, IndexOf(groups, GroupOf(entry))),
            // On enter rather than on scroll, because every step past a category would otherwise be a write to
            // disk and a rebuild of the menu the player is standing in.
            OnSelected = selected =>
            {
                if (selected.SelectedIndex < 0 || selected.SelectedIndex >= groups.Count)
                {
                    return;
                }
                Move(entry, groups[selected.SelectedIndex]);
            },
        };
    }
    #endregion
    #region Actions
    private async Task SaveCurrentAsync()
    {
        // Read before the prompt as well as after it, so a player who changes clothes mid-prompt saves nothing
        // rather than saving the wrong outfit.
        _ = PedAppearanceReader.ReadClothes(Native.PlayerPedId());
        if (await UserInput.GetTextAsync(
            new InputPrompt(MenuText.Key(Loc.SavedClothes.NamePrompt), NameLength),
            new InputPrompt(MenuText.Key(Loc.SavedClothes.DescriptionPrompt), DescriptionLength),
            new InputPrompt(
                MenuText.Key(Loc.SavedClothes.CategoryPrompt),
                NameLength,
                suggestions: CategorySuggestions(),
                suggestWhenEmpty: true)) is not { } answers)
        {
            return;
        }
        var name = answers[0].Trim();
        if (name.Length == 0)
        {
            return;
        }
        var ped = Native.PlayerPedId();
        var outfit = PedAppearanceReader.ReadClothes(ped);
        Report(
            SavedClothesStore.Save(
                new SavedClothes
                {
                    Name = name,
                    Description = answers[1].Trim(),
                    Category = answers[2].Trim(),
                    Gender = CurrentGender(),
                    Outfit = outfit,
                },
                replacing: false),
            name);
    }
    private void Replace(SavedClothesEntry entry)
    {
        var ped = Native.PlayerPedId();
        entry.Clothes.Outfit = PedAppearanceReader.ReadClothes(ped);
        entry.Clothes.Gender = CurrentGender();
        var outcome = SavedClothesStore.Save(entry.Clothes, replacing: true);
        if (outcome is SaveOutcome.Saved)
        {
            Notifications.Success(MenuText.Key(
                Loc.SavedClothes.Replaced,
                ("name", MenuText.Literal(entry.Clothes.Name))));
            RebuildEverything();
            return;
        }
        Report(outcome, entry.Clothes.Name);
    }
    private static async Task WearAsync(SavedClothesEntry entry)
    {
        var ped = Native.PlayerPedId();
        var gender = entry.Clothes.Gender;
        if ((gender is "male" or "female") && PedSpawning.IsWearingFreemode())
        {
            var male = PedSpawning.IsFreemodeMale(Native.GetEntityModel(ped));
            if ((gender == "male") != male)
            {
                Notifications.Warning(MenuText.Key(Loc.SavedClothes.GenderMismatch));
            }
        }
        PedAppearanceWriter.ApplyClothes(ped, entry.Clothes.Outfit);
        await API.Delay(0);
        var model = Native.GetEntityModel(ped);
        var worn = PedAppearanceReader.ReadClothes(ped);
        var differences = PedAppearanceDiff.Compare(
            ClothesAppearance(model, entry.Clothes.Outfit),
            ClothesAppearance(model, worn));
        var name = MenuText.Literal(entry.Clothes.Name);
        if (differences.Count == 0)
        {
            Notifications.Success(MenuText.Key(Loc.SavedClothes.Worn, ("name", name)));
            return;
        }
        Notifications.Warning(
            MenuText.Key(
                Loc.SavedClothes.WornPartial,
                ("name", name),
                ("count", MenuText.Literal(differences.Count.ToString(CultureInfo.InvariantCulture)))));
    }
    private async Task EditAsync(SavedClothesEntry entry)
    {
        if (await UserInput.GetTextAsync(
            new InputPrompt(MenuText.Key(Loc.SavedClothes.NamePrompt), NameLength, entry.Clothes.Name),
            new InputPrompt(
                MenuText.Key(Loc.SavedClothes.DescriptionPrompt),
                DescriptionLength,
                entry.Clothes.Description)) is not { } answers)
        {
            return;
        }
        var name = answers[0].Trim();
        if (name.Length == 0)
        {
            return;
        }
        if (!SavedClothesStore.Edit(entry, name, answers[1].Trim()))
        {
            Notifications.Error(MenuText.Key(Loc.SavedClothes.NameTaken, ("name", MenuText.Literal(name))));
            return;
        }
        Notifications.Success(MenuText.Key(Loc.SavedClothes.Edited, ("name", MenuText.Literal(name))));
        RebuildEverything();
    }
    private async Task DuplicateAsync(SavedClothesEntry entry)
    {
        var typed = await UserInput.GetTextAsync(
            MenuText.Key(Loc.SavedClothes.DuplicatePrompt, ("name", MenuText.Literal(entry.Clothes.Name))),
            NameLength,
            entry.Clothes.Name);
        if (string.IsNullOrWhiteSpace(typed))
        {
            return;
        }
        var name = typed.Trim();
        var outcome = SavedClothesStore.Duplicate(entry, name);
        if (outcome is not SaveOutcome.Saved)
        {
            Report(outcome, name);
            return;
        }
        Notifications.Success(MenuText.Key(Loc.SavedClothes.Duplicated, ("name", MenuText.Literal(name))));
        RebuildEverything();
    }
    private void Move(SavedClothesEntry entry, string category)
    {
        if (!SavedClothesStore.MoveToCategory(entry.Clothes, category))
        {
            Notifications.Error(MenuText.Key(
                Loc.SavedClothes.OverwriteRefused,
                ("name", MenuText.Literal(entry.Clothes.Name))));
            return;
        }
        Notifications.Success(MenuText.Key(
            Loc.SavedClothes.Moved,
            ("name", category.Length == 0
                ? MenuText.Key(Loc.SavedClothes.Uncategorised)
                : MenuText.Literal(category))));
        RebuildEverything();
    }
    private void Delete(SavedClothesEntry entry)
    {
        SavedClothesStore.Delete(entry.Clothes.Name);
        _selected = null;
        Notifications.Success(MenuText.Key(Loc.SavedClothes.Deleted));
        RebuildEverything();
    }
    private async Task CreateCategoryAsync()
    {
        if (await UserInput.GetTextAsync(
            new InputPrompt(MenuText.Key(Loc.SavedClothes.CategoryName), NameLength),
            new InputPrompt(MenuText.Key(Loc.SavedClothes.CategoryDescriptionPrompt), DescriptionLength)) is not { } answers)
        {
            return;
        }
        var name = answers[0].Trim();
        if (name.Length == 0)
        {
            return;
        }
        if (!SavedClothesStore.AddCategory(name, answers[1].Trim()))
        {
            Notifications.Error(MenuText.Key(Loc.SavedClothes.CategoryNameTaken, ("name", MenuText.Literal(name))));
            return;
        }
        Notifications.Success(MenuText.Key(Loc.SavedClothes.CategoryCreated, ("name", MenuText.Literal(name))));
        RebuildEverything();
    }
    private async Task EditCategoryAsync(List<SavedClothesCategory> categories, int index)
    {
        if (index < 0 || index >= categories.Count)
        {
            return;
        }
        var category = categories[index];
        if (await UserInput.GetTextAsync(
            new InputPrompt(MenuText.Key(Loc.SavedClothes.CategoryName), NameLength, category.Name),
            new InputPrompt(
                MenuText.Key(Loc.SavedClothes.CategoryDescriptionPrompt),
                DescriptionLength,
                category.Description)) is not { } answers)
        {
            return;
        }
        var name = answers[0].Trim();
        if (name.Length == 0)
        {
            return;
        }
        if (!SavedClothesStore.EditCategory(category.Name, name, answers[1].Trim()))
        {
            Notifications.Error(MenuText.Key(Loc.SavedClothes.CategoryNameTaken, ("name", MenuText.Literal(name))));
            return;
        }
        // The player may be standing in a category that just changed its name, and the clothes menu filters on
        // that name.
        if (string.Equals(_category, category.Name, StringComparison.OrdinalIgnoreCase))
        {
            _category = name;
        }
        Notifications.Success(MenuText.Key(Loc.SavedClothes.CategoryEdited, ("name", MenuText.Literal(name))));
        RebuildEverything();
    }
    private void Report(SaveOutcome outcome, string name)
    {
        var named = MenuText.Literal(name);
        if (outcome is SaveOutcome.Saved)
        {
            Notifications.Success(MenuText.Key(Loc.SavedClothes.Saved, ("name", named)));
            RebuildEverything();
            return;
        }
        Notifications.Error(MenuText.Key(
            outcome switch
            {
                SaveOutcome.NameTaken => Loc.SavedClothes.NameTaken,
                SaveOutcome.Refused => Loc.SavedClothes.OverwriteRefused,
                _ => Loc.SavedClothes.SaveFailed,
            },
            ("name", named)));
    }
    #endregion
    #region Plumbing
    private static string CurrentGender()
    {
        if (!PedSpawning.IsWearingFreemode())
        {
            return string.Empty;
        }
        return PedSpawning.IsFreemodeMale(Native.GetEntityModel(Native.PlayerPedId())) ? "male" : "female";
    }
    private static MenuText GenderLabel(string gender) => gender switch
    {
        "male" => MenuText.Key(Loc.SavedClothes.GenderMale),
        "female" => MenuText.Key(Loc.SavedClothes.GenderFemale),
        _ => MenuText.Key(Loc.SavedClothes.GenderAny),
    };
    private static PedAppearance ClothesAppearance(uint modelHash, PedOutfit outfit) => new()
    {
        ModelHash = modelHash,
        Components = outfit.Components,
        Props = outfit.Props,
    };
    private void RebuildEverything()
    {
        if (_root is { } root)
        {
            Refill(root, RootRows());
        }
        Fill(_clothesMenu, ClothesRows());
        Fill(_detailMenu, DetailRows());
    }
    private static void Fill(DetachedMenu? menu, IReadOnlyList<MenuEntry> rows)
    {
        if (menu is not { } detached)
        {
            return;
        }
        Refill(detached.Builder, rows);
    }
    // Rebuilding drops every item and MenuAPI puts the highlight back on the first one, which moves the
    // player's selection out from under them.
    private static void Refill(MenuBuilder builder, IReadOnlyList<MenuEntry> rows)
    {
        var was = builder.Menu.CurrentIndex;
        var offset = builder.Menu.ViewIndexOffset;
        builder.ClearEntries();
        builder.AddRange(rows);
        var keep = was < builder.Menu.GetMenuItems().Count;
        builder.Menu.RefreshIndex(keep ? was : 0, keep ? offset : 0);
    }
    private string CategoryTitle() =>
        _category.Length == 0 ? Localizer.Current.Get(Loc.SavedClothes.Uncategorised) : _category;
    private static IReadOnlyList<InputSuggestion> CategorySuggestions()
    {
        var outfits = SavedClothesStore.All();
        var declared = SavedClothesStore.Categories();
        var rows = new List<InputSuggestion>();
        foreach (var name in GroupNames(outfits))
        {
            if (name.Length == 0)
            {
                continue;
            }
            rows.Add(new InputSuggestion
            {
                Value = name,
                Label = $"({Count(outfits, name).ToString(CultureInfo.InvariantCulture)})",
                Detail = DeclaredDescription(declared, name),
            });
        }
        return rows;
    }
    private static string DeclaredDescription(List<SavedClothesCategory> declared, string name)
    {
        foreach (var category in declared)
        {
            if (string.Equals(category.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                return category.Description;
            }
        }
        return string.Empty;
    }
    // Declared categories are included too, so an outfit naming a category nobody made is still reachable.
    private static List<string> GroupNames(List<SavedClothesEntry> outfits)
    {
        var names = new List<string>();
        foreach (var entry in outfits)
        {
            Include(names, GroupOf(entry));
        }
        foreach (var category in SavedClothesStore.Categories())
        {
            Include(names, category.Name);
        }
        names.Sort(static (left, right) => string.Compare(left, right, StringComparison.OrdinalIgnoreCase));
        // Uncategorised first, so the group everything lands in by default is where people look.
        if (IndexOf(names, string.Empty) is > 0 and var index)
        {
            names.RemoveAt(index);
            names.Insert(0, string.Empty);
        }
        return names;
    }
    private static void Include(List<string> names, string name)
    {
        if (IndexOf(names, name) < 0)
        {
            names.Add(name);
        }
    }
    // By hand rather than List.IndexOf or Contains, which reach for EqualityComparer<string>.Default and
    // the client sandbox refuses to load it.
    private static int IndexOf(List<string> names, string name)
    {
        for (var index = 0; index < names.Count; index++)
        {
            if (string.Equals(names[index], name, StringComparison.OrdinalIgnoreCase))
            {
                return index;
            }
        }
        return -1;
    }
    private static string GroupOf(SavedClothesEntry entry) => entry.Clothes.Category.Trim();
    private static int Count(List<SavedClothesEntry> outfits, string group)
    {
        var count = 0;
        foreach (var entry in outfits)
        {
            if (string.Equals(GroupOf(entry), group, StringComparison.OrdinalIgnoreCase))
            {
                count++;
            }
        }
        return count;
    }
    private static string NameAt(List<SavedClothesCategory> categories, int index) =>
        index >= 0 && index < categories.Count ? categories[index].Name : string.Empty;
    #endregion
}
