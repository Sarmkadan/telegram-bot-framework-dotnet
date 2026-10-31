#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using Microsoft.Extensions.DependencyInjection;

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
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddInlineQueryHandling(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

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
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddInlineQueryHandlingWithLocalCache(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<Caching.ICacheProvider, Caching.LocalCacheProvider>();
        services.AddSingleton<IInlineQueryService, InlineQueryService>();
        return services;
    }

    /// <summary>
    /// Returns the query text in the normalized form used for case-insensitive matching:
    /// surrounding whitespace removed and converted to lower case using the invariant culture.
    /// </summary>
    /// <param name="query">The inline query whose text should be normalized.</param>
    /// <returns>The normalized query text; an empty string when the query has no text.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="query"/> is null.</exception>
    public static string GetNormalizedQueryText(this Models.InlineQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        return query.Query.Trim().ToLowerInvariant();
    }
}