#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Keyboard;

/// <summary>
/// Provides validation helpers for <see cref="InlineKeyboardBuilder"/> to ensure
/// keyboard configurations are valid before building or using them.
/// </summary>
public static class InlineKeyboardBuilderValidation
{
    /// <summary>
    /// Validates the current state of an <see cref="InlineKeyboardBuilder"/> and returns
    /// a list of human-readable problems found.
    /// </summary>
    /// <param name="value">The builder instance to validate.</param>
    /// <returns>
    /// An empty list if the builder is valid; otherwise, a list of error messages
    /// describing each validation failure.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    public static IReadOnlyList<string> Validate(this InlineKeyboardBuilder value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate that we can build the keyboard (checks for empty keyboard)
        try
        {
            _ = value.Build();
        }
        catch (InvalidOperationException ex)
        {
            errors.Add(ex.Message);
            return errors.AsReadOnly(); // Early return if build fails
        }

        // Validate the built markup if successful
        var markup = value.Build();

        // Validate each row in the built markup
        for (int rowIndex = 0; rowIndex < markup.InlineKeyboard.Count; rowIndex++)
        {
            var row = markup.InlineKeyboard[rowIndex];

            if (row.Count == 0)
            {
                errors.Add($"Row {rowIndex} is empty.");
                continue;
            }

            // Validate each button in the row
            for (int buttonIndex = 0; buttonIndex < row.Count; buttonIndex++)
            {
                var button = row[buttonIndex];

                // Validate button text
                if (string.IsNullOrWhiteSpace(button.Text))
                {
                    errors.Add($"Button at row {rowIndex}, position {buttonIndex} has empty or whitespace text.");
                }
                else if (button.Text.Length > InlineKeyboardBuilderValidationConstants.MaxButtonTextLength)
                {
                    errors.Add($"Button at row {rowIndex}, position {buttonIndex} has text longer than {InlineKeyboardBuilderValidationConstants.MaxButtonTextLength} characters (length: {button.Text.Length}).");
                }

                // Validate button type-specific properties
                switch (button.Type)
                {
                    case InlineButtonType.Callback:
                        if (string.IsNullOrEmpty(button.CallbackData))
                        {
                            errors.Add($"Callback button at row {rowIndex}, position {buttonIndex} has null or empty CallbackData.");
                        }
                        else if (System.Text.Encoding.UTF8.GetByteCount(button.CallbackData) > Models.Menu.MaxCallbackDataBytes)
                        {
                            errors.Add($"Callback button at row {rowIndex}, position {buttonIndex} has CallbackData exceeding {Models.Menu.MaxCallbackDataBytes} bytes (byte length: {System.Text.Encoding.UTF8.GetByteCount(button.CallbackData)}).");
                        }
                        break;

                    case InlineButtonType.Url:
                        if (string.IsNullOrWhiteSpace(button.Url))
                        {
                            errors.Add($"URL button at row {rowIndex}, position {buttonIndex} has null or empty Url.");
                        }
                        else if (!Uri.TryCreate(button.Url, UriKind.Absolute, out _))
                        {
                            errors.Add($"URL button at row {rowIndex}, position {buttonIndex} has invalid URL format: '{button.Url}'");
                        }
                        break;

                    case InlineButtonType.SwitchInline:
                        // SwitchInlineQuery can be empty string, which is valid
                        if (button.SwitchInlineQuery?.Length > InlineKeyboardBuilderValidationConstants.MaxButtonTextLength)
                        {
                            errors.Add($"Switch-inline button at row {rowIndex}, position {buttonIndex} has SwitchInlineQuery exceeding {InlineKeyboardBuilderValidationConstants.MaxButtonTextLength} characters (length: {button.SwitchInlineQuery?.Length ?? 0}).");
                        }
                        break;
                }
            }
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="InlineKeyboardBuilder"/> is in a valid state.
    /// </summary>
    /// <param name="value">The builder instance to validate.</param>
    /// <returns>
    /// <see langword="true"/> if the builder is valid; otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    public static bool IsValid(this InlineKeyboardBuilder value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="InlineKeyboardBuilder"/> is in a valid state,
    /// throwing an <see cref="ArgumentException"/> with a detailed message listing all
    /// validation failures if it is not.
    /// </summary>
    /// <param name="value">The builder instance to validate.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the builder contains validation errors. The exception message
    /// lists all problems found.
    /// </exception>
    public static void EnsureValid(this InlineKeyboardBuilder value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();

        if (errors.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"InlineKeyboardBuilder validation failed with {errors.Count} error(s):{Environment.NewLine}" +
            string.Join(Environment.NewLine, errors));
    }
}