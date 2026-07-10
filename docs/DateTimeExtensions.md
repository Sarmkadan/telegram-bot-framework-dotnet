# DateTimeExtensions

A utility class providing common date and time manipulation methods for working with `DateTime` values in .NET applications, particularly in the context of Telegram bot development where time-based operations are frequent.

## API

### `public static long ToUnixTimestamp(DateTime dateTime)`
Converts a `DateTime` instance to a Unix timestamp (seconds since January 1, 1970, UTC).

- **Parameters**
  - `dateTime`: The `DateTime` value to convert. Assumed to be in UTC if `Kind` is `Unspecified`.
- **Return value**
  - A `long` representing the Unix timestamp in seconds.
- **Throws**
  - `ArgumentOutOfRangeException`: If `dateTime` is outside the range representable by Unix timestamps.

---

### `public static DateTime FromUnixTimestamp(long unixTimestamp)`
Converts a Unix timestamp (seconds since January 1, 1970, UTC) to a `DateTime` instance.

- **Parameters**
  - `unixTimestamp`: The Unix timestamp to convert.
- **Return value**
  - A `DateTime` instance in UTC with `Kind` set to `Utc`.
- **Throws**
  - `ArgumentOutOfRangeException`: If `unixTimestamp` is outside the valid range for `DateTime`.

---

### `public static bool IsPast(DateTime dateTime)`
Determines whether the given `DateTime` is in the past relative to `DateTime.UtcNow`.

- **Parameters**
  - `dateTime`: The `DateTime` to evaluate.
- **Return value**
  - `true` if `dateTime` is earlier than the current UTC time; otherwise, `false`.
- **Remarks**
  - Timezone-agnostic. Compares values directly; assumes `dateTime` is in UTC if `Kind` is `Unspecified`.

---

### `public static bool IsFuture(DateTime dateTime)`
Determines whether the given `DateTime` is in the future relative to `DateTime.UtcNow`.

- **Parameters**
  - `dateTime`: The `DateTime` to evaluate.
- **Return value**
  - `true` if `dateTime` is later than the current UTC time; otherwise, `false`.
- **Remarks**
  - Timezone-agnostic. Compares values directly; assumes `dateTime` is in UTC if `Kind` is `Unspecified`.

---
### `public static DateTime StartOfDay(DateTime dateTime)`
Returns a `DateTime` representing the start of the day (midnight) for the given date.

- **Parameters**
  - `dateTime`: The input `DateTime`.
- **Return value**
  - A `DateTime` with time components set to `00:00:00`.
- **Remarks**
  - Preserves the `Kind` of the input `dateTime`.

---
### `public static DateTime EndOfDay(DateTime dateTime)`
Returns a `DateTime` representing the end of the day (23:59:59.999) for the given date.

- **Parameters**
  - `dateTime`: The input `DateTime`.
- **Return value**
  - A `DateTime` with time components set to `23:59:59.999`.
- **Remarks**
  - Preserves the `Kind` of the input `dateTime`.

---
### `public static DateTime StartOfWeek(DateTime dateTime, DayOfWeek startDay = DayOfWeek.Monday)`
Returns a `DateTime` representing the start of the week (midnight of the first day of the week) for the given date.

- **Parameters**
  - `dateTime`: The input `DateTime`.
  - `startDay`: The day of the week considered the start of the week. Defaults to `Monday`.
- **Return value**
  - A `DateTime` at the start of the week containing `dateTime`.
- **Remarks**
  - Preserves the `Kind` of the input `dateTime`.

---
### `public static DateTime EndOfWeek(DateTime dateTime, DayOfWeek startDay = DayOfWeek.Monday)`
Returns a `DateTime` representing the end of the week (23:59:59.999 of the last day of the week) for the given date.

- **Parameters**
  - `dateTime`: The input `DateTime`.
  - `startDay`: The day of the week considered the start of the week. Defaults to `Monday`.
- **Return value**
  - A `DateTime` at the end of the week containing `dateTime`.
- **Remarks**
  - Preserves the `Kind` of the input `dateTime`.

---
### `public static DateTime StartOfMonth(DateTime dateTime)`
Returns a `DateTime` representing the start of the month (midnight of the first day of the month) for the given date.

- **Parameters**
  - `dateTime`: The input `DateTime`.
- **Return value**
  - A `DateTime` at the start of the month containing `dateTime`.
- **Remarks**
  - Preserves the `Kind` of the input `dateTime`.

---
### `public static DateTime EndOfMonth(DateTime dateTime)`
Returns a `DateTime` representing the end of the month (23:59:59.999 of the last day of the month) for the given date.

- **Parameters**
  - `dateTime`: The input `DateTime`.
- **Return value**
  - A `DateTime` at the end of the month containing `dateTime`.
- **Remarks**
  - Preserves the `Kind` of the input `dateTime`.

---
### `public static string ToRelativeTimeString(DateTime dateTime, bool includeSeconds = false)`
Converts a `DateTime` to a human-readable relative time string (e.g., "2 minutes ago", "in 3 hours").

- **Parameters**
  - `dateTime`: The input `DateTime`.
  - `includeSeconds`: If `true`, includes seconds in the output for recent times (e.g., "30 seconds ago").
- **Return value**
  - A localized string describing the relative time.
- **Remarks**
  - Uses `DateTime.UtcNow` as the reference point.
  - Handles past and future times appropriately.

---
### `public static bool IsBetween(DateTime dateTime, DateTime start, DateTime end)`
Determines whether a `DateTime` falls between two other `DateTime` values (inclusive).

- **Parameters**
  - `dateTime`: The `DateTime` to check.
  - `start`: The start of the range.
  - `end`: The end of the range.
- **Return value**
  - `true` if `dateTime` is between `start` and `end` (inclusive); otherwise, `false`.
- **Remarks**
  - Compares values directly; assumes all inputs are in UTC if `Kind` is `Unspecified`.

---
### `public static DateTime AddBusinessDays(DateTime dateTime, int days)`
Adds a specified number of business days (Monday–Friday) to a `DateTime`.

- **Parameters**
  - `dateTime`: The starting `DateTime`.
  - `days`: The number of business days to add (can be negative).
- **Return value**
  - A `DateTime` representing the result after adding the business days.
- **Throws**
  - `ArgumentOutOfRangeException`: If the resulting date is outside the valid `DateTime` range.
- **Remarks**
  - Skips weekends and does not account for holidays.

---
### `public static int GetAge(DateTime birthDate)`
Calculates the age in years from a given birth date to the current date.

- **Parameters**
  - `birthDate`: The birth date.
- **Return value**
  - The age in whole years.
- **Remarks**
  - Uses `DateTime.UtcNow` as the reference date.
  - Returns `0` if `birthDate` is in the future.

## Usage
