# InlineKeyboardBuilder

`InlineKeyboardBuilder` provides a fluent API for constructing `InlineKeyboardMarkup` instances used in Telegram bot messages. It manages a two-dimensional grid of `InlineKeyboardButton` objects, tracks the current row and the most recently added button's properties, and exposes both the raw button layout and higher-level abstractions such as `ToMenu` for menu-driven interactions.

## API

### Constructors

```csharp
public InlineKeyboardBuilder()
```
Creates a new builder with an empty button grid and no active row. All button-level properties (`Text`, `CallbackData`, `Url`, `SwitchInlineQuery`, `Type`) are set to `null` or their default values.

### Static Factory

```csharp
public static InlineKeyboardBuilder Create()
```
Returns a new `InlineKeyboardBuilder` instance. Equivalent to calling the parameterless constructor. Provided for fluent usage patterns where a static entry point is preferred.

### Button Addition Methods

```csharp
public InlineKeyboardBuilder AddButton(string text, string callbackData)
```
Appends a callback button to the current row. Sets `Text` to `text`, `CallbackData` to `callbackData`, `Type` to `InlineButtonType.Callback`, and clears `Url` and `SwitchInlineQuery`. If no row exists, a new row is started automatically. Returns the builder for chaining.

```csharp
public InlineKeyboardBuilder AddUrlButton(string text, string url)
```
Appends a URL button to the current row. Sets `Text` to `text`, `Url` to `url`, `Type` to `InlineButtonType.Url`, and clears `CallbackData` and `SwitchInlineQuery`. If no row exists, a new row is started automatically. Returns the builder for chaining.

```csharp
public InlineKeyboardBuilder AddSwitchInlineButton(string text, string switchInlineQuery)
```
Appends a switch-to-inline button to the current row. Sets `Text` to `text`, `SwitchInlineQuery` to `switchInlineQuery`, `Type` to `InlineButtonType.SwitchInline`, and clears `CallbackData` and `Url`. If no row exists, a new row is started automatically. Returns the builder for chaining.

### Row Management

```csharp
public InlineKeyboardBuilder NewRow()
```
Finalizes the current row (if any) and starts a new empty row. Subsequent `AddButton`, `AddUrlButton`, or `AddSwitchInlineButton` calls will place buttons on this new row. Returns the builder for chaining.

### Build Methods

```csharp
public InlineKeyboardMarkup Build()
```
Constructs and returns an `InlineKeyboardMarkup` from the current button grid. Each row in the grid becomes an `IEnumerable<InlineKeyboardButton>` in the markup. The builder remains usable after this call; further modifications will affect a subsequent `Build`.

```csharp
public Models.Menu ToMenu()
```
Wraps the current button grid in a `Models.Menu` object. The returned menu can be used with menu-driven navigation components in the framework. The builder remains usable after this call.

### Properties

```csharp
public IReadOnlyList<IReadOnlyList<InlineKeyboardButton>> InlineKeyboard { get; }
```
Exposes the current button grid as a read-only list of rows, where each row is a read-only list of `InlineKeyboardButton`. Modifications to the builder (adding buttons or rows) are reflected immediately in this property.

```csharp
public string[][] ToButtonLabels { get; }
```
Returns a jagged array of button label strings corresponding to the current grid layout. Each inner array represents a row, and each string is the `Text` property of the corresponding `InlineKeyboardButton`. The array is computed on each access and reflects the current state.

```csharp
public string Text { get; }
```
The text of the most recently added button. `null` if no button has been added yet.

```csharp
public string? CallbackData { get; }
```
The callback data of the most recently added button. `null` if the last button was not a callback button or no button has been added.

```csharp
public string? Url { get; }
```
The URL of the most recently added button. `null` if the last button was not a URL button or no button has been added.

```csharp
public string? SwitchInlineQuery { get; }
```
The switch-inline query of the most recently added button. `null` if the last button was not a switch-inline button or no button has been added.

```csharp
public InlineButtonType Type { get; }
```
The type of the most recently added button. Defaults to the enum's zero value if no button has been added.

## Usage

### Example 1: Simple Inline Keyboard with Mixed Button Types

```csharp
var markup = InlineKeyboardBuilder.Create()
    .AddButton("Yes", "confirm_yes")
    .AddButton("No", "confirm_no")
    .NewRow()
    .AddUrlButton("Docs", "https://example.com/docs")
    .Build();

// Send with a Telegram bot client
await botClient.SendTextMessageAsync(
    chatId,
    "Please confirm:",
    replyMarkup: markup);
```

This produces a keyboard with two rows: the first row contains two callback buttons ("Yes" and "No"), and the second row contains a single URL button ("Docs").

### Example 2: Building a Menu for Framework Navigation

```csharp
var menu = InlineKeyboardBuilder.Create()
    .AddButton("Profile", "/profile")
    .AddButton("Settings", "/settings")
    .NewRow()
    .AddButton("Back", "/main")
    .ToMenu();

// The menu can be passed to framework components that handle navigation
navigationService.NavigateTo(menu);
```

This constructs a two-row menu with navigation callbacks and wraps it in a `Models.Menu` object suitable for the framework's menu system.

## Notes

- **Row auto-creation**: If `AddButton`, `AddUrlButton`, or `AddSwitchInlineButton` is called when no row exists (including immediately after construction or after `Build`/`ToMenu` without starting a new row), a new row is created automatically. This ensures the first button is never lost.
- **Property exposure of last button**: The `Text`, `CallbackData`, `Url`, `SwitchInlineQuery`, and `Type` properties reflect only the *most recently added* button. They are not aggregates of the entire keyboard. Adding a new button overwrites these values.
- **Builder reuse**: Calling `Build` or `ToMenu` does not reset the builder. The internal grid remains intact, and further button additions will append to the current row. To create a completely new keyboard, instantiate a new builder.
- **Thread safety**: `InlineKeyboardBuilder` is not thread-safe. Its mutable internal state (the button grid and last-button properties) is modified by every fluent method. Concurrent calls from multiple threads without external synchronization will lead to data races and corrupted keyboard layouts.
- **Empty keyboard**: Calling `Build` on a builder with no rows produces an `InlineKeyboardMarkup` with an empty button collection, which is valid per the Telegram Bot API but results in no visible keyboard.
- **`ToButtonLabels` consistency**: The property computes its result from the current grid on every access. If buttons are added after reading the property, a subsequent read will return updated labels. No caching is performed.
