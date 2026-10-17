using System.Timers;
using Timer = System.Timers.Timer;

namespace Standard.Threading;

public sealed class Debouncer : IDisposable
{
    private readonly Action _action;
    private readonly Timer _timer;
    private readonly Lock _sync = new();
    private bool _disposed;

    public Debouncer(Action action, double interval = 1000)
    {
        ArgumentNullException.ThrowIfNull(action);
        _action = action;
        _timer = new Timer(interval) { AutoReset = false };
        _timer.Elapsed += OnTimerElapsed;
    }

    public void Invoke()
    {
        lock (_sync)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            _timer.Stop();
            _timer.Start();
        }
    }

    private void OnTimerElapsed(object? sender, ElapsedEventArgs args)
    {
        lock (_sync)
        {
            if (_disposed || _timer.Enabled) return;
            _action();
        }
    }

    public void Dispose()
    {
        lock (_sync)
        {
            if (_disposed) return;
            _disposed = true;
            _timer.Elapsed -= OnTimerElapsed;
            _timer.Dispose();
        }
    }
}
