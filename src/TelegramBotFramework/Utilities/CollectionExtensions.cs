// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Utilities;

/// <summary>
/// Extension methods for collections like lists, enumerables, and dictionaries.
/// Provides batch operations, safe access, and chunking utilities.
/// </summary>
public static class CollectionExtensions
{
    /// <summary>
    /// Safely gets an item at the specified index or returns default if out of range.
    /// </summary>
    public static T? GetOrDefault<T>(this IList<T> list, int index, T? defaultValue = default)
    {
        if (list == null || index < 0 || index >= list.Count)
            return defaultValue;

        return list[index];
    }

    /// <summary>
    /// Chunks a collection into smaller batches of specified size.
    /// </summary>
    public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> source, int batchSize)
    {
        if (batchSize <= 0)
            throw new ArgumentException("Batch size must be greater than 0", nameof(batchSize));

        var batch = new List<T>(batchSize);

        foreach (var item in source)
        {
            batch.Add(item);

            if (batch.Count == batchSize)
            {
                yield return batch.AsReadOnly();
                batch = new List<T>(batchSize);
            }
        }

        if (batch.Count > 0)
            yield return batch.AsReadOnly();
    }

    /// <summary>
    /// Returns distinct items by a specified key selector.
    /// Useful for grouping by a specific property while keeping objects intact.
    /// </summary>
    public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector)
    {
        var seenKeys = new HashSet<TKey>();

        foreach (var item in source)
        {
            var key = keySelector(item);
            if (seenKeys.Add(key))
                yield return item;
        }
    }

    /// <summary>
    /// Determines whether a collection is null or empty.
    /// </summary>
    public static bool IsNullOrEmpty<T>(this IEnumerable<T>? source)
    {
        return source == null || !source.Any();
    }

    /// <summary>
    /// Determines whether a collection has any items.
    /// </summary>
    public static bool HasItems<T>(this IEnumerable<T>? source)
    {
        return source != null && source.Any();
    }

    /// <summary>
    /// Shuffles a collection randomly using Fisher-Yates algorithm.
    /// </summary>
    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
    {
        var list = source.ToList();
        var random = new Random();

        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = random.Next(i + 1);
            (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
        }

        return list;
    }

    /// <summary>
    /// Adds multiple items to a collection at once.
    /// </summary>
    public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
    {
        if (collection == null || items == null)
            return;

        foreach (var item in items)
            collection.Add(item);
    }

    /// <summary>
    /// Converts enumerable to a dictionary with safe handling of duplicate keys.
    /// In case of duplicate keys, the first occurrence is kept.
    /// </summary>
    public static Dictionary<TKey, TValue> ToDictionarySafe<TSource, TKey, TValue>(
        this IEnumerable<TSource> source,
        Func<TSource, TKey> keySelector,
        Func<TSource, TValue> valueSelector) where TKey : notnull
    {
        var dict = new Dictionary<TKey, TValue>();

        foreach (var item in source)
        {
            var key = keySelector(item);
            if (!dict.ContainsKey(key))
                dict[key] = valueSelector(item);
        }

        return dict;
    }

    /// <summary>
    /// Executes an action for each item in the collection.
    /// Useful for side effects in LINQ chains.
    /// </summary>
    public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        foreach (var item in source)
        {
            action(item);
            yield return item;
        }
    }
}
