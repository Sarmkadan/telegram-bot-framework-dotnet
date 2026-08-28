#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Diagnostics.CodeAnalysis;

namespace TelegramBotFramework.Models;

/// <summary>
/// Provides validation helpers for <see cref="UserSession"/> instances.
/// </summary>
public static class UserSessionValidation
{
    /// <summary>
    /// Validates the session and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The session to validate.</param>
    /// <returns>An empty list if valid, otherwise a list of error messages.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> ValidateSession(this UserSession value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate SessionId
        if (string.IsNullOrWhiteSpace(value.SessionId))
        {
            errors.Add(UserSessionValidationConstants.SessionIdCannotBeNullOrWhitespace);
        }
        else if (value.SessionId.Length > UserSessionValidationConstants.SessionIdMaxLength)
        {
            errors.Add(string.Format(UserSessionValidationConstants.SessionIdCannotExceedMaxLength, UserSessionValidationConstants.SessionIdMaxLength));
        }

        // Validate UserId
        if (value.UserId <= 0)
        {
            errors.Add(UserSessionValidationConstants.UserIdMustBePositive);
        }

        // Validate ChatId
        if (value.ChatId <= 0)
        {
            errors.Add(UserSessionValidationConstants.ChatIdMustBePositive);
        }

        // Validate CurrentContext
        if (string.IsNullOrWhiteSpace(value.CurrentContext))
        {
            errors.Add(UserSessionValidationConstants.CurrentContextCannotBeNullOrWhitespace);
        }
        else if (value.CurrentContext.Length > UserSessionValidationConstants.CurrentContextMaxLength)
        {
            errors.Add(string.Format(UserSessionValidationConstants.CurrentContextCannotExceedMaxLength, UserSessionValidationConstants.CurrentContextMaxLength));
        }

        // Validate CurrentMenuId
        if (value.CurrentMenuId?.Length > UserSessionValidationConstants.CurrentMenuIdMaxLength)
        {
            errors.Add(string.Format(UserSessionValidationConstants.CurrentMenuIdCannotExceedMaxLength, UserSessionValidationConstants.CurrentMenuIdMaxLength));
        }

        // Validate CreatedAt
        if (value.CreatedAt == default)
        {
            errors.Add(UserSessionValidationConstants.CreatedAtMustBeSet);
        }
        else if (value.CreatedAt > DateTime.UtcNow.AddMinutes(UserSessionValidationConstants.CreatedAtFutureMinutesThreshold))
        {
            errors.Add(UserSessionValidationConstants.CreatedAtCannotBeInFuture);
        }

        // Validate LastActivityAt
        if (value.LastActivityAt.HasValue)
        {
            var lastActivity = value.LastActivityAt.Value;
            if (lastActivity == default)
            {
                errors.Add(UserSessionValidationConstants.LastActivityAtMustBeValidIfSet);
            }
            else if (lastActivity > DateTime.UtcNow.AddMinutes(UserSessionValidationConstants.CreatedAtFutureMinutesThreshold))
            {
                errors.Add(UserSessionValidationConstants.LastActivityAtCannotBeInFuture);
            }
            else if (lastActivity < value.CreatedAt)
            {
                errors.Add(UserSessionValidationConstants.LastActivityAtCannotBeBeforeCreatedAt);
            }
        }

        // Validate ExpiresAt
        if (value.ExpiresAt.HasValue)
        {
            var expiresAt = value.ExpiresAt.Value;
            if (expiresAt == default)
            {
                errors.Add(UserSessionValidationConstants.ExpiresAtMustBeValidIfSet);
            }
            else if (expiresAt < value.CreatedAt)
            {
                errors.Add(UserSessionValidationConstants.ExpiresAtCannotBeBeforeCreatedAt);
            }
            else if (expiresAt > DateTime.UtcNow.AddYears(UserSessionValidationConstants.ExpiresAtFutureYearsThreshold))
            {
                errors.Add(UserSessionValidationConstants.ExpiresAtCannotBeMoreThanOneYearInFuture);
            }
        }

        // Validate ContextData
        if (value.ContextData is not null)
        {
            if (value.ContextData.Count > UserSessionValidationConstants.ContextDataMaxEntries)
            {
                errors.Add(string.Format(UserSessionValidationConstants.ContextDataCannotContainMoreThanMaxEntries, UserSessionValidationConstants.ContextDataMaxEntries));
            }
            else
            {
                foreach (var kvp in value.ContextData)
                {
                    if (string.IsNullOrWhiteSpace(kvp.Key))
                    {
                        errors.Add(UserSessionValidationConstants.ContextDataContainsEntryWithNullOrWhitespaceKey);
                        break;
                    }

                    if (kvp.Key.Length > UserSessionValidationConstants.ContextDataKeyMaxLength)
                    {
                        errors.Add(string.Format(UserSessionValidationConstants.ContextDataKeyCannotExceedMaxLength, UserSessionValidationConstants.ContextDataKeyMaxLength));
                        break;
                    }

                    if (string.IsNullOrWhiteSpace(kvp.Value))
                    {
                        errors.Add(string.Format(UserSessionValidationConstants.ContextDataKeyHasNullOrWhitespaceValue, kvp.Key));
                        break;
                    }

                    if (kvp.Value.Length > UserSessionValidationConstants.ContextDataValueMaxLength)
                    {
                        errors.Add(string.Format(UserSessionValidationConstants.ContextDataValueForKeyCannotExceedMaxLength, kvp.Key, UserSessionValidationConstants.ContextDataValueMaxLength));
                        break;
                    }
                }
            }
        }

        // Validate CommandHistory
        if (value.CommandHistory is not null)
        {
            if (value.CommandHistory.Count > UserSessionValidationConstants.CommandHistoryMaxEntries)
            {
                errors.Add(string.Format(UserSessionValidationConstants.CommandHistoryCannotContainMoreThanMaxEntries, UserSessionValidationConstants.CommandHistoryMaxEntries));
            }
            else
            {
                foreach (var command in value.CommandHistory)
                {
                    if (string.IsNullOrWhiteSpace(command))
                    {
                        errors.Add(UserSessionValidationConstants.CommandHistoryContainsNullOrWhitespaceEntry);
                        break;
                    }

                    if (command.Length > UserSessionValidationConstants.CommandHistoryEntryMaxLength)
                    {
                        errors.Add(string.Format(UserSessionValidationConstants.CommandHistoryEntryCannotExceedMaxLength, UserSessionValidationConstants.CommandHistoryEntryMaxLength));
                        break;
                    }
                }
            }
        }

        // Validate InteractionCount
        if (value.InteractionCount < 0)
        {
            errors.Add(UserSessionValidationConstants.InteractionCountCannotBeNegative);
        }

        // Validate UserInput
        if (value.UserInput?.Length > UserSessionValidationConstants.UserInputMaxLength)
        {
            errors.Add(string.Format(UserSessionValidationConstants.UserInputCannotExceedMaxLength, UserSessionValidationConstants.UserInputMaxLength));
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the session is valid.
    /// </summary>
    /// <param name="value">The session to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    public static bool IsValid(this UserSession value)
    {
        return value.ValidateSession().Count == 0;
    }

    /// <summary>
    /// Ensures the session is valid, throwing an <see cref="ArgumentException"/> if not.
    /// </summary>
    /// <param name="value">The session to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the session is invalid, containing all validation errors.</exception>
    public static void EnsureValid(this UserSession value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.ValidateSession();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"UserSession validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}",
                nameof(value));
        }
    }
}