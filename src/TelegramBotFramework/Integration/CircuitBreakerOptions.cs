namespace TelegramBotFramework.Integration;

using System;

/// <summary>
/// Configuration options for <see cref="TelegramCircuitBreaker"/>.
/// </summary>
public sealed class CircuitBreakerOptions
{
    /// <summary>
    /// Gets or sets the number of consecutive failed Telegram API calls that opens the circuit. Defaults to 5.
    /// </summary>
    public int FailureThreshold { get; set; } = TelegramCircuitBreakerConstants.DefaultFailureThreshold;

    /// <summary>
    /// Gets or sets how long the circuit stays open before a single probe call is admitted. Defaults to 30 seconds.
    /// </summary>
    public TimeSpan OpenDuration { get; set; } = TelegramCircuitBreakerConstants.DefaultOpenDuration;

    /// <summary>
    /// Validates the configured values.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <see cref="FailureThreshold"/> is less than 1 or <see cref="OpenDuration"/> is not positive.</exception>
    public void Validate()
    {
        if (FailureThreshold < 1)
            throw new ArgumentOutOfRangeException(nameof(FailureThreshold), TelegramCircuitBreakerConstants.FailureThresholdErrorMessage);

        if (OpenDuration <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(OpenDuration), TelegramCircuitBreakerConstants.OpenDurationErrorMessage);
    }
}
