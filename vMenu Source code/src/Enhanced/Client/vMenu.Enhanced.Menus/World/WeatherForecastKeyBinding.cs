using CitizenFX.FiveM.Shared;

namespace vMenu.Enhanced.Menus.World;

public static class WeatherForecastKeyBinding
{
    private const string Command = "vmenu:forecast";

    private const string DefaultKey = "EQUALS";

    private static bool _registered;

    public static void Register(Action onPressed)
    {
        if (_registered)
        {
            return;
        }

        _registered = true;

        SharedAPI.Commands.RegisterCommand(Command, false, onPressed);

        KeyMapping.Register(Command, null, "vMenu：切换天气预报（关闭/简洁/完整）", DefaultKey, null);
    }
}
