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
[Route(WebhookControllerConstants.Route)]
public sealed class WebhookController : ControllerBase, IWebhookController
{
    private readonly WebhookService _webhookService;
    private readonly ILogger<WebhookController> _logger;

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
    [HttpPost(WebhookControllerConstants.TelegramRoute)]
    [Consumes(WebhookControllerConstants.JsonMediaType)]
    public async Task<IActionResult> ReceiveUpdate(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(WebhookControllerConstants.EndpointCalledLogMessage, Request.Path, Request.Method);

        // Validate request body size to prevent DoS attacks
        if (Request.ContentLength.HasValue && Request.ContentLength.Value > _webhookService.Options.MaxRequestBodySize)
        {
            _logger.LogWarning(WebhookControllerConstants.RequestBodyTooLargeLogMessage,
                Request.ContentLength.Value, _webhookService.Options.MaxRequestBodySize);
            return StatusCode(WebhookControllerConstants.PayloadTooLargeStatusCode,
                WebhookControllerConstants.RequestBodyTooLargeMessage);
        }

        string body;
        using (var reader = new System.IO.StreamReader(Request.Body))
        {
            body = await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
        }

        if (string.IsNullOrWhiteSpace(body))
        {
            _logger.LogWarning(WebhookControllerConstants.EmptyRequestBodyLogMessage);
            return BadRequest(WebhookControllerConstants.EmptyRequestBodyMessage);
        }

        _logger.LogDebug(WebhookControllerConstants.RequestBodyReceivedLogMessage, body.Length);

        Request.Headers.TryGetValue(WebhookControllerConstants.SecretTokenHeader, out var secretTokenValue);
        var secretToken = secretTokenValue.ToString();

        _logger.LogDebug(WebhookControllerConstants.ValidatingSecretTokenLogMessage);

        var update = await _webhookService.ParseAndValidateAsync(body, secretToken, cancellationToken)
            .ConfigureAwait(false);

        if (update is null)
        {
            // ParseAndValidateAsync already logged the reason (invalid signature or parse failure)
            _logger.LogWarning(WebhookControllerConstants.ValidationFailedLogMessage);
            return Unauthorized();
        }

        _logger.LogInformation(
            WebhookControllerConstants.ValidationSucceededLogMessage,
            update.UpdateId,
            update.MessageType);

        await _webhookService.DispatchUpdateAsync(update, cancellationToken).ConfigureAwait(false);

        _logger.LogDebug(WebhookControllerConstants.UpdateDispatchedLogMessage, update.UpdateId);

        // Always return 200 OK so Telegram does not re-deliver
        return Ok();
    }

    /// <summary>
    /// Returns current webhook status (registered URL, dispatched update count, etc.).
    /// Useful for health checks and diagnostics.
    /// </summary>
    [HttpGet(WebhookControllerConstants.InfoRoute)]
    public IActionResult GetInfo()
    {
        var info = _webhookService.GetInfo();
        return Ok(info);
    }
}
