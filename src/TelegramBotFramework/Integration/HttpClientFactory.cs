#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Integration;

/// <summary>
/// Factory for creating and managing HTTP clients with pre-configured settings.
/// Handles connection pooling, timeouts, and retry policies consistently.
/// </summary>
public sealed class HttpClientFactory
{
    private readonly Dictionary<string, HttpClient> _httpClients = new();
    private readonly object _lockObj = new();

    /// <summary>
    /// Gets or creates an HttpClient for a specific base URL.
    /// </summary>
    public HttpClient GetClient(string baseUrl, TimeSpan? timeout = null)
    {
        return GetClientCore(baseUrl, baseUrl, timeout, configure: null);
    }

    private HttpClient GetClientCore(string cacheKey, string baseUrl, TimeSpan? timeout, Action<HttpClient>? configure)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
            throw new ArgumentException("Base URL cannot be empty", nameof(baseUrl));

        lock (_lockObj)
        {
            if (_httpClients.TryGetValue(cacheKey, out var existingClient))
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

            configure?.Invoke(client);

            _httpClients[cacheKey] = client;
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
        ArgumentNullException.ThrowIfNull(headers);

        // Header sets get their own cache entry: mutating the DefaultRequestHeaders
        // of the shared per-base-URL client is not thread-safe and would leak the
        // headers into every other consumer of that base URL.
        var fingerprint = string.Join("\n", headers.OrderBy(h => h.Key, StringComparer.Ordinal)
            .Select(h => h.Key + ":" + h.Value));
        var cacheKey = baseUrl + "|headers|" + fingerprint;

        return GetClientCore(cacheKey, baseUrl, timeout: null, configure: client =>
        {
            foreach (var header in headers)
            {
                client.DefaultRequestHeaders.Remove(header.Key);
                client.DefaultRequestHeaders.Add(header.Key, header.Value);
            }
        });
    }

    /// <summary>
    /// Creates an HTTP client with authentication.
    /// </summary>
    public HttpClient GetClientWithAuth(string baseUrl, string authToken, string scheme = "Bearer")
    {
        if (string.IsNullOrWhiteSpace(authToken))
            throw new ArgumentException("Auth token cannot be empty", nameof(authToken));

        // Authenticated clients are cached separately per credential so the token
        // never bleeds into the unauthenticated shared client for the same host.
        var cacheKey = baseUrl + "|auth|" + scheme + "|" + authToken;

        return GetClientCore(cacheKey, baseUrl, timeout: null, configure: client =>
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(scheme, authToken));
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