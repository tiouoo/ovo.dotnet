using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Standard.Mvvm.Tasks;

public sealed class ManagedTask : ObservableObject
{
    private readonly ObservableCollection<TaskAction> _actions = [];
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly ObservableCollection<ManagedTask> _children = [];
    private readonly TaskCompletionSource _completionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly ObservableCollection<TaskLogEntry> _logs = [];
    private Func<TaskExecutionContext, Task>? _operation;

    internal ManagedTask(ManagedTask? parent, TaskOptions options, Func<TaskExecutionContext, Task>? operation)
    {
        ArgumentNullException.ThrowIfNull(options);

        Id = Guid.NewGuid();
        Parent = parent;
        Name = options.Name ?? throw new ArgumentException("任务名称不能为空。", nameof(options));
        Description = options.Description;
        Progress = options.Progress;
        _operation = operation;
        _cancellationTokenSource = parent is null
            ? new CancellationTokenSource()
            : CancellationTokenSource.CreateLinkedTokenSource(parent.CancellationToken);

        Children = new ReadOnlyObservableCollection<ManagedTask>(_children);
        Actions = new ReadOnlyObservableCollection<TaskAction>(_actions);
        Logs = new ReadOnlyObservableCollection<TaskLogEntry>(_logs);
        _children.CollectionChanged += OnChildrenChanged;

        foreach (var action in options.Actions) _actions.Add(new TaskAction(this, action));
    }

    public Guid Id { get; }

    public string Name { get; }

    public string? Description
    {
        get;
        private set
        {
            if (!SetProperty(ref field, value)) return;
            OnPropertyChanged(nameof(DisplayDescription));
            Parent?.NotifyDisplayDescriptionChanged();
        }
    }

    public ManagedTask? Parent { get; }

    public ReadOnlyObservableCollection<ManagedTask> Children { get; }

    public ReadOnlyObservableCollection<TaskAction> Actions { get; }

    public ReadOnlyObservableCollection<TaskLogEntry> Logs { get; }

    public ManagedTaskStatus Status
    {
        get;
        private set
        {
            if (!SetProperty(ref field, value)) return;

            OnPropertyChanged(nameof(IsTerminal));
            OnPropertyChanged(nameof(CanBeCancelled));
            OnPropertyChanged(nameof(DisplayDescription));
            Parent?.NotifyDisplayDescriptionChanged();
            foreach (var action in _actions) action.Refresh();
        }
    } = ManagedTaskStatus.Pending;

    public DateTimeOffset CreatedAt { get; } = DateTimeOffset.Now;

    public DateTimeOffset? StartedAt
    {
        get;
        private set => SetProperty(ref field, value);
    }

    public DateTimeOffset? CompletedAt
    {
        get;
        private set => SetProperty(ref field, value);
    }

    public Exception? Exception
    {
        get;
        private set => SetProperty(ref field, value);
    }

    public string? ErrorMessage => Exception?.Message;

    public string? DisplayDescription => GetDisplayTask()?.Description ?? Description;

    public double? Progress
    {
        get;
        private set => SetProperty(ref field, value);
    }

    public double? AggregateProgress
    {
        get
        {
            var progressValues = new List<double>();
            CollectProgress(this, progressValues);
            return progressValues.Count == 0 ? null : progressValues.Average();
        }
    }

    public bool IsTerminal =>
        Status is ManagedTaskStatus.Cancelled or ManagedTaskStatus.Completed or ManagedTaskStatus.Faulted;

    public bool CanBeCancelled =>
        Status is ManagedTaskStatus.Pending or ManagedTaskStatus.Waiting or ManagedTaskStatus.Running;

    public bool IsCancellationRequested => _cancellationTokenSource.IsCancellationRequested;

    public CancellationToken CancellationToken => _cancellationTokenSource.Token;

    public Task Completion => _completionSource.Task;

    public void Start()
    {
        if (Status != ManagedTaskStatus.Pending)
            throw new InvalidOperationException("只有等待执行的任务可以启动。");

        StartedAt = DateTimeOffset.Now;
        Status = ManagedTaskStatus.Running;

        if (_operation is not null) _ = RunAsync();
    }

    public void Start(Func<TaskExecutionContext, Task> operation)
    {
        ArgumentNullException.ThrowIfNull(operation);
        if (_operation is not null)
            throw new InvalidOperationException("此任务已配置执行器。");

        _operation = operation;
        Start();
    }

    public void RequestCancellation()
    {
        if (!CanBeCancelled) return;

        Status = ManagedTaskStatus.Cancelling;
        LogInformation("已请求取消任务，正在等待清理完成。");

        _ = Task.Run(CancelCore);

        foreach (var child in _children) child.MarkCancelling();
    }

    private void CancelCore()
    {
        try
        {
            _cancellationTokenSource.Cancel();
        }
        catch
        {
            // ignored
        }
    }

    private void MarkCancelling()
    {
        if (!CanBeCancelled) return;

        Status = ManagedTaskStatus.Cancelling;
        foreach (var child in _children) child.MarkCancelling();
    }

    public string GetFormattedLog()
    {
        var builder = new StringBuilder();
        builder.AppendLine($"任务: {Name}");
        builder.AppendLine($"状态: {Status}");
        builder.AppendLine($"创建时间: {CreatedAt:O}");

        foreach (var entry in _logs)
        {
            builder.Append($"[{entry.Timestamp:O}] [{entry.Level}] {entry.Message}");
            if (entry.Exception is not null) builder.Append($"{Environment.NewLine}{entry.Exception}");
            builder.AppendLine();
        }

        return builder.ToString();
    }

    public ManagedTask CreateChild(TaskOptions options)
    {
        return CreateChildCore(options, null);
    }

    public ManagedTask CreateChild(TaskOptions options, Func<TaskExecutionContext, Task> operation)
    {
        return CreateChildCore(options, operation);
    }

    private ManagedTask CreateChildCore(TaskOptions options, Func<TaskExecutionContext, Task>? operation)
    {
        if (IsCancellationRequested || IsTerminal)
            throw new InvalidOperationException("已取消或已完成的任务不能创建子任务。");

        var child = new ManagedTask(this, options, operation);
        _children.Add(child);
        return child;
    }

    public TaskAction AddAction(TaskActionDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        if (IsTerminal) throw new InvalidOperationException("已结束的任务不能添加操作。");

        var action = new TaskAction(this, definition);
        _actions.Add(action);
        return action;
    }

    public void RefreshActions()
    {
        foreach (var action in _actions) action.Refresh();
    }

    public void Complete()
    {
        EnsureCanComplete();
        CompleteCore(IsCancellationRequested ? ManagedTaskStatus.Cancelled : ManagedTaskStatus.Completed);
    }

    public void Fault(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        EnsureCanComplete();
        Exception = exception;
        OnPropertyChanged(nameof(ErrorMessage));
        LogError("任务执行失败。", exception);
        CompleteCore(ManagedTaskStatus.Faulted);
    }

    internal void SetWaiting(string? description)
    {
        EnsureActive();
        if (description is not null) Description = description;
        Status = ManagedTaskStatus.Waiting;
    }

    internal void SetRunning(string? description)
    {
        EnsureActive();
        if (description is not null) Description = description;
        Status = ManagedTaskStatus.Running;
    }

    internal void SetDescription(string? description)
    {
        Description = description;
    }

    internal void ReportProgress(double? progress)
    {
        if (progress is < 0 or > 1)
            throw new ArgumentOutOfRangeException(nameof(progress), "任务进度必须在 0 到 1 之间。");

        EnsureActive();
        Progress = progress;
        NotifyAggregateProgressChanged();
    }

    internal void LogDebug(string message)
    {
        AddLog(TaskLogLevel.Debug, message);
    }

    internal void LogInformation(string message)
    {
        AddLog(TaskLogLevel.Information, message);
    }

    internal void LogWarning(string message)
    {
        AddLog(TaskLogLevel.Warning, message);
    }

    internal void LogError(string message, Exception? exception = null)
    {
        AddLog(TaskLogLevel.Error, message, exception);
    }

    private async Task RunAsync()
    {
        StartedAt = DateTimeOffset.Now;
        Status = ManagedTaskStatus.Running;

        try
        {
            await _operation!(new TaskExecutionContext(this));
            if (IsCancellationRequested)
                CompleteCore(ManagedTaskStatus.Cancelled);
            else
                CompleteCore(ManagedTaskStatus.Completed);
        }
        catch (OperationCanceledException) when (IsCancellationRequested)
        {
            CompleteCore(ManagedTaskStatus.Cancelled);
        }
        catch (Exception exception)
        {
            Exception = exception;
            OnPropertyChanged(nameof(ErrorMessage));
            LogError("任务执行失败。", exception);
            CompleteCore(ManagedTaskStatus.Faulted);
        }
    }

    private void CompleteCore(ManagedTaskStatus status)
    {
        if (status == ManagedTaskStatus.Completed) Progress = 1;
        NotifyAggregateProgressChanged();
        Status = status;
        CompletedAt = DateTimeOffset.Now;
        _completionSource.TrySetResult();
    }

    private static void CollectProgress(ManagedTask task, ICollection<double> progressValues)
    {
        if (task.Progress is { } progress) progressValues.Add(progress);
        foreach (var child in task._children) CollectProgress(child, progressValues);
    }

    private void OnChildrenChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is not null)
            foreach (ManagedTask child in e.NewItems)
                child.PropertyChanged += OnChildPropertyChanged;

        if (e.OldItems is not null)
            foreach (ManagedTask child in e.OldItems)
                child.PropertyChanged -= OnChildPropertyChanged;

        NotifyAggregateProgressChanged();
    }

    private void OnChildPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(Progress) or nameof(AggregateProgress)) NotifyAggregateProgressChanged();
        if (e.PropertyName is nameof(Description) or nameof(DisplayDescription) or nameof(Status))
            NotifyDisplayDescriptionChanged();
    }

    private void NotifyAggregateProgressChanged()
    {
        OnPropertyChanged(nameof(AggregateProgress));
        Parent?.NotifyAggregateProgressChanged();
    }

    private void NotifyDisplayDescriptionChanged()
    {
        OnPropertyChanged(nameof(DisplayDescription));
        Parent?.NotifyDisplayDescriptionChanged();
    }

    private ManagedTask? GetDisplayTask()
    {
        return GetDescendants()
            .Where(task => !string.IsNullOrWhiteSpace(task.Description))
            .OrderByDescending(task => task.Status switch
            {
                ManagedTaskStatus.Faulted => 4,
                ManagedTaskStatus.Running or ManagedTaskStatus.Cancelling => 3,
                ManagedTaskStatus.Pending or ManagedTaskStatus.Waiting => 2,
                _ => 1
            })
            .ThenByDescending(task => task.CreatedAt)
            .FirstOrDefault();
    }

    private IEnumerable<ManagedTask> GetDescendants()
    {
        foreach (var child in _children)
        {
            yield return child;
            foreach (var descendant in child.GetDescendants()) yield return descendant;
        }
    }

    private void AddLog(TaskLogLevel level, string message, Exception? exception = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        _logs.Add(new TaskLogEntry(DateTimeOffset.Now, level, message, exception));
        foreach (var action in _actions) action.Refresh();
    }

    private void EnsureActive()
    {
        CancellationToken.ThrowIfCancellationRequested();

        if (IsTerminal)
            throw new InvalidOperationException("已取消或已完成的任务不能更新执行状态。");
    }

    private void EnsureCanComplete()
    {
        if (Status is not (ManagedTaskStatus.Running or ManagedTaskStatus.Waiting or ManagedTaskStatus.Cancelling))
            throw new InvalidOperationException("只有运行中、等待中或取消中的任务可以结束。");
    }
}