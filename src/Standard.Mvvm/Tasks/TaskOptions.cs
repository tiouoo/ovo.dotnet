namespace Standard.Mvvm.Tasks;

public sealed class TaskOptions
{
    public required string Name { get; init; }

    public string? Description { get; init; }

    public double? Progress { get; init; }

    public IReadOnlyCollection<TaskActionDefinition> Actions { get; init; } = [];
}