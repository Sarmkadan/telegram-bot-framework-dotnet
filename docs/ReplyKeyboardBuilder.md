# ReplyKeyboardBuilder

A fluent builder for constructing Telegram `ReplyKeyboardMarkup` objects with a clean API for adding buttons, controlling layout, and configuring keyboard behavior. It simplifies the creation of custom reply keyboards for Telegram bots by providing method chaining and sensible defaults.

## API

### `public ReplyKeyboardBuilder`
The default constructor initializes a new builder instance with an empty keyboard layout.

### `public static ReplyKeyboardBuilder Create()`
Creates and returns a new instance of `ReplyKeyboardBuilder` with default settings.

### `public ReplyKeyboardBuilder AddButton(string text)`
Adds a button with the specified text to the current row. The button will not send data unless explicitly handled by the bot logic.

- **Parameters**
  - `text` – The text to display on the button.
- **Return Value**
  - Returns the builder instance for method chaining.
- **Exceptions**
  - Throws `ArgumentNullException` if `text` is `null`.

### `public ReplyKeyboardBuilder AddButton(InlineKeyboardButton button)`
Adds a pre-configured inline keyboard button to the current row.

- **Parameters**
  - `button` – The `InlineKeyboardButton` to add.
- **Return Value**
  - Returns the builder instance for method chaining.
- **Exceptions**
  - Throws `ArgumentNullException` if `button` is `null`.

### `public ReplyKeyboardBuilder NewRow()`
Starts a new row of buttons in the keyboard layout. Any subsequent buttons will be placed in a new horizontal row.

- **Return Value**
  - Returns the builder instance for method chaining.

### `public ReplyKeyboardBuilder OneTime()`
Configures the keyboard to be one-time use. The keyboard will be hidden immediately after the user selects a button.

- **Return Value**
  - Returns the builder instance for method chaining.

### `public ReplyKeyboardBuilder Persistent()`
Configures the keyboard to remain visible after a button is pressed. This is the default behavior.

- **Return Value**
  - Returns the builder instance for method chaining.

### `public ReplyKeyboardBuilder Resize()`
Configures the keyboard to resize based on its content. This is the default behavior.

- **Return Value**
  - Returns the builder instance for method chaining.

### `public ReplyKeyboardBuilder NoResize()`
Configures the keyboard to not resize. The keyboard will maintain a fixed size regardless of content.

- **Return Value**
  - Returns the builder instance for method chaining.

### `public ReplyKeyboardMarkup Build()`
Constructs and returns the `ReplyKeyboardMarkup` instance based on the current builder configuration.

- **Return Value**
  - A configured `ReplyKeyboardMarkup` instance.
- **Exceptions**
  - Throws `InvalidOperationException` if no buttons have been added to any row.

### `public Models.Menu ToMenu()`
Converts the current keyboard configuration into a `Menu` model object. Useful for integrating with menu systems.

- **Return Value**
  - A `Menu` instance representing the keyboard layout and behavior.
- **Exceptions**
  - Throws `InvalidOperationException` if no buttons have been added to any row.

## Usage

### Example 1: Basic Keyboard with Two Rows
```csharp
var keyboard = ReplyKeyboardBuilder.Create()
    .AddButton("Start")
    .AddButton("Help")
    .NewRow()
    .AddButton("Settings")
    .AddButton("Exit")
    .OneTime()
    .Resize()
    .Build();
```

### Example 2: Using Inline Buttons and Persistence
```csharp
var inlineButton = InlineKeyboardButton.WithCallbackData("Confirm", "confirm_123");
var keyboard = ReplyKeyboardBuilder.Create()
    .AddButton("Cancel")
    .AddButton(inlineButton)
    .NewRow()
    .AddButton("Retry")
    .Persistent()
    .NoResize()
    .Build();
```

## Notes

- The builder is not thread-safe. Concurrent access from multiple threads requires external synchronization.
- Calling `Build()` or `ToMenu()` without any buttons added throws `InvalidOperationException` to prevent sending empty keyboards.
- Method chaining order does not affect the final layout; only the sequence of `AddButton` and `NewRow` calls determines button placement.
- Configuration methods like `OneTime()`, `Persistent()`, `Resize()`, and `NoResize()` override each other in the order they are called. The last one invoked takes effect.
