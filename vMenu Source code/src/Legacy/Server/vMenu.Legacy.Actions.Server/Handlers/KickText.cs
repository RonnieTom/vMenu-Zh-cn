namespace vMenu.Enhanced.Actions.Server.Handlers;

internal static class KickText
{
    public static string For(string by, string? reason)
    {
        var trimmed = reason?.Trim() ?? string.Empty;

        return trimmed.Length > 0
            ? $"你已被 {by} 踢出服务器，原因：{trimmed}"
            : $"你已被 {by} 踢出服务器。";
    }
}
