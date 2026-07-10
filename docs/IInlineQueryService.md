# IInlineQueryService

The `IInlineQueryService` interface defines the contract for managing inline query operations in a Telegram bot framework, including handling, caching, and recording inline query requests. It is designed to support efficient processing of inline queries by leveraging caching mechanisms and providing methods to interact with the underlying query infrastructure.

## API

### `InlineQueryService`

The default implementation of `IInlineQueryService` provided by the framework. This service handles the lifecycle of inline queries, including processing, caching, and cache invalidation.

### `async Task<Models.PagedInlineQueryResult> HandleAsync`

Processes an incoming inline query and returns a paged result set containing the query results.

- **Parameters**
  - None (uses the context of the current inline query from the framework).
- **Return Value**
  - A `Task<Models.PagedInlineQueryResult>` representing the paged results of the inline query.
- **Exceptions**
  - Throws `ArgumentNullException` if the current inline query context is null.
  - Throws `InvalidOperationException` if the query cannot be processed due to an invalid state.

### `async Task<Models.PagedInlineQueryResult?> GetCachedAsync`

Retrieves a cached result for a previously processed inline query, if available.

- **Parameters**
  - None (uses the identifier of the current inline query from the framework).
- **Return Value**
  - A `Task<Models.PagedInlineQueryResult?>` representing the cached result, or `null` if no cached result exists.
- **Exceptions**
  - Throws `InvalidOperationException` if the query identifier is invalid or corrupted.

### `async Task InvalidateCacheAsync`

Invalidates the cache for the current inline query, forcing a fresh processing on the next request.

- **Parameters**
  - None (uses the identifier of the current inline query from the framework).
- **Return Value**
  - A `Task` representing the asynchronous operation.
- **Exceptions**
  - Throws `InvalidOperationException` if the query identifier is invalid or corrupted.

### `async Task RecordQueryAsync`

Records the current inline query for caching and tracking purposes.

- **Parameters**
  - None (uses the context of the current inline query from the framework).
- **Return Value**
  - A `Task` representing the asynchronous operation.
- **Exceptions**
  - Throws `ArgumentNullException` if the inline query context is null.
  - Throws `InvalidOperationException` if the query cannot be recorded due to an invalid state.

## Usage

### Example 1: Handling an Inline Query
