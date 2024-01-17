# IRateLimitingStrategy

The `IRateLimitingStrategy` interface defines the contract for rate-limiting implementations in the Telegram Bot Framework for .NET. It provides methods to check if a request is allowed, track remaining requests, and manage token-based or window-based rate limiting strategies.

## API

### `TokenBucketStrategy`
A concrete rate-limiting strategy implementing a token bucket algorithm. Tokens are consumed per request and replenished over time.

### `bool IsRequestAllowed()`
Determines whether a new request is permitted under the current rate-limiting rules.

- **Returns**: `true` if the request is allowed; otherwise, `false`.
- **Throws**: No documented exceptions.

### `int GetRemainingRequests()`
Returns the number of requests remaining before the rate limit is exceeded.

- **Returns**: The remaining request count as an integer.
- **Throws**: No documented exceptions.

### `double AvailableTokens`
Gets the current number of available tokens in the bucket.

- **Returns**: A `double` representing the available token count.
- **Throws**: No documented exceptions.

### `TokenBucket`
(Inferred as a property or field of `TokenBucketStrategy`.) The underlying token bucket instance managing token consumption and replenishment.

### `void Replenish()`
Replenishes the token bucket to its maximum capacity.

- **Throws**: No documented exceptions.

---

### `SlidingWindowStrategy`
A concrete rate-limiting strategy implementing a sliding window algorithm. Tracks request counts within a rolling time window.

### `bool IsRequestAllowed()`
Determines whether a new request is permitted under the sliding window rate-limiting rules.

- **Returns**: `true` if the request is allowed; otherwise, `false`.
- **Throws**: No documented exceptions.

### `int GetRemainingRequests()`
Returns the number of requests remaining before the sliding window rate limit is exceeded.

- **Returns**: The remaining request count as an integer.
- **Throws**: No documented exceptions.

---

### `FixedWindowStrategy`
A concrete rate-limiting strategy implementing a fixed window algorithm. Tracks request counts within fixed time intervals.

### `bool IsRequestAllowed()`
Determines whether a new request is permitted under the fixed window rate-limiting rules.

- **Returns**: `true` if the request is allowed; otherwise, `false`.
- **Throws**: No documented exceptions.

### `int GetRemainingRequests()`
Returns the number of requests remaining before the fixed window rate limit is exceeded.

- **Returns**: The remaining request count as an integer.
- **Throws**: No documented exceptions.

### `DateTime WindowStartTime`
Gets the start time of the current fixed window.

- **Returns**: A `DateTime` representing the window start.
- **Throws**: No documented exceptions.

### `DateTime WindowEndTime`
Gets the end time of the current fixed window.

- **Returns**: A `DateTime` representing the window end.
- **Throws**: No documented exceptions.

### `int RequestCount`
Gets the number of requests made in the current fixed window.

- **Returns**: The request count as an integer.
- **Throws**: No documented exceptions.

## Usage

### Example 1: Using `TokenBucketStrategy` for token-based rate limiting
