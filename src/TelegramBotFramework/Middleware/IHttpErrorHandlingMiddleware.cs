#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace TelegramBotFramework.Middleware
{
    /// <summary>
    /// Interface for HTTP error handling middleware.
    /// </summary>
    public interface IHttpErrorHandlingMiddleware
    {
        string ErrorCode { get; set; }
        string Message { get; set; }
        DateTime Timestamp { get; set; }
        string Path { get; set; }
        string TraceId { get; set; }
        Task InvokeAsync(HttpContext context);
    }
}