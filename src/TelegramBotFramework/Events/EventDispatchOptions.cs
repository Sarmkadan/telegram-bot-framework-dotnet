#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace TelegramBotFramework.Events;

/// <summary>
/// Configures how an event bus dispatches an event to its handlers and how handler failures are reported.
/// </summary>
public sealed class EventDispatchOptions
{
    /// <summary>
    /// The default upper bound on concurrently running handlers in <see cref="EventDispatchMode.Parallel"/> mode.
    /// </summary>
    public const int DefaultMaxDegreeOfParallelism = 4;

    /// <summary>
    /// Gets the dispatch mode. Defaults to <see cref="EventDispatchMode.Sequential"/>.
    /// </summary>
    public EventDispatchMode Mode { get; init; } = EventDispatchMode.Sequential;

    /// <summary>
    /// Gets the maximum number of handlers that run at the same time in <see cref="EventDispatchMode.Parallel"/> mode.
    /// Must be at least 1. Ignored in sequential mode.
    /// </summary>
    public int MaxDegreeOfParallelism { get; init; } = DefaultMaxDegreeOfParallelism;

    /// <summary>
    /// Gets a value indicating whether <c>PublishAsync</c> rethrows handler failures once every handler has run.
    /// A single failure is rethrown as the original exception; multiple failures are rethrown as an
    /// <see cref="AggregateException"/>. When <c>false</c>, failures are only logged and published as
    /// <see cref="HandlerFailedEvent"/>. Defaults to <c>true</c>.
    /// </summary>
    public bool ThrowOnHandlerFailure { get; init; } = true;

    /// <summary>
    /// Validates the option values.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><see cref="MaxDegreeOfParallelism"/> is less than 1.</exception>
    internal void Validate()
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(MaxDegreeOfParallelism, 1);
    }
}
