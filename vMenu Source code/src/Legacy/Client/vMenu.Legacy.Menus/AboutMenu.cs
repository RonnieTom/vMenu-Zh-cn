using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.Configuration;
using vMenu.Enhanced.Data.Configuration;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.Localization;

using AboutSetting = vMenu.Enhanced.Data.Configuration.Settings.About;
using DebuggingSettings = vMenu.Enhanced.Data.Configuration.Settings.Debugging;

using vMenu.Legacy.Hosting;

namespace vMenu.Enhanced.Menus;

public sealed class AboutMenu : MenuDefinition
{
    public override MenuText Title => MenuText.Key(Loc.About.Title);

    public override MenuText Subtitle => MenuText.Key(Loc.About.SubtitleLegacy);

    public override MenuText LinkDescription => MenuText.Key(Loc.About.LinkDescription);

    protected override void Build(MenuBuilder menu)
    {
        menu.Entries.Add(Fact(
            Loc.About.Version,
            Loc.About.VersionDescription,
            MenuText.From(Metadata("version"))));

        // Not the manifest's author field, which carries a legal name.
        menu.Entries.Add(Fact(
            Loc.About.Author,
            Loc.About.AuthorDescription,
            MenuText.Key(Loc.About.AuthorName)));

        menu.Entries.Add(Fact(
            Loc.About.ClientDebug,
            Loc.About.ClientDebugDescription,
            MenuText.From(State(DebuggingSettings.Client))));

        menu.Entries.Add(Fact(
            Loc.About.ServerDebug,
            Loc.About.ServerDebugDescription,
            MenuText.From(State(DebuggingSettings.Server))));

        menu.Entries.Add(Fact(
            Loc.About.Experimental,
            Loc.About.ExperimentalDescription,
            MenuText.From(State(DebuggingSettings.ExperimentalFeatures))));

        menu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.About.Documentation),
            Description = MenuText.From(() => ClientConfig.Value(AboutSetting.DocumentationUrl)),
        });

        menu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.About.Discord),
            Description = MenuText.From(() => ClientConfig.Value(AboutSetting.DiscordUrl)),
        });

        menu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.About.Credits),
            Description = MenuText.Key(Loc.About.CreditsDescription),
        });
    }

    private static ButtonEntry Fact(string textKey, string descriptionKey, MenuText value) =>
        new()
        {
            Text = MenuText.Key(textKey),
            Description = MenuText.Key(descriptionKey),
            Label = value,
        };

    // A manifest value, or a marker when the key is absent.
    private static Func<string> Metadata(string key) =>
        () =>
        {
            var value = Read(key);

            return value ?? Localizer.Current.Get(Loc.About.Unknown);
        };

    // A flag as words rather than as true or false.
    private static Func<string> State(BoolSetting setting) =>
        () => Localizer.Current.Get(
            ClientConfig.Value(setting) ? Loc.About.Enabled : Loc.About.Disabled);

    private static string? Read(string key)
    {
        var resource = API.GetCurrentResourceName();

        if (API.GetNumResourceMetadata(resource, key) == 0)
        {
            return null;
        }

        var value = API.GetResourceMetadata(resource, key, 0)?.Trim();

        return string.IsNullOrEmpty(value) ? null : value;
    }
}
