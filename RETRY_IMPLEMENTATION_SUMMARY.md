# Telegram API Retry Mechanism Implementation Summary

## Overview

This implementation adds robust retry functionality with Retry-After header honoring to the `TelegramApiClient` class, addressing the requirement to handle Telegram's 429 rate limiting responses and transient 5xx errors.

## Changes Made

### 1. New Files Created

#### `TelegramApiRetryOptions.cs`
- **Purpose**: Configuration class for retry policy settings
- **Key Features**:
  - `MaxRetryAttempts`: Maximum number of retry attempts (default: 3)
  - `BaseDelayMilliseconds`: Starting delay for exponential backoff (default: 1000ms)
  - `MaxDelayMilliseconds`: Maximum delay between retries (default: 30000ms)
  - `RespectRetryAfter`: Whether to honor Telegram's Retry-After header (default: true)
  - Individual toggles for different error types (429, 5xx, network errors, etc.)
  - Validation method to ensure configuration is valid

#### `TelegramApiRetryHandler.cs`
- **Purpose**: Core retry logic handler that implements the actual retry mechanism
- **Key Features**:
  - `ExecuteWithRetryAsync()`: Handles POST requests with retry logic
  - `ExecuteGetWithRetryAsync()`: Handles GET requests with retry logic
  - Honors Telegram's `Retry-After` header from both HTTP headers and response body
  - Implements exponential backoff with jitter for better load distribution
  - Smart retry logic that respects idempotency of methods
  - Comprehensive error handling for various HTTP status codes
  - Detailed logging at each retry attempt

### 2. Modified Files

#### `TelegramApiClient.cs`
- **Changes**:
  - Added constructor parameter: `TelegramApiRetryOptions? retryOptions = null`
  - Added private fields: `_retryHandler` and `_retryOptions`
  - Updated constructor to initialize retry handler with validated options
  - Modified `SendApiRequestAsync<T>()` to use retry handler instead of direct HTTP calls
  - Modified `GetApiRequestAsync()` to use retry handler for GET requests
  - Added `IsIdempotentMethod()` helper method to determine safe retry candidates
  - Enhanced error handling with specific exception types (`TelegramRateLimitedException`, `TelegramServerException`)

## Technical Details

### Retry Logic Flow

1. **Request Execution**: API call is made through the retry handler
2. **Error Detection**: Handler checks for:
   - HTTP 429 (Too Many Requests) → Extracts `Retry-After` header/body
   - HTTP 5xx errors → Treats as transient failures
   - HTTP 408/502/503/504 → Treats as transient failures
   - Network/Timeout errors → Treats as retryable
3. **Retry Decision**:
   - For **idempotent methods** (e.g., `getMe`, `getUpdates`, `getFile`): Retries on all eligible errors
   - For **non-idempotent methods** (e.g., `sendMessage`, `sendPhoto`, `editMessageText`): Only retries on network/timeout errors that occur **before** bytes are sent
4. **Delay Calculation**:
   - If `Retry-After` header/body is present: Uses that value
   - Otherwise: Applies exponential backoff with jitter
   - Formula: `delay = min(baseDelay * 2^(attempt-1), maxDelay)`
   - Jitter: Random factor between 0.5x and 1.5x to prevent thundering herd
5. **Max Attempts**: Stops retrying after reaching `MaxRetryAttempts`


### Idempotency Classification

The implementation classifies Telegram API methods as:

**Idempotent (safe to retry)**:
- `getMe`
- `getUpdates`
- `getFile`
- `getChat*` methods
- `getSticker*` methods
- `answerInlineQuery`
- `getMyCommands`
- `setMyCommands` (actually modifies state but included for common patterns)

**Non-Idempotent (careful retry)**:
- All `send*` methods
- `editMessage*` methods
- `deleteMessage`
- `answerCallbackQuery`
- `setWebhook`/`deleteWebhook`
- State-modifying operations

### Exception Handling

- `TelegramRateLimitedException`: Thrown when rate limited (429) after max retries
- `TelegramServerException`: Thrown when server errors (5xx) persist after max retries
- Both exceptions are caught in the client and logged appropriately
- Methods return `false` or `null` on failure, maintaining backward compatibility

## Configuration Examples

### Default Configuration
```csharp
var client = new TelegramApiClient(botToken);
// Uses defaults:
// MaxRetryAttempts = 3
// BaseDelayMilliseconds = 1000
// MaxDelayMilliseconds = 30000
// RespectRetryAfter = true
// All retry options = true
```

### Custom Configuration
```csharp
var retryOptions = new TelegramApiRetryOptions
{
    MaxRetryAttempts = 5,
    BaseDelayMilliseconds = 500,
    MaxDelayMilliseconds = 60000,
    RetryOnRateLimited = true,
    RetryOnServerErrors = true,
    RetryOnNetworkErrors = true,
    RespectRetryAfter = true
};

var client = new TelegramApiClient(botToken, retryOptions: retryOptions);
```

### Dependency Injection Configuration
```csharp
// In Startup.cs or similar
services.AddSingleton<TelegramApiRetryOptions>(_ => new TelegramApiRetryOptions
{
    MaxRetryAttempts = 3,
    BaseDelayMilliseconds = 1000,
    MaxDelayMilliseconds = 30000
});

services.AddSingleton<ITelegramApiClient, TelegramApiClient>();
```

## Benefits


1. **Automatic Rate Limit Handling**: Automatically respects Telegram's `Retry-After` header when available
2. **Resilience**: Survives transient network issues and temporary server problems
3. **Backward Compatible**: Existing code continues to work without changes
4. **Configurable**: Fine-grained control over retry behavior
5. **Smart Retry**: Respects idempotency to avoid duplicate operations
6. **Exponential Backoff**: Prevents overwhelming servers during outages
7. **Jitter**: Adds randomness to prevent synchronized retries
8. **Comprehensive Logging**: Detailed logging at each retry attempt for observability

## Testing

The implementation:
- ✅ Compiles successfully with no errors
- ✅ Passes all existing tests (warnings only in test projects are pre-existing)
- ✅ Maintains backward compatibility with existing `ITelegramApiClient` interface
- ✅ Follows .NET best practices for resilience
- ✅ Includes proper XML documentation
- ✅ Handles edge cases (invalid configurations, null values, etc.)

## Error Codes Handled

| Status Code | Description | Retry Behavior |
|------------|-------------|---------------|
| 429 | Too Many Requests | ✅ Retries with Retry-After header |
| 408 | Request Timeout | ✅ Retries |
| 500 | Internal Server Error | ✅ Retries |
| 502 | Bad Gateway | ✅ Retries |
| 503 | Service Unavailable | ✅ Retries |
| 504 | Gateway Timeout | ✅ Retries |

## Performance Considerations

- **Minimal Overhead**: Retry handler only adds overhead when errors occur
- **No Retry on Success**: Zero impact on successful API calls
- **Exponential Backoff**: Prevents retry storms
- **Jitter**: Distributes retry load evenly
- **Cancellation Support**: Properly respects cancellation tokens

## Migration Guide

No migration needed! The implementation is fully backward compatible:

```csharp
// Old code (still works)
var client = new TelegramApiClient(botToken);

// New code (with custom retry options)
var client = new TelegramApiClient(
    botToken,
    retryOptions: new TelegramApiRetryOptions
    {
        MaxRetryAttempts = 5
    }
);
```

## Future Enhancements (Not Implemented)

Potential future improvements could include:
- Integration with Polly library for more sophisticated policies
- Circuit breaker pattern for sustained failures
- Metrics collection for monitoring retry behavior
- Distributed tracing integration
- Async circuit breaker for high-throughput scenarios

---

**Implementation Date**: 2026-07-24  
**Status**: ✅ Complete and tested  
**Build Status**: Passing (exit code 0)  
**Backward Compatibility**: 100%
