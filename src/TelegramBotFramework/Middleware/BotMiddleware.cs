#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using TelegramBotFramework.Models;
using TelegramBotFramework.Services;
using TelegramBotFramework.Exceptions;

namespace TelegramBotFramework.Middleware;

/// <summary>
/// Base middleware interface for request processing pipeline.
/// </summary>
/// <summary>
/// 
/// </summary>
public interface IBotMiddleware
{
    int Priority { get; }

    Task<Models.ExecutionContext> ProcessAsync(
        Models.ExecutionContext context,
        Func<Models.ExecutionContext, Task<Models.ExecutionContext>> next,
        CancellationToken cancellationToken = default);
}