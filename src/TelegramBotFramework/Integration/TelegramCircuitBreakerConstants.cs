namespace TelegramBotFramework.Integration;

using System;

/// <summary>
/// Centralizes defaults and transition reasons used by <see cref="TelegramCircuitBreaker"/>.
/// </summary>
internal static class TelegramCircuitBreakerConstants
{
    /// <summary>Default number of consecutive failures that opens the circuit.</summary>
    public const int DefaultFailureThreshold = 5;

    /// <summary>Default time the circuit stays open before a probe call is admitted.</summary>
    public static readonly TimeSpan DefaultOpenDuration = TimeSpan.FromSeconds(30);

    /// <summary>How long callers wait before retrying while a half-open probe is still in flight.</summary>
    public static readonly TimeSpan ProbeInFlightWaitDuration = TimeSpan.FromSeconds(1);

    /// <summary>Format for the reason attached to a transition caused by consecutive failures.</summary>
    public const string ConsecutiveFailuresReasonFormat = "{0} consecutive Telegram API failures";

    /// <summary>Reason attached to the transition from open to half-open.</summary>
    public const string OpenDurationElapsedReason = "Open duration elapsed; admitting a probe call";

    /// <summary>Reason attached to the transition from half-open to open when the probe fails.</summary>
    public const string ProbeFailedReason = "Probe call to Telegram API failed";

    /// <summary>Reason attached to the transition from half-open to closed when the probe succeeds.</summary>
    public const string ProbeSucceededReason = "Probe call to Telegram API succeeded";

    /// <summary>Message used when <see cref="CircuitBreakerOptions.FailureThreshold"/> is invalid.</summary>
    public const string FailureThresholdErrorMessage = "Failure threshold must be at least 1";

    /// <summary>Message used when <see cref="CircuitBreakerOptions.OpenDuration"/> is invalid.</summary>
    public const string OpenDurationErrorMessage = "Open duration must be positive";
}
