// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Services;

/// <summary>
/// Dependency injection extensions for registering inline query handling.
/// </summary>
public static class InlineQueryExtensions
{
    /// <summary>
    /// Registers <see cref="IInlineQueryService"/> and its default implementation.
    /// Requires <see cref="Caching.ICacheProvider"/> to be registered separately — for example
    /// via a custom cache provider or by calling
    /// <see cref="AddInlineQueryHandlingWithLocalCache"/> instead.
    /// </summary>
    /// <param name="services">The service collection to extend.</param>
    /// <returns>The same <paramref name="services"/> instance for fluent chaining.</returns>
    public static Microsoft.Extensions.DependencyInjection.IServiceCollection AddInlineQueryHandling(
        this Microsoft.Extensions.DependencyInjection.IServiceCollection services)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        services.AddSingleton<IInlineQueryService, InlineQueryService>();
        return services;
    }

    /// <summary>
    /// Registers <see cref="IInlineQueryService"/> together with the built-in
    /// <see cref="Caching.LocalCacheProvider"/> as a convenience when no cache provider has been
    /// configured yet. Suitable for single-instance deployments and local development.
    /// </summary>
    /// <param name="services">The service collection to extend.</param>
    /// <returns>The same <paramref name="services"/> instance for fluent chaining.</returns>
    public static Microsoft.Extensions.DependencyInjection.IServiceCollection AddInlineQueryHandlingWithLocalCache(
        this Microsoft.Extensions.DependencyInjection.IServiceCollection services)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        services.AddSingleton<Caching.ICacheProvider, Caching.LocalCacheProvider>();
        services.AddSingleton<IInlineQueryService, InlineQueryService>();
        return services;
    }
}
