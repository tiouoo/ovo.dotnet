using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace Standard.Logging;

public static class Logger
{
    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error,
        Fatal
    }

    private static int MaxLogBackups = 3;
    private static readonly Lock LockObj = new();
    private static string _logFilePath = string.Empty;
    private static bool _initialized;
    private static bool _showCallSource;
    private static int _minimumLevel = (int)LogLevel.Info;
    private static readonly StringBuilder LogCache = new();

    public static LogLevel MinimumLevel
    {
        get => (LogLevel)Volatile.Read(ref _minimumLevel);
        set => Volatile.Write(ref _minimumLevel,
            Enum.IsDefined(value) ? (int)value : (int)LogLevel.Info);
    }

    public static void Initialize(LoggerOptions options)
    {
        Console.OutputEncoding = Encoding.UTF8;
        ArgumentNullException.ThrowIfNull(options);
        lock (LockObj)
        {
            if (_initialized) return;

            try
            {
                var logDirectory = options.LogDirectory;
                if (string.IsNullOrWhiteSpace(logDirectory))
                    throw new ArgumentNullException(nameof(logDirectory), "日志输出目录不能为空。");
                
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");

                var header = $"== {options.AppName} Log {timestamp} ==\n" +
                             $"Version: {options.Version}\n" +
                             $"OS: {Environment.OSVersion}\n" +
                             $"Runtime: {Environment.Version}\n" +
                             "===============================\n";
                options.HeaderAction?.Invoke();
                if (options.AppendHeader) File.AppendAllText(_logFilePath, header);


                MaxLogBackups = options.LogFileCount;
                Volatile.Write(ref _showCallSource, options.ShowCallSource);

                if (!Directory.Exists(logDirectory))
                    Directory.CreateDirectory(logDirectory);

                _logFilePath = Path.Combine(logDirectory, "latest.log");
                
                if (File.Exists(_logFilePath))
                {
                    var backupPath = Path.Combine(logDirectory, $"log_{timestamp}.log");
                    try
                    {
                        File.Move(_logFilePath, backupPath);
                    }
                    catch (Exception exception)
                    {
                        WriteFallbackError("轮换旧日志文件失败，将继续写入当前日志文件。", exception);
                    }
                }

                CleanupOldLogFiles(logDirectory);

                _initialized = true;

                if (LogCache.Length > 0)
                {
                    lock (LockObj)
                    {
                        File.AppendAllText(_logFilePath, LogCache.ToString());
                    }

                    LogCache.Clear();
                }

                Info("日志系统初始化完成");
            }
            catch (Exception ex)
            {
                WriteFallbackError("日志系统初始化失败。", ex);
            }
        }
    }

    private static void CleanupOldLogFiles(string logDirectory)
    {
        try
        {
            var backupFiles = Directory.GetFiles(logDirectory, "log_*.log")
                .Select(f => new FileInfo(f))
                .OrderByDescending(f => f.CreationTime)
                .ToList();

            if (backupFiles.Count > MaxLogBackups)
                foreach (var file in backupFiles.Skip(MaxLogBackups))
                    try
                    {
                        file.Delete();
                        Info($"删除过时日志文件 : {file.Name}");
                    }
                    catch (Exception exception)
                    {
                        WriteFallbackError($"删除过时日志文件失败：{file.FullName}", exception);
                    }
        }
        catch (Exception ex)
        {
            WriteFallbackError("清理过时日志文件失败。", ex);
        }
    }

    private static void WriteLog(LogLevel level, string message)
    {
        if (level < MinimumLevel)
            return;

        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        var threadId = Thread.CurrentThread.ManagedThreadId.ToString();
        var source = Volatile.Read(ref _showCallSource) ? $"[{GetCallSource()}] " : string.Empty;
        var logEntry = $"[{timestamp}] [{level}] [Thread-{threadId}] {source}{message}\n";

        try
        {
            lock (LockObj)
            {
                if (_initialized) File.AppendAllText(_logFilePath, logEntry);
                else LogCache.Append(logEntry);
            }

            Console.WriteLine($"[{level}] {source}{message}");
        }
        catch (Exception ex)
        {
            WriteFallbackError($"写入 {level} 日志失败。", ex);
        }
    }

    public static void Append(string message)
    {
        try
        {
            lock (LockObj)
            {
                if (_initialized) File.AppendAllText(_logFilePath, message + "\n");
                else LogCache.Append(message + "\n");
            }

            Console.WriteLine(message);
        }
        catch (Exception ex)
        {
            WriteFallbackError($"写入 {message} 日志失败。", ex);
        }
    }

    public static void Debug(string message)
    {
        WriteLog(LogLevel.Debug, message);
    }

    public static void Info(string message)
    {
        WriteLog(LogLevel.Info, message);
    }

    public static void Warning(string message)
    {
        WriteLog(LogLevel.Warning, message);
    }

    public static void Error(string message)
    {
        WriteLog(LogLevel.Error, message);
    }

    public static void Error(Exception ex)
    {
        Error(ex.ToString());
    }

    public static void Error(string message, Exception ex)
    {
        Error($"{message}{Environment.NewLine}{ex}");
    }

    public static void Fatal(string message)
    {
        WriteLog(LogLevel.Fatal, message);
    }

    public static void Fatal(Exception ex)
    {
        Fatal(ex.ToString());
    }

    public static void Fatal(string message, Exception ex)
    {
        Fatal($"{message}{Environment.NewLine}{ex}");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    [UnconditionalSuppressMessage("Trimming", "IL2026",
        Justification = "Call source diagnostics intentionally inspect stack frame methods at runtime.")]
    [UnconditionalSuppressMessage("Trimming", "IL2075",
        Justification = "Call source diagnostics intentionally inspect declaring type methods at runtime.")]
    private static string GetCallSource()
    {
        var frames = new StackTrace().GetFrames();
        foreach (var frame in frames)
        {
            var method = frame.GetMethod();
            var declaringType = method?.DeclaringType;
            if (method is null || declaringType is null || declaringType == typeof(Logger))
                continue;

            if (typeof(IAsyncStateMachine).IsAssignableFrom(declaringType))
            {
                var stateMachineType = declaringType;
                declaringType = stateMachineType.DeclaringType;
                method = declaringType?.GetMethods(BindingFlags.Instance | BindingFlags.Static |
                                                   BindingFlags.Public | BindingFlags.NonPublic)
                             .FirstOrDefault(candidate =>
                                 candidate.GetCustomAttribute<AsyncStateMachineAttribute>()?.StateMachineType ==
                                 stateMachineType)
                         ?? method;
            }

            var typeName = declaringType?.FullName;
            return string.IsNullOrEmpty(typeName)
                ? method.Name
                : $"{typeName}.{method.Name}";
        }

        return "Unknown";
    }

    private static void WriteFallbackError(string message, Exception exception)
    {
        try
        {
            Console.Error.WriteLine($"[Logger] {message}{Environment.NewLine}{exception}");
        }
        catch
        {
            // ignored
        }
    }
}
