#nullable enable
namespace TelegramBotFramework.Integration;

using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Defines the contract for external API integration services.
/// </summary>
public interface IExternalApiIntegration
{
    /// <summary>
    /// Makes a GET request to an external API with retry logic.
    /// </summary>
    Task<T?> GetAsync<T>(string url, int maxRetries = 3);

    /// <summary>
    /// Makes a POST request to an external API.
    /// </summary>
    Task<bool> PostAsync<TRequest>(string url, TRequest payload, string? apiKey = null);

    /// <summary>
    /// Makes a request with custom headers.
    /// </summary>
    Task<string?> GetWithHeadersAsync(string url, Dictionary<string, string> headers);
}