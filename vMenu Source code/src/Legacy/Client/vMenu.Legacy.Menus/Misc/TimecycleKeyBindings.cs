using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Legacy.Hosting;

namespace vMenu.Enhanced.Menus.Misc;

internal static class TimecycleKeyBindings
{
    private const string IntensityUpCommand = "vmenu:timecycle:intensityup";

    private const string IntensityDownCommand = "vmenu:timecycle:intensitydown";

    private const string ClearCommand = "vmenu:timecycle:clear";

    private const string SearchCommand = "vmenu:timecycle:search";

    private const string TopCommand = "vmenu:timecycle:top";

    private const string FavoriteCommand = "vmenu:timecycle:favorite";

    private static bool _registered;

    internal static int IntensityUpControl { get; } = BindingControl(IntensityUpCommand);

    internal static int IntensityDownControl { get; } = BindingControl(IntensityDownCommand);

    internal static int ClearControl { get; } = BindingControl(ClearCommand);

    internal static int SearchControl { get; } = BindingControl(SearchCommand);

    internal static int TopControl { get; } = BindingControl(TopCommand);

    internal static int FavoriteControl { get; } = BindingControl(FavoriteCommand);

    internal static void Register(
        Action onIntensityUp,
        Action onIntensityDown,
        Action onClear,
        Action onSearch,
        Action onTop,
        Action onFavorite)
    {
        if (_registered)
        {
            return;
        }

        _registered = true;

        Commands.Register(IntensityUpCommand, false, onIntensityUp);
        Commands.Register(IntensityDownCommand, false, onIntensityDown);
        Commands.Register(ClearCommand, false, onClear);
        Commands.Register(SearchCommand, false, onSearch);
        Commands.Register(TopCommand, false, onTop);
        Commands.Register(FavoriteCommand, false, onFavorite);

        API.RegisterKeyMapping(IntensityUpCommand, "vMenu：提高滤镜强度", "keyboard", "PAGEUP");
        API.RegisterKeyMapping(IntensityDownCommand, "vMenu：降低滤镜强度", "keyboard", "PAGEDOWN");
        API.RegisterKeyMapping(ClearCommand, "vMenu：清除全部滤镜", "keyboard", "DELETE");
        API.RegisterKeyMapping(SearchCommand, "vMenu：搜索滤镜", "keyboard", "0");
        API.RegisterKeyMapping(TopCommand, "vMenu：回到滤镜列表顶部", "keyboard", "MINUS");
        API.RegisterKeyMapping(FavoriteCommand, "vMenu：收藏滤镜", "keyboard", "END");
    }

    private static int BindingControl(string command) => API.GetHashKey(command) | int.MinValue;
}
