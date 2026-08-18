using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TelegramBotFramework.Integration
{
    /// <summary>
    /// Extension methods that add convenient helpers for <see cref="ExternalApiIntegration"/>.
    /// </summary>
    public static class ExternalApiIntegrationExtensions
    {
        /// <summary>
        /// Retrieves the response body as a string.
        /// </summary>
        /// <param name="api">The <see cref="ExternalApiIntegration"/> instance.</param>
        /// <param name="url">The request URL.</param>
        /// <returns>The response body, or <c>null</c> if the request fails or returns no content.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="api"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="url"/> is <c>null</c> or whitespace.</exception>
        public static async Task<string?> GetStringAsync(this ExternalApiIntegration api, string url)
        {
            ArgumentNullException.ThrowIfNull(api);
            ArgumentException.ThrowIfNullOrEmpty(url);

            // Re‑use the generic GET method with <string> as the expected type.
            return await api.GetAsync<string>(url);
        }

        /// <summary>
        /// Retrieves a JSON response as a raw string and parses it into the specified type using
        /// <see cref="ExternalApiIntegration.ParseResponse{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the response into.</typeparam>
        /// <param name="api">The <see cref="ExternalApiIntegration"/> instance.</param>
        /// <param name="url">The request URL.</param>
        /// <returns>An instance of <typeparamref name="T"/> if parsing succeeds; otherwise <c>null</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="api"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="url"/> is <c>null</c> or whitespace.</exception>
        public static async Task<T?> GetAndParseAsync<T>(this ExternalApiIntegration api, string url)
        {
            ArgumentNullException.ThrowIfNull(api);
            ArgumentException.ThrowIfNullOrEmpty(url);

            var raw = await api.GetAsync<string>(url);
            return raw is null ? default : ExternalApiIntegration.ParseResponse<T>(raw);
        }

        /// <summary>
        /// Sends a POST request with the supplied payload and returns the boolean result from the underlying
        /// <see cref="ExternalApiIntegration.PostAsync{TRequest}"/> method.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request payload.</typeparam>
        /// <param name="api">The <see cref="ExternalApiIntegration"/> instance.</param>
        /// <param name="url">The request URL.</param>
        /// <param name="request">The request payload.</param>
        /// <returns><c>true</c> if the POST succeeded according to the underlying implementation; otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="api"/> or <paramref name="request"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="url"/> is <c>null</c> or whitespace.</exception>
        public static async Task<bool> PostAndVerifyAsync<TRequest>(this ExternalApiIntegration api, string url, TRequest request)
        {
            ArgumentNullException.ThrowIfNull(api);
            ArgumentException.ThrowIfNullOrEmpty(url);
            ArgumentNullException.ThrowIfNull(request);

            return await api.PostAsync<TRequest>(url, request);
        }

        /// <summary>
        /// Executes multiple GET requests in parallel and returns the collection of results.
        /// </summary>
        /// <typeparam name="T">The expected type of each GET response.</typeparam>
        /// <param name="api">The <see cref="ExternalApiIntegration"/> instance.</param>
        /// <param name="urls">A collection of URLs to request.</param>
        /// <returns>An <see cref="IReadOnlyList{T}"/> containing the results of each request, preserving order.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="api"/> or <paramref name="urls"/> is <c>null</c>.</exception>
        public static async Task<IReadOnlyList<T?>> GetMultipleAsync<T>(this ExternalApiIntegration api, IEnumerable<string> urls)
        {
            ArgumentNullException.ThrowIfNull(api);
            ArgumentNullException.ThrowIfNull(urls);

            var tasks = urls.Select(url => api.GetAsync<T>(url));
            var results = await Task.WhenAll(tasks);
            return results;
        }
    }
}
