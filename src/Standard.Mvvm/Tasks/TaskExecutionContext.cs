namespace Standard.Mvvm.Tasks;

public sealed class TaskExecutionContext
{
    internal TaskExecutionContext(ManagedTask task)
    {
        Task = task;
    }

    public ManagedTask Task { get; }

    public CancellationToken CancellationToken => Task.CancellationToken;

    public void SetWaiting(string? description = null)
    {
        Task.SetWaiting(description);
    }

    public void SetRunning(string? description = null)
    {
        Task.SetRunning(description);
    }

    public void SetDescription(string? description)
    {
        Task.SetDescription(description);
    }

    public void ReportProgress(double? progress)
    {
        Task.ReportProgress(progress);
    }

    public void LogDebug(string message)
    {
        Task.LogDebug(message);
    }

    public void LogInformation(string message)
    {
        Task.LogInformation(message);
    }

    public void LogWarning(string message)
    {
        Task.LogWarning(message);
    }

    public void LogError(string message, Exception? exception = null)
    {
        Task.LogError(message, exception);
    }

    public ManagedTask CreateChild(TaskOptions options)
    {
        return Task.CreateChild(options);
    }

    public ManagedTask CreateChild(TaskOptions options, Func<TaskExecutionContext, Task> operation)
    {
        return Task.CreateChild(options, operation);
    }

    public TaskAction AddAction(TaskActionDefinition definition)
    {
        return Task.AddAction(definition);
    }

    public void Complete()
    {
        Task.Complete();
    }

    public void Fault(Exception exception)
    {
        Task.Fault(exception);
    }
}