#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using TelegramBotFramework.Services;

namespace TelegramBotFramework.Controllers;

/// <summary>
/// Defines the contract for the webhook HTTP server that receives Telegram updates.
/// </summary>
public interface IWebhookServer : IAsyncDisposable
{
    /// <summary>
    /// Starts the webhook server and begins listening for incoming updates.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the startup.</param>
    Task StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops the webhook server gracefully.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the shutdown.</param>
    Task StopAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets whether the server is currently running.
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    /// Gets the URL the server is listening on.
    /// </summary>
    string? ListeningUrl { get; }
}

/// <summary>
/// Self-contained Kestrel-based HTTP server that hosts webhook endpoints
/// for receiving Telegram updates. Configures the minimal ASP.NET pipeline
/// with health-check, webhook and optional admin routes.
/// </summary>
public sealed class WebhookServer : IWebhookServer
{
    private readonly Models.BotConfiguration _configuration;
    private readonly IUpdateHandler _updateHandler;
    private readonly ILogger<WebhookServer> _logger;
    private WebApplication? _app;
    private bool _isRunning;

    /// <inheritdoc />
    public bool IsRunning => _isRunning;

    /// <inheritdoc />
    public string? ListeningUrl { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="WebhookServer"/> class.
    /// </summary>
    /// <param name="configuration">The bot configuration containing webhook settings.</param>
    /// <param name="updateHandler">The handler that processes incoming Telegram updates.</param>
    /// <param name="logger">The logger instance.</param>
    public WebhookServer(
        Models.BotConfiguration configuration,
        IUpdateHandler updateHandler,
        ILogger<WebhookServer> logger)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _updateHandler = updateHandler ?? throw new ArgumentNullException(nameof(updateHandler));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_isRunning)
        {
            _logger.LogWarning("Webhook server is already running");
            return;
        }

        var builder = WebApplication.CreateBuilder();

        builder.WebHost.UseUrls(_configuration.WebhookUrl ?? "http://0.0.0.0:8443");

        builder.Services.AddSingleton(_updateHandler);
        builder.Services.AddSingleton(_configuration);
        builder.Services.AddControllers()
            .AddApplicationPart(typeof(WebhookController).Assembly);

        _app = builder.Build();

        _app.MapControllers();
        _app.MapGet("/health", () => Results.Ok(new
        {
            status = "healthy",
            updatesProcessed = _updateHandler.ProcessedCount,
            updatesFailed = _updateHandler.FailedCount,
            timestamp = DateTime.UtcNow
        }));

        await _app.StartAsync(cancellationToken).ConfigureAwait(false);

        ListeningUrl = _configuration.WebhookUrl ?? "http://0.0.0.0:8443";
        _isRunning = true;

        _logger.LogInformation("Webhook server started on {Url}", ListeningUrl);
    }

    /// <inheritdoc />
    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (!_isRunning || _app is null)
        {
            _logger.LogWarning("Webhook server is not running");
            return;
        }

        await _app.StopAsync(cancellationToken).ConfigureAwait(false);
        _isRunning = false;
        ListeningUrl = null;

        _logger.LogInformation("Webhook server stopped");
    }

    /// <summary>
    /// Disposes of the web application resources.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_app is not null)
        {
            if (_isRunning)
            {
                await StopAsync().ConfigureAwait(false);
            }
            await _app.DisposeAsync().ConfigureAwait(false);
            _app = null;
        }
    }
}
