#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Keyboard;

/// <summary>
/// Fluent builder for constructing Telegram inline keyboard markups.
/// Supports callback buttons, URL buttons, and switch-inline buttons,
/// organised into rows automatically or explicitly via <see cref="NewRow"/>.
/// </summary>
/// <example>
/// <code>
/// var keyboard = InlineKeyboardBuilder.Create()
///     .AddButton("✅ Confirm", "confirm")
///     .AddButton("❌ Cancel", "cancel")
///     .NewRow()
///     .AddUrlButton("🌐 Visit Site", "https://example.com")
///     .Build();
///
/// // Use with TelegramApiClient:
/// await apiClient.SendMessageWithInlineKeyboardAsync(chatId, "Choose:", keyboard);
/// </code>
/// </example>
public sealed class InlineKeyboardBuilder : IInlineKeyboardBuilder
{
    private readonly List<List<InlineKeyboardButton>> _rows = new();
    private List<InlineKeyboardButton> _currentRow = new();
    private readonly int _maxButtonsPerRow;

    /// <summary>
    /// Initialises a new <see cref="InlineKeyboardBuilder"/>.
    /// </summary>
    /// <param name="maxButtonsPerRow">
    /// Maximum buttons placed on a row before automatically starting a new one.
    /// Defaults to <c>3</c>. Use <see cref="NewRow"/> to force an earlier row break.
    /// </param>
    public InlineKeyboardBuilder(int maxButtonsPerRow = 3)
    {
        if (maxButtonsPerRow < 1)
            throw new ArgumentOutOfRangeException(nameof(maxButtonsPerRow), "Must be at least 1.");

        _maxButtonsPerRow = maxButtonsPerRow;
    }

    /// <summary>Creates a new builder instance with the default row width.</summary>
    public static InlineKeyboardBuilder Create(int maxButtonsPerRow = 3) => new(maxButtonsPerRow);

    /// <summary>
    /// Gets the rows of buttons that compose the keyboard.
    /// </summary>
    public IReadOnlyList<IReadOnlyList<InlineKeyboardButton>> InlineKeyboard
    {
        get
        {
            FlushCurrentRow();
            return _rows
                .Select(row => (IReadOnlyList<InlineKeyboardButton>)row.AsReadOnly())
                .ToList()
                .AsReadOnly();
        }
    }

    // -------------------------------------------------------------------------
    // Button adders
    // -------------------------------------------------------------------------

    /// <summary>
    /// Adds a callback button. When pressed, Telegram sends the <paramref name="callbackData"/>
    /// value back to the bot as a callback query.
    /// </summary>
    /// <param name="text">Button label shown to the user.</param>
    /// <param name="callbackData">
    /// Data sent in the callback query. Must not exceed 64 bytes (UTF-8).
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="callbackData"/> exceeds the 64-byte Telegram limit.
    /// </exception>
    public InlineKeyboardBuilder AddButton(string text, string callbackData)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Button text cannot be empty.", nameof(text));

        ValidateCallbackData(callbackData);

        return AppendButton(new InlineKeyboardButton
        {
            Text         = text,
            CallbackData = callbackData,
            Type         = InlineButtonType.Callback
        });
    }

    /// <summary>
    /// Adds a URL button. When pressed, the user's Telegram client opens the given URL.
    /// </summary>
    public InlineKeyboardBuilder AddUrlButton(string text, string url)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Button text cannot be empty.", nameof(text));

        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("URL cannot be empty.", nameof(url));

        return AppendButton(new InlineKeyboardButton
        {
            Text = text,
            Url  = url,
            Type = InlineButtonType.Url
        });
    }

    /// <summary>
    /// Adds a switch-inline button. When pressed, Telegram opens the inline query mode
    /// in the current chat, pre-filled with the optional <paramref name="query"/>.
    /// </summary>
    public InlineKeyboardBuilder AddSwitchInlineButton(string text, string query = "")
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Button text cannot be empty.", nameof(text));

        return AppendButton(new InlineKeyboardButton
        {
            Text              = text,
            SwitchInlineQuery = query,
            Type              = InlineButtonType.SwitchInline
        });
    }

    /// <summary>
    /// Forces the next button(s) onto a new row, even if the current row is not full.
    /// </summary>
    public InlineKeyboardBuilder NewRow()
    {
        FlushCurrentRow();
        return this;
    }

    // -------------------------------------------------------------------------
    // Build
    // -------------------------------------------------------------------------

    /// <summary>
    /// Builds and returns an immutable <see cref="InlineKeyboardMarkup"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when no buttons have been added.</exception>
    public InlineKeyboardMarkup Build()
    {
        FlushCurrentRow();

        if (_rows.Count == 0)
            throw new InvalidOperationException("Cannot build an empty keyboard — add at least one button.");

        var rows = _rows
            .Select(row => (IReadOnlyList<InlineKeyboardButton>)row.AsReadOnly())
            .ToList()
            .AsReadOnly();

        return new InlineKeyboardMarkup(rows);
    }

    /// <summary>
    /// Converts the current builder state to a <see cref="Models.Menu"/> for storage in the menu repository.
    /// </summary>
    /// <param name="menuId">Unique identifier for the menu.</param>
    /// <param name="title">Human-readable menu title.</param>
    public Models.Menu ToMenu(string menuId, string title)
    {
        if (string.IsNullOrWhiteSpace(menuId))
            throw new ArgumentException("Menu ID cannot be empty.", nameof(menuId));
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty.", nameof(title));

        var markup  = Build();
        var menu    = new Models.Menu
        {
            Id              = menuId,
            Title           = title,
            Type            = Models.MenuType.Inline,
            IsActive        = true,
            MaxButtonsPerRow = _maxButtonsPerRow
        };

        int order = 0;
        foreach (var row in markup.InlineKeyboard)
        {
            foreach (var btn in row)
            {
                menu.Buttons.Add(new Models.MenuButton
                {
                    Label        = btn.Text,
                    CallbackData = btn.CallbackData ?? string.Empty,
                    Url          = btn.Url,
                    Action       = btn.Type switch
                    {
                        InlineButtonType.Url        => Models.ButtonAction.OpenUrl,
                        InlineButtonType.SwitchInline => Models.ButtonAction.SwitchInline,
                        _                           => Models.ButtonAction.Callback
                    },
                    DisplayOrder = order++,
                    IsVisible    = true
                });
            }
        }

        return menu;
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    private InlineKeyboardBuilder AppendButton(InlineKeyboardButton button)
    {
        if (_currentRow.Count >= _maxButtonsPerRow)
            FlushCurrentRow();

        _currentRow.Add(button);
        return this;
    }

    private void FlushCurrentRow()
    {
        if (_currentRow.Count > 0)
        {
            _rows.Add(_currentRow);
            _currentRow = new List<InlineKeyboardButton>();
        }
    }

    private static void ValidateCallbackData(string callbackData)
    {
        if (string.IsNullOrEmpty(callbackData))
            return;

        var byteLength = System.Text.Encoding.UTF8.GetByteCount(callbackData);
        if (byteLength > Models.Menu.MaxCallbackDataBytes)
            throw new ArgumentException(
                $"Callback data '{callbackData}' is {byteLength} bytes, which exceeds " +
                $"Telegram's {Models.Menu.MaxCallbackDataBytes}-byte limit.",
                nameof(callbackData));
    }
}

// =============================================================================
// Keyboard markup model
// =============================================================================

/// <summary>
/// Immutable representation of a Telegram inline keyboard, ready to be serialised
/// and attached to any outgoing message.
/// </summary>
public sealed class InlineKeyboardMarkup
{
    /// <summary>Gets the rows of buttons that compose the keyboard.</summary>
    public IReadOnlyList<IReadOnlyList<InlineKeyboardButton>> InlineKeyboard { get; }

    internal InlineKeyboardMarkup(IReadOnlyList<IReadOnlyList<InlineKeyboardButton>> rows)
    {
        InlineKeyboard = rows ?? throw new ArgumentNullException(nameof(rows));
    }

    /// <summary>
    /// Returns a two-dimensional array of button labels compatible with
    /// <c>TelegramApiClient.SendMessageWithButtonsAsync</c>.
    /// </summary>
    public string[][] ToButtonLabels()
        => InlineKeyboard
            .Select(row => row.Select(b => b.Text).ToArray())
            .ToArray();

    /// <summary>Returns the total number of buttons across all rows.</summary>
    public int TotalButtonCount => InlineKeyboard.Sum(r => r.Count);

    /// <summary>Returns the number of rows in the keyboard.</summary>
    public int RowCount => InlineKeyboard.Count;
}

// =============================================================================
// Button model
// =============================================================================

/// <summary>
/// Represents a single button within an <see cref="InlineKeyboardMarkup"/>.
/// </summary>
public sealed class InlineKeyboardButton
{
    /// <summary>Gets the label displayed to the user.</summary>
    public string Text { get; init; } = string.Empty;

    /// <summary>Gets the callback data sent when the button is tapped (callback buttons only).</summary>
    public string? CallbackData { get; init; }

    /// <summary>Gets the URL opened when the button is tapped (URL buttons only).</summary>
    public string? Url { get; init; }

    /// <summary>Gets the inline-query string pre-filled when the button is tapped (switch-inline buttons only).</summary>
    public string? SwitchInlineQuery { get; init; }

    /// <summary>Gets the semantic type that determines the button's Telegram behaviour.</summary>
    public InlineButtonType Type { get; init; } = InlineButtonType.Callback;
}

/// <summary>
/// Distinguishes the Telegram behaviour triggered when an inline button is pressed.
/// </summary>
public enum InlineButtonType
{
    /// <summary>Sends a callback query with the configured data back to the bot.</summary>
    Callback = 0,

    /// <summary>Opens the configured URL in the Telegram client browser.</summary>
    Url = 1,

    /// <summary>Switches the user to inline-query mode pre-filled with an optional query string.</summary>
    SwitchInline = 2
}
