using System;

namespace TelegramBotFramework.ConversationFlow;

internal static class FileConversationStateStoreValidationConstants
{
    // Error messages
    public const string DirectoryPathCannotBeNullOrWhitespace = "Directory path cannot be null or whitespace.";
    public const string ConfiguredDirectoryDoesNotExist = "Configured directory does not exist and cannot be created automatically.";
    public const string DirectoryValidationFailed = "Directory validation failed: {0}";

    public const string StateIdCannotBeNullOrWhitespace = "StateId cannot be null or whitespace.";
    public const string FlowIdCannotBeNullOrWhitespace = "FlowId cannot be null or whitespace.";
    public const string UserIdMustBeNonDefault = "UserId must be a non-default value (cannot be 0).";
    public const string ChatIdMustBeNonDefault = "ChatId must be a non-default value (cannot be 0).";
    public const string CurrentStepIdCannotBeNullOrWhitespace = "CurrentStepId cannot be null or whitespace.";

    public const string StartedAtCannotBeDefault = "StartedAt cannot be the default DateTime value.";
    public const string StartedAtMustBeUtc = "StartedAt must be in UTC timezone.";
    public const string LastActivityAtCannotBeDefault = "LastActivityAt cannot be the default DateTime value.";
    public const string LastActivityAtMustBeUtc = "LastActivityAt must be in UTC timezone.";
    public const string CompletedAtCannotBeDefaultWhenSet = "CompletedAt cannot be the default DateTime value when set.";
    public const string CompletedAtMustBeUtcWhenSet = "CompletedAt must be in UTC timezone when set.";
    public const string CompletedAtCannotBeEarlierThanStartedAt = "CompletedAt cannot be earlier than StartedAt.";

    public const string StatusIsInvalid = "Status '{0}' is not a valid FlowStateStatus value.";

    public const string VariablesDictionaryCannotBeNull = "Variables dictionary cannot be null.";
    public const string HistoryListCannotBeNull = "History list cannot be null.";

    public const string HistoryEntryStepIdCannotBeNullOrWhitespace = "History entry StepId cannot be null or whitespace.";
    public const string HistoryEntryEnteredAtCannotBeDefault = "History entry EnteredAt cannot be the default DateTime value.";
    public const string HistoryEntryEnteredAtMustBeUtc = "History entry EnteredAt must be in UTC timezone.";
    public const string HistoryEntryCompletedAtMustBeUtcWhenSet = "History entry CompletedAt must be in UTC timezone when set.";
    public const string HistoryEntryCompletedAtCannotBeEarlierThanEnteredAt = "History entry CompletedAt cannot be earlier than EnteredAt.";

    public const string HistoryContainsNullEntry = "History contains a null entry.";
}