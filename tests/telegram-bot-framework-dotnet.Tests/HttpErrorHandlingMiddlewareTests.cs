using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using TelegramBotFramework.Middleware;
using Xunit;

namespace TelegramBotFramework.Tests;

public class HttpErrorHandlingMiddlewareTests
{
    private static HttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        // give the request a path so we can assert it later
        context.Request.Path = "/test/path";
        // set a deterministic TraceIdentifier for easier assertions
        context.TraceIdentifier = "trace-123";
        return context;
    }

    private static async Task<T> ReadResponseBodyAsync<T>(HttpResponse response)
    {
        response.Body.Seek(0, SeekOrigin.Begin);
        return await JsonSerializer.DeserializeAsync<T>(response.Body, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;
    }

    [Fact]
    public async Task InvokeAsync_CallsNext_WhenNoException()
    {
        // Arrange
        var called = false;
        RequestDelegate next = ctx =>
        {
            called = true;
            return Task.CompletedTask;
        };

        var middleware = new HttpErrorHandlingMiddleware(next, NullLogger<HttpErrorHandlingMiddleware>.Instance);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(called, "The next delegate should have been invoked.");
        Assert.Equal(200, context.Response.StatusCode); // default status code when nothing is set
        Assert.Equal(0, context.Response.Body.Length); // no body written
    }

    [Fact]
    public async Task InvokeAsync_ReturnsBadRequest_ForArgumentException()
    {
        // Arrange
        RequestDelegate next = _ => throw new ArgumentException("Invalid argument supplied");
        var middleware = new HttpErrorHandlingMiddleware(next, NullLogger<HttpErrorHandlingMiddleware>.Instance);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(400, context.Response.StatusCode);
        var error = await ReadResponseBodyAsync<HttpErrorResponse>(context.Response);
        Assert.Equal("INVALID_ARGUMENT", error.ErrorCode);
        Assert.Equal("Invalid argument supplied", error.Message);
        Assert.Equal("/test/path", error.Path);
        Assert.Equal("trace-123", error.TraceId);
        Assert.True(error.Timestamp <= DateTime.UtcNow && error.Timestamp > DateTime.UtcNow.AddMinutes(-1));
    }

    [Fact]
    public async Task InvokeAsync_MapsTimeoutException_ToRequestTimeout()
    {
        // Arrange
        RequestDelegate next = _ => throw new TimeoutException();
        var middleware = new HttpErrorHandlingMiddleware(next, NullLogger<HttpErrorHandlingMiddleware>.Instance);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(408, context.Response.StatusCode);
        var error = await ReadResponseBodyAsync<HttpErrorResponse>(context.Response);
        Assert.Equal("REQUEST_TIMEOUT", error.ErrorCode);
        Assert.Equal("Request processing timed out", error.Message);
    }

    [Fact]
    public void PropertyDefaults_AreInitializedCorrectly()
    {
        // Arrange
        RequestDelegate next = _ => Task.CompletedTask;
        var middleware = new HttpErrorHandlingMiddleware(next, NullLogger<HttpErrorHandlingMiddleware>.Instance);

        // Assert
        Assert.Equal(string.Empty, middleware.ErrorCode);
        Assert.Equal(string.Empty, middleware.Message);
        Assert.Equal(default(DateTime), middleware.Timestamp);
        Assert.Equal(string.Empty, middleware.Path);
        Assert.Equal(string.Empty, middleware.TraceId);
    }
}
