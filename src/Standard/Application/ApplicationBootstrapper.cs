using System;
using System.IO;
using Standard.Logging;
using Standard.Platform;

namespace Standard.Application;

public static class ApplicationBootstrapper
{
    public static void Initialize(string appName, string? settingsDirectoryName = null, string version = "Unknown")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(appName);
        Logger.Initialize(new LoggerOptions
        {
            LogDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                settingsDirectoryName ?? appName, "Logs"),
            AppName = appName,
            Version = version
        });
        if (RuntimeEnvironment.IsDesktop) DesktopPlatformDetector.Initialize();
    }
}
