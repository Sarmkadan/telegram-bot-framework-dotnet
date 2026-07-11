# WebhookServiceExtensions

Provides extension methods and utility functions for registering, managing, and interacting with a Telegram Bot webhook service within the dependency injection container.

## API

### `public static async Task<bool> EnsureRegisteredAsync(IServiceProvider serviceProvider)`

Ensures that the webhook is registered with Telegram's servers by calling the `setWebhook` API. If the webhook is already registered and matches the current configuration, the operation is idempotent and returns `true`.

- **Parameters**
  - `serviceProvider`: The service provider used to resolve required services (`ILogger<WebhookService>`, `TelegramApiClient`, `WebhookOptions`).
- **Return value**
  - `Task<bool>`: `true` if the webhook was registered or already matches; `false` if registration failed.
- **Exceptions**
  - Throws `ArgumentNullException` if `serviceProvider` is `null`.
  - Propagates exceptions from `TelegramApiClient` on network or API errors.

---

### `public static async Task<bool> EnsureUnregisteredAsync(IServiceProvider serviceProvider)`

Ensures that any existing webhook is unregistered from Telegram's servers by calling the `deleteWebhook` API. Useful for cleanup or switching to polling.

- **Parameters**
  - `serviceProvider`: The service provider used to resolve required services (`ILogger<WebhookService>`, `TelegramApiClient`).
- **Return value**
  - `Task<bool>`: `true` if the webhook was unregistered or did not exist; `false` if unregistration failed.
- **Exceptions**
  - Throws `ArgumentNullException` if `serviceProvider` is `null`.
  - Propagates exceptions from `TelegramApiClient` on network or API errors.

---
### `public static ILogger<WebhookService> GetLogger(IServiceProvider serviceProvider)`

Resolves the logger instance used by the webhook service from the service provider.

- **Parameters**
  - `serviceProvider`: The service provider used to resolve the logger.
- **Return value**
  - `ILogger<WebhookService>`: The logger instance.
- **Exceptions**
  - Throws `ArgumentNullException` if `serviceProvider` is `null`.
  - Throws `InvalidOperationException` if the logger is not registered in the container.

---
### `public static TelegramApiClient GetApiClient(IServiceProvider serviceProvider)`

Resolves the Telegram API client used by the webhook service from the service provider.

- **Parameters**
  - `serviceProvider`: The service provider used to resolve the API client.
- **Return value**
  - `TelegramApiClient`: The API client instance.
- **Exceptions**
  - Throws `ArgumentNullException` if `serviceProvider` is `null`.
  - Throws `InvalidOperationException` if the client is not registered in the container.

---
### `public static WebhookOptions GetOptions(IServiceProvider serviceProvider)`

Resolves the webhook configuration options from the service provider.

- **Parameters**
  - `serviceProvider`: The service provider used to resolve the options.
- **Return value**
  - `WebhookOptions`: The configuration options.
- **Exceptions**
  - Throws `ArgumentNullException` if `serviceProvider` is `null`.
  - Throws `InvalidOperationException` if the options are not registered in the container.

---
### `public static IServiceCollection AddWebhookService(IServiceCollection services, Action<WebhookOptions> configureOptions = null)`

Registers the webhook service and related dependencies with the service collection.

- **Parameters**
  - `services`: The service collection to configure.
  - `configureOptions`: Optional action to configure webhook options.
- **Return value**
  - `IServiceCollection`: The configured service collection.
- **Exceptions**
  - Throws `ArgumentNullException` if `services` is `null`.

---
### `public static long GetUpdatesDispatchedCount(IServiceProvider serviceProvider)`

Gets the total number of updates dispatched by the webhook service.

- **Parameters**
  - `serviceProvider`: The service provider used to resolve the service.
- **Return value**
  - `long`: The count of dispatched updates.
- **Exceptions**
  - Throws `ArgumentNullException` if `serviceProvider` is `null`.
  - Throws `InvalidOperationException` if the service is not registered in the container.

---
### `public static DateTime? GetRegisteredAt(IServiceProvider serviceProvider)`

Gets the timestamp when the webhook was last successfully registered with Telegram.

- **Parameters**
  - `serviceProvider`: The service provider used to resolve the service.
- **Return value**
  - `DateTime?`: The timestamp of last successful registration, or `null` if never registered.
- **Exceptions**
  - Throws `ArgumentNullException` if `serviceProvider` is `null`.
  - Throws `InvalidOperationException` if the service is not registered in the container.

## Usage

### Example 1: Registering a webhook on startup
