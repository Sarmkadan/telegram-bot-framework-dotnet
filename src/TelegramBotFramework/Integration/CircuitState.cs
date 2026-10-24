namespace TelegramBotFramework.Integration;

/// <summary>
/// Represents the state of a <see cref="TelegramCircuitBreaker"/>.
/// </summary>
public enum CircuitState
{
    /// <summary>Calls flow normally and consecutive failures are counted.</summary>
    Closed,

    /// <summary>Calls are rejected without reaching the Telegram API until the open duration elapses.</summary>
    Open,

    /// <summary>A single probe call is admitted to test whether the Telegram API has recovered.</summary>
    HalfOpen
}
