#nullable enable

namespace TelegramBotFramework.BackgroundWorkers;

/// <summary>
/// Constants for <see cref="ScheduledTaskManagerExtensions"/>.
/// </summary>
internal static class ScheduledTaskManagerExtensionsConstants
{
    public const string FutureRunTimeErrorMessage = "Run time must be in the future.";
    public const string AtLeastOneTimeOfDayRequired = "At least one time of day must be specified.";
    public const string CouldNotDetermineValidRunTime = "Could not determine valid run time from provided times.";
    public const int DefaultWaitTimeoutSeconds = 30;
    public const int TaskDelayMilliseconds = 100;
}