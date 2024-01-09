# StringExtensions

A utility class providing common string manipulation and validation methods for .NET applications, particularly in the context of Telegram bot development.

## API

### `public static string Truncate(string value, int maxLength, string suffix = "…")`

Truncates a string to a specified maximum length and appends an optional suffix if truncation occurs.

- **Parameters**:
  - `value`: The input string to truncate.
  - `maxLength`: The maximum allowed length of the resulting string.
  - `suffix`: The suffix to append when truncation occurs. Defaults to `"…"`.
- **Return value**: The truncated string if `value` exceeds `maxLength`, otherwise the original string.
- **Exceptions**: Throws `ArgumentNullException` if `value` is `null`.
- **Exceptions**: Throws `ArgumentOutOfRangeException` if `maxLength` is negative.

---

### `public static string ToSlug(string value)`

Converts a string into a URL-friendly slug by normalizing Unicode characters, removing diacritics, and replacing spaces and special characters with hyphens.

- **Parameters**:
  - `value`: The input string to convert.
- **Return value**: A slugified version of the input string in lowercase, with consecutive hyphens collapsed.
- **Exceptions**: Throws `ArgumentNullException` if `value` is `null`.

---

### `public static bool IsValidEmail(string email)`

Validates whether a string is a syntactically valid email address.

- **Parameters**:
  - `email`: The email address string to validate.
- **Return value**: `true` if the string is a valid email address; otherwise, `false`.
- **Exceptions**: Throws `ArgumentNullException` if `email` is `null`.

---

### `public static bool IsAlphanumeric(string value)`

Determines whether a string contains only alphanumeric characters (letters and digits).

- **Parameters**:
  - `value`: The string to check.
- **Return value**: `true` if the string is non-null and contains only alphanumeric characters; otherwise, `false`.
- **Exceptions**: Throws `ArgumentNullException` if `value` is `null`.

---
### `public static string Repeat(string value, int count)`

Repeats a string a specified number of times.

- **Parameters**:
  - `value`: The string to repeat.
  - `count`: The number of times to repeat the string.
- **Return value**: A new string consisting of `value` repeated `count` times.
- **Exceptions**: Throws `ArgumentNullException` if `value` is `null`.
- **Exceptions**: Throws `ArgumentOutOfRangeException` if `count` is negative.

---
### `public static string Reverse(string value)`

Reverses the order of characters in a string.

- **Parameters**:
  - `value`: The string to reverse.
- **Return value**: A new string with characters in reverse order.
- **Exceptions**: Throws `ArgumentNullException` if `value` is `null`.

---
### `public static string ExtractNumbers(string value)`

Extracts all numeric characters from a string and concatenates them into a single string.

- **Parameters**:
  - `value`: The input string to process.
- **Return value**: A string containing only the numeric characters from `value`, in order of appearance.
- **Exceptions**: Throws `ArgumentNullException` if `value` is `null`.

---
### `public static string EnsureStartsWith(string value, string prefix)`

Ensures that a string starts with a specified prefix by prepending it if missing.

- **Parameters**:
  - `value`: The input string to check.
  - `prefix`: The prefix to ensure is present.
- **Return value**: The original string if it already starts with `prefix`; otherwise, a new string with `prefix` prepended.
- **Exceptions**: Throws `ArgumentNullException` if `value` or `prefix` is `null`.

---
### `public static string EnsureEndsWith(string value, string suffix)`

Ensures that a string ends with a specified suffix by appending it if missing.

- **Parameters**:
  - `value`: The input string to check.
  - `suffix`: The suffix to ensure is present.
- **Return value**: The original string if it already ends with `suffix`; otherwise, a new string with `suffix` appended.
- **Exceptions**: Throws `ArgumentNullException` if `value` or `suffix` is `null`.

---
### `public static string Capitalize(string value)`

Capitalizes the first character of a string and makes the rest lowercase.

- **Parameters**:
  - `value`: The input string to capitalize.
- **Return value**: A new string with the first character converted to uppercase and the rest to lowercase.
- **Exceptions**: Throws `ArgumentNullException` if `value` is `null`.
- **Exceptions**: Returns an empty string if `value` is empty.

## Usage
