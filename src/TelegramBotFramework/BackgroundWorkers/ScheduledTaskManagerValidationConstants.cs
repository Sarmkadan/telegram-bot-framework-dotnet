#nullable enable

namespace TelegramBotFramework.BackgroundWorkers;

/// <summary>
/// Contains constant values used in <see cref="ScheduledTaskManagerValidation"/>.
/// </summary>
internal static class ScheduledTaskManagerValidationConstants
{
    public const string IdIsNullOrWhitespace = "Id is null or whitespace.";
    public const string NameIsNullOrWhitespace = "Name is null or whitespace.";
    public const string TaskFuncIsNull = "TaskFunc is null.";
    public const string IntervalMustBeGreaterThanZero = "Interval must be greater than zero.";
    public const string CreatedAtIsDefault = "CreatedAt is default (DateTime.MinValue).";
    public const string CreatedAtIsInTheFuture = "CreatedAt is in the future.";
    public const string LastExecutedAtIsDefault = "LastExecutedAt is default (DateTime.MinValue).";
    public const string LastExecutedAtIsInTheFuture = "LastExecutedAt is in the future.";
    public const string LastExecutedAtCannotBeBeforeCreatedAt = "LastExecutedAt cannot be before CreatedAt.";
    public const string LastSuccessAtIsDefault = "LastSuccessAt is default (DateTime.MinValue).";
    public const string LastSuccessAtIsInTheFuture = "LastSuccessAt is in the future.";
    public const string LastSuccessAtCannotBeBeforeCreatedAt = "LastSuccessAt cannot be before CreatedAt.";
    public const string LastExecutedAtCannotBeAfterLastSuccessAt = "LastExecutedAt cannot be after LastSuccessAt.";
    public const string LastErrorAtIsDefault = "LastErrorAt is default (DateTime.MinValue).";
    public const string LastErrorAtIsInTheFuture = "LastErrorAt is in the future.";
    public const string LastErrorAtCannotBeBeforeCreatedAt = "LastErrorAt cannot be before CreatedAt.";
    public const string LastExecutedAtCannotBeBeforeLastErrorAt = "LastExecutedAt cannot be before LastErrorAt.";
    public const string LastErrorMustBeSetWhenLastErrorAtIsSet = "LastError must be set when LastErrorAt is set.";
    public const string ExecutionCountCannotBeNegative = "ExecutionCount cannot be negative.";
    public const string LastErrorIsEmptyButNotNull = "LastError is empty but not null.";
    public const int FutureCheckMinutes = 5;
}