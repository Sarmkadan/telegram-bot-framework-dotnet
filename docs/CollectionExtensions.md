# CollectionExtensions

A utility class providing extension methods for common collection operations in .NET, including safe dictionary conversions, null checks, and item manipulation.

## API

### `GetOrDefault<T>(this ICollection<T>? collection, T defaultValue = default)`
Returns the collection if it is not null; otherwise, returns a new collection containing only the specified default value.
- **Parameters**:
  - `collection`: The collection to check.
  - `defaultValue`: The value to return if the collection is null (default: `default(T)`).
- **Returns**: The original collection or a singleton collection with `defaultValue`.
- **Throws**: None.

### `IsNullOrEmpty<T>(this ICollection<T>? collection)`
Determines whether the collection is null or empty.
- **Parameters**:
  - `collection`: The collection to evaluate.
- **Returns**: `true` if the collection is null or has no items; otherwise, `false`.
- **Throws**: None.

### `HasItems<T>(this ICollection<T>? collection)`
Determines whether the collection is not null and contains at least one item.
- **Parameters**:
  - `collection`: The collection to evaluate.
- **Returns**: `true` if the collection is not null and has items; otherwise, `false`.
- **Throws**: None.

### `Shuffle<T>(this IEnumerable<T> source)`
Randomizes the order of items in the source sequence using a cryptographically secure random number generator.
- **Parameters**:
  - `source`: The sequence to shuffle.
- **Returns**: A new `IEnumerable<T>` with items in randomized order.
- **Throws**: `ArgumentNullException` if `source` is null.

### `AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)`
Adds all items from the specified collection to the target collection.
- **Parameters**:
  - `collection`: The target collection.
  - `items`: The items to add.
- **Throws**: `ArgumentNullException` if `collection` or `items` is null.

### `ToDictionarySafe<TSource, TKey, TValue>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TValue> valueSelector, IEqualityComparer<TKey>? comparer = null)`
Converts the source sequence to a dictionary, skipping items with duplicate keys.
- **Parameters**:
  - `source`: The sequence to convert.
  - `keySelector`: Function to extract keys.
  - `valueSelector`: Function to extract values.
  - `comparer`: Optional key comparer (default: `null`).
- **Returns**: A `Dictionary<TKey, TValue>` with unique keys.
- **Throws**: `ArgumentNullException` if `source`, `keySelector`, or `valueSelector` is null.

### `ForEach<T>(this IEnumerable<T> source, Action<T> action)`
Applies the specified action to each item in the source sequence.
- **Parameters**:
  - `source`: The sequence to iterate.
  - `action`: The action to apply to each item.
- **Throws**: `ArgumentNullException` if `source` or `action` is null.

## Usage
