#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace TelegramBotFramework.Models;

/// <summary>
/// Constants for UserSessionValidation.
/// </summary>
internal static class UserSessionValidationConstants
{
    // SessionId validation
    public const int SessionIdMaxLength = 100;
    public const string SessionIdCannotBeNullOrWhitespace = "SessionId cannot be null or whitespace.";
    public const string SessionIdCannotExceedMaxLength = "SessionId cannot exceed {0} characters.";

    // UserId validation
    public const string UserIdMustBePositive = "UserId must be a positive integer greater than zero.";

    // ChatId validation
    public const string ChatIdMustBePositive = "ChatId must be a positive integer greater than zero.";

    // CurrentContext validation
    public const int CurrentContextMaxLength = 50;
    public const string CurrentContextCannotBeNullOrWhitespace = "CurrentContext cannot be null or whitespace.";
    public const string CurrentContextCannotExceedMaxLength = "CurrentContext cannot exceed {0} characters.";

    // CurrentMenuId validation
    public const int CurrentMenuIdMaxLength = 50;
    public const string CurrentMenuIdCannotExceedMaxLength = "CurrentMenuId cannot exceed {0} characters.";

    // CreatedAt validation
    public const string CreatedAtMustBeSet = "CreatedAt must be set to a valid DateTime.";
    public const string CreatedAtCannotBeInFuture = "CreatedAt cannot be in the future.";
    public const int CreatedAtFutureMinutesThreshold = 5;

    // LastActivityAt validation
    public const string LastActivityAtMustBeValidIfSet = "LastActivityAt must be a valid DateTime if set.";
    public const string LastActivityAtCannotBeInFuture = "LastActivityAt cannot be in the future.";
    public const string LastActivityAtCannotBeBeforeCreatedAt = "LastActivityAt cannot be before CreatedAt.";

    // ExpiresAt validation
    public const string ExpiresAtMustBeValidIfSet = "ExpiresAt must be a valid DateTime if set.";
    public const string ExpiresAtCannotBeBeforeCreatedAt = "ExpiresAt cannot be before CreatedAt.";
    public const string ExpiresAtCannotBeMoreThanOneYearInFuture = "ExpiresAt cannot be more than 1 year in the future.";
    public const int ExpiresAtFutureYearsThreshold = 1;

    // ContextData validation
    public const int ContextDataMaxEntries = 1000;
    public const int ContextDataKeyMaxLength = 100;
    public const int ContextDataValueMaxLength = 1000;
    public const string ContextDataCannotContainMoreThanMaxEntries = "ContextData dictionary cannot contain more than {0} entries.";
    public const string ContextDataContainsEntryWithNullOrWhitespaceKey = "ContextData contains an entry with null or whitespace key.";
    public const string ContextDataKeyCannotExceedMaxLength = "ContextData key cannot exceed {0} characters.";
    public const string ContextDataKeyHasNullOrWhitespaceValue = "ContextData key '{0}' has null or whitespace value.";
    public const string ContextDataValueForKeyCannotExceedMaxLength = "ContextData value for key '{0}' cannot exceed {1} characters.";

    // CommandHistory validation
    public const int CommandHistoryMaxEntries = 50;
    public const int CommandHistoryEntryMaxLength = 200;
    public const string CommandHistoryCannotContainMoreThanMaxEntries = "CommandHistory cannot contain more than {0} entries.";
    public const string CommandHistoryContainsNullOrWhitespaceEntry = "CommandHistory contains null or whitespace entry.";
    public const string CommandHistoryEntryCannotExceedMaxLength = "CommandHistory entry cannot exceed {0} characters.";

    // InteractionCount validation
    public const string InteractionCountCannotBeNegative = "InteractionCount cannot be negative.";

    // UserInput validation
    public const int UserInputMaxLength = 1000;
    public const string UserInputCannotExceedMaxLength = "UserInput cannot exceed {0} characters.";
}