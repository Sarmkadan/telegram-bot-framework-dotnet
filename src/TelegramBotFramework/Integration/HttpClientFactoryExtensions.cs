using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Text;

namespace TelegramBotFramework.Integration
{
    public static class HttpClientFactoryExtensions
    {
        public static HttpClient GetClientWithCustomTimeout(string baseUrl, TimeSpan timeout)
        {
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

        public static HttpClient GetClientWithCustomHeaders(string baseUrl, Dictionary<string, string> headers)
        {
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