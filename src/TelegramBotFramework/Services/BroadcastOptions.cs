#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace TelegramBotFramework.Services;

/// <summary>
/// Configuration options for broadcast operations.
/// </summary>
public sealed class BroadcastOptions
{
    /// <summary>
    /// Maximum messages per second (default: 25).
    /// Set to 0 for unlimited rate (not recommended for production).
    /// </summary>
    public int MessagesPerSecond { get; set; } = 25;

    /// <summary>
    /// Maximum concurrent operations (default: 5).
    /// Controls how many messages can be in flight simultaneously.
    /// </summary>
    public int MaxConcurrency { get; set; } = 5;

    /// <summary>
    /// Maximum retry attempts for failed messages (default: 3).
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Delay between retry attempts (default: 1 second).
    /// </summary>
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Whether to continue on error (default: true).
    /// If false, the entire broadcast will fail on first error.
    /// </summary>
    public bool ContinueOnError { get; set; } = true;

    /// <summary>
    /// Optional custom message formatter.
    /// </summary>
    public Func<string, long, string>? MessageFormatter { get; set; }

    /// <summary>
    /// Optional delay between batches when rate limiting is active.
    /// </summary>
    public TimeSpan? BatchDelay { get; set; }
}
