using Standard.Logging;

namespace Standard.Platform;

public static class RuntimeEnvironment
{
    private static bool _initialized;

    public static RuntimeKind Current { get; private set; } = RuntimeKind.Unknown;

    public static bool IsDesktop => Current == RuntimeKind.Desktop;
    public static bool IsMobile => Current is RuntimeKind.Android or RuntimeKind.Ios;
    public static bool IsAndroid => Current == RuntimeKind.Android;
    public static bool IsIos => Current == RuntimeKind.Ios;
    public static bool IsBrowser => Current == RuntimeKind.Browser;

    public static void Initialize(RuntimeKind kind)
    {
        if (!Enum.IsDefined(kind) || kind == RuntimeKind.Unknown)
            throw new ArgumentOutOfRangeException(nameof(kind));
        if (_initialized) return;

        Current = kind;
        _initialized = true;

        Logger.Info($"[Platform] 运行环境系统初始化成功，当前运行于: {Current}");
    }
}