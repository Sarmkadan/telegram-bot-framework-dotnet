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
    /// Receives a Telegram update delivered via webhook and queues it for background processing.
    /// Returns <c>200 OK</c> as soon as the update is queued, so Telegram does not retry because of
    /// slow handlers; processing errors never affect the response.
    /// </summary>
    /// <remarks>
    /// When <see cref="WebhookOptions.SecretToken"/> is configured, the controller
    /// validates the <c>X-Telegram-Bot-Api-Secret-Token</c> header and returns
    /// <c>401 Unauthorized</c> on mismatch. When the queue is full under
    /// <see cref="WebhookQueueFullPolicy.Reject"/>, it returns <c>429 Too Many Requests</c> with a
    /// <c>Retry-After</c> header so Telegram redelivers the update. While the service is stopping, it returns
    /// <c>503 Service Unavailable</c>.
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

        // Queue for the background workers so the response does not wait for handler work
        return _webhookService.EnqueueUpdate(update) switch
        {
            WebhookEnqueueResult.Accepted => Ok(),
            WebhookEnqueueResult.Rejected => QueueFull(),
            _ => ServiceStopping()
        };
    }

    /// <summary>
    /// Answers <c>429 Too Many Requests</c> with <c>Retry-After</c> so Telegram redelivers the rejected update later.
    /// </summary>
    private IActionResult QueueFull()
    {
        Response.Headers[WebhookControllerConstants.RetryAfterHeaderName] =
            WebhookControllerConstants.QueueFullRetryAfterSeconds;
        return StatusCode(WebhookControllerConstants.TooManyRequestsStatusCode,
            WebhookControllerConstants.QueueFullMessage);
    }

    /// <summary>
    /// Answers <c>503 Service Unavailable</c> while the bot is shutting down and no longer queues updates.
    /// </summary>
    private IActionResult ServiceStopping()
    {
        _logger.LogWarning(WebhookControllerConstants.ServiceStoppingLogMessage);
        return StatusCode(WebhookControllerConstants.ServiceUnavailableStatusCode,
            WebhookControllerConstants.ServiceUnavailableMessage);
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
