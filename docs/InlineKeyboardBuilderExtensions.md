# InlineKeyboardBuilderExtensions

The `InlineKeyboardBuilderExtensions` class provides a set of extension methods for the `InlineKeyboardBuilder` type, designed to streamline the construction of complex Telegram inline keyboards. These methods facilitate the rapid addition of button rows, grids, and specialized layouts (such as pagination and confirmation dialogs) by abstracting away repetitive boilerplate code required to instantiate and configure `InlineKeyboardButton` objects.

## API

### AddButtons
```csharp
public static InlineKeyboardBuilder AddButtons(this InlineKeyboardBuilder builder, IEnumerable<...> buttons)
```
Appends a new row containing the specified collection of standard inline buttons to the keyboard.
*   **Parameters**:
    *   `builder`: The target `InlineKeyboardBuilder` instance.
    *   `buttons`: An enumerable collection of button definitions (typically text/callback pairs).
*   **Returns**: The same `InlineKeyboardBuilder` instance to allow method chaining.
*   **Throws**: `ArgumentNullException` if `builder` or `buttons` is null.

### AddUrlButtons
```csharp
public static InlineKeyboardBuilder AddUrlButtons(this InlineKeyboardBuilder builder, IEnumerable<...> buttons)
```
Appends a new row containing the specified collection of URL buttons to the keyboard.
*   **Parameters**:
    *   `builder`: The target `InlineKeyboardBuilder` instance.
    *   `buttons`: An enumerable collection of URL button definitions (text and target URI).
*   **Returns**: The same `InlineKeyboardBuilder` instance to allow method chaining.
*   **Throws**: `ArgumentNullException` if `builder` or `buttons` is null.

### AddButtonRow
```csharp
public static InlineKeyboardBuilder AddButtonRow(this InlineKeyboardBuilder builder, params ... buttons)
```
Appends a new row containing the provided variable arguments of standard inline buttons.
*   **Parameters**:
    *   `builder`: The target `InlineKeyboardBuilder` instance.
    *   `buttons`: A parameter array of button definitions.
*   **Returns**: The same `InlineKeyboardBuilder` instance to allow method chaining.
*   **Throws**: `ArgumentNullException` if `builder` is null.

### AddUrlButtonRow
```csharp
public static InlineKeyboardBuilder AddUrlButtonRow(this InlineKeyboardBuilder builder, params ... buttons)
```
Appends a new row containing the provided variable arguments of URL buttons.
*   **Parameters**:
    *   `builder`: The target `InlineKeyboardBuilder` instance.
    *   `buttons`: A parameter array of URL button definitions.
*   **Returns**: The same `InlineKeyboardBuilder` instance to allow method chaining.
*   **Throws**: `ArgumentNullException` if `builder` is null.

### AddConfirmationRow
```csharp
public static InlineKeyboardBuilder AddConfirmationRow(...)
```
Appends a standardized row containing "Confirm" and "Cancel" buttons, typically used for action verification dialogs.
*   **Parameters**: Configuration parameters for button labels and callback data (specifics depend on internal overload resolution).
*   **Returns**: The same `InlineKeyboardBuilder` instance to allow method chaining.
*   **Throws**: `ArgumentNullException` if `builder` is null.

### AddPaginationRow
```csharp
public static InlineKeyboardBuilder AddPaginationRow(...)
```
Appends a standardized row containing navigation controls (e.g., Previous, Next, Page Indicator) for paginated content.
*   **Parameters**: Configuration parameters for current page index, total pages, and callback data prefixes.
*   **Returns**: The same `InlineKeyboardBuilder` instance to allow method chaining.
*   **Throws**: `ArgumentNullException` if `builder` is null; may throw `ArgumentOutOfRangeException` if page indices are invalid.

### AddSwitchInlineButtons
```csharp
public static InlineKeyboardBuilder AddSwitchInlineButtons(this InlineKeyboardBuilder builder, params ... buttons)
```
Appends a new row containing buttons that trigger the "Switch Inline Query" behavior.
*   **Parameters**:
    *   `builder`: The target `InlineKeyboardBuilder` instance.
    *   `buttons`: A parameter array of switch inline button definitions (text and query string).
*   **Returns**: The same `InlineKeyboardBuilder` instance to allow method chaining.
*   **Throws**: `ArgumentNullException` if `builder` is null.

### AddFullWidthButton
```csharp
public static InlineKeyboardBuilder AddFullWidthButton(...)
```
Appends a new row containing a single button that spans the entire width of the keyboard.
*   **Parameters**: Configuration parameters for the button text and callback data.
*   **Returns**: The same `InlineKeyboardBuilder` instance to allow method chaining.
*   **Throws**: `ArgumentNullException` if `builder` is null.

### AddButtonGrid
```csharp
public static InlineKeyboardBuilder AddButtonGrid(this InlineKeyboardBuilder builder, ...)
```
Appends multiple rows to form a grid layout based on a provided collection of standard buttons and a specified column count.
*   **Parameters**:
    *   `builder`: The target `InlineKeyboardBuilder` instance.
    *   Additional parameters defining the button collection and grid dimensions (columns).
*   **Returns**: The same `InlineKeyboardBuilder` instance to allow method chaining.
*   **Throws**: `ArgumentNullException` if `builder` or the button collection is null; `ArgumentOutOfRangeException` if column count is less than 1.

### AddUrlButtonGrid
```csharp
public static InlineKeyboardBuilder AddUrlButtonGrid(this InlineKeyboardBuilder builder, ...)
```
Appends multiple rows to form a grid layout based on a provided collection of URL buttons and a specified column count.
*   **Parameters**:
    *   `builder`: The target `InlineKeyboardBuilder` instance.
    *   Additional parameters defining the URL button collection and grid dimensions (columns).
*   **Returns**: The same `InlineKeyboardBuilder` instance to allow method chaining.
*   **Throws**: `ArgumentNullException` if `builder` or the button collection is null; `ArgumentOutOfRangeException` if column count is less than 1.

## Usage

### Example 1: Creating a Paginated List with Confirmation
This example demonstrates building a keyboard that displays pagination controls followed by a confirmation row for user action.

```csharp
using Telegram.Bot.Framework.Abstractions.Builders;

public InlineKeyboardMarkup BuildPaginationKeyboard(int currentPage, int totalPages)
{
    var builder = new InlineKeyboardBuilder();

    // Add a pagination row with custom callback prefixes
    builder.AddPaginationRow(currentPage, totalPages, "nav_prev", "nav_next");

    // Add a confirmation row for the selected item
    builder.AddConfirmationRow("confirm_action", "cancel_action");

    return builder.Build();
}
```

### Example 2: Constructing a Grid of URL Buttons
This example illustrates how to generate a responsive grid of buttons linking to external documentation pages.

```csharp
using Telegram.Bot.Framework.Abstractions.Builders;
using System.Collections.Generic;

public InlineKeyboardMarkup BuildDocsGrid()
{
    var links = new List<(string Text, string Url)>
    {
        ("API Reference", "https://example.com/docs/api"),
        ("Tutorials", "https://example.com/docs/tutorials"),
        ("Community", "https://example.com/community"),
        ("GitHub", "https://github.com/example/repo")
    };

    var builder = new InlineKeyboardBuilder();

    // Arrange the links in a 2-column grid
    builder.AddUrlButtonGrid(links, columns: 2);

    // Add a full-width support button at the bottom
    builder.AddFullWidthButton("Contact Support", "callback_support");

    return builder.Build();
}
```

## Notes

*   **Immutability and Chaining**: All extension methods return the modified `InlineKeyboardBuilder` instance. This design supports fluent chaining but implies that the underlying state of the builder is mutated. Care should be taken if the same builder instance is shared across multiple threads.
*   **Thread Safety**: The `InlineKeyboardBuilder` class and these extension methods are not thread-safe. If constructing a keyboard in a concurrent environment, ensure that a unique builder instance is used per thread or synchronize access to the builder object.
*   **Grid Layout Constraints**: When using `AddButtonGrid` or `AddUrlButtonGrid`, if the total number of buttons is not evenly divisible by the specified column count, the final row will contain fewer buttons than the others. Passing a column count of less than 1 will result in an exception.
*   **Null Handling**: While the methods guard against null `builder` instances, passing null collections (where `IEnumerable` is expected) will typically trigger an `ArgumentNullException`. Empty collections are generally valid and result in no buttons being added for that specific call.
*   **Button Limits**: Telegram imposes a limit on the number of buttons per row and the total number of buttons in an inline keyboard. These methods do not automatically enforce Telegram's specific limits; exceeding them will cause the API request sending the keyboard to fail.
