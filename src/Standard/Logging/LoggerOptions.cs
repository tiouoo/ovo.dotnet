using System;

namespace Standard.Logging;

public sealed class LoggerOptions
{
    public required string LogDirectory { get; init; }
    public string AppName { get; init; } = "Application";
    public string Version { get; init; } = "Unknown";
    public int LogFileCount { get; init; } = 3;
    public bool AppendHeader { get; init; } = true;
    public Action? HeaderAction { get; init; }
    public bool ShowCallSource { get; init; }
}
