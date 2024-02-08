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
    /// <param name="list">The list to access.</param>
    /// <param name="index">The zero-based index of the item to retrieve.</param>
    /// <param name="defaultValue">The default value to return if index is out of range.</param>
    /// <returns>The item at the specified index, or defaultValue if index is out of range.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="list"/> is <see langword="null"/></exception>
    public static T? GetOrDefault<T>(this IList<T> list, int index, T? defaultValue = default)
    {
        ArgumentNullException.ThrowIfNull(list);

        if (index < 0 || index >= list.Count)
            return defaultValue;

        return list[index];
    }

    /// <summary>
    /// Determines whether a collection is null or empty.
    /// </summary>
    /// <param name="source">The collection to check.</param>
    /// <returns><see langword="true"/> if the collection is null or empty; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/></exception>
    public static bool IsNullOrEmpty<T>(this IEnumerable<T>? source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return !source.Any();
    }

    /// <summary>
    /// Determines whether a collection has any items.
    /// </summary>
    /// <param name="source">The collection to check.</param>
    /// <returns><see langword="true"/> if the collection is not null and has at least one item; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/></exception>
    public static bool HasItems<T>(this IEnumerable<T>? source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Any();
    }

    /// <summary>
    /// Shuffles a collection randomly using Fisher-Yates algorithm.
    /// </summary>
    /// <param name="source">The collection to shuffle.</param>
    /// <returns>A new enumerable containing the shuffled items.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/></exception>
    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
    {
        ArgumentNullException.ThrowIfNull(source);

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
    /// <param name="collection">The collection to add items to.</param>
    /// <param name="items">The items to add.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="collection"/> is <see langword="null"/>
    /// or <paramref name="items"/> is <see langword="null"/>.
    /// </exception>
    public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
    {
        ArgumentNullException.ThrowIfNull(collection);
        ArgumentNullException.ThrowIfNull(items);

        foreach (var item in items)
        {
            collection.Add(item);
        }
    }

    /// <summary>
    /// Converts enumerable to a dictionary with safe handling of duplicate keys.
    /// In case of duplicate keys, the first occurrence is kept.
    /// </summary>
    /// <param name="source">The source enumerable to convert.</param>
    /// <param name="keySelector">Function to extract key from each element.</param>
    /// <param name="valueSelector">Function to extract value from each element.</param>
    /// <typeparam name="TSource">The type of elements in the source.</typeparam>
    /// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
    /// <returns>A dictionary containing the converted elements.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="source"/> is <see langword="null"/>
    /// or <paramref name="keySelector"/> is <see langword="null"/>
    /// or <paramref name="valueSelector"/> is <see langword="null"/>.
    /// </exception>
    public static Dictionary<TKey, TValue> ToDictionarySafe<TSource, TKey, TValue>(
        this IEnumerable<TSource> source,
        Func<TSource, TKey> keySelector,
        Func<TSource, TValue> valueSelector) where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(keySelector);
        ArgumentNullException.ThrowIfNull(valueSelector);

        var dict = new Dictionary<TKey, TValue>();

        foreach (var item in source)
        {
            var key = keySelector(item);
            if (!dict.ContainsKey(key))
            {
                dict[key] = valueSelector(item);
            }
        }

        return dict;
    }

    /// <summary>
    /// Executes an action for each item in the collection.
    /// Useful for side effects in LINQ chains.
    /// </summary>
    /// <param name="source">The source collection.</param>
    /// <param name="action">The action to execute for each item.</param>
    /// <returns>The original collection for method chaining.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="source"/> is <see langword="null"/>
    /// or <paramref name="action"/> is <see langword="null"/>.
    /// </exception>
    public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(action);

        foreach (var item in source)
        {
            action(item);
            yield return item;
        }
    }
}