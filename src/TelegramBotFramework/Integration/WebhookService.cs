#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace TelegramBotFramework.Integration;

/// <summary>
/// Production implementation of <see cref="IWebhookService"/>.
/// Registers and unregisters the Telegram webhook automatically as a hosted service,
/// and dispatches validated updates to subscribed handlers.
/// </summary>
public sealed class WebhookService : IWebhookService, IHostedService
{
	private readonly ITelegramApiClient _apiClient;
	private readonly WebhookHandler _webhookHandler;
	private readonly WebhookOptions _options;
	private readonly ILogger<WebhookService> _logger;

	private bool _isRegistered;
	private DateTime? _registeredAt;
	private long _updatesDispatched;

	/// <inheritdoc/>
	public event Func<TelegramUpdate, Task>? OnUpdateReceived;

	/// <summary>
	/// Initialises a new instance of <see cref="WebhookService"/>.
	/// </summary>
	public WebhookService(
		ITelegramApiClient apiClient,
		WebhookOptions options,
		ILogger<WebhookService> logger)
	{
		_apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
		_options = options ?? throw new ArgumentNullException(nameof(options));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		_webhookHandler = new WebhookHandler();
	}

	// -------------------------------------------------------------------------
	// IHostedService
	// -------------------------------------------------------------------------

	/// <summary>Registers the webhook on application startup.</summary>
	public async Task StartAsync(CancellationToken cancellationToken)
	{
		await RegisterAsync(cancellationToken).ConfigureAwait(false);
	}

	/// <summary>Unregisters the webhook on application shutdown.</summary>
	public async Task StopAsync(CancellationToken cancellationToken)
	{
		await UnregisterAsync(cancellationToken).ConfigureAwait(false);
	}

	// -------------------------------------------------------------------------
	// IWebhookService
	// -------------------------------------------------------------------------

	/// <inheritdoc/>
	public async Task RegisterAsync(CancellationToken cancellationToken = default)
	{
		try
		{
			_options.Validate();

			var success = await _apiClient.SetWebhookAsync(_options.Url).ConfigureAwait(false);

			if (success)
			{
				_isRegistered = true;
				_registeredAt = DateTime.UtcNow;

				_logger.LogInformation(
					"Webhook registered successfully — URL: {WebhookUrl}, MaxConnections: {MaxConnections}",
					_options.Url, _options.MaxConnections);
			}
			else
			{
				_logger.LogError("Failed to register webhook at URL: {WebhookUrl}", _options.Url);
			}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Exception while registering webhook");
			throw;
		}
	}

	/// <inheritdoc/>
	public async Task UnregisterAsync(CancellationToken cancellationToken = default)
	{
		if (!_isRegistered)
			return;

		try
		{
			var success = await _apiClient.RemoveWebhookAsync().ConfigureAwait(false);

			if (success)
			{
				_isRegistered = false;
				_logger.LogInformation("Webhook unregistered successfully");
			}
			else
			{
				_logger.LogWarning("Webhook unregistration request returned a failure response");
			}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Exception while unregistering webhook");
		}
	}

	/// <inheritdoc/>
	public async Task DispatchUpdateAsync(TelegramUpdate update, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(update);

		System.Threading.Interlocked.Increment(ref _updatesDispatched);

		var handler = OnUpdateReceived;
		if (handler is null)
		{
			_logger.LogDebug(
				"Received update {UpdateId} but no handlers are subscribed", update.UpdateId);
			return;
		}

		try
		{
			await handler(update).ConfigureAwait(false);

			_logger.LogDebug(
				"Dispatched update {UpdateId} of type {UpdateType}", update.UpdateId, update.MessageType);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex,
				"Error dispatching update {UpdateId}", update.UpdateId);
		}
	}

	/// <inheritdoc/>
	public WebhookInfo GetInfo() => new()
	{
		IsRegistered = _isRegistered,
		Url = _isRegistered ? _options.Url : null,
		RegisteredAt = _registeredAt,
		UpdatesDispatched = _updatesDispatched
	};

	/// <summary>
	/// Parses and validates a raw JSON payload received at the webhook endpoint.
	/// Returns the parsed update, or <c>null</c> if the payload is invalid or the
	/// secret-token check fails.
	/// </summary>
	/// <param name="jsonBody">The raw request body.</param>
	/// <param name="secretTokenHeader">
	/// Value of the <c>X-Telegram-Bot-Api-Secret-Token</c> header, if present.
	/// </param>
	public async Task<TelegramUpdate?> ParseAndValidateAsync(
		string jsonBody,
		string? secretTokenHeader)
	{
		if (!_webhookHandler.ValidateSecretToken(secretTokenHeader, _options.SecretToken))
		{
			_logger.LogWarning("Rejected webhook request: secret token validation failed");
			return null;
		}

		return await _webhookHandler.ProcessUpdateAsync(jsonBody).ConfigureAwait(false);
	}
}
