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
            errors.Add("SessionId cannot be null or whitespace.");
        }
        else if (value.SessionId.Length > 100)
        {
            errors.Add("SessionId cannot exceed 100 characters.");
        }

        // Validate UserId
        if (value.UserId <= 0)
        {
            errors.Add("UserId must be a positive integer greater than zero.");
        }

        // Validate ChatId
        if (value.ChatId <= 0)
        {
            errors.Add("ChatId must be a positive integer greater than zero.");
        }

        // Validate CurrentContext
        if (string.IsNullOrWhiteSpace(value.CurrentContext))
        {
            errors.Add("CurrentContext cannot be null or whitespace.");
        }
        else if (value.CurrentContext.Length > 50)
        {
            errors.Add("CurrentContext cannot exceed 50 characters.");
        }

        // Validate CurrentMenuId
        if (value.CurrentMenuId?.Length > 50)
        {
            errors.Add("CurrentMenuId cannot exceed 50 characters.");
        }

        // Validate CreatedAt
        if (value.CreatedAt == default)
        {
            errors.Add("CreatedAt must be set to a valid DateTime.");
        }
        else if (value.CreatedAt > DateTime.UtcNow.AddMinutes(5))
        {
            errors.Add("CreatedAt cannot be in the future.");
        }

        // Validate LastActivityAt
        if (value.LastActivityAt.HasValue)
        {
            var lastActivity = value.LastActivityAt.Value;
            if (lastActivity == default)
            {
                errors.Add("LastActivityAt must be a valid DateTime if set.");
            }
            else if (lastActivity > DateTime.UtcNow.AddMinutes(5))
            {
                errors.Add("LastActivityAt cannot be in the future.");
            }
            else if (lastActivity < value.CreatedAt)
            {
                errors.Add("LastActivityAt cannot be before CreatedAt.");
            }
        }

        // Validate ExpiresAt
        if (value.ExpiresAt.HasValue)
        {
            var expiresAt = value.ExpiresAt.Value;
            if (expiresAt == default)
            {
                errors.Add("ExpiresAt must be a valid DateTime if set.");
            }
            else if (expiresAt < value.CreatedAt)
            {
                errors.Add("ExpiresAt cannot be before CreatedAt.");
            }
            else if (expiresAt > DateTime.UtcNow.AddYears(1))
            {
                errors.Add("ExpiresAt cannot be more than 1 year in the future.");
            }
        }

        // Validate ContextData
        if (value.ContextData is not null)
        {
            if (value.ContextData.Count > 1000)
            {
                errors.Add("ContextData dictionary cannot contain more than 1000 entries.");
            }
            else
            {
                foreach (var kvp in value.ContextData)
                {
                    if (string.IsNullOrWhiteSpace(kvp.Key))
                    {
                        errors.Add("ContextData contains an entry with null or whitespace key.");
                        break;
                    }

                    if (kvp.Key.Length > 100)
                    {
                        errors.Add("ContextData key cannot exceed 100 characters.");
                        break;
                    }

                    if (string.IsNullOrWhiteSpace(kvp.Value))
                    {
                        errors.Add($"ContextData key '{kvp.Key}' has null or whitespace value.");
                        break;
                    }

                    if (kvp.Value.Length > 1000)
                    {
                        errors.Add($"ContextData value for key '{kvp.Key}' cannot exceed 1000 characters.");
                        break;
                    }
                }
            }
        }

        // Validate CommandHistory
        if (value.CommandHistory is not null)
        {
            if (value.CommandHistory.Count > 50)
            {
                errors.Add("CommandHistory cannot contain more than 50 entries.");
            }
            else
            {
                foreach (var command in value.CommandHistory)
                {
                    if (string.IsNullOrWhiteSpace(command))
                    {
                        errors.Add("CommandHistory contains null or whitespace entry.");
                        break;
                    }

                    if (command.Length > 200)
                    {
                        errors.Add("CommandHistory entry cannot exceed 200 characters.");
                        break;
                    }
                }
            }
        }

        // Validate InteractionCount
        if (value.InteractionCount < 0)
        {
            errors.Add("InteractionCount cannot be negative.");
        }

        // Validate UserInput
        if (value.UserInput?.Length > 1000)
        {
            errors.Add("UserInput cannot exceed 1000 characters.");
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