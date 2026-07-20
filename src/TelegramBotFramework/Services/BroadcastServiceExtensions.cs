#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TelegramBotFramework.Integration;

namespace TelegramBotFramework.Services;

/// <summary>
/// Extension methods for registering BroadcastService in DI container.
/// </summary>
public static class BroadcastServiceExtensions
{
    /// <summary>
    /// Adds BroadcastService to the service collection.
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddBroadcastService(this IServiceCollection services)
    {
        services.TryAddTransient<IBroadcastService, BroadcastService>();
        return services;
    }

    /// <summary>
    /// Adds BroadcastService with custom configuration to the service collection.
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configure">Configuration action</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddBroadcastService(
        this IServiceCollection services,
        Action<BroadcastOptions> configure)
    {
        if (configure == null)
        {
            throw new ArgumentNullException(nameof(configure));
        }

        services.Configure(configure);
        services.TryAddTransient<IBroadcastService, BroadcastService>();
        return services;
    }

    /// <summary>
    /// Adds BroadcastService as a singleton with custom configuration.
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configure">Configuration action</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddBroadcastServiceSingleton(
        this IServiceCollection services,
        Action<BroadcastOptions> configure)
    {
        if (configure == null)
        {
            throw new ArgumentNullException(nameof(configure));
        }

        services.Configure(configure);
        services.AddSingleton<IBroadcastService>(provider =>
        {
            var options = new BroadcastOptions();
            configure(options);
            var apiClient = provider.GetRequiredService<ITelegramApiClient>();
            return new BroadcastService(apiClient);
        });
        return services;
    }
}
