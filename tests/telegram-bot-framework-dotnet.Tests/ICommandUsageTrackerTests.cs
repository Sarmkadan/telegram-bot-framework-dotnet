#nullable enable

namespace TelegramBotFramework.Tests
{
    public interface ICommandUsageTrackerTests
    {
        void RecordCommandInvocation_RecordsInvocation();
        void RecordCommandInvocation_NormalizesCommandName();
        void GetTopCommands_ReturnsCommandsSortedByCountDescending();
        void GetTopCommands_WithZeroOrNegativeCount_ReturnsEmptyList();
        void GetLastUsedTimestamp_ReturnsCorrectTimestamp();
        void GetLastUsedTimestamp_ForNeverUsedCommand_ReturnsNull();
        void GetAllCommandStats_ReturnsAllStatistics();
        void RecordCommandInvocation_WithNullOrEmptyCommandName_DoesNotThrow();
        void GetLastUsedTimestamp_WithNullOrEmptyCommandName_ReturnsNull();
        void RecordCommandInvocation_TracksFirstAndLastUsedTimestamps();
    }
}