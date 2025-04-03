// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Integration;

using System.Text.Json;
using Utilities;

/// <summary>
/// Handles integration with external APIs for data enrichment and service calls.
/// Provides retry logic, timeout handling, and response parsing.
/// </summary>
public class ExternalApiIntegration
{
    private readonly HttpClientFactory _httpClientFactory;
    private readonly ILogger<ExternalApiIntegration> _logger;

    public ExternalApiIntegration(HttpClientFactory? httpClientFactory = null, ILogger<ExternalApiIntegration>? logger = null)
    {
        _httpClientFactory = httpClientFactory ?? new HttpClientFactory();
        _logger = logger ?? new ConsoleLogger<ExternalApiIntegration>();
    }

    /// <summary>
    /// Makes a GET request to an external API with retry logic.
    /// </summary>
    public async Task<T?> GetAsync<T>(string url, int maxRetries = 3)
    {
        if (string.IsNullOrWhiteSpace(url) || !ValidationUtility.IsValidUrl(url))
        {
            _logger.LogWarning("Invalid URL for external API call: {Url}", url);
            return default;
        }

        var uri = new Uri(url);
        var client = _httpClientFactory.GetClient(uri.Scheme + "://" + uri.Host);

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                var response = await client.GetAsync(uri.PathAndQuery);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonUtility.Deserialize<T>(content);
                }

                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests ||
                    response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                {
                    // Retry with exponential backoff
                    await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt - 1)));
                    continue;
                }

                _logger.LogWarning("External API returned error: {StatusCode}", response.StatusCode);
                return default;
            }
            catch (HttpRequestException ex) when (attempt < maxRetries)
            {
                _logger.LogWarning(ex, "Attempt {Attempt} failed for external API call, retrying...", attempt);
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt - 1)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling external API: {Url}", url);
                return default;
            }
        }

        _logger.LogError("External API call failed after {MaxRetries} retries: {Url}", maxRetries, url);
        return default;
    }

    /// <summary>
    /// Makes a POST request to an external API.
    /// </summary>
    public async Task<bool> PostAsync<TRequest>(string url, TRequest payload, string? apiKey = null)
    {
        if (string.IsNullOrWhiteSpace(url) || !ValidationUtility.IsValidUrl(url))
        {
            _logger.LogWarning("Invalid URL for external API call: {Url}", url);
            return false;
        }

        try
        {
            var uri = new Uri(url);
            HttpClient client = string.IsNullOrEmpty(apiKey)
                ? _httpClientFactory.GetClient(uri.Scheme + "://" + uri.Host)
                : _httpClientFactory.GetClientWithAuth(uri.Scheme + "://" + uri.Host, apiKey);

            var json = JsonUtility.Serialize(payload);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await client.PostAsync(uri.PathAndQuery, content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("External API POST succeeded: {Url}", url);
                return true;
            }

            _logger.LogWarning("External API POST failed: {Url}, Status: {StatusCode}", url, response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error posting to external API: {Url}", url);
            return false;
        }
    }

    /// <summary>
    /// Makes a request with custom headers.
    /// </summary>
    public async Task<string?> GetWithHeadersAsync(string url, Dictionary<string, string> headers)
    {
        if (string.IsNullOrWhiteSpace(url) || !ValidationUtility.IsValidUrl(url))
            return null;

        try
        {
            var uri = new Uri(url);
            var client = _httpClientFactory.GetClientWithHeaders(uri.Scheme + "://" + uri.Host, headers);
            var response = await client.GetAsync(uri.PathAndQuery);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsStringAsync();

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling external API with custom headers: {Url}", url);
            return null;
        }
    }

    /// <summary>
    /// Parses JSON response from external API.
    /// </summary>
    public static T? ParseResponse<T>(string jsonContent)
    {
        return JsonUtility.Deserialize<T>(jsonContent);
    }
}
