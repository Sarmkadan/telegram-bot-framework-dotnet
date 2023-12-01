#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.AspNetCore.Mvc;
using TelegramBotFramework.Integration;

namespace TelegramBotFramework.Controllers;

/// <summary>
/// ASP.NET Core controller that exposes the Telegram webhook endpoint.
/// Telegram calls <c>POST /api/webhook/telegram</c> (or the path configured in
/// <see cref="WebhookOptions.ListenPath"/> ) with each incoming update.
/// </summary>
[ApiController]
[Route("api/webhook")]
public sealed class WebhookController : ControllerBase
{
    private readonly WebhookService _webhookService;
    private readonly ILogger<WebhookController> _logger;

    private const string SecretTokenHeader = "X-Telegram-Bot-Api-Secret-Token";

    /// <summary>
    /// Initialises a new instance of <see cref="WebhookController"/>.
    /// </summary>
    public WebhookController(
        WebhookService webhookService,
        ILogger<WebhookController> logger)
    {
        _webhookService = webhookService ?? throw new ArgumentNullException(nameof(webhookService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Receives and processes a Telegram update delivered via webhook.
    /// Returns <c>200 OK</c> immediately to prevent Telegram from retrying, regardless
    /// of downstream processing errors (per Telegram Bot API guidance).
    /// </summary>
    /// <remarks>
    /// When <see cref="WebhookOptions.SecretToken"/> is configured, the controller
    /// validates the <c>X-Telegram-Bot-Api-Secret-Token</c> header and returns
    /// <c>401 Unauthorized</c> on mismatch.
    /// </remarks>
    [HttpPost("telegram")]
    [Consumes("application/json")]
    public async Task<IActionResult> ReceiveUpdate(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Webhook endpoint called - Path: {Path}, Method: {Method}", Request.Path, Request.Method);

        string body;
        using (var reader = new System.IO.StreamReader(Request.Body))
        {
            body = await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
        }

        if (string.IsNullOrWhiteSpace(body))
        {
            _logger.LogWarning("Received empty webhook request body");
            return BadRequest("Request body is required.");
        }

        _logger.LogDebug("Webhook request body received - Length: {BodyLength} bytes", body.Length);

        Request.Headers.TryGetValue(SecretTokenHeader, out var secretTokenValue);
        var secretToken = secretTokenValue.ToString();

        _logger.LogDebug("Validating webhook request with secret token");

        var update = await _webhookService.ParseAndValidateAsync(body, secretToken)
            .ConfigureAwait(false);

        if (update is null)
        {
            // ParseAndValidateAsync already logged the reason (invalid signature or parse failure)
            _logger.LogWarning("Webhook request validation failed - Invalid signature or parse error");
            return Unauthorized();
        }

        _logger.LogInformation(
            "Webhook request validated successfully - UpdateId: {UpdateId}, Type: {UpdateType}",
            update.UpdateId,
            update.MessageType);

        await _webhookService.DispatchUpdateAsync(update, cancellationToken).ConfigureAwait(false);

        _logger.LogDebug("Webhook update dispatched successfully - UpdateId: {UpdateId}", update.UpdateId);

        // Always return 200 OK so Telegram does not re-deliver
        return Ok();
    }

    /// <summary>
    /// Returns current webhook status (registered URL, dispatched update count, etc.).
    /// Useful for health checks and diagnostics.
    /// </summary>
    [HttpGet("info")]
    public IActionResult GetInfo()
    {
        var info = _webhookService.GetInfo();
        return Ok(info);
    }
}
