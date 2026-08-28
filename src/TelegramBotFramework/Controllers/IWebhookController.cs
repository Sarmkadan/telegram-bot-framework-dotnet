#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.AspNetCore.Mvc;
using System.Threading;

namespace TelegramBotFramework.Controllers;

/// <summary>
/// Interface for ASP.NET Core controller that exposes the Telegram webhook endpoint.
/// </summary>
public interface IWebhookController
{
    /// <summary>
    /// Receives and processes a Telegram update delivered via webhook.
    /// Returns <c>200 OK</c> immediately to prevent Telegram from retrying, regardless
    /// of downstream processing errors (per Telegram Bot API guidance).
    /// </summary>
    Task<IActionResult> ReceiveUpdate(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns current webhook status (registered URL, dispatched update count, etc.).
    /// Useful for health checks and diagnostics.
    /// </summary>
    IActionResult GetInfo();
}