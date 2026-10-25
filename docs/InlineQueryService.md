# InlineQueryService

`InlineQueryService` is the default implementation of `IInlineQueryService`. It answers Telegram inline queries with a paginated result set. The full list of matching results is produced by a caller-supplied factory on a cache miss, cached by normalized query text, and sliced into pages for each request. Telegram's `offset` string selects the page.

Source: `src/TelegramBotFramework/Services/InlineQueryService.cs`

## Purpose

- **Pagination from Telegram's offset.** The query's `Offset` is parsed as a 1-based page number. An empty, non-numeric, or non-positive offset means page 1. The result's `NextOffset` is set to the next page number, or to an empty string when no further pages exist.
- **Result caching.** The full result list is stored in `ICacheProvider` under `inline_query_<normalized query>`, with a 5-minute expiry. Later pages of the same query are served from the cache without calling the factory again. The key is the query text lowercased and trimmed, so the cache is shared across users.
- **Query lifecycle tracking.** `HandleAsync` updates the `InlineQuery.Status` property: `Processing` before the work, `Answered` on success (with `AnsweredAt` set), and `Failed` on error. On failure the error message is stored in metadata under the `error` key and the exception is rethrown.

Constants (`IInlineQueryServiceConstants`, internal): cache key prefix `inline_query_`, default page size `10`, default cache expiry `5` minutes.

## Construction and registration

The constructor takes an `ICacheProvider` and an `ILogger<InlineQueryService>`. Both arguments must be non-null, otherwise `ArgumentNullException` is thrown.

```csharp
public InlineQueryService(
    Caching.ICacheProvider cache,
    Microsoft.Extensions.Logging.ILogger<InlineQueryService> logger)
```

The class is `sealed` and is normally registered through the extension methods in `InlineQueryExtensions`:

| Method | Registers |
|---|---|
| `AddInlineQueryHandling()` | `IInlineQueryService` → `InlineQueryService` (singleton). You must register an `ICacheProvider` yourself. |
| `AddInlineQueryHandlingWithLocalCache()` | The same, plus `ICacheProvider` → `LocalCacheProvider` (singleton). Suited to single-instance deployments. |

## Public methods

All methods are declared on `IInlineQueryService`, and `InlineQueryService` implements each one.

### `Task<PagedInlineQueryResult> HandleAsync(InlineQuery query, Func<InlineQuery, CancellationToken, Task<IList<InlineQueryResult>>> resultsFactory, int pageSize = 10, CancellationToken cancellationToken = default)`

Answers an inline query with one page of results.

- `query`: the incoming query. Its `Offset` selects the page. Its `Query` text is the cache key.
- `resultsFactory`: called only on a cache miss. It must return the complete list of matches for the query, not just one page.
- `pageSize`: the number of results per page.
- `cancellationToken`: passed to the factory and to the cache.

Returns a `PagedInlineQueryResult` with `Results`, `TotalCount`, `PageNumber`, `PageSize`, and `NextOffset`. A page past the end of the list returns an empty `Results` list.

Exceptions: any exception from the factory or the cache is logged, the query is marked `Failed`, and the exception is rethrown.

### `Task<PagedInlineQueryResult?> GetCachedAsync(string queryText, int pageNumber = 1, CancellationToken cancellationToken = default)`

Returns a page from the cache without calling any factory. Returns `null` when the entry is absent or expired.

Note: this method always paginates with `IInlineQueryServiceConstants.DefaultPageSize` (10). It ignores any `pageSize` that was passed to `HandleAsync`. If you use a non-default page size, don't rely on this method to read those pages.

### `Task InvalidateCacheAsync(string queryText, CancellationToken cancellationToken = default)`

Removes the cache entry for `queryText`, after the same normalization as the other methods. The next `HandleAsync` call for that text runs the factory again.

### `Task RecordQueryAsync(InlineQuery query, int resultCount, CancellationToken cancellationToken = default)`

Writes an `Information` log line with the user ID, query text, result count, and processing duration. It does not change the response path or the cache.

## Usage example

Registration, with the local cache provider:

```csharp
builder.Services.AddInlineQueryHandlingWithLocalCache();
```

Handling an inline query from a controller or handler, with a factory that searches a catalogue:

```csharp
public sealed class ProductInlineHandler
{
    private readonly IInlineQueryService _inlineQueries;
    private readonly IProductCatalog _catalog;

    public ProductInlineHandler(IInlineQueryService inlineQueries, IProductCatalog catalog)
    {
        _inlineQueries = inlineQueries;
        _catalog = catalog;
    }

    public async Task<TelegramBotFramework.Models.PagedInlineQueryResult> AnswerAsync(
        TelegramBotFramework.Models.InlineQuery query,
        CancellationToken cancellationToken)
    {
        var page = await _inlineQueries.HandleAsync(
            query,
            async (q, ct) =>
            {
                var products = await _catalog.SearchAsync(q.Query, ct).ConfigureAwait(false);
                return products
                    .Select(p => new TelegramBotFramework.Models.InlineQueryResult
                    {
                        Title = p.Name,
                        Description = p.Price.ToString("C"),
                        Content = p.Description
                    })
                    .ToList<TelegramBotFramework.Models.InlineQueryResult>();
            },
            pageSize: 20,
            cancellationToken: cancellationToken);

        await _inlineQueries.RecordQueryAsync(query, page.TotalCount, cancellationToken);
        return page;
    }
}
```

The caller then passes `page.Results` and `page.NextOffset` to Telegram's `AnswerInlineQuery` call. Telegram calls back with the next offset, and the next `HandleAsync` call uses the cached list.

When the underlying data changes, invalidate the query text:

```csharp
await _inlineQueries.InvalidateCacheAsync("shoes", cancellationToken);
```

## Notes

- Cache entries are keyed only by normalized query text. Any user who sends the same query within the 5-minute window gets the same cached list, so the factory must not return user-specific results.
- `HandleAsync` calls `query.Validate()` before doing any work.
- Each `HandleAsync` call that misses the cache produces a complete result list, which is then paginated. The list is held in memory by the cache provider, so keep factory results reasonably small.
