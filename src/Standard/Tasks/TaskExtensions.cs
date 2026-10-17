using Standard.Logging;

namespace Standard.Tasks;

public static class TaskExtensions
{
    public static void Forget(this Task task, string operation)
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentException.ThrowIfNullOrWhiteSpace(operation);

        _ = task.ContinueWith(
            completedTask => Logger.Error($"后台操作失败：{operation}", completedTask.Exception!),
            CancellationToken.None,
            TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default);
    }
}