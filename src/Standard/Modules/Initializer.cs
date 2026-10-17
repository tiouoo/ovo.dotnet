using Standard.Modules.DiskIO;
using Standard.Modules.Platform;

namespace Standard.Modules;

public class Initializer
{
    public static void Program(string appName, string? nameSpace = null, string version = "Unknown")
    {
        Logger.Initialize(Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), nameSpace ?? appName, "Logs"),
            appName, version);
        if (RunnerTypeDetector.IsDesktop) DesktopTypeDetector.Initialize();
    }
}