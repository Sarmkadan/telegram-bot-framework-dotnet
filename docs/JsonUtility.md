# JsonUtility

A utility class providing static methods for common JSON serialization, deserialization, validation, parsing, merging, querying, and formatting operations using `System.Text.Json`.

## API

### `public static string Serialize<T>(T obj)`

Serializes the provided object into a JSON string.

- **Parameters**:
  - `obj` – The object to serialize.
- **Returns**: A JSON string representation of `obj`.
- **Exceptions**: Throws `ArgumentNullException` if `obj` is `null`.

---

### `public static T? Deserialize<T>(string json)`

Deserializes a JSON string into an instance of type `T`.

- **Parameters**:
  - `json` – The JSON string to deserialize.
- **Returns**: An instance of `T` if deserialization succeeds; otherwise, `null`.
- **Exceptions**: Throws `ArgumentNullException` if `json` is `null`.

---

### `public static bool TryDeserialize<T>(string json, out T? result)`

Attempts to deserialize a JSON string into an instance of type `T`.

- **Parameters**:
  - `json` – The JSON string to deserialize.
  - `result` – When this method returns, contains the deserialized object or `null` if deserialization fails.
- **Returns**: `true` if deserialization succeeds; otherwise, `false`.
- **Exceptions**: Throws `ArgumentNullException` if `json` is `null`.

---

### `public static bool IsValidJson(string json)`

Determines whether the provided string is valid JSON.

- **Parameters**:
  - `json` – The string to validate.
- **Returns**: `true` if the string is valid JSON; otherwise, `false`.
- **Exceptions**: Throws `ArgumentNullException` if `json` is `null`.

---

### `public static JsonElement? ParseJson(string json)`

Parses a JSON string into a `JsonElement` structure.

- **Parameters**:
  - `json` – The JSON string to parse.
- **Returns**: A `JsonElement` representing the parsed JSON, or `null` if parsing fails.
- **Exceptions**: Throws `ArgumentNullException` if `json` is `null`.

---

### `public static string MergeJson(string baseJson, string overlayJson)`

Merges two JSON strings by overlaying properties from `overlayJson` onto `baseJson`.

- **Parameters**:
  - `baseJson` – The base JSON string.
  - `overlayJson` – The JSON string whose properties override those in `baseJson`.
- **Returns**: A merged JSON string.
- **Exceptions**:
  - Throws `ArgumentNullException` if either `baseJson` or `overlayJson` is `null`.
  - Throws `JsonException` if either input is not valid JSON.

---

### `public static string? GetPropertyValue(string json, string propertyPath)`

Retrieves the string value of a property specified by a dot-separated path.

- **Parameters**:
  - `json` – The JSON string to query.
  - `propertyPath` – A dot-separated path to the property (e.g., `"user.name"`).
- **Returns**: The string value of the property if found and is a string; otherwise, `null`.
- **Exceptions**:
  - Throws `ArgumentNullException` if either `json` or `propertyPath` is `null`.
  - Throws `JsonException` if `json` is not valid JSON.

---

### `public static string PrettyPrint(string json)`

Formats a JSON string with indentation and line breaks for readability.

- **Parameters**:
  - `json` – The JSON string to format.
- **Returns**: A human-readable JSON string.
- **Exceptions**:
  - Throws `ArgumentNullException` if `json` is `null`.
  - Throws `JsonException` if `json` is not valid JSON.

---
### `public static string Minify(string json)`

Removes all whitespace and formatting from a JSON string.

- **Parameters**:
  - `json` – The JSON string to minify.
- **Returns**: A compact JSON string with no unnecessary whitespace.
- **Exceptions**:
  - Throws `ArgumentNullException` if `json` is `null`.
  - Throws `JsonException` if `json` is not valid JSON.

## Usage
