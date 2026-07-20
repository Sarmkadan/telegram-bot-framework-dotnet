#nullable enable

using TelegramBotFramework.Utilities;

namespace TelegramBotFramework.Keyboard;

/// <summary>
/// Provides extension methods for <see cref="InlineKeyboardBuilder"/> to simplify common keyboard patterns.
/// </summary>
public static class InlineKeyboardBuilderExtensions
{
    /// <summary>
    /// Adds multiple callback buttons in a single fluent call.
    /// </summary>
    /// <param name="builder">The keyboard builder.</param>
    /// <param name="buttons">Collection of (text, callbackData) pairs.</param>
    /// <returns>The builder for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="builder"/> or <paramref name="buttons"/> is <see langword="null"/>.</exception>
    public static InlineKeyboardBuilder AddButtons(this InlineKeyboardBuilder builder, IEnumerable<(string Text, string CallbackData)> buttons)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(buttons);

        foreach (var (text, callbackData) in buttons)
        {
            builder.AddButton(text, callbackData);
        }
        return builder;
    }

    /// <summary>
    /// Adds multiple URL buttons in a single fluent call.
    /// </summary>
    /// <param name="builder">The keyboard builder.</param>
    /// <param name="buttons">Collection of (text, url) pairs.</param>
    /// <returns>The builder for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="builder"/> or <paramref name="buttons"/> is <see langword="null"/>.</exception>
    public static InlineKeyboardBuilder AddUrlButtons(this InlineKeyboardBuilder builder, IEnumerable<(string Text, string Url)> buttons)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(buttons);

        foreach (var (text, url) in buttons)
        {
            builder.AddUrlButton(text, url);
        }
        return builder;
    }

    /// <summary>
    /// Adds a row of callback buttons with automatic row management.
    /// </summary>
    /// <param name="builder">The keyboard builder.</param>
    /// <param name="buttons">Collection of (text, callbackData) pairs for the row.</param>
    /// <returns>The builder for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="builder"/> is <see langword="null"/>.</exception>
    public static InlineKeyboardBuilder AddButtonRow(this InlineKeyboardBuilder builder, params (string Text, string CallbackData)[] buttons)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(buttons);

        foreach (var (text, callbackData) in buttons)
        {
            builder.AddButton(text, callbackData);
        }
        return builder;
    }

    /// <summary>
    /// Adds a row of URL buttons with automatic row management.
    /// </summary>
    /// <param name="builder">The keyboard builder.</param>
    /// <param name="buttons">Collection of (text, url) pairs for the row.</param>
    /// <returns>The builder for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="builder"/> is <see langword="null"/>.</exception>
    public static InlineKeyboardBuilder AddUrlButtonRow(this InlineKeyboardBuilder builder, params (string Text, string Url)[] buttons)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(buttons);

        foreach (var (text, url) in buttons)
        {
            builder.AddUrlButton(text, url);
        }
        return builder;
    }

    /// <summary>
    /// Adds a confirmation row with ✅ Confirm and ❌ Cancel buttons.
    /// </summary>
    /// <param name="builder">The keyboard builder.</param>
    /// <param name="confirmCallbackData">Callback data for the confirm button.</param>
    /// <param name="cancelCallbackData">Callback data for the cancel button.</param>
    /// <returns>The builder for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="builder"/> is <see langword="null"/>.</exception>
    public static InlineKeyboardBuilder AddConfirmationRow(this InlineKeyboardBuilder builder, string confirmCallbackData = "confirm", string cancelCallbackData = "cancel")
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder
            .AddButton("✅ Confirm", confirmCallbackData)
            .AddButton("❌ Cancel", cancelCallbackData);
    }

    /// <summary>
    /// Adds a pagination row with previous/next navigation buttons.
    /// </summary>
    /// <param name="builder">The keyboard builder.</param>
    /// <param name="hasPrevious">Whether previous page is available.</param>
    /// <param name="hasNext">Whether next page is available.</param>
    /// <param name="pageNumber">Current page number (displayed in the center).</param>
    /// <param name="baseCallbackData">Base callback data for pagination (e.g., "page_").</param>
    /// <returns>The builder for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="builder"/> is <see langword="null"/>.</exception>
    public static InlineKeyboardBuilder AddPaginationRow(
        this InlineKeyboardBuilder builder,
        bool hasPrevious,
        bool hasNext,
        int pageNumber,
        string baseCallbackData = "page")
    {
        ArgumentNullException.ThrowIfNull(builder);

        if (hasPrevious)
        {
            builder.AddButton("⬅️ Previous", $"{baseCallbackData}_{pageNumber - 1}");
        }

        builder.AddButton($"📄 Page {pageNumber}", $"{baseCallbackData}_{pageNumber}_current");

        if (hasNext)
        {
            builder.AddButton("Next ➡️", $"{baseCallbackData}_{pageNumber + 1}");
        }

        return builder;
    }

    /// <summary>
    /// Adds a row of switch-inline buttons for quick actions.
    /// </summary>
    /// <param name="builder">The keyboard builder.</param>
    /// <param name="buttons">Collection of (text, query) pairs.</param>
    /// <returns>The builder for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="builder"/> is <see langword="null"/>.</exception>
    public static InlineKeyboardBuilder AddSwitchInlineButtons(this InlineKeyboardBuilder builder, params (string Text, string Query)[] buttons)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(buttons);

        foreach (var (text, query) in buttons)
        {
            builder.AddSwitchInlineButton(text, query);
        }
        return builder;
    }

    /// <summary>
    /// Adds a row with a single callback button that spans the full width of the keyboard.
    /// </summary>
    /// <param name="builder">The keyboard builder.</param>
    /// <param name="text">Button text.</param>
    /// <param name="callbackData">Callback data. Defaults to a no-op payload when omitted.</param>
    /// <returns>The builder for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="builder"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="text"/> is null or whitespace.</exception>
    public static InlineKeyboardBuilder AddFullWidthButton(this InlineKeyboardBuilder builder, string text, string? callbackData = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        builder.AddButton(text, callbackData ?? "noop");

        return builder.NewRow();
    }

    /// <summary>
    /// Adds a grid of callback buttons from a 2D array.
    /// </summary>
    /// <param name="builder">The keyboard builder.</param>
    /// <param name="grid">2D array where each cell is (text, callbackData) or null for empty cell.</param>
    /// <returns>The builder for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="builder"/> is <see langword="null"/>.</exception>
    public static InlineKeyboardBuilder AddButtonGrid(this InlineKeyboardBuilder builder, (string Text, string CallbackData)?[,] grid)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(grid);

        int rows = grid.GetLength(0);
        int cols = grid.GetLength(1);

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                var item = grid[i, j];
                if (item is not null)
                {
                    builder.AddButton(item.Value.Text, item.Value.CallbackData);
                }
            }
            if (i < rows - 1)
            {
                builder.NewRow();
            }
        }
        return builder;
    }

    /// <summary>
    /// Adds a grid of URL buttons from a 2D array.
    /// </summary>
    /// <param name="builder">The keyboard builder.</param>
    /// <param name="grid">2D array where each cell is (text, url) or null for empty cell.</param>
    /// <returns>The builder for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="builder"/> is <see langword="null"/>.</exception>
    public static InlineKeyboardBuilder AddUrlButtonGrid(this InlineKeyboardBuilder builder, (string Text, string Url)?[,] grid)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(grid);

        int rows = grid.GetLength(0);
        int cols = grid.GetLength(1);

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                var item = grid[i, j];
                if (item is not null)
                {
                    builder.AddUrlButton(item.Value.Text, item.Value.Url);
                }
            }
            if (i < rows - 1)
            {
                builder.NewRow();
            }
        }
        return builder;
    }

    /// <summary>
    /// Adds a callback button with HMAC-SHA256 signature protection.
    /// The callback data will include a truncated HMAC signature to prevent forgery.
    /// </summary>
    /// <param name="builder">The keyboard builder.</param>
    /// <param name="text">Button label shown to the user.</param>
    /// <param name="data">The original callback data payload.</param>
    /// <param name="secret">Secret key for HMAC signing.</param>
    /// <returns>The builder for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if builder, text, data, or secret is null.</exception>
    /// <exception cref="ArgumentException">
    /// Thrown if the resulting signed callback data exceeds Telegram's 64-byte limit.
    /// </exception>
    public static InlineKeyboardBuilder AddSignedButton(this InlineKeyboardBuilder builder, string text, string data, string secret)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrWhiteSpace(text);
        ArgumentException.ThrowIfNullOrWhiteSpace(data);
        ArgumentException.ThrowIfNullOrWhiteSpace(secret);

        var signedData = CallbackDataSigner.Sign(data, secret);
        return builder.AddButton(text, signedData);
    }

    /// <summary>
    /// Adds multiple signed callback buttons in a single fluent call.
    /// Each button's callback data will include an HMAC signature.
    /// </summary>
    /// <param name="builder">The keyboard builder.</param>
    /// <param name="secret">Secret key for HMAC signing.</param>
    /// <param name="buttons">Collection of (text, data) pairs to sign.</param>
    /// <returns>The builder for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if builder, secret, or buttons is null.</exception>
    /// <exception cref="ArgumentException">
    /// Thrown if any resulting signed callback data exceeds Telegram's 64-byte limit.
    /// </exception>
    public static InlineKeyboardBuilder AddSignedButtons(this InlineKeyboardBuilder builder, string secret, IEnumerable<(string Text, string Data)> buttons)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrWhiteSpace(secret);
        ArgumentNullException.ThrowIfNull(buttons);

        foreach (var (text, data) in buttons)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(text);
            ArgumentException.ThrowIfNullOrWhiteSpace(data);

            var signedData = CallbackDataSigner.Sign(data, secret);
            builder.AddButton(text, signedData);
        }

        return builder;
    }

    /// <summary>
    /// Adds a signed confirmation row with ✅ Confirm and ❌ Cancel buttons.
    /// Both buttons include HMAC signatures for security.
    /// </summary>
    /// <param name="builder">The keyboard builder.</param>
    /// <param name="secret">Secret key for HMAC signing.</param>
    /// <param name="confirmCallbackData">Callback data for the confirm button (default: "confirm").</param>
    /// <param name="cancelCallbackData">Callback data for the cancel button (default: "cancel").</param>
    /// <returns>The builder for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if builder or secret is null.</exception>
    /// <exception cref="ArgumentException">Thrown if secret is empty or whitespace.</exception>
    public static InlineKeyboardBuilder AddSignedConfirmationRow(this InlineKeyboardBuilder builder, string secret, string confirmCallbackData = "confirm", string cancelCallbackData = "cancel")
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrWhiteSpace(secret);

        return builder
            .AddSignedButton("✅ Confirm", confirmCallbackData, secret)
            .AddSignedButton("❌ Cancel", cancelCallbackData, secret);
    }

    /// <summary>
    /// Adds a signed pagination row with previous/next navigation buttons.
    /// All buttons include HMAC signatures for security.
    /// </summary>
    /// <param name="builder">The keyboard builder.</param>
    /// <param name="secret">Secret key for HMAC signing.</param>
    /// <param name="hasPrevious">Whether previous page is available.</param>
    /// <param name="hasNext">Whether next page is available.</param>
    /// <param name="pageNumber">Current page number (displayed in the center).</param>
    /// <param name="baseCallbackData">Base callback data for pagination (e.g., "page_").</param>
    /// <returns>The builder for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if builder or secret is null.</exception>
    /// <exception cref="ArgumentException">Thrown if secret is empty or whitespace.</exception>
    public static InlineKeyboardBuilder AddSignedPaginationRow(
        this InlineKeyboardBuilder builder,
        string secret,
        bool hasPrevious,
        bool hasNext,
        int pageNumber,
        string baseCallbackData = "page")
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrWhiteSpace(secret);

        if (hasPrevious)
        {
            var prevData = CallbackDataSigner.Sign($"{baseCallbackData}_{pageNumber - 1}", secret);
            builder.AddButton("⬅️ Previous", prevData);
        }

        var currentData = CallbackDataSigner.Sign($"{baseCallbackData}_{pageNumber}_current", secret);
        builder.AddButton($"📄 Page {pageNumber}", currentData);

        if (hasNext)
        {
            var nextData = CallbackDataSigner.Sign($"{baseCallbackData}_{pageNumber + 1}", secret);
            builder.AddButton("Next ➡️", nextData);
        }

        return builder;
    }
}