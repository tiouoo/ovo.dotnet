using Avalonia.Input;

namespace Standard.Events;

public static class ApplicationEvents
{
    public static event Action? SaveSettings;

    public static void RaiseSaveSettings()
    {
        SaveSettings?.Invoke();
    }

    public static event Func<Task<bool>>? Exiting;

    public static async Task<bool> RaiseExitingAsync()
    {
        var handlers = Exiting;
        if (handlers is null) return true;

        foreach (var handler in handlers.GetInvocationList())
        {
            var asyncHandler = (Func<Task<bool>>)handler;
            var canContinue = await asyncHandler.Invoke();

            if (!canContinue) return false;
        }

        return true;
    }

    public static event Action<object?, DragEventArgs>? DragDrop;

    public static void RaiseDragDrop(object? sender, DragEventArgs e)
    {
        DragDrop?.Invoke(sender, e);
    }
}