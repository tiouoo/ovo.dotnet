using Standard.Logging;

namespace Standard.IO;

public static class DirectoryUtilities
{
    public static void EnsureDirectoryExists(string path) => Directory.CreateDirectory(path);

    public static void ClearDirectory(string path, IEnumerable<string>? excludedPaths = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        var comparer = OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
        var exclusions = new HashSet<string>(
            (excludedPaths ?? []).Select(NormalizePath), comparer);
        ClearDirectoryCore(Path.GetFullPath(path), exclusions);
    }

    private static string NormalizePath(string path) => Path.TrimEndingDirectorySeparator(Path.GetFullPath(path));

    private static void ClearDirectoryCore(string path, HashSet<string> exclusions)
    {
        if (exclusions.Contains(NormalizePath(path)) || !Directory.Exists(path)) return;
        foreach (var entry in Directory.EnumerateFileSystemEntries(path))
        {
            if (exclusions.Contains(NormalizePath(entry))) continue;
            var attributes = File.GetAttributes(entry);
            if ((attributes & FileAttributes.Directory) == 0)
                File.Delete(entry);
            else if ((attributes & FileAttributes.ReparsePoint) != 0)
                Directory.Delete(entry);
            else
            {
                ClearDirectoryCore(entry, exclusions);
                if (!Directory.EnumerateFileSystemEntries(entry).Any()) Directory.Delete(entry);
            }
        }
    }

    public static bool TryClearDirectory(string path, IEnumerable<string>? excludedPaths = null)
    {
        try
        {
            ClearDirectory(path, excludedPaths);
            return true;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            Logger.Error(exception);
            return false;
        }
    }
}
