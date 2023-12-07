#nullable enable

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
    public static InlineKeyboardBuilder AddButtons(this InlineKeyboardBuilder builder, IEnumerable<(string Text, string CallbackData)> buttons)
    {
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
    public static InlineKeyboardBuilder AddUrlButtons(this InlineKeyboardBuilder builder, IEnumerable<(string Text, string Url)> buttons)
    {
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
    public static InlineKeyboardBuilder AddButtonRow(this InlineKeyboardBuilder builder, params (string Text, string CallbackData)[] buttons)
    {
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
    public static InlineKeyboardBuilder AddUrlButtonRow(this InlineKeyboardBuilder builder, params (string Text, string Url)[] buttons)
    {
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
    public static InlineKeyboardBuilder AddConfirmationRow(this InlineKeyboardBuilder builder, string confirmCallbackData = "confirm", string cancelCallbackData = "cancel")
    {
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
    public static InlineKeyboardBuilder AddPaginationRow(
        this InlineKeyboardBuilder builder,
        bool hasPrevious,
        bool hasNext,
        int pageNumber,
        string baseCallbackData = "page")
    {
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
    public static InlineKeyboardBuilder AddSwitchInlineButtons(this InlineKeyboardBuilder builder, params (string Text, string Query)[] buttons)
    {
        foreach (var (text, query) in buttons)
        {
            builder.AddSwitchInlineButton(text, query);
        }
        return builder;
    }

    /// <summary>
    /// Adds a row with a single button that spans the full width (simulated by adding a disabled button placeholder).
    /// </summary>
    /// <param name="builder">The keyboard builder.</param>
    /// <param name="text">Button text.</param>
    /// <param name="callbackData">Callback data (optional).</param>
    /// <returns>The builder for fluent chaining.</returns>
    public static InlineKeyboardBuilder AddFullWidthButton(this InlineKeyboardBuilder builder, string text, string? callbackData = null)
    {
        if (callbackData is not null)
        {
            builder.AddButton(text, callbackData);
        }
        else
        {
            // For URL buttons as full-width
            builder.AddUrlButton(text, "#"); // Placeholder URL
        }
        return builder.NewRow();
    }

    /// <summary>
    /// Adds a grid of callback buttons from a 2D array.
    /// </summary>
    /// <param name="builder">The keyboard builder.</param>
    /// <param name="grid">2D array where each cell is (text, callbackData) or null for empty cell.</param>
    /// <returns>The builder for fluent chaining.</returns>
    public static InlineKeyboardBuilder AddButtonGrid(this InlineKeyboardBuilder builder, (string Text, string CallbackData)?[,] grid)
    {
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
    public static InlineKeyboardBuilder AddUrlButtonGrid(this InlineKeyboardBuilder builder, (string Text, string Url)?[,] grid)
    {
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
}
