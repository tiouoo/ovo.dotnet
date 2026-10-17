using Standard.Events;
using Standard.Extensions;
using Standard.IO;
using Standard.Tasks;
using Standard.Threading;

static void Check(bool condition, string message)
{
    if (!condition) throw new Exception(message);
}

var path = Path.Combine(Path.GetTempPath(), "standard-tests-" + Guid.NewGuid());
try
{
    var kept = Path.Combine(path, "nested", "keep");
    Directory.CreateDirectory(kept);
    File.WriteAllText(Path.Combine(kept, "data"), "preserved");
    File.WriteAllText(Path.Combine(path, "nested", "delete"), "removed");
    DirectoryUtilities.ClearDirectory(path, [kept]);
    Check(File.ReadAllText(Path.Combine(kept, "data")) == "preserved", "Excluded subtree lost.");
    Check(!File.Exists(Path.Combine(path, "nested", "delete")), "Sibling file retained.");
    DirectoryUtilities.ClearDirectory(path);
    Check(!Directory.EnumerateFileSystemEntries(path).Any(), "Directory not cleared.");
}
finally
{
    if (Directory.Exists(path)) Directory.Delete(path, recursive: true);
}

Check(1023.ToHumanReadableSize() == "1023 bytes", "Unit boundary below 1024.");
Check(1024.ToReadableSize() == (1d, "KiB"), "Unit boundary at 1024.");
Check(long.MaxValue.ToFileSize().Contains("EiB"), "Large file size overflow.");

var invoked = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
using (var debouncer = new Debouncer(() => invoked.TrySetResult(), 40))
{
    debouncer.Invoke();
    await invoked.Task.WaitAsync(TimeSpan.FromSeconds(5));
}
var disposed = new Debouncer(() => throw new Exception("Disposed callback ran."));
disposed.Dispose();
try { disposed.Invoke(); throw new Exception("Disposed instance accepted invocation."); }
catch (ObjectDisposedException) { }

Func<Task<bool>> veto = () => Task.FromResult(false);
ApplicationEvents.Exiting += veto;
try { Check(!await ApplicationEvents.RaiseExitingAsync(), "Exit veto ignored."); }
finally { ApplicationEvents.Exiting -= veto; }

var task = TaskManager.Instance.CreateTask(new TaskOptions { Name = "Smoke test" }, _ => Task.CompletedTask);
Check(task.Status == ManagedTaskStatus.Pending, "Task started before Start.");
task.Start();
await task.Completion;
Check(task.Status == ManagedTaskStatus.Completed, "Task did not complete.");
Check(TaskManager.Instance.RemoveTerminalTask(task), "Completed task not removed.");
var now = new DateTimeOffset(2026, 10, 17, 12, 0, 0, TimeSpan.FromHours(8));
Check(now.AddMinutes(-5).ToRelativeTime(referenceTime: now) == "5 分钟前", "Default language changed.");
Check(now.AddMinutes(-1).ToRelativeTime("en", now) == "1 minute ago", "English singular failed.");
Check(now.AddMinutes(-5).ToRelativeTime("en-US", now) == "5 minutes ago", "English plural failed.");
Check(now.AddHours(2).ToRelativeTime("en", now) == "in 2 hours", "Future time failed.");
Check(now.AddDays(-1).ToRelativeTime("en", now) == "yesterday", "Yesterday failed.");
Check(now.AddDays(1).ToRelativeTime(referenceTime: now) == "明天", "Tomorrow failed.");
Check(now.AddSeconds(-60).ToRelativeTime("en", now) == "1 minute ago", "Minute boundary failed.");
Check(now.AddHours(-1).ToRelativeTime("en", now) == "1 hour ago", "Hour boundary failed.");
Check(now.AddDays(-30).ToRelativeTime("en", now) == "2026-09-17", "Absolute fallback changed.");
Check(now.AddDays(-60).ToRelativeTime("en", now, true) == "2 months ago", "Relative months failed.");
Check(now.AddDays(365).ToRelativeTime("en", now, true) == "in 1 year", "Relative year failed.");
Check(now.ToOffset(TimeSpan.Zero).ToRelativeTime("en", now) == "just now", "Offsets not normalized.");
Check(now.DateTime.AddMinutes(-5).ToRelativeTime(referenceTime: now.DateTime) == "5 分钟前", "DateTime overload failed.");
Check(DateTime.Now.AddMinutes(-5).ToRelativeTime().EndsWith("分钟前"), "Original invocation failed.");
Check(DateTimeOffset.UnixEpoch.ToUnixTimestamp() == 0, "Epoch seconds failed.");
Check(DateTimeOffset.UnixEpoch.AddMilliseconds(1234).ToUnixMilliseconds() == 1234, "Epoch milliseconds failed.");
try { now.ToRelativeTime("fr", now); throw new Exception("Unsupported language accepted."); }
catch (NotSupportedException) { }

const string unicode = "中文 English 👋";
Check(unicode.ToBase64().FromBase64() == unicode, "UTF-8 Base64 round trip failed.");
Check(unicode.ToBase64(System.Text.Encoding.Unicode).FromBase64(System.Text.Encoding.Unicode) == unicode,
    "Custom encoding failed.");
Check(((object?)null).AsJson() == "null", "Null JSON serialization failed.");
Check(new[] { 1, 2 }.AsJson(false).FromJson<int[]>()!.SequenceEqual([1, 2]), "JSON round trip failed.");
Check("invalid".ToInt(42) == 42, "Integer fallback changed.");
var collection = new System.Collections.ObjectModel.ObservableCollection<int> { 1, 2 };
collection.AddRange(collection.Where(value => value > 0));
Check(collection.SequenceEqual([1, 2, 1, 2]), "Self-add failed.");
Console.WriteLine("All regression checks passed.");
