#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace TelegramBotFramework.Services;

/// <summary>
/// Configuration options for broadcast operations.
/// </summary>
public sealed class BroadcastOptions : IBroadcastOptions, IEquatable<BroadcastOptions>
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

    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>true if the current object is equal to the <paramref name="other">parameter</paramref>; otherwise, false.</returns>
    public bool Equals(BroadcastOptions? other)
    {
        if (ReferenceEquals(other, null))
            return false;

        if (ReferenceEquals(this, other))
            return true;

        return MessagesPerSecond == other.MessagesPerSecond
            && MaxConcurrency == other.MaxConcurrency
            && MaxRetryAttempts == other.MaxRetryAttempts
            && RetryDelay.Equals(other.RetryDelay)
            && ContinueOnError == other.ContinueOnError
            && MessageFormatter == other.MessageFormatter
            && BatchDelay == other.BatchDelay;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(obj, null))
            return false;

        if (ReferenceEquals(this, obj))
            return true;

        if (obj.GetType() != GetType())
            return false;

        return Equals((BroadcastOptions)obj);
    }

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current object.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(MessagesPerSecond, MaxConcurrency, MaxRetryAttempts, RetryDelay, ContinueOnError, MessageFormatter, BatchDelay);
    }

    /// <summary>
    /// Equality operator.
    /// </summary>
    /// <param name="left">Left operand.</param>
    /// <param name="right">Right operand.</param>
    /// <returns>true if operands are equal; otherwise, false.</returns>
    public static bool operator ==(BroadcastOptions? left, BroadcastOptions? right)
    {
        if (ReferenceEquals(left, right))
            return true;

        if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            return false;

        return left.Equals(right);
    }

    /// <summary>
    /// Inequality operator.
    /// </summary>
    /// <param name="left">Left operand.</param>
    /// <param name="right">Right operand.</param>
    /// <returns>true if operands are not equal; otherwise, false.</returns>
    public static bool operator !=(BroadcastOptions? left, BroadcastOptions? right)
    {
        return !(left == right);
    }

    /// <summary>
    /// Returns a string representation of the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString()
    {
        return $"BroadcastOptions {{ MessagesPerSecond = {MessagesPerSecond}, MaxConcurrency = {MaxConcurrency}, MaxRetryAttempts = {MaxRetryAttempts}, RetryDelay = {RetryDelay}, ContinueOnError = {ContinueOnError}, MessageFormatter = {MessageFormatter} }}";
    }
}
