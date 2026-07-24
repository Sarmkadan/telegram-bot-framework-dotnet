#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBotFramework.Keyboard;

/// <summary>
/// Extension methods for <see cref="ReplyKeyboardBuilder"/> that provide additional
/// fluent APIs for common keyboard construction patterns.
/// </summary>
public static class ReplyKeyboardBuilderExtensions
{
    /// <summary>
    /// Adds multiple buttons to the current row from a collection of texts.
    /// </summary>
    /// <param name="builder">The keyboard builder.</param>
    /// <param name="texts">Collection of button texts to add.</param>
    /// <returns>The same builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="builder"/> or <paramref name="texts"/> is <see langword="null"/>.
    /// </exception>
    public static ReplyKeyboardBuilder AddButtons(this ReplyKeyboardBuilder builder, IEnumerable<string> texts)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(texts);

        foreach (var text in texts)
        {
            builder.AddButton(text);
        }

        return builder;
    }

    /// <summary>
    /// Adds multiple buttons to the current row from a collection of texts,
    /// each configured with a custom action.
    /// </summary>
    /// <param name="builder">The keyboard builder.</param>
    /// <param name="buttonConfigs">Collection of (text, configure) tuples to add.</param>
    /// <returns>The same builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="builder"/> or <paramref name="buttonConfigs"/> is <see langword="null"/>.
    /// </exception>
    public static ReplyKeyboardBuilder AddButtons(this ReplyKeyboardBuilder builder, IEnumerable<(string Text, Action<ReplyKeyboardButton> Configure)> buttonConfigs)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(buttonConfigs);

        foreach (var (text, configure) in buttonConfigs)
        {
            builder.AddButton(text, configure);
        }

        return builder;
    }

    /// <summary>
    /// Adds a row of buttons from a collection of texts.
    /// </summary>
    /// <param name="builder">The keyboard builder.</param>
    /// <param name="texts">Collection of button texts to add as a new row.</param>
    /// <returns>The same builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="builder"/> or <paramref name="texts"/> is <see langword="null"/>.
    /// </exception>
    public static ReplyKeyboardBuilder AddRow(this ReplyKeyboardBuilder builder, IEnumerable<string> texts)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(texts);

        builder.NewRow();
        return builder.AddButtons(texts);
    }

    /// <summary>
    /// Adds a row of buttons with custom configuration from a collection.
    /// </summary>
    /// <param name="builder">The keyboard builder.</param>
    /// <param name="buttonConfigs">Collection of (text, configure) tuples to add as a new row.</param>
    /// <returns>The same builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="builder"/> or <paramref name="buttonConfigs"/> is <see langword="null"/>.
    /// </exception>
    public static ReplyKeyboardBuilder AddRow(this ReplyKeyboardBuilder builder, IEnumerable<(string Text, Action<ReplyKeyboardButton> Configure)> buttonConfigs)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(buttonConfigs);

        builder.NewRow();
        return builder.AddButtons(buttonConfigs);
    }

    /// <summary>
    /// Adds a button that requests the user's contact information.
    /// </summary>
    /// <param name="builder">The keyboard builder.</param>
    /// <param name="text">Button label shown to the user.</param>
    /// <returns>The same builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="builder"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="text"/> is null, empty, or whitespace.
    /// </exception>
    public static ReplyKeyboardBuilder AddContactButton(this ReplyKeyboardBuilder builder, string text)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.AddButton(text, button => button.RequestContact = true);
    }

    /// <summary>
    /// Adds a button that requests the user's location.
    /// </summary>
    /// <param name="builder">The keyboard builder.</param>
    /// <param name="text">Button label shown to the user.</param>
    /// <returns>The same builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="builder"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="text"/> is null, empty, or whitespace.
    /// </exception>
    public static ReplyKeyboardBuilder AddLocationButton(this ReplyKeyboardBuilder builder, string text)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.AddButton(text, button => button.RequestLocation = true);
    }

    /// <summary>
    /// Adds multiple contact request buttons to the current row.
    /// </summary>
    /// <param name="builder">The keyboard builder.</param>
    /// <param name="texts">Collection of button texts to add as contact request buttons.</param>
    /// <returns>The same builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="builder"/> or <paramref name="texts"/> is <see langword="null"/>.
    /// </exception>
    public static ReplyKeyboardBuilder AddContactButtons(this ReplyKeyboardBuilder builder, IEnumerable<string> texts)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(texts);

        foreach (var text in texts)
        {
            builder.AddContactButton(text);
        }

        return builder;
    }

    /// <summary>
    /// Adds multiple location request buttons to the current row.
    /// </summary>
    /// <param name="builder">The keyboard builder.</param>
    /// <param name="texts">Collection of button texts to add as location request buttons.</param>
    /// <returns>The same builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="builder"/> or <paramref name="texts"/> is <see langword="null"/>.
    /// </exception>
    public static ReplyKeyboardBuilder AddLocationButtons(this ReplyKeyboardBuilder builder, IEnumerable<string> texts)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(texts);

        foreach (var text in texts)
        {
            builder.AddLocationButton(text);
        }

        return builder;
    }

    /// <summary>
    /// Adds a predefined set of common action buttons (Home, Back, Menu, Help).
    /// </summary>
    /// <param name="builder">The keyboard builder.</param>
    /// <param name="includeHome">Whether to include a Home button.</param>
    /// <param name="includeBack">Whether to include a Back button.</param>
    /// <param name="includeMenu">Whether to include a Menu button.</param>
    /// <param name="includeHelp">Whether to include a Help button.</param>
    /// <returns>The same builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="builder"/> is <see langword="null"/>.
    /// </exception>
    public static ReplyKeyboardBuilder AddCommonActions(this ReplyKeyboardBuilder builder,
        bool includeHome = true,
        bool includeBack = true,
        bool includeMenu = true,
        bool includeHelp = true)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var actions = new List<(string Text, Action<ReplyKeyboardButton>? Configure)>(4);

        if (includeHome) actions.Add(("🏠 Home", null));
        if (includeBack) actions.Add(("⬅️ Back", null));
        if (includeMenu) actions.Add(("📋 Menu", null));
        if (includeHelp) actions.Add(("❓ Help", null));

        return builder.AddButtons(actions.Select(x => x.Text).Zip(
            actions.Select(x => x.Configure ?? (_ => { }))));
    }

    /// <summary>
    /// Configures the keyboard to be one-time and resized in a single call.
    /// </summary>
    /// <param name="builder">The keyboard builder.</param>
    /// <returns>The same builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="builder"/> is <see langword="null"/>.
    /// </exception>
    public static ReplyKeyboardBuilder OneTimeResize(this ReplyKeyboardBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.OneTime().Resize();
    }

    /// <summary>
    /// Configures the keyboard to be persistent and not resized in a single call.
    /// </summary>
    /// <param name="builder">The keyboard builder.</param>
    /// <returns>The same builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="builder"/> is <see langword="null"/>.
    /// </exception>
    public static ReplyKeyboardBuilder PersistentNoResize(this ReplyKeyboardBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.Persistent().NoResize();
    }

    /// <summary>
    /// Adds a numeric keypad with digits 0-9 and common action buttons.
    /// Useful for PIN entry or numeric selection interfaces.
    /// </summary>
    /// <param name="builder">The keyboard builder.</param>
    /// <param name="includeZero">Whether to include 0 button.</param>
    /// <param name="includeClear">Whether to include a Clear button.</param>
    /// <param name="includeEnter">Whether to include an Enter button.</param>
    /// <returns>The same builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="builder"/> is <see langword="null"/>.
    /// </exception>
    public static ReplyKeyboardBuilder AddNumericKeypad(this ReplyKeyboardBuilder builder,
        bool includeZero = true,
        bool includeClear = true,
        bool includeEnter = true)
    {
        ArgumentNullException.ThrowIfNull(builder);

        // Add digits 1-9
        for (int i = 1; i <= 9; i++)
        {
            builder.AddButton(i.ToString(CultureInfo.InvariantCulture));
        }

        // Add 0 in a new row
        if (includeZero)
        {
            builder.NewRow().AddButton("0");
        }

        // Add action buttons
        var actions = new List<string>();
        if (includeClear) actions.Add("⌫ Clear");
        if (includeEnter) actions.Add("✓ Enter");

        if (actions.Count > 0)
        {
            builder.NewRow().AddButtons(actions);
        }

        return builder;
    }
}