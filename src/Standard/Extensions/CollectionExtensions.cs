using System.Collections.ObjectModel;

namespace Standard.Extensions;

public static class CollectionExtensions
{
    public static void AddRange<T>(this ObservableCollection<T> collection, IEnumerable<T> items)
    {
        ArgumentNullException.ThrowIfNull(collection);
        ArgumentNullException.ThrowIfNull(items);
        foreach (var item in items.ToArray()) collection.Add(item);
    }
    public static bool IsNullOrEmpty<T>(this IEnumerable<T>? source)
    {
        return source == null || !source.Any();
    }

    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (action == null) throw new ArgumentNullException(nameof(action));

        foreach (var item in source) action(item);
    }

    public static bool In<T>(this T val, params T[] values)
    {
        if (values == null) return false;
        return values.Contains(val);
    }
}
