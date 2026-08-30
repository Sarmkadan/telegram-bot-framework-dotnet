#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Keyboard;

/// <summary>
/// Provides validation helpers for <see cref="IReplyKeyboardBuilder"/> to ensure
/// keyboard configurations are valid before building or using them.
/// </summary>
public static class ReplyKeyboardBuilderValidation
{
    /// <summary>
    /// Validates the current state of an <see cref="IReplyKeyboardBuilder"/> and returns
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
    public static IReadOnlyList<string> Validate(this IReplyKeyboardBuilder value)
    {
        ArgumentNullException.ThrowIfNull(value);

        // We need to cast to ReplyKeyboardBuilder to access Build method
        if (value is not ReplyKeyboardBuilder builder)
        {
            throw new ArgumentException(ReplyKeyboardBuilderValidationConstants.ValidatorOnlyWorksWithReplyKeyboardBuilderInstances, nameof(value));
        }

        var errors = new List<string>();

        // Validate that we can build the keyboard (checks for empty keyboard)
        try
        {
            _ = builder.Build();
        }
        catch (InvalidOperationException ex)
        {
            errors.Add(ex.Message);
            return errors.AsReadOnly(); // Early return if build fails
        }

        // Validate the built markup if successful
        var markup = builder.Build();

        // Validate each row in the built markup
        var rowIndex = 0;
        foreach (var row in markup.Keyboard)
        {
            if (row.Count() == 0)
            {
                errors.Add($"Row {rowIndex} is empty.");
                rowIndex++;
                continue;
            }

            // Validate each button in the row
            var buttonIndex = 0;
            foreach (var button in row)
            {
                // Validate button text
                if (string.IsNullOrWhiteSpace(button.Text))
                {
                    errors.Add(string.Format(ReplyKeyboardBuilderValidationConstants.ButtonEmptyTextFormat, rowIndex, buttonIndex));
                }
                else if (button.Text.Length > ReplyKeyboardBuilderValidationConstants.MaxButtonTextLength)
                {
                    errors.Add(string.Format(ReplyKeyboardBuilderValidationConstants.ButtonTextTooLongFormat, rowIndex, buttonIndex, ReplyKeyboardBuilderValidationConstants.MaxButtonTextLength, button.Text.Length));
                }

                buttonIndex++;
            }

            rowIndex++;
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="IReplyKeyboardBuilder"/> is in a valid state.
    /// </summary>
    /// <param name="value">The builder instance to validate.</param>
    /// <returns>
    /// <see langword="true"/> if the builder is valid; otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    public static bool IsValid(this IReplyKeyboardBuilder value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="IReplyKeyboardBuilder"/> is in a valid state,
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
    public static void EnsureValid(this IReplyKeyboardBuilder value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();

        if (errors.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"ReplyKeyboardBuilder validation failed with {errors.Count} error(s):{Environment.NewLine}" +
            string.Join(Environment.NewLine, errors));
    }
}
