namespace Standard.Tasks;

public sealed class TaskActionDefinition
{
    public required string Name { get; init; }

    public string? Description { get; init; }

    public string? IconKey { get; init; }

    public required Func<ManagedTask, CancellationToken, Task> ExecuteAsync { get; init; }

    public Func<ManagedTask, bool>? CanExecute { get; init; }

    public Func<ManagedTask, bool>? IsVisible { get; init; }
}