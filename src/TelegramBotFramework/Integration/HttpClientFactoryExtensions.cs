using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Text;

namespace TelegramBotFramework.Integration
{
    /// <summary>
    /// Provides factory methods for creating pre-configured <see cref="HttpClient"/> instances.
    /// </summary>
    public static class HttpClientFactoryExtensions
    {
        /// <summary>
        /// Creates an <see cref="HttpClient"/> with the specified base address and request timeout.
        /// </summary>
        /// <param name="baseUrl">The base URL used for relative request addresses.</param>
        /// <param name="timeout">The timeout applied to each request.</param>
        /// <returns>A new <see cref="HttpClient"/> configured with the given base address and timeout.</returns>
        public static HttpClient GetClientWithCustomTimeout(string baseUrl, TimeSpan timeout)
        {
            ArgumentNullException.ThrowIfNull(baseUrl);

            return new HttpClient(new SocketsHttpHandler
            {
                PooledConnectionLifetime = TimeSpan.FromMinutes(2),
                PooledConnectionIdleTimeout = TimeSpan.FromMinutes(1),
                AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
            })
            {
                BaseAddress = new Uri(baseUrl),
                Timeout = timeout
            };
        }

        /// <summary>
        /// Creates an <see cref="HttpClient"/> with the specified base address and default request headers.
        /// </summary>
        /// <param name="baseUrl">The base URL used for relative request addresses.</param>
        /// <param name="headers">The default headers to add to the client. An existing header with the same name is replaced.</param>
        /// <returns>A new <see cref="HttpClient"/> configured with the given base address and headers.</returns>
        public static HttpClient GetClientWithCustomHeaders(string baseUrl, Dictionary<string, string> headers)
        {
            ArgumentNullException.ThrowIfNull(baseUrl);
            ArgumentNullException.ThrowIfNull(headers);

            var client = new HttpClient(new SocketsHttpHandler
            {
                PooledConnectionLifetime = TimeSpan.FromMinutes(2),
                PooledConnectionIdleTimeout = TimeSpan.FromMinutes(1),
                AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
            })
            {
                BaseAddress = new Uri(baseUrl)
            };

            foreach (var header in headers)
            {
                client.DefaultRequestHeaders.Remove(header.Key);
                client.DefaultRequestHeaders.Add(header.Key, header.Value);
            }

            return client;
        }
    }
}