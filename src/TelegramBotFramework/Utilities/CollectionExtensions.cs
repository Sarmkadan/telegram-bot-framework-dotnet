#nullable enable
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
        if (list  is null || index < 0 || index >= list.Count)
            return defaultValue;

        return list[index];
    }

    /// <summary>
    /// Determines whether a collection is null or empty.
    /// </summary>
    public static bool IsNullOrEmpty<T>(this IEnumerable<T>? source)
    {
        return source  is null || !source.Any();
    }

    /// <summary>
    /// Determines whether a collection has any items.
    /// </summary>
    public static bool HasItems<T>(this IEnumerable<T>? source)
    {
        return source  is not null && source.Any();
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
        if (collection  is null || items  is null)
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