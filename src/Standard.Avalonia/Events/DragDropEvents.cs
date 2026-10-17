using Avalonia.Input;

namespace Standard.Avalonia.Events;

public static class DragDropEvents
{
    public static event Action<object?, DragEventArgs>? DragDrop;

    public static void RaiseDragDrop(object? sender, DragEventArgs e)
    {
        DragDrop?.Invoke(sender, e);
    }
}
