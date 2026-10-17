namespace Standard.Mvvm.Tasks;

public sealed record TaskLogEntry(
    DateTimeOffset Timestamp,
    TaskLogLevel Level,
    string Message,
    Exception? Exception = null);