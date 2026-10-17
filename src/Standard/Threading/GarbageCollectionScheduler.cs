namespace Standard.Threading;

public static class GarbageCollectionScheduler
{
    public static async Task RunAsync(TimeSpan interval, CancellationToken cancellationToken = default)
    {
        using var timer = new PeriodicTimer(interval);
        while (await timer.WaitForNextTickAsync(cancellationToken))
            GC.Collect(2, GCCollectionMode.Optimized, blocking: false);
    }
}
