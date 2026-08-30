#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;

namespace TelegramBotFramework.Integration;

/// <summary>
/// Extension methods for <see cref="WebhookService"/> that provide common webhook management operations.
/// </summary>
public static class WebhookServiceExtensions
{
    /// <summary>
    /// Ensures the webhook is registered, retrying if necessary.
    /// </summary>
    /// <param name="service">The webhook service instance.</param>
    /// <param name="maxRetries">Maximum number of retry attempts. Must be greater than 0.</param>
    /// <param name="retryDelayMs">Delay between retries in milliseconds. Must be greater than 0.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if registration succeeded, false otherwise.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxRetries"/> is less than or equal to 0, or <paramref name="retryDelayMs"/> is less than or equal to 0.</exception>
    public static async Task<bool> EnsureRegisteredAsync(
        this WebhookService service,
        int maxRetries = WebhookServiceExtensionsConstants.DefaultMaxRetries,
        int retryDelayMs = WebhookServiceExtensionsConstants.DefaultRetryDelayMs,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(maxRetries, 0);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(retryDelayMs, 0);

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                await service.RegisterAsync(cancellationToken).ConfigureAwait(false);

                if (service.GetInfo().IsRegistered)
                {
                    return true;
                }

                if (attempt < maxRetries)
                {
                    await Task.Delay(retryDelayMs, cancellationToken).ConfigureAwait(false);
                }
            }
            catch (Exception ex) when (attempt < maxRetries)
            {
                service.GetLogger().LogWarning(ex,
                    WebhookServiceExtensionsConstants.LogWebhookRegistrationAttemptFailed,
                    attempt, maxRetries);
                await Task.Delay(retryDelayMs, cancellationToken).ConfigureAwait(false);
            }
        }

        return false;
    }

    /// <summary>
    /// Ensures the webhook is unregistered.
    /// </summary>
    /// <param name="service">The webhook service instance.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if unregistration succeeded or was already unregistered, false otherwise.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is <see langword="null"/></exception>
    public static async Task<bool> EnsureUnregisteredAsync(
        this WebhookService service,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);

        try
        {
            await service.UnregisterAsync(cancellationToken).ConfigureAwait(false);
            return !service.GetInfo().IsRegistered;
        }
        catch (Exception ex)
        {
            service.GetLogger().LogError(ex, "Failed to unregister webhook");
            return false;
        }
    }

    /// <summary>
    /// Gets the logger associated with the webhook service.
    /// </summary>
    /// <param name="service">The webhook service instance.</param>
    /// <returns>The logger instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is <see langword="null"/></exception>
    /// <exception cref="InvalidOperationException">Logger field not found or invalid.</exception>
    public static ILogger<WebhookService> GetLogger(this WebhookService service)
    {
        ArgumentNullException.ThrowIfNull(service);

        var loggerField = typeof(WebhookService).GetField(
            WebhookServiceExtensionsConstants.LoggerFieldName,
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (loggerField?.GetValue(service) is ILogger<WebhookService> logger)
        {
            return logger;
        }

        throw new InvalidOperationException(WebhookServiceExtensionsConstants.ExceptionLoggerNotFound);
    }

    /// <summary>
    /// Gets the API client associated with the webhook service.
    /// </summary>
    /// <param name="service">The webhook service instance.</param>
    /// <returns>The Telegram API client instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is <see langword="null"/></exception>
    /// <exception cref="InvalidOperationException">API client field not found or invalid.</exception>
    public static TelegramApiClient GetApiClient(this WebhookService service)
    {
        ArgumentNullException.ThrowIfNull(service);

        var apiClientField = typeof(WebhookService).GetField(
            WebhookServiceExtensionsConstants.ApiClientFieldName,
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (apiClientField?.GetValue(service) is TelegramApiClient apiClient)
        {
            return apiClient;
        }

        throw new InvalidOperationException(WebhookServiceExtensionsConstants.ExceptionApiClientNotFound);
    }

    /// <summary>
    /// Gets the webhook options associated with the webhook service.
    /// </summary>
    /// <param name="service">The webhook service instance.</param>
    /// <returns>The webhook options instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is <see langword="null"/></exception>
    /// <exception cref="InvalidOperationException">Options field not found or invalid.</exception>
    public static WebhookOptions GetOptions(this WebhookService service)
    {
        ArgumentNullException.ThrowIfNull(service);

        var optionsField = typeof(WebhookService).GetField(
            WebhookServiceExtensionsConstants.OptionsFieldName,
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (optionsField?.GetValue(service) is WebhookOptions options)
        {
            return options;
        }

        throw new InvalidOperationException(WebhookServiceExtensionsConstants.ExceptionOptionsNotFound);
    }

    /// <summary>
    /// Registers the webhook as a hosted service in the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Action to configure webhook options.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> or <paramref name="configure"/> is <see langword="null"/></exception>
    public static IServiceCollection AddWebhookService(
        this IServiceCollection services,
        Action<WebhookOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        services.AddSingleton<IWebhookService>(provider =>
        {
            var options = new WebhookOptions();
            configure(options);
            options.Validate();

            var apiClient = provider.GetRequiredService<TelegramApiClient>();
            var logger = provider.GetRequiredService<ILogger<WebhookService>>();

            return new WebhookService(apiClient, options, logger);
        })
        .AddSingleton<IHostedService>(provider =>
            provider.GetRequiredService<IWebhookService>() as IHostedService ?? throw new InvalidOperationException(WebhookServiceExtensionsConstants.ExceptionWebhookServiceNotFound)
        );

        return services;
    }

    /// <summary>
    /// Gets the number of updates dispatched by the webhook service.
    /// </summary>
    /// <param name="service">The webhook service instance.</param>
    /// <returns>The count of dispatched updates.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is <see langword="null"/></exception>
    public static long GetUpdatesDispatchedCount(this WebhookService service)
    {
        ArgumentNullException.ThrowIfNull(service);

        var updatesDispatchedField = typeof(WebhookService).GetField(
            WebhookServiceExtensionsConstants.UpdatesDispatchedFieldName,
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (updatesDispatchedField?.GetValue(service) is long count)
        {
            return count;
        }

        return 0;
    }

    /// <summary>
    /// Gets the registration timestamp of the webhook.
    /// </summary>
    /// <param name="service">The webhook service instance.</param>
    /// <returns>The registration timestamp if registered, null otherwise.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is <see langword="null"/></exception>
    public static DateTime? GetRegisteredAt(this WebhookService service)
    {
        ArgumentNullException.ThrowIfNull(service);

        var registeredAtField = typeof(WebhookService).GetField(
            "_registeredAt",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        return registeredAtField?.GetValue(service) as DateTime?;
    }
}