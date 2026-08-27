#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using Microsoft.AspNetCore.Http;

namespace TelegramBotFramework.Middleware
{
    /// <summary>
    /// Fluent builder for <see cref="HttpErrorHandlingMiddleware"/> that validates on build.
    /// </summary>
    public sealed class HttpErrorHandlingMiddlewareBuilder
    {
        private readonly HttpErrorHandlingMiddleware _middleware;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpErrorHandlingMiddlewareBuilder"/> class.
        /// </summary>
        /// <param name="middleware">The middleware to configure.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="middleware"/> is null.</exception>
        public HttpErrorHandlingMiddlewareBuilder(HttpErrorHandlingMiddleware middleware)
        {
            ArgumentNullException.ThrowIfNull(middleware);
            _middleware = middleware;
        }

        /// <summary>
        /// Pre-fills the builder from an existing <see cref="HttpErrorHandlingMiddleware"/> instance.
        /// </summary>
        /// <param name="template">The middleware to copy from.</param>
        /// <returns>The builder instance for chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="template"/> is null.</exception>
        public static HttpErrorHandlingMiddlewareBuilder From(HttpErrorHandlingMiddleware template)
        {
            ArgumentNullException.ThrowIfNull(template);
            return new HttpErrorHandlingMiddlewareBuilder(template);
        }

        /// <summary>
        /// Sets the error code.
        /// </summary>
        /// <param name="errorCode">The error code.</param>
        /// <returns>The builder instance for chaining.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="errorCode"/> is null or empty.</exception>
        public HttpErrorHandlingMiddlewareBuilder WithErrorCode(string errorCode)
        {
            ArgumentException.ThrowIfNullOrEmpty(errorCode);
            _middleware.ErrorCode = errorCode;
            return this;
        }

        /// <summary>
        /// Sets the error message.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <returns>The builder instance for chaining.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="message"/> is null or empty.</exception>
        public HttpErrorHandlingMiddlewareBuilder WithMessage(string message)
        {
            ArgumentException.ThrowIfNullOrEmpty(message);
            _middleware.Message = message;
            return this;
        }

        /// <summary>
        /// Sets the timestamp.
        /// </summary>
        /// <param name="timestamp">The timestamp.</param>
        /// <returns>The builder instance for chaining.</returns>
        public HttpErrorHandlingMiddlewareBuilder WithTimestamp(DateTime timestamp)
        {
            _middleware.Timestamp = timestamp;
            return this;
        }

        /// <summary>
        /// Sets the request path.
        /// </summary>
        /// <param name="path">The request path.</param>
        /// <returns>The builder instance for chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="path"/> is null.</exception>
        public HttpErrorHandlingMiddlewareBuilder WithPath(string? path)
        {
            ArgumentNullException.ThrowIfNull(path);
            _middleware.Path = path;
            return this;
        }

        /// <summary>
        /// Sets the trace identifier.
        /// </summary>
        /// <param name="traceId">The trace identifier.</param>
        /// <returns>The builder instance for chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="traceId"/> is null.</exception>
        public HttpErrorHandlingMiddlewareBuilder WithTraceId(string? traceId)
        {
            ArgumentNullException.ThrowIfNull(traceId);
            _middleware.TraceId = traceId;
            return this;
        }

        /// <summary>
        /// Builds and returns the configured <see cref="HttpErrorHandlingMiddleware"/> instance.
        /// </summary>
        /// <returns>The configured <see cref="HttpErrorHandlingMiddleware"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when required properties are missing.</exception>
        public HttpErrorHandlingMiddleware Build()
        {
            ArgumentException.ThrowIfNullOrEmpty(_middleware.ErrorCode);
            ArgumentException.ThrowIfNullOrEmpty(_middleware.Message);
            return _middleware;
        }
    }
}