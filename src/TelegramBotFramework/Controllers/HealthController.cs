#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using Microsoft.AspNetCore.Mvc;
using TelegramBotFramework.Integration;

namespace TelegramBotFramework.Controllers;

/// <summary>
/// Health check controller for monitoring bot status.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class HealthController : ControllerBase
{
    private readonly IWebhookService _webhookService;
    private readonly ILogger<HealthController> _logger;
    private readonly DateTime _applicationStartTime;

    /// <summary>
    /// Initializes a new instance of <see cref="HealthController"/>.
    /// </summary>
    public HealthController(
        IWebhookService webhookService,
        ILogger<HealthController> logger)
    {
        _webhookService = webhookService ?? throw new ArgumentNullException(nameof(webhookService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _applicationStartTime = DateTime.UtcNow;
    }

    /// <summary>
    /// Health check endpoint.
    /// </summary>
    [HttpGet]
    public IActionResult Get()
    {
        try
        {
            var webhookInfo = _webhookService.GetInfo();
            var uptime = DateTime.UtcNow - _applicationStartTime;

            var response = new
            {
                status = "healthy",
                uptime = uptime.ToString("c"), // "hh:mm:ss.fff" format
                uptimeSeconds = (long)uptime.TotalSeconds,
                applicationStartTime = _applicationStartTime,
                webhook = new
                {
                    isRegistered = webhookInfo.IsRegistered,
                    url = webhookInfo.Url,
                    registeredAt = webhookInfo.RegisteredAt,
                    updatesDispatched = webhookInfo.UpdatesDispatched
                },
                timestamp = DateTime.UtcNow
            };

            _logger.LogInformation("Health check successful - Uptime: {Uptime}, Updates: {UpdatesCount}",
                uptime.ToString("c"), webhookInfo.UpdatesDispatched);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                status = "unhealthy",
                error = "Health check failed",
                timestamp = DateTime.UtcNow
            });
        }
    }
}