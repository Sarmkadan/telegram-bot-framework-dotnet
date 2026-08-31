namespace TelegramBotFramework.Tests;

internal static class UserSessionValidationTestsConstants
{
    public const string ValidSessionId = "valid-session-id";
    public const long ValidUserId = 12345;
    public const long ValidChatId = 67890;
    public const string ValidCurrentContext = "menu";
    public const int ValidInteractionCount = 5;
    public const int CreatedAtMinutesAgo = -10;
    public const int LastActivityAtMinutesAgo = -5;
    public const int ExpiresAtHoursFromNow = 1;
    public const int FutureMinutes = 10;
    public const int InvalidZeroId = 0;
    public const int InvalidNegativeValue = -1;
    public const char RepeatedCharacter = 'a';
    public const string WhitespaceValue = "   ";
    public static readonly DateTime DateTimeMinValue = DateTime.MinValue;

    // SessionId validation
    public const string SessionIdCannotBeNullOrWhitespace = "SessionId cannot be null or whitespace.";
    public const int SessionIdMaxLength = 100;
    public const string SessionIdExceedsMaxLength = "SessionId cannot exceed 100 characters.";

    // UserId validation
    public const string UserIdMustBePositive = "UserId must be a positive integer greater than zero.";

    // ChatId validation
    public const string ChatIdMustBePositive = "ChatId must be a positive integer greater than zero.";

    // CurrentContext validation
    public const string CurrentContextCannotBeNullOrWhitespace = "CurrentContext cannot be null or whitespace.";
    public const int CurrentContextMaxLength = 50;
    public const string CurrentContextExceedsMaxLength = "CurrentContext cannot exceed 50 characters.";

    // CurrentMenuId validation
    public const int CurrentMenuIdMaxLength = 50;
    public const string CurrentMenuIdExceedsMaxLength = "CurrentMenuId cannot exceed 50 characters.";

    // CreatedAt validation
    public const string CreatedAtMustBeSet = "CreatedAt must be set to a valid DateTime.";
    public const string CreatedAtCannotBeInFuture = "CreatedAt cannot be in the future.";

    // LastActivityAt validation
    public const string LastActivityAtMustBeValidIfSet = "LastActivityAt must be a valid DateTime if set.";
    public const string LastActivityAtCannotBeInFuture = "LastActivityAt cannot be in the future.";
    public const string LastActivityAtCannotBeBeforeCreatedAt = "LastActivityAt cannot be before CreatedAt.";

    // ExpiresAt validation
    public const string ExpiresAtMustBeValidIfSet = "ExpiresAt must be a valid DateTime if set.";
    public const string ExpiresAtCannotBeBeforeCreatedAt = "ExpiresAt cannot be before CreatedAt.";
    public const int ExpiresAtMaxYearsInFuture = 1;
    public const string ExpiresAtCannotBeMoreThanOneYearInFuture = "ExpiresAt cannot be more than 1 year in the future.";

    // ContextData validation
    public const int ContextDataMaxEntries = 1000;
    public const string ContextDataCannotContainMoreThanMaxEntries = "ContextData dictionary cannot contain more than 1000 entries.";
    public const string ContextDataContainsEntryWithNullOrWhitespaceKey = "ContextData contains an entry with null or whitespace key.";
    public const int ContextDataKeyMaxLength = 100;
    public const string ContextDataKeyCannotExceedMaxLength = "ContextData key cannot exceed 100 characters.";
    public const string ContextDataKeyHasNullOrWhitespaceValueFormat = "ContextData key '{0}' has null or whitespace value.";
    public const int ContextDataValueMaxLength = 1000;
    public const string ContextDataValueForKeyCannotExceedMaxLengthFormat = "ContextData value for key '{0}' cannot exceed 1000 characters.";

    // CommandHistory validation
    public const int CommandHistoryMaxEntries = 50;
    public const string CommandHistoryCannotContainMoreThanMaxEntries = "CommandHistory cannot contain more than 50 entries.";
    public const string CommandHistoryContainsNullOrWhitespaceEntry = "CommandHistory contains null or whitespace entry.";
    public const int CommandHistoryEntryMaxLength = 200;
    public const string CommandHistoryEntryCannotExceedMaxLength = "CommandHistory entry cannot exceed 200 characters.";

    // InteractionCount validation
    public const string InteractionCountCannotBeNegative = "InteractionCount cannot be negative.";

    // UserInput validation
    public const int UserInputMaxLength = 1000;
    public const string UserInputCannotExceedMaxLength = "UserInput cannot exceed 1000 characters.";

    public const int LengthBeyondLimit = 1;
    public const int YearsBeyondExpirationLimit = 1;
    public const int ExpiresAtCreatedOffsetMinutes = -30;
    public const int ExpiresAtActivityOffsetMinutes = -20;
    public const int ExpiresAtInvalidOffsetMinutes = -35;
    public const string ContextDataKey = "key";
    public const string ContextDataValue = "value";
    public const string ContextDataKeyFormat = "key{0}";
    public const string ContextDataValueFormat = "value{0}";
    public const string CommandHistoryEntryFormat = "command{0}";
    public const string EnsureValidFailureMessagePattern = "*UserSession validation failed*";
}
