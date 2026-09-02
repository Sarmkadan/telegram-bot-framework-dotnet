#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;

namespace TelegramBotFramework.Keyboard;

/// <summary>
/// Provides validation helpers for <see cref="IReplyKeyboardBuilder"/> to ensure
/// keyboard configurations are valid before building or using them.
/// </summary>
/// <remarks>
/// This class contains extension methods that validate the state of a <see cref="ReplyKeyboardBuilder"/>
/// instance and report any validation errors.
/// </remarks>
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
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="value"/> is not an instance of <see cref="ReplyKeyboardBuilder"/>.
    /// </exception>
    public static IReadOnlyList<string> Validate(this IReplyKeyboardBuilder value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (value is not ReplyKeyboardBuilder builder)
        {
            throw new ArgumentException(ReplyKeyboardBuilderValidationConstants.ValidatorOnlyWorksWithReplyKeyboardBuilderInstances, nameof(value));
        }

        var errors = new List<string>();

        try
        {
            var markup = builder.Build();

            var rowIndex = 0;
            foreach (var row in markup.Keyboard)
            {
                if (!row.Any())
                {
                    errors.Add(string.Format(CultureInfo.InvariantCulture, "Row {0} is empty.", rowIndex));
                    rowIndex++;
                    continue;
                }

                var buttonIndex = 0;
                foreach (var button in row)
                {
                    if (string.IsNullOrWhiteSpace(button.Text))
                    {
                        errors.Add(string.Format(CultureInfo.InvariantCulture, ReplyKeyboardBuilderValidationConstants.ButtonEmptyTextFormat, rowIndex, buttonIndex));
                    }
                    else if (button.Text.Length > ReplyKeyboardBuilderValidationConstants.MaxButtonTextLength)
                    {
                        errors.Add(string.Format(
                            CultureInfo.InvariantCulture,
                            ReplyKeyboardBuilderValidationConstants.ButtonTextTooLongFormat,
                            rowIndex,
                            buttonIndex,
                            ReplyKeyboardBuilderValidationConstants.MaxButtonTextLength,
                            button.Text.Length));
                    }

                    buttonIndex++;
                }

                rowIndex++;
            }
        }
        catch (InvalidOperationException ex)
        {
            errors.Add(ex.Message);
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
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="value"/> is not an instance of <see cref="ReplyKeyboardBuilder"/>.
    /// </exception>
    public static bool IsValid(this IReplyKeyboardBuilder value) => value.Validate().Count == 0;

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
    /// Thrown when <paramref name="value"/> is not an instance of <see cref="ReplyKeyboardBuilder"/> or when the builder contains validation errors.
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
            string.Format(
                CultureInfo.InvariantCulture,
                ReplyKeyboardBuilderValidationConstants.ValidationFailedHeader,
                errors.Count) + Environment.NewLine + string.Join(Environment.NewLine, errors),
            nameof(value));
    }
}
