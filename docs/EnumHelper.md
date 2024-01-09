# EnumHelper

Utility class providing static helper methods for working with .NET enumerations. It centralizes common enum operations such as retrieving all values, parsing, attribute extraction, flag checking, and conversion to dictionaries, promoting reuse and type‑safe handling of enum types across the telegram‑bot‑framework‑dotnet project.

## API

### `public static IEnumerable<T> GetAllValues<T>() where T : Enum`
- **Purpose**: Returns an enumeration of all defined constants of the enum type `T`.
- **Parameters**: None.
- **Return Value**: `IEnumerable<T>` containing each member of `T`.
- **Exceptions**: Throws `ArgumentException` if `T` is not an enum type.

### `public static T TryParse<T>(string value) where T : Enum`
- **Purpose**: Attempts to convert the string representation of an enum constant to its equivalent `T` value.
- **Parameters**: 
  - `value`: The string to parse. Case‑insensitive; may include whitespace.
- **Return Value**: The enum value represented by `value`.
- **Exceptions**: 
  - Throws `ArgumentNullException` if `value` is `null`.
  - Throws `ArgumentException` if `value` does not correspond to a defined constant of `T` or if `T` is not an enum type.

### `public static string GetDescription<T>(T enumValue) where T : Enum`
- **Purpose**: Retrieves the description associated with `enumValue` via the `System.ComponentModel.DescriptionAttribute`.
- **Parameters**: 
  - `enumValue`: The enum member whose description is requested.
- **Return Value**: The description string if a `DescriptionAttribute` is present; otherwise, the string representation of `enumValue` (`enumValue.ToString()`).
- **Exceptions**: Throws `ArgumentException` if `T` is not an enum type.

### `public static Dictionary<string, T> EnumToDictionary<T>() where T : Enum`
- **Purpose**: Builds a dictionary mapping the string names of each enum constant to its underlying value.
- **Parameters**: None.
- **Return Value**: `Dictionary<string, T>` where the key is `Enum.GetName(typeof(T), value)` and the value is the enum constant.
- **Exceptions**: Throws `ArgumentException` if `T` is not an enum type.

### `public static bool HasFlag<T>(T value, T flag) where T : Enum`
- **Purpose**: Determines whether one or more bit fields are set in `value` (i.e., whether `value` has the flags specified by `flag`).
- **Parameters**: 
  - `value`: The enum value to test.
  - `flag`: The enum value containing the flags to check.
- **Return Value**: `true` if all flags in `flag` are set in `value`; otherwise `false`.
- **Exceptions**: Throws `ArgumentException` if `T` is not an enum type or if `T` is not decorated with the `FlagsAttribute`.

### `public static object GetNumericValue<T>(T enumValue) where T : Enum`
- **Purpose**: Returns the underlying integral value of `enumValue` as a boxed `object`.
- **Parameters**: 
  - `enumValue`: The enum member to convert.
- **Return Value**: An `object` representing the numeric value of `enumValue` (type depends on the underlying enum type, e.g., `int`, `long`).
- **Exceptions**: Throws `ArgumentException` if `T` is not an enum type.

### `public static IEnumerable<T> GetAttributes<T>(T enumValue) where T : Attribute`
- **Purpose**: Retrieves all custom attributes of type `T` applied to the specified enum member.
- **Parameters**: 
  - `enumValue`: The enum member whose attributes are inspected.
- **Return Value**: `IEnumerable<T>` containing the attributes of type `T`; empty if none are present.
- **Exceptions**: Throws `ArgumentException` if the enum type of `enumValue` is not an enum, or if `T` is not a class derived from `System.Attribute`.

### `public static Dictionary<T, string> EnumToDisplayDictionary<T>() where T : Enum`
- **Purpose**: Creates a dictionary mapping each enum constant to its display name as defined by `System.ComponentModel.DataAnnotations.DisplayAttribute`.
- **Parameters**: None.
- **Return Value**: `Dictionary<T, string>` where the key is the enum constant and the value is the `Name` property of the `DisplayAttribute`; if no such attribute exists, the enum constant's name (`ToString()`) is used.
- **Exceptions**: Throws `ArgumentException` if `T` is not an enum type.

### `public static bool IsValid<T>(T value) where T : Enum`
- **Purpose**: Indicates whether `value` is a defined member of the enum type `T`.
- **Parameters**: 
  - `value`: The enum value to validate.
- **Return Value**: `true` if `value` corresponds to a constant of `T`; otherwise `false`.
- **Exceptions**: Throws `ArgumentException` if `T` is not an enum type.

### `public static string GetName<T>(T enumValue) where T : Enum`
- **Purpose**: Retrieves the string name of the enum constant `enumValue`.
- **Parameters**: 
  - `enumValue`: The enum member whose name is desired.
- **Return Value**: The name of `enumValue` as returned by `Enum.GetName(typeof(T), enumValue)`.
- **Exceptions**: Throws `ArgumentException` if `T` is not an enum type.

## Usage

```csharp
using TelegramBotFrameworkDotnet.Helpers; // namespace containing EnumHelper
using System.ComponentModel;

public enum Priority
{
    [Description("Low importance")]
    Low = 1,
    [Description("Normal importance")]
    Normal = 2,
    [Description("High importance")]
    High = 3
}

// Get all enum values
foreach (var p in EnumHelper.GetAllValues<Priority>())
{
    Console.WriteLine($"{p}: {EnumHelper.GetDescription(p)}");
}

// Safe parsing with fallback
string input = "High";
Priority priority = EnumHelper.TryParse<Priority>(input);
Console.WriteLine($"Parsed priority: {priority}");
```

```csharp
using System;
using System.Collections.Generic;

[Flags]
public enum Permissions
{
    None = 0,
    Read = 1,
    Write = 2,
    Execute = 4
}

// Build a dictionary for UI binding
Dictionary<Permissions, string> displayMap = EnumHelper.EnumToDisplayDictionary<Permissions>();
foreach (var kvp in displayMap)
{
    Console.WriteLine($"{kvp.Key} -> {kvp.Value}");
}

// Flag checking
Permissions userPerm = Permissions.Read | Permissions.Write;
bool canWrite = EnumHelper.HasFlag(userPerm, Permissions.Write); // true
bool canExecute = EnumHelper.HasFlag(userPerm, Permissions.Execute); // false
```

## Notes

- All generic methods enforce the `where T : Enum` constraint at compile time; passing a non‑enum type results in an `ArgumentException`.
- `GetAllValues<T>`, `EnumToDictionary<T>`, and `EnumToDisplayDictionary<T>` allocate new collections on each call; if frequent invocation is performance‑critical, consider caching the results.
- `TryParse<T>` throws on invalid input; for a non‑throwing pattern, wrap the call in a try/catch or use `Enum.TryParse` directly.
- `HasFlag<T>` requires the enum to be marked with `[Flags]`; otherwise the method will throw an `ArgumentException`.
- `GetAttributes<T>` returns attributes applied to the specific enum member, not to the enum type itself.
- The class contains only static members and no internal state, making it thread‑safe for concurrent use by multiple threads. No locking is required.
