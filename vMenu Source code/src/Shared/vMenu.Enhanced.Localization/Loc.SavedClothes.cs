namespace vMenu.Enhanced.Localization;

public static partial class Loc
{
    public static class SavedClothes
    {
        [English("Saved Clothes")]
        public const string Title = "savedclothes.title";

        [English("Saved Clothes Menu")]
        public const string Subtitle = "savedclothes.subtitle";

        [English("Save the clothes and props you are wearing, and put them back on later without changing your face, hair or tattoos.")]
        public const string LinkDescription = "savedclothes.link.desc";

        #region Saving

        [English("Save Current Clothes")]
        public const string SaveCurrent = "savedclothes.save";

        [English("Stores only the clothing components and prop accessories you are wearing on this computer. Face, hair and tattoos are left alone.")]
        public const string SaveCurrentDescription = "savedclothes.save.desc";

        [English("Give this outfit a name")]
        public const string NamePrompt = "savedclothes.save.name";

        [English("Anything you want to remember about it? Leave this empty for nothing")]
        public const string DescriptionPrompt = "savedclothes.save.description";

        [English("Which category should it go in? Leave this empty for none")]
        public const string CategoryPrompt = "savedclothes.save.category";

        [English("Saved as ~g~{name}~s~.")]
        public const string Saved = "savedclothes.save.success";

        [English("~r~Could not save~s~ that outfit.")]
        public const string SaveFailed = "savedclothes.save.failed";

        [English("You already have a saved outfit called ~y~{name}~s~.")]
        public const string NameTaken = "savedclothes.save.nametaken";

        #endregion

        #region Browsing

        [English("Uncategorised")]
        public const string Uncategorised = "savedclothes.uncategorised";

        [English("The ~y~{count}~s~ outfit(s) you saved into this category.")]
        public const string CategoryRowDescription = "savedclothes.category.row.desc";

        [English("A ~y~{gender}~s~ outfit. Press enter for what you can do with it.")]
        public const string OutfitRowDescription = "savedclothes.outfit.row.desc";

        [English("Nothing Saved Yet")]
        public const string NoOutfits = "savedclothes.empty";

        [English("Dress how you like and use Save Current Clothes, and it will show up here.")]
        public const string NoOutfitsDescription = "savedclothes.empty.desc";

        #endregion

        #region Categories

        [English("Create Category")]
        public const string CreateCategory = "savedclothes.categories.create";

        [English("Make a new group to sort your saved outfits into.")]
        public const string CreateCategoryDescription = "savedclothes.categories.create.desc";

        [English("Category name")]
        public const string CategoryName = "savedclothes.categories.name";

        [English("A short description of this category")]
        public const string CategoryDescriptionPrompt = "savedclothes.categories.description";

        [English("Edit Category")]
        public const string EditCategory = "savedclothes.categories.edit";

        [English("Pick a category and press enter to give it a different name or description. The outfits in it come along.")]
        public const string EditCategoryDescription = "savedclothes.categories.edit.desc";

        [English("That category is now ~g~{name}~s~.")]
        public const string CategoryEdited = "savedclothes.categories.edited";

        [English("~r~Delete Category")]
        public const string DeleteCategory = "savedclothes.categories.delete";

        [English("Removes a category. The outfits in it are kept and move to Uncategorised.")]
        public const string DeleteCategoryDescription = "savedclothes.categories.delete.desc";

        [English("~r~Delete {name}? Its outfits move to Uncategorised. Press enter again to confirm.")]
        public const string DeleteCategoryConfirm = "savedclothes.categories.delete.confirm";

        [English("Created the ~g~{name}~s~ category.")]
        public const string CategoryCreated = "savedclothes.categories.created";

        [English("~g~Category deleted~s~.")]
        public const string CategoryDeleted = "savedclothes.categories.deleted";

        [English("You already have a category called ~y~{name}~s~.")]
        public const string CategoryNameTaken = "savedclothes.categories.nametaken";

        #endregion

        #region Acting on one outfit

        [English("Wear Outfit")]
        public const string Wear = "savedclothes.wear";

        [English("Puts these clothes and props on you. Face, hair, tattoos and anything else stay as they are.")]
        public const string WearDescription = "savedclothes.wear.desc";

        [English("You are now wearing ~g~{name}~s~.")]
        public const string Worn = "savedclothes.wear.success";

        [English("You are wearing ~y~{name}~s~, but ~y~{count}~s~ piece(s) could not be put on.")]
        public const string WornPartial = "savedclothes.wear.partial";

        [English("This outfit was saved on a ~y~different freemode gender~s~. It will still be worn, but some pieces may not fit.")]
        public const string GenderMismatch = "savedclothes.wear.gendermismatch";

        [English("Edit")]
        public const string Edit = "savedclothes.edit";

        [English("Give this saved outfit a different name or description.")]
        public const string EditDescription = "savedclothes.edit.desc";

        [English("Saved as ~g~{name}~s~.")]
        public const string Edited = "savedclothes.edit.success";

        [English("Duplicate")]
        public const string Duplicate = "savedclothes.duplicate";

        [English("Saves a second copy of this outfit under another name, leaving this one alone.")]
        public const string DuplicateDescription = "savedclothes.duplicate.desc";

        [English("Name for the copy of {name}")]
        public const string DuplicatePrompt = "savedclothes.duplicate.prompt";

        [English("Copied to ~g~{name}~s~.")]
        public const string Duplicated = "savedclothes.duplicate.success";

        [English("Move To Category")]
        public const string MoveToCategory = "savedclothes.move";

        [English("Put this outfit into a different category. Press enter to apply the one you picked.")]
        public const string MoveToCategoryDescription = "savedclothes.move.desc";

        [English("Moved to ~g~{name}~s~.")]
        public const string Moved = "savedclothes.move.success";

        [English("~r~Replace With Current Clothes")]
        public const string Replace = "savedclothes.replace";

        [English("Overwrites this save with the clothes and props you are wearing right now, keeping the name and the category.")]
        public const string ReplaceDescription = "savedclothes.replace.desc";

        [English("~r~Overwrite {name} with the clothes you are wearing? Press enter again to confirm.")]
        public const string ReplaceConfirm = "savedclothes.replace.confirm";

        [English("~g~{name}~s~ now matches the clothes you are wearing.")]
        public const string Replaced = "savedclothes.replace.success";

        [English("~r~Delete")]
        public const string Delete = "savedclothes.delete";

        [English("Forgets this saved outfit for good.")]
        public const string DeleteDescription = "savedclothes.delete.desc";

        [English("~r~Delete {name}? Press enter again to confirm.")]
        public const string DeleteConfirm = "savedclothes.delete.confirm";

        [English("~g~Saved outfit deleted~s~.")]
        public const string Deleted = "savedclothes.delete.success";

        #endregion

        #region Written by a newer build

        [English("~y~Newer")]
        public const string NewerBuildLabel = "savedclothes.newer.label";

        [English("This outfit was saved by a newer version of vMenu than the one this server runs, so some of it cannot be read here. You can still wear, duplicate and delete it, but editing and overwriting are not offered, because either would throw away the parts this version does not understand.")]
        public const string NewerBuildDescription = "savedclothes.newer.desc";

        [English("Save As A New Outfit")]
        public const string SaveAsNew = "savedclothes.newer.saveasnew";

        [English("Saves the clothes you are wearing under a new name, leaving the newer save untouched.")]
        public const string SaveAsNewDescription = "savedclothes.newer.saveasnew.desc";

        [English("~y~{name}~s~ was saved by a newer version of vMenu, so it was left alone. Save what you are wearing under a new name instead.")]
        public const string OverwriteRefused = "savedclothes.newer.refused";

        #endregion

        #region Gender labels

        [English("male")]
        public const string GenderMale = "savedclothes.gender.male";

        [English("female")]
        public const string GenderFemale = "savedclothes.gender.female";

        [English("any")]
        public const string GenderAny = "savedclothes.gender.any";

        #endregion
    }
}