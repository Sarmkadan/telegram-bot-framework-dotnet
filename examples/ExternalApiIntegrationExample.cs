#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using TelegramBotFramework.Integration;

namespace TelegramBotFramework.Examples
{
    /// <summary>
    /// External API integration example demonstrating how to call third-party APIs,
    /// handle responses, implement retry logic, and manage timeouts.
    /// </summary>
public sealed class ExternalApiIntegrationExample
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ExternalApiIntegrationExample> _logger;
        private readonly ExternalApiIntegration _externalApi;
        private readonly HttpClientFactory _httpClientFactory;

        public ExternalApiIntegrationExample(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _logger = serviceProvider.GetRequiredService<ILogger<ExternalApiIntegrationExample>>();
            _externalApi = serviceProvider.GetRequiredService<ExternalApiIntegration>();
            _httpClientFactory = serviceProvider.GetRequiredService<HttpClientFactory>();
        }

        public async Task RunAsync()
        {
            _logger.LogInformation("Starting ExternalApiIntegrationExample");

            try
            {
                // Demonstrate various API integration patterns
                await FetchPublicApiDataAsync();
                await HandleApiErrorsAsync();
                await ImplementRetryLogicAsync();
                await CacheApiResponsesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ExternalApiIntegrationExample");
                throw;
            }
        }

        private async Task FetchPublicApiDataAsync()
        {
            _logger.LogInformation("--- Fetching Public API Data ---");

            try
            {
                // Example: Call a public API
                var httpClient = _httpClientFactory.GetHttpClient();

                // Example API call (using JSONPlaceholder for testing)
                var request = new HttpRequestMessage(HttpMethod.Get, "https://jsonplaceholder.typicode.com/users/1");
                var response = await httpClient.SendAsync(request, TimeSpan.FromSeconds(10));

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("API Response: {Content}", content);

                    // Parse JSON response
                    var jsonDoc = JsonDocument.Parse(content);
                    var name = jsonDoc.RootElement.GetProperty("name").GetString();
                    _logger.LogInformation("Parsed user name: {Name}", name);
                }
                else
                {
                    _logger.LogError("API call failed with status: {StatusCode}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching public API data");
            }
        }

        private async Task HandleApiErrorsAsync()
        {
            _logger.LogInformation("--- Handling API Errors ---");

            try
            {
                var httpClient = _httpClientFactory.GetHttpClient();

                // Example: Call endpoint that returns error
                var request = new HttpRequestMessage(HttpMethod.Get,
                    "https://jsonplaceholder.typicode.com/invalid-endpoint");

                var response = await httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    switch (response.StatusCode)
                    {
                        case System.Net.HttpStatusCode.NotFound:
                            _logger.LogWarning("Resource not found (404)");
                            break;

                        case System.Net.HttpStatusCode.Unauthorized:
                            _logger.LogWarning("Authentication failed (401)");
                            break;

                        case System.Net.HttpStatusCode.TooManyRequests:
                            _logger.LogWarning("Rate limited (429)");
                            break;

                        case System.Net.HttpStatusCode.InternalServerError:
                            _logger.LogError("Server error (500)");
                            break;

                        default:
                            _logger.LogError("API error: {StatusCode}", response.StatusCode);
                            break;
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed");
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "API request timeout");
            }
        }

        private async Task ImplementRetryLogicAsync()
        {
            _logger.LogInformation("--- Implementing Retry Logic ---");

            var maxRetries = 3;
            var retryDelay = TimeSpan.FromMilliseconds(100);
            var success = false;

            for (int i = 0; i < maxRetries && !success; i++)
            {
                try
                {
                    _logger.LogInformation("Attempt {Attempt} of {MaxRetries}", i + 1, maxRetries);

                    var httpClient = _httpClientFactory.GetHttpClient();
                    var request = new HttpRequestMessage(HttpMethod.Get,
                        "https://jsonplaceholder.typicode.com/users/1");

                    var response = await httpClient.SendAsync(request, TimeSpan.FromSeconds(5));

                    if (response.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("API call succeeded");
                        success = true;
                    }
                    else if (i < maxRetries - 1)
                    {
                        _logger.LogWarning("API call failed, retrying after {Delay}ms",
                            retryDelay.TotalMilliseconds);
                        await Task.Delay(retryDelay);
                        retryDelay = TimeSpan.FromMilliseconds(retryDelay.TotalMilliseconds * 2);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Attempt {Attempt} failed", i + 1);

                    if (i < maxRetries - 1)
                    {
                        _logger.LogWarning("Retrying after {Delay}ms", retryDelay.TotalMilliseconds);
                        await Task.Delay(retryDelay);
                        retryDelay = TimeSpan.FromMilliseconds(retryDelay.TotalMilliseconds * 2);
                    }
                    else
                    {
                        _logger.LogError("All retry attempts failed");
                    }
                }
            }
        }

        private async Task CacheApiResponsesAsync()
        {
            _logger.LogInformation("--- Caching API Responses ---");

            var cacheProvider = _serviceProvider.GetRequiredService<Caching.ICacheProvider>();

            const string cacheKey = "api:user:1";
            var cacheExpiration = TimeSpan.FromMinutes(5);

            // Try to get from cache first
            var cachedData = await cacheProvider.GetAsync(cacheKey);

            if (cachedData  is not null)
            {
                _logger.LogInformation("Retrieved from cache: {Data}", cachedData);
            }
            else
            {
                _logger.LogInformation("Cache miss, fetching from API");

                try
                {
                    var httpClient = _httpClientFactory.GetHttpClient();
                    var request = new HttpRequestMessage(HttpMethod.Get,
                        "https://jsonplaceholder.typicode.com/users/1");

                    var response = await httpClient.SendAsync(request);

                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();

                        // Cache the response
                        await cacheProvider.SetAsync(cacheKey, content, cacheExpiration);
                        _logger.LogInformation("Cached API response with {Expiration} TTL",
                            cacheExpiration.TotalMinutes);

                        _logger.LogInformation("API Response cached: {Response}", content);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error fetching and caching API response");
                }
            }
        }
    }
}