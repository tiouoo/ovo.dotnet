using System.Runtime.InteropServices;
using Standard.Logging;

namespace Standard.Platform;

public static class DesktopPlatformDetector
{
    private static bool _initialized;

    public static DesktopPlatform Current { get; private set; } = DesktopPlatform.Unknown;

    public static void Initialize()
    {
        if (_initialized) return;

        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                Current = DesktopPlatform.Windows;
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                Current = DesktopPlatform.Linux;
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                Current = DesktopPlatform.MacOs;
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
                Current = DesktopPlatform.FreeBsd;
            else
                Current = DesktopPlatform.Unknown;

            _initialized = true;

            Logger.Info($"[Platform] 平台检测系统显式初始化成功，当前平台为: {Current}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Platform] 平台检测失败: {ex.Message}");
        }
    }
}

