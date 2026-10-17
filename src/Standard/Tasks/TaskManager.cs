using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Standard.Tasks;

public sealed class TaskManager : ObservableObject
{
    private readonly ObservableCollection<ManagedTask> _rootTasks = [];

    private TaskManager()
    {
        RootTasks = new ReadOnlyObservableCollection<ManagedTask>(_rootTasks);
        _rootTasks.CollectionChanged += OnRootTasksChanged;
    }

    public static TaskManager Instance { get; } = new();

    public ReadOnlyObservableCollection<ManagedTask> RootTasks { get; }

    public ManagedTask? CurrentTask => GetDisplayRootTasks().LastOrDefault();

    public ManagedTaskStatus? CurrentStatus => CurrentTask?.Status;

    public string? CurrentTaskTitle
    {
        get
        {
            var task = CurrentTask;
            if (task is null) return null;

            return task.Name;
        }
    }

    public int AdditionalTaskCount => Math.Max(0, _rootTasks.Count - 1);

    public bool HasAdditionalTasks => AdditionalTaskCount > 0;

    public ManagedTask CreateTask(TaskOptions options)
    {
        var task = new ManagedTask(null, options, null);
        _rootTasks.Add(task);
        return task;
    }

    public ManagedTask CreateTask(TaskOptions options, Func<TaskExecutionContext, Task> operation)
    {
        ArgumentNullException.ThrowIfNull(operation);
        var task = new ManagedTask(null, options, operation);
        _rootTasks.Add(task);
        return task;
    }

    public void RequestCancellationForAll()
    {
        foreach (var task in _rootTasks) task.RequestCancellation();
    }

    public bool RemoveTerminalTask(ManagedTask task)
    {
        ArgumentNullException.ThrowIfNull(task);
        if (task.Parent is not null || !task.IsTerminal) return false;

        return _rootTasks.Remove(task);
    }

    private void OnRootTasksChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is not null)
            foreach (ManagedTask task in e.NewItems)
                ObserveTaskTree(task);

        if (e.OldItems is not null)
            foreach (ManagedTask task in e.OldItems)
                StopObservingTaskTree(task);

        NotifyCurrentTaskChanged();
    }

    private void ObserveTaskTree(ManagedTask task)
    {
        task.PropertyChanged += OnTaskPropertyChanged;
        ((INotifyCollectionChanged)task.Children).CollectionChanged += OnTaskChildrenChanged;
        foreach (var child in task.Children) ObserveTaskTree(child);
    }

    private void StopObservingTaskTree(ManagedTask task)
    {
        task.PropertyChanged -= OnTaskPropertyChanged;
        ((INotifyCollectionChanged)task.Children).CollectionChanged -= OnTaskChildrenChanged;
        foreach (var child in task.Children) StopObservingTaskTree(child);
    }

    private void OnTaskChildrenChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is not null)
            foreach (ManagedTask task in e.NewItems)
                ObserveTaskTree(task);
        if (e.OldItems is not null)
            foreach (ManagedTask task in e.OldItems)
                StopObservingTaskTree(task);
        NotifyCurrentTaskChanged();
    }

    private void OnTaskPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(ManagedTask.Status) or nameof(ManagedTask.Description))
            NotifyCurrentTaskChanged();
    }

    private void NotifyCurrentTaskChanged()
    {
        OnPropertyChanged(nameof(CurrentTask));
        OnPropertyChanged(nameof(CurrentStatus));
        OnPropertyChanged(nameof(CurrentTaskTitle));
        OnPropertyChanged(nameof(AdditionalTaskCount));
        OnPropertyChanged(nameof(HasAdditionalTasks));
    }

    private IReadOnlyList<ManagedTask> GetDisplayRootTasks()
    {
        if (_rootTasks.Count == 0) return [];

        var priority = _rootTasks.Max(GetDisplayPriority);
        return _rootTasks.Where(task => GetDisplayPriority(task) == priority).ToList();
    }

    private static int GetDisplayPriority(ManagedTask task)
    {
        return task.Status switch
        {
            ManagedTaskStatus.Faulted => 4,
            ManagedTaskStatus.Running or ManagedTaskStatus.Cancelling => 3,
            ManagedTaskStatus.Pending or ManagedTaskStatus.Waiting => 2,
            _ => 1
        };
    }
}