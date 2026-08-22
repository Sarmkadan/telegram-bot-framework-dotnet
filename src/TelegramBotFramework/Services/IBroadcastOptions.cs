namespace TelegramBotFramework.Services
{
    /// <summary>
    /// Interface for broadcast options.
    /// </summary>
    public interface IBroadcastOptions
    {
        int MessagesPerSecond { get; set; }
        int MaxConcurrency { get; set; }
        int MaxRetryAttempts { get; set; }
        TimeSpan RetryDelay { get; set; }
        bool ContinueOnError { get; set; }
        Func<string, long, string>? MessageFormatter { get; set; }
        TimeSpan? BatchDelay { get; set; }
    }
}
