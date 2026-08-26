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
public interface IHttpClientFactory
{
    /// <summary>
    /// Gets or creates an HttpClient for a specific base URL.
    /// </summary>
    HttpClient GetClient(string baseUrl, TimeSpan? timeout = null);

    /// <summary>
    /// Creates a default HTTP client for Telegram API.
    /// </summary>
    HttpClient GetTelegramClient();

    /// <summary>
    /// Creates an HTTP client with custom headers.
    /// </summary>
    HttpClient GetClientWithHeaders(string baseUrl, Dictionary<string, string> headers);

    /// <summary>
    /// Creates an HTTP client with authentication.
    /// </summary>
    HttpClient GetClientWithAuth(string baseUrl, string authToken, string scheme = "Bearer");

    /// <summary>
    /// Disposes all cached HTTP clients.
    /// </summary>
    void Dispose();
}