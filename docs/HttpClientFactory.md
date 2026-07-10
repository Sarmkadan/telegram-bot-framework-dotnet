# HttpClientFactory

The `HttpClientFactory` class serves as the central mechanism for managing and retrieving `HttpClient` instances within the `telegram-bot-framework-dotnet` ecosystem, ensuring efficient resource utilization and consistent configuration for network communications. It encapsulates the instantiation and lifecycle management of HTTP clients, offering specialized methods to obtain clients prepared for general web requests, communication with the Telegram Bot API, or requests requiring specific headers and authentication credentials. By centralizing these operations, the factory promotes standardized networking practices and simplifies the handling of complex connection requirements across the framework.

## API

*   `public HttpClient GetClient()`: Retrieves a standard `HttpClient` instance configured for general-purpose HTTP requests.
*   `public HttpClient GetTelegramClient()`: Retrieves an `HttpClient` instance pre-configured with the necessary base addresses and default headers required for communication with the Telegram Bot API.
*   `public HttpClient GetClientWithHeaders()`: Retrieves an `HttpClient` instance pre-populated with default framework-specific headers to streamline requests to services requiring additional metadata.
*   `public HttpClient GetClientWithAuth()`: Retrieves an `HttpClient` instance pre-configured with the authentication tokens or credentials necessary for accessing protected API endpoints.
*   `public void Dispose()`: Releases all resources associated with the factory, including any underlying `HttpMessageHandler` instances or shared connections.

## Usage

```csharp
// Example 1: Retrieving and using a Telegram API client
var factory = new HttpClientFactory();
try
{
    using (var client = factory.GetTelegramClient())
    {
        var response = await client.GetAsync("getMe");
        // Process the response from the Telegram Bot API
    }
}
finally
{
    factory.Dispose();
}

// Example 2: Retrieving an authenticated client for an external service
var factory = new HttpClientFactory();
try
{
    using (var client = factory.GetClientWithAuth())
    {
        var response = await client.GetAsync("https://api.example.com/protected-resource");
        // Process the response from the protected endpoint
    }
}
finally
{
    factory.Dispose();
}
```

## Notes

*   **Thread-Safety:** The `HttpClientFactory` implementation is designed to be thread-safe, enabling multiple threads to concurrently request and utilize `HttpClient` instances without requiring external synchronization.
*   **Disposal:** The `Dispose()` method must be explicitly called to ensure that all underlying resources and connections managed by the factory are properly released. Neglecting to dispose of the factory when it is no longer required may result in resource exhaustion or leaks, particularly in long-running applications that frequently instantiate factories.
*   **Instance Lifecycle:** While the factory facilitates the creation of `HttpClient` instances, developers should adhere to `HttpClient` best practices, including proper disposal of the returned instances—typically via `using` statements if the client's lifecycle is intended for a single operation—to prevent socket exhaustion.
