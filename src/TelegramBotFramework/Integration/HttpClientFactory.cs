// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Integration;

/// <summary>
/// Factory for creating and managing HTTP clients with pre-configured settings.
/// Handles connection pooling, timeouts, and retry policies consistently.
/// </summary>
public class HttpClientFactory
{
    private readonly Dictionary<string, HttpClient> _httpClients = new();
    private readonly object _lockObj = new();

    /// <summary>
    /// Gets or creates an HttpClient for a specific base URL.
    /// </summary>
    public HttpClient GetClient(string baseUrl, TimeSpan? timeout = null)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
            throw new ArgumentException("Base URL cannot be empty", nameof(baseUrl));

        lock (_lockObj)
        {
            if (_httpClients.TryGetValue(baseUrl, out var existingClient))
                return existingClient;

            var client = new HttpClient(new SocketsHttpHandler
            {
                PooledConnectionLifetime = TimeSpan.FromMinutes(2),
                PooledConnectionIdleTimeout = TimeSpan.FromMinutes(1),
                AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
            })
            {
                BaseAddress = new Uri(baseUrl),
                Timeout = timeout ?? TimeSpan.FromSeconds(30)
            };

            client.DefaultRequestHeaders.Add("User-Agent", "TelegramBotFramework/1.0");
            client.DefaultRequestHeaders.Add("Accept", "application/json");

            _httpClients[baseUrl] = client;
            return client;
        }
    }

    /// <summary>
    /// Creates a default HTTP client for Telegram API.
    /// </summary>
    public HttpClient GetTelegramClient()
    {
        const string telegramBaseUrl = "https://api.telegram.org";
        return GetClient(telegramBaseUrl, TimeSpan.FromSeconds(45));
    }

    /// <summary>
    /// Creates an HTTP client with custom headers.
    /// </summary>
    public HttpClient GetClientWithHeaders(string baseUrl, Dictionary<string, string> headers)
    {
        var client = GetClient(baseUrl);

        foreach (var header in headers)
        {
            // Remove existing header if present to avoid conflicts
            client.DefaultRequestHeaders.Remove(header.Key);
            client.DefaultRequestHeaders.Add(header.Key, header.Value);
        }

        return client;
    }

    /// <summary>
    /// Creates an HTTP client with authentication.
    /// </summary>
    public HttpClient GetClientWithAuth(string baseUrl, string authToken, string scheme = "Bearer")
    {
        var client = GetClient(baseUrl);
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(scheme, authToken);
        return client;
    }

    /// <summary>
    /// Disposes all cached HTTP clients.
    /// </summary>
    public void Dispose()
    {
        lock (_lockObj)
        {
            foreach (var client in _httpClients.Values)
            {
                client?.Dispose();
            }

            _httpClients.Clear();
        }
    }
}
