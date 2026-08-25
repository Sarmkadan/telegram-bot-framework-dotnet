#nullable enable

namespace TelegramBotFramework.Integration;

/// <summary>
/// Configuration options for <see cref="PollingStrategy"/>.
/// Defaults mirror the previously hardcoded polling behavior.
/// </summary>
public sealed class PollingOptions
{
    /// <summary>
    /// Gets or sets the delay between polls when no updates are available. Defaults to 1 second.
    /// </summary>
    public TimeSpan PollInterval { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Gets or sets the maximum number of updates processed per polling cycle. Defaults to 100.
    /// </summary>
    public int MaxUpdatesPerBatch { get; set; } = 100;

    /// <summary>
    /// Gets or sets the maximum number of concurrent in-flight updates. Defaults to 1000.
    /// </summary>
    public int MaxInFlightUpdates { get; set; } = 1000;

    /// <summary>
    /// Gets or sets the graceful shutdown timeout used when draining in-flight handlers. Defaults to 30 seconds.
    /// </summary>
    public TimeSpan ShutdownTimeout { get; set; } = TimeSpan.FromSeconds(30);
}
