using CitizenFX.FiveM.Client;
using CitizenFX.FiveM.Shared;

using vMenu.Enhanced.Configuration;
using vMenu.Enhanced.Data.Configuration.Settings;

namespace vMenu.Enhanced.Freecam;

internal abstract class BoundKey(string command, string mapping, int padDefault)
{
    internal string Command { get; } = command;

    internal int PadDefault { get; } = padDefault;

    internal int KeyboardControl { get; } = FreecamKeyBindings.BindingControl(mapping);

    internal int PadControl { get; } = FreecamKeyBindings.BindingControl($"{mapping}:pad");

    internal int Control(bool keyboard) => keyboard ? KeyboardControl : FreecamKeyBindings.PadIcon(PadControl, PadDefault);
}

internal sealed class HeldKey(string command, int padDefault = 0) : BoundKey(command, $"+{command}", padDefault)
{
    internal bool Keyboard { get; set; }

    internal bool Pad { get; set; }

    internal bool Down => Keyboard || Pad;
}

internal sealed class TapKey(string command, int padDefault = 0) : BoundKey(command, command, padDefault)
{
    internal Action? Pressed { get; set; }
}

// Game controls on the same controller buttons as the defaults, shown when the game draws a controller
// key mapping as blank.
internal static class PadDefaults
{
    internal const int FrontendLeft = 189;
    internal const int FrontendRight = 190;
    internal const int FrontendAccept = 201;
    internal const int FrontendX = 203;
    internal const int FrontendLb = 205;
    internal const int FrontendRb = 206;
    internal const int FrontendLt = 207;
    internal const int FrontendRt = 208;
    internal const int ScriptLeftStick = 230;
}

internal static class FreecamKeyBindings
{
    private const string Prefix = "vmenu:freecam:";

    private const int ControlGroup = 0;

    private const string Keyboard = "keyboard";

    private const string Pad = "PAD_ANALOGBUTTON";

    private const string Wheel = "MOUSE_WHEEL";

    internal static HeldKey Forward { get; } = new(Prefix + "forward");

    internal static HeldKey Backward { get; } = new(Prefix + "backward");

    internal static HeldKey Left { get; } = new(Prefix + "left");

    internal static HeldKey Right { get; } = new(Prefix + "right");

    internal static HeldKey Up { get; } = new(Prefix + "up", PadDefaults.FrontendRb);

    internal static HeldKey Down { get; } = new(Prefix + "down", PadDefaults.FrontendLb);

    internal static HeldKey ZoomIn { get; } = new(Prefix + "zoomin", PadDefaults.FrontendRt);

    internal static HeldKey ZoomOut { get; } = new(Prefix + "zoomout", PadDefaults.FrontendLt);

    internal static HeldKey RollLeft { get; } = new(Prefix + "rollleft", PadDefaults.FrontendLeft);

    internal static HeldKey RollRight { get; } = new(Prefix + "rollright", PadDefaults.FrontendRight);

    internal static HeldKey Fast { get; } = new(Prefix + "fast", PadDefaults.FrontendAccept);

    internal static HeldKey Slow { get; } = new(Prefix + "slow", PadDefaults.FrontendX);

    internal static HeldKey Toggle { get; } = new(Prefix + "toggle");

    internal static TapKey Reset { get; } = new(Prefix + "reset", PadDefaults.ScriptLeftStick);

    internal static TapKey SpeedUp { get; } = new(Prefix + "speedup");

    internal static TapKey SpeedDown { get; } = new(Prefix + "speeddown");

    internal static TapKey AddKeyframe { get; } = new(Prefix + "addkeyframe");

    internal static TapKey DeleteKeyframe { get; } = new(Prefix + "deletekeyframe");

    internal static TapKey Play { get; } = new(Prefix + "play");

    internal static TapKey HideHud { get; } = new(Prefix + "hidehud");

    internal static TapKey Hints { get; } = new(Prefix + "hints");

    private static bool _registered;

    internal static void Register(Func<bool> isActive, Action onTogglePressed, Action onToggleReleased)
    {
        if (_registered)
        {
            return;
        }

        _registered = true;

        var toggleKey = ClientConfig.Value(KeyBindings.FreecamToggleKey);

        if (string.IsNullOrWhiteSpace(toggleKey))
        {
            toggleKey = "F3";
        }

        SharedAPI.Commands.RegisterCommand($"+{Toggle.Command}", false, onTogglePressed);
        SharedAPI.Commands.RegisterCommand($"-{Toggle.Command}", false, onToggleReleased);
        Native.RegisterKeyMapping($"+{Toggle.Command}", "vMenu 自由镜头：开关（按住则留在镜头位置）", Keyboard, toggleKey);

        RegisterHold(Forward, "前进", "W", null);
        RegisterHold(Backward, "后退", "S", null);
        RegisterHold(Left, "左移", "A", null);
        RegisterHold(Right, "右移", "D", null);
        RegisterHold(Up, "上升", "PAGEUP", "R1_INDEX");
        RegisterHold(Down, "下降", "PAGEDOWN", "L1_INDEX");
        RegisterHold(ZoomIn, "放大", "ADD", "R2_INDEX");
        RegisterHold(ZoomOut, "缩小", "SUBTRACT", "L2_INDEX");
        RegisterHold(RollLeft, "左滚转", "LEFT", "LLEFT_INDEX");
        RegisterHold(RollRight, "右滚转", "RIGHT", "LRIGHT_INDEX");
        RegisterHold(Fast, "加速移动", "LSHIFT", "RDOWN_INDEX");
        RegisterHold(Slow, "减速移动", "LCONTROL", "RLEFT_INDEX");

        RegisterTap(Reset, "重置滚转与缩放", Keyboard, "R", "L3_INDEX", isActive);
        RegisterTap(SpeedUp, "提高速度", Wheel, "IOM_WHEEL_UP", null, isActive);
        RegisterTap(SpeedDown, "降低速度", Wheel, "IOM_WHEEL_DOWN", null, isActive);
        RegisterTap(AddKeyframe, "添加路径关键帧", Keyboard, "INSERT", null, isActive);
        RegisterTap(DeleteKeyframe, "删除上一关键帧", Keyboard, "DELETE", null, isActive);
        RegisterTap(Play, "播放或停止路径", Keyboard, "SPACE", null, isActive);
        RegisterTap(HideHud, "隐藏 HUD", Keyboard, "H", null, isActive);
        RegisterTap(Hints, "显示或隐藏按键提示", Keyboard, "K", null, isActive);
    }

    internal static int BindingControl(string command) => API.HashSigned(command) | int.MinValue;

    internal static int PadIcon(int mapped, int fallback)
    {
        if (fallback == 0)
        {
            return mapped;
        }

        var unbound = BindingControl(Prefix + "unbound");

        return Native.GetControlInstructionalButton(ControlGroup, mapped, true) == Native.GetControlInstructionalButton(ControlGroup, unbound, true)
            ? fallback
            : mapped;
    }

    private static void RegisterTap(TapKey key, string description, string device, string defaultKey, string? padButton, Func<bool> isActive)
    {
        var onPressed = new Action(() => API.RunOnMainThread(() =>
        {
            if (isActive())
            {
                key.Pressed?.Invoke();
            }
        }));

        SharedAPI.Commands.RegisterCommand(key.Command, false, onPressed);
        Native.RegisterKeyMapping(key.Command, $"vMenu 自由镜头：{description}", device, defaultKey);

        if (padButton is null)
        {
            return;
        }

        SharedAPI.Commands.RegisterCommand($"{key.Command}:pad", false, onPressed);
        Native.RegisterKeyMapping($"{key.Command}:pad", $"vMenu 自由镜头：{description}（手柄）", Pad, padButton);
    }

    // Holds only record the key, so a key already held when the freecam starts keeps working.
    private static void RegisterHold(HeldKey key, string description, string defaultKey, string? padButton)
    {
        SharedAPI.Commands.RegisterCommand($"+{key.Command}", false, new Action(() => key.Keyboard = true));
        SharedAPI.Commands.RegisterCommand($"-{key.Command}", false, new Action(() => key.Keyboard = false));
        Native.RegisterKeyMapping($"+{key.Command}", $"vMenu 自由镜头：{description}", Keyboard, defaultKey);

        if (padButton is null)
        {
            return;
        }

        SharedAPI.Commands.RegisterCommand($"+{key.Command}:pad", false, new Action(() => key.Pad = true));
        SharedAPI.Commands.RegisterCommand($"-{key.Command}:pad", false, new Action(() => key.Pad = false));
        Native.RegisterKeyMapping($"+{key.Command}:pad", $"vMenu 自由镜头：{description}（手柄）", Pad, padButton);
    }
}
