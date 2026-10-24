#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace TelegramBotFramework.Events;

/// <summary>
/// Determines how an event bus invokes the handlers subscribed to a single event.
/// </summary>
public enum EventDispatchMode
{
    /// <summary>
    /// Handlers run one after another in subscription order.
    /// </summary>
    Sequential,

    /// <summary>
    /// Handlers run concurrently, bounded by <see cref="EventDispatchOptions.MaxDegreeOfParallelism"/>.
    /// </summary>
    Parallel
}
