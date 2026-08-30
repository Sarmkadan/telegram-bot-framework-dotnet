#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Services;

/// <summary>
/// Handles Telegram inline queries with transparent result caching and page-based pagination.
/// </summary>
public interface IInlineQueryService
{
    /// <summary>
    /// Processes an inline query and returns a paginated result set.
    /// The full result list is fetched via <paramref name="resultsFactory"/> on cache miss and
    /// cached for subsequent pages of the same query within the TTL window.
    /// </summary>
    /// <param name="query">The incoming inline query, including its Telegram pagination offset.</param>
    /// <param name="resultsFactory">
    /// Delegate invoked on a cache miss; must return the complete list of matching results.
    /// </param>
    /// <param name="pageSize">Number of results per page (default 10).</param>
    /// <param name="cancellationToken">Propagates cancellation to factory and cache operations.</param>
    Task<Models.PagedInlineQueryResult> HandleAsync(
        Models.InlineQuery query,
        Func<Models.InlineQuery, CancellationToken, Task<IList<Models.InlineQueryResult>>> resultsFactory,
        int pageSize = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns cached results for the given query text and page without invoking the factory,
    /// or null when the cache entry is absent or expired.
    /// </summary>
    Task<Models.PagedInlineQueryResult?> GetCachedAsync(
        string queryText,
        int pageNumber = 1,
        CancellationToken cancellationToken = default);

    /// <summary>Removes cached results for the given query text.</summary>
    Task InvalidateCacheAsync(string queryText, CancellationToken cancellationToken = default);

    /// <summary>Records query telemetry for monitoring and analytics without affecting the response path.</summary>
    Task RecordQueryAsync(Models.InlineQuery query, int resultCount, CancellationToken cancellationToken = default);
}

/// <summary>
/// Default implementation of <see cref="IInlineQueryService"/>.
/// </summary>
public sealed class InlineQueryService : IInlineQueryService
{
    private static readonly TimeSpan DefaultCacheExpiry = TimeSpan.FromMinutes(IInlineQueryServiceConstants.DefaultCacheExpiryInMinutes);

    private readonly Caching.ICacheProvider _cache;
    private readonly Microsoft.Extensions.Logging.ILogger<InlineQueryService> _logger;

    /// <summary>
    /// Initialises a new <see cref="InlineQueryService"/>.
    /// </summary>
    public InlineQueryService(
        Caching.ICacheProvider cache,
        Microsoft.Extensions.Logging.ILogger<InlineQueryService> logger)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<Models.PagedInlineQueryResult> HandleAsync(
        Models.InlineQuery query,
        Func<Models.InlineQuery, CancellationToken, Task<IList<Models.InlineQueryResult>>> resultsFactory,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        query.Validate();

        // Derive the page number from Telegram's offset string; default to 1 for the first request.
        var pageNumber = int.TryParse(query.Offset, out var parsed) && parsed > 0 ? parsed : 1;
        var cacheKey = BuildCacheKey(query.Query);

        query.Status = Models.InlineQueryStatus.Processing;
        _logger.LogDebug("Handling inline query {QueryId} page {Page} for user {UserId}",
            query.QueryId, pageNumber, query.UserId);

        try
        {
            var allResults = await _cache.GetOrCreateAsync(
                cacheKey,
                () => resultsFactory(query, cancellationToken),
                DefaultCacheExpiry);

            var paged = Paginate(allResults, pageNumber, pageSize);

            query.Status = Models.InlineQueryStatus.Answered;
            query.AnsweredAt = DateTime.UtcNow;

            _logger.LogInformation(
                "Inline query {QueryId} answered: {Count}/{Total} results (page {Page})",
                query.QueryId, paged.Results.Count, paged.TotalCount, pageNumber);

            return paged;
        }
        catch (Exception ex)
        {
            query.Status = Models.InlineQueryStatus.Failed;
            query.SetMetadata("error", ex.Message);
            _logger.LogError(ex, "Inline query {QueryId} failed", query.QueryId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<Models.PagedInlineQueryResult?> GetCachedAsync(
        string queryText,
        int pageNumber = 1,
        CancellationToken cancellationToken = default)
    {
        await Task.Delay(0, cancellationToken).ConfigureAwait(false);
        var allResults = await _cache.GetAsync<IList<Models.InlineQueryResult>>(BuildCacheKey(queryText)).ConfigureAwait(false);
        return allResults  is null ? null : Paginate(allResults, pageNumber, IInlineQueryServiceConstants.DefaultPageSize);
    }

    /// <inheritdoc/>
    public async Task InvalidateCacheAsync(string queryText, CancellationToken cancellationToken = default)
    {
        await Task.Delay(0, cancellationToken).ConfigureAwait(false);
        await _cache.RemoveAsync(BuildCacheKey(queryText)).ConfigureAwait(false);
        _logger.LogDebug("Cache invalidated for inline query '{Query}'", queryText);
    }

    /// <inheritdoc/>
    public async Task RecordQueryAsync(Models.InlineQuery query, int resultCount, CancellationToken cancellationToken = default)
    {
        await Task.Delay(0, cancellationToken).ConfigureAwait(false);
        _logger.LogInformation(
            "Inline query recorded: user={UserId} query='{Query}' results={Count} duration={Duration}ms",
            query.UserId, query.Query, resultCount, query.GetProcessingDurationMs());
    }

    private static string BuildCacheKey(string queryText) =>
        $"{IInlineQueryServiceConstants.CacheKeyPrefix}{queryText.ToLowerInvariant().Trim()}";

    private static Models.PagedInlineQueryResult Paginate(
        IList<Models.InlineQueryResult> allResults,
        int pageNumber,
        int pageSize)
    {
        var total = allResults.Count;
        var skip = (pageNumber - 1) * pageSize;
        var page = allResults.Skip(skip).Take(pageSize).ToList();
        var hasNext = skip + page.Count < total;

        return new Models.PagedInlineQueryResult
        {
            Results = page,
            TotalCount = total,
            PageNumber = pageNumber,
            PageSize = pageSize,
            NextOffset = hasNext ? (pageNumber + 1).ToString() : string.Empty
        };
    }
}