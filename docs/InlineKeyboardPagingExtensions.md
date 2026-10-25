# InlineKeyboardPagingExtensions

The `InlineKeyboardPagingExtensions` class is a static extension class for `InlineKeyboardBuilder` that adds a ready-made pagination row to an inline keyboard. The row contains an optional **« Prev** button, a non-interactive page counter (for example `Page 2/5`), and an optional **Next »** button. The helper starts a new row, decides which navigation buttons to show based on the current page, and derives callback data from a prefix you supply. You handle the callbacks in your own update handler.

Namespace: `TelegramBotFramework.Keyboard`
Source: `src/TelegramBotFramework/Keyboard/InlineKeyboardPagingExtensions.cs`

## API

### AddPaginationRow
```csharp
public static InlineKeyboardBuilder AddPaginationRow(
    this InlineKeyboardBuilder builder,
    int currentPage,
    int totalPages,
    string callbackPrefix)
```
Starts a new row and appends the pagination controls to it, in this order:

1. **Previous** – `« Prev` with callback data `{callbackPrefix}_prev` when `currentPage > 1`. Otherwise a `—` placeholder with empty callback data.
2. **Counter** – `Page {currentPage}/{totalPages}` with empty callback data. It is shown on every page and is not meant to be acted on.
3. **Next** – `Next »` with callback data `{callbackPrefix}_next` when `currentPage < totalPages`. Otherwise a `—` placeholder with empty callback data.

*   **Parameters**:
    *   `builder`: The `InlineKeyboardBuilder` to extend.
    *   `currentPage`: The page being displayed, 1-based. Must be between `1` and `totalPages`.
    *   `totalPages`: The total number of pages. Must be at least `1`.
    *   `callbackPrefix`: Prefix for the navigation callback data. The method appends `_prev` or `_next`. Must not be null, empty, or whitespace.
*   **Returns**: The same `InlineKeyboardBuilder` instance, so further fluent calls can be chained.
*   **Throws**:
    *   `ArgumentNullException` if `builder` is `null`.
    *   `ArgumentException` if `callbackPrefix` is `null`, empty, or whitespace.
    *   `ArgumentOutOfRangeException` if `totalPages < 1`, or if `currentPage` is outside `1..totalPages`.
    *   `ArgumentException` (from `InlineKeyboardBuilder.AddButton`) if the generated callback data exceeds Telegram's 64-byte limit.

## Behavior notes

*   **Callback data is never empty for real navigation.** The placeholder buttons (`—` and the counter) are created with `string.Empty` as callback data. The builder accepts this, but Telegram still delivers a callback query when they are pressed. Your handler should treat an empty or unrecognised callback as a no-op.
*   **Matching the suffix.** Only `_prev` and `_next` are generated. Parse them with an exact match on the full string, or with `EndsWith`, against the same prefix you passed in.
*   **Name overlap.** `InlineKeyboardBuilderExtensions` (`src/TelegramBotFramework/Keyboard/InlineKeyboardBuilderExtensions.cs`) also defines an `AddPaginationRow` extension with a different signature, `(bool hasPrevious, bool hasNext, int pageNumber, string baseCallbackData = ...)`. Both live in the same namespace. Choose the overload by its arguments, and check which one you are calling when reading code that uses both.
*   **Page state is yours.** The helper does not store the current page. Keep it in your session or callback data and pass it in on each render.

## Usage example

Render page 2 of a five-page catalog and handle navigation:

```csharp
using TelegramBotFramework.Keyboard;

const string prefix = "catalog";
var currentPage = 2;
var totalPages = 5;

// Build the keyboard: two item rows, then the pagination row on its own line
var markup = InlineKeyboardBuilder.Create()
    .AddButton("Item 11", "item_11")
    .AddButton("Item 12", "item_12")
    .AddPaginationRow(currentPage, totalPages, prefix)
    .Build();

// Row 1: Item 11, Item 12
// Row 2: « Prev (catalog_prev) | Page 2/5 (no callback) | Next » (catalog_next)
```

Handling the callback when a navigation button is pressed:

```csharp
static int ResolvePage(string callbackData, int currentPage, int totalPages)
{
    if (callbackData == "catalog_prev" && currentPage > 1)
        return currentPage - 1;

    if (callbackData == "catalog_next" && currentPage < totalPages)
        return currentPage + 1;

    // Empty or unknown callback (e.g. the page counter): keep the current page
    return currentPage;
}
```

After resolving the new page, call `AddPaginationRow` again with the updated `currentPage` and edit the message with the new keyboard.

## See also

*   [InlineKeyboardBuilder](InlineKeyboardBuilder.md) – the builder this class extends.
*   [InlineKeyboardBuilderExtensions](InlineKeyboardBuilderExtensions.md) – other keyboard helpers, including the alternative `AddPaginationRow` overload.
