#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// Tests for RateLimitingMiddleware class
// =============================================================================

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TelegramBotFramework.Models;
using TelegramBotFramework.Services;
using TelegramBotFramework.Strategies;
using ExecutionContext = TelegramBotFramework.Models.ExecutionContext;
using Xunit;

namespace TelegramBotFramework.Middleware.Tests;

/// <summary>
/// Tests for the RateLimitingMiddleware class.
/// </summary>
public sealed class RateLimitingMiddlewareTests : IRateLimitingMiddlewareTests
{
    private readonly Mock<ICommandService> _commandServiceMock;
    private readonly Mock<ILogger<RateLimitingMiddleware>> _loggerMock;
    private readonly BotConfiguration _configuration;
    private readonly FixedWindowStrategy _rateLimitingStrategy;

    public RateLimitingMiddlewareTests()
    {
        _commandServiceMock = new Mock<ICommandService>();
        _loggerMock = new Mock<ILogger<RateLimitingMiddleware>>();
        _configuration = new BotConfiguration
        {
            EnableRateLimiting = true,
            RateLimitPerMinute = 3,
            AdminIds = new List<long> { 999 }
        };
        _rateLimitingStrategy = new FixedWindowStrategy(3, TimeSpan.FromMinutes(1));
    }

    /// <summary>
    /// Tests that middleware with rate limiting disabled passes context to next.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_WhenRateLimitingDisabled_PassesToNext()
    {
        _loggerMock.Object.LogInformation("Starting disabled rate limiting test with rate limiting enabled: {EnableRateLimiting}", _configuration.EnableRateLimiting);

        // Arrange
        _configuration.EnableRateLimiting = false;
        _loggerMock.Object.LogWarning("Rate limiting is disabled; verifying fallback processing for user {UserId}", 123L);
        var middleware = new RateLimitingMiddleware(
            _commandServiceMock.Object,
            _configuration,
            _rateLimitingStrategy,
            _loggerMock.Object
        );

        var context = new ExecutionContext
        {
            UserId = 123,
            ChatId = 456,
            User = new BotUser { TelegramId = 123, FirstName = "Test" },
            IsValid = true
        };

        var nextCalled = false;
        Task<TelegramBotFramework.Models.ExecutionContext> Next(TelegramBotFramework.Models.ExecutionContext ctx)
        {
            nextCalled = true;
            return Task.FromResult(ctx);
        }

        // Act
        var result = await middleware.ProcessAsync(context, Next, CancellationToken.None);

        // Assert
        result.Should().BeSameAs(context);
        nextCalled.Should().BeTrue();
        _loggerMock.Invocations.Should().NotContain(x => x.Arguments.Any(a =>
            a.ToString() != null && a.ToString().Contains("RateLimitingMiddleware")));
        _loggerMock.Object.LogInformation("Completed disabled rate limiting test for user {UserId}; next called: {NextCalled}", context.UserId, nextCalled);
    }

    /// <summary>
    /// Tests that middleware with invalid context passes to next.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_WhenContextInvalid_PassesToNext()
    {
        _loggerMock.Object.LogInformation("Starting invalid context test for user {UserId} and chat {ChatId}", 0L, 456L);

        // Arrange
        var middleware = new RateLimitingMiddleware(
            _commandServiceMock.Object,
            _configuration,
            _rateLimitingStrategy,
            _loggerMock.Object
        );

        var context = new ExecutionContext
        {
            UserId = 0, // Invalid
            ChatId = 456,
            User = new BotUser { TelegramId = 123, FirstName = "Test" },
            IsValid = false
        };

        var nextCalled = false;
        Task<TelegramBotFramework.Models.ExecutionContext> Next(TelegramBotFramework.Models.ExecutionContext ctx)
        {
            nextCalled = true;
            return Task.FromResult(ctx);
        }

        // Act
        _loggerMock.Object.LogWarning("Processing invalid context through fallback path for user {UserId}", context.UserId);
        var result = await middleware.ProcessAsync(context, Next, CancellationToken.None);

        // Assert
        result.Should().BeSameAs(context);
        nextCalled.Should().BeTrue();
        _loggerMock.Object.LogInformation("Completed invalid context test for user {UserId}; next called: {NextCalled}", context.UserId, nextCalled);
    }

    /// <summary>
    /// Tests that middleware with null user logs warning and passes to next.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_WhenUserNull_LogsWarningAndPassesToNext()
    {
        _loggerMock.Object.LogInformation("Starting missing user test for user {UserId} and chat {ChatId}", 123L, 456L);

        // Arrange
        var middleware = new RateLimitingMiddleware(
            _commandServiceMock.Object,
            _configuration,
            _rateLimitingStrategy,
            _loggerMock.Object
        );

        var context = new ExecutionContext
        {
            UserId = 123,
            ChatId = 456,
            User = null,
            IsValid = true
        };

        var nextCalled = false;
        Task<TelegramBotFramework.Models.ExecutionContext> Next(TelegramBotFramework.Models.ExecutionContext ctx)
        {
            nextCalled = true;
            return Task.FromResult(ctx);
        }

        // Act
        _loggerMock.Object.LogWarning("User is missing from context for user {UserId}; verifying fallback processing", context.UserId);
        var result = await middleware.ProcessAsync(context, Next, CancellationToken.None);

        // Assert
        result.Should().BeSameAs(context);
        nextCalled.Should().BeTrue();
        _loggerMock.Invocations.Should().Contain(x => x.Arguments.Any(a =>
            a.ToString() != null && a.ToString().Contains("User not found")));
        _loggerMock.Object.LogInformation("Completed missing user test for user {UserId}; next called: {NextCalled}", context.UserId, nextCalled);
    }

    /// <summary>
    /// Tests that admin user bypasses rate limiting.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_WhenUserIsAdmin_BypassesRateLimit()
    {
        _loggerMock.Object.LogInformation("Starting admin bypass test for user {UserId}", 999L);

        // Arrange
        var middleware = new RateLimitingMiddleware(
            _commandServiceMock.Object,
            _configuration,
            _rateLimitingStrategy,
            _loggerMock.Object
        );

        var context = new ExecutionContext
        {
            UserId = 999,
            ChatId = 456,
            User = new BotUser { TelegramId = 999, FirstName = "Admin" },
            IsValid = true
        };

        var nextCalled = false;
        Task<TelegramBotFramework.Models.ExecutionContext> Next(TelegramBotFramework.Models.ExecutionContext ctx)
        {
            nextCalled = true;
            return Task.FromResult(ctx);
        }

        // Act
        _loggerMock.Object.LogWarning("Admin user {UserId} is using the rate limit bypass path", context.UserId);
        var result = await middleware.ProcessAsync(context, Next, CancellationToken.None);

        // Assert
        result.Should().BeSameAs(context);
        nextCalled.Should().BeTrue();
        _loggerMock.Invocations.Should().Contain(x => x.Arguments.Any(a =>
            a.ToString() != null && a.ToString().Contains("is admin, bypassing rate limit")));
        _loggerMock.Object.LogInformation("Completed admin bypass test for user {UserId}; next called: {NextCalled}", context.UserId, nextCalled);
    }

    /// <summary>
    /// Tests that user under rate limit passes to next middleware.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_WhenUnderRateLimit_PassesToNext()
    {
        _loggerMock.Object.LogInformation("Starting under rate limit test with limit {RateLimitPerMinute}", _configuration.RateLimitPerMinute);

        // Arrange
        var middleware = new RateLimitingMiddleware(
            _commandServiceMock.Object,
            _configuration,
            _rateLimitingStrategy,
            _loggerMock.Object
        );

        var context = new ExecutionContext
        {
            UserId = 123,
            ChatId = 456,
            User = new BotUser { TelegramId = 123, FirstName = "User" },
            IsValid = true
        };

        var nextCalled = false;
        Task<TelegramBotFramework.Models.ExecutionContext> Next(TelegramBotFramework.Models.ExecutionContext ctx)
        {
            nextCalled = true;
            return Task.FromResult(ctx);
        }

        // Act - first request should pass
        var result = await middleware.ProcessAsync(context, Next, CancellationToken.None);

        // Assert
        result.Should().BeSameAs(context);
        nextCalled.Should().BeTrue();
        context.Errors.Should().BeEmpty();
        context.IsValid.Should().BeTrue();
        _loggerMock.Object.LogInformation("Completed under rate limit test for user {UserId}; context valid: {IsValid}", context.UserId, context.IsValid);
    }

    /// <summary>
    /// Tests that user over rate limit gets blocked and error is added.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_WhenOverRateLimit_BlocksAndAddsError()
    {
        _loggerMock.Object.LogInformation("Starting exceeded rate limit test with limit {RateLimitPerMinute}", _configuration.RateLimitPerMinute);

        // Arrange - exhaust rate limit
        var middleware = new RateLimitingMiddleware(
            _commandServiceMock.Object,
            _configuration,
            _rateLimitingStrategy,
            _loggerMock.Object
        );

        var context = new ExecutionContext
        {
            UserId = 123,
            ChatId = 456,
            User = new BotUser { TelegramId = 123, FirstName = "User" },
            IsValid = true
        };

        // First 3 requests should pass
        var nextCalled1 = false;
        Task<ExecutionContext> Next1(ExecutionContext ctx)
        {
            nextCalled1 = true;
            return Task.FromResult(ctx);
        }
        await middleware.ProcessAsync(context, Next1, CancellationToken.None);
        nextCalled1.Should().BeTrue();

        var nextCalled2 = false;
        Task<ExecutionContext> Next2(ExecutionContext ctx)
        {
            nextCalled2 = true;
            return Task.FromResult(ctx);
        }
        await middleware.ProcessAsync(context, Next2, CancellationToken.None);
        nextCalled2.Should().BeTrue();

        var nextCalled3 = false;
        Task<ExecutionContext> Next3(ExecutionContext ctx)
        {
            nextCalled3 = true;
            return Task.FromResult(ctx);
        }
        await middleware.ProcessAsync(context, Next3, CancellationToken.None);
        nextCalled3.Should().BeTrue();

        // Fourth request should be blocked
        var nextCalled4 = false;
        Task<ExecutionContext> Next4(ExecutionContext ctx)
        {
            nextCalled4 = true;
            return Task.FromResult(ctx);
        }

        // Act
        _loggerMock.Object.LogWarning("Submitting additional request for user {UserId} after {RequestCount} allowed requests", context.UserId, 3);
        var result = await middleware.ProcessAsync(context, Next4, CancellationToken.None);

        // Assert
        result.Should().BeSameAs(context);
        nextCalled4.Should().BeFalse(); // next should not be called
        context.Errors.Should().ContainSingle(e => e.Contains("Rate limit exceeded"));
        context.IsValid.Should().BeFalse();
        _loggerMock.Invocations.Should().Contain(x => x.Arguments.Any(a =>
            a.ToString() != null && a.ToString().Contains("exceeded rate limit")));
        _loggerMock.Object.LogInformation("Completed exceeded rate limit test for user {UserId}; context valid: {IsValid}", context.UserId, context.IsValid);
    }

    /// <summary>
    /// Tests that different users are limited independently.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_DifferentUsersLimitedIndependently()
    {
        _loggerMock.Object.LogInformation("Starting independent user limits test for users {FirstUserId} and {SecondUserId}", 111L, 222L);

        // Arrange
        var middleware = new RateLimitingMiddleware(
            _commandServiceMock.Object,
            _configuration,
            _rateLimitingStrategy,
            _loggerMock.Object
        );

        var user1Context = new ExecutionContext
        {
            UserId = 111,
            ChatId = 456,
            User = new BotUser { TelegramId = 111, FirstName = "User1" },
            IsValid = true
        };

        var user2Context = new ExecutionContext
        {
            UserId = 222,
            ChatId = 789,
            User = new BotUser { TelegramId = 222, FirstName = "User2" },
            IsValid = true
        };

        // User 1 exhausts limit
        Task<ExecutionContext> Next1(ExecutionContext ctx)
        {
            return Task.FromResult(ctx);
        }
        await middleware.ProcessAsync(user1Context, Next1, CancellationToken.None);
        await middleware.ProcessAsync(user1Context, Next1, CancellationToken.None);
        await middleware.ProcessAsync(user1Context, Next1, CancellationToken.None);

        // User 2 should still be able to make requests
        var next2Called = false;
        Task<ExecutionContext> Next2(ExecutionContext ctx)
        {
            next2Called = true;
            return Task.FromResult(ctx);
        }

        // Act
        var result = await middleware.ProcessAsync(user2Context, Next2, CancellationToken.None);

        // Assert
        result.Should().BeSameAs(user2Context);
        next2Called.Should().BeTrue();
        user2Context.Errors.Should().BeEmpty();
        user2Context.IsValid.Should().BeTrue();
        _loggerMock.Object.LogInformation("Completed independent user limits test for user {UserId}; context valid: {IsValid}", user2Context.UserId, user2Context.IsValid);
    }

    /// <summary>
    /// Tests that rate limiting works with TokenBucketStrategy.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_WithTokenBucketStrategy_WorksCorrectly()
    {
        _loggerMock.Object.LogInformation("Starting token bucket strategy test with capacity {Capacity} and refill rate {RefillRate}", 5, 1);

        // Arrange
        var tokenBucketStrategy = new TokenBucketStrategy(5, 1); // 5 tokens, 1 token per second
        var middleware = new RateLimitingMiddleware(
            _commandServiceMock.Object,
            _configuration,
            tokenBucketStrategy,
            _loggerMock.Object
        );

        var context = new ExecutionContext
        {
            UserId = 333,
            ChatId = 456,
            User = new BotUser { TelegramId = 333, FirstName = "User" },
            IsValid = true
        };

        // First 5 requests should pass
        for (int i = 0; i < 5; i++)
        {
            var nextCalled = false;
            Task<ExecutionContext> Next(ExecutionContext ctx)
            {
                nextCalled = true;
                return Task.FromResult(ctx);
            }
            var result = await middleware.ProcessAsync(context, Next, CancellationToken.None);
            nextCalled.Should().BeTrue();
        }

        // Sixth request should be blocked
        var nextCalled6 = false;
        Task<ExecutionContext> Next6(ExecutionContext ctx)
        {
            nextCalled6 = true;
            return Task.FromResult(ctx);
        }

        // Act
        _loggerMock.Object.LogWarning("Submitting request {RequestNumber} after token bucket capacity is exhausted for user {UserId}", 6, context.UserId);
        var result6 = await middleware.ProcessAsync(context, Next6, CancellationToken.None);

        // Assert
        result6.Should().BeSameAs(context);
        nextCalled6.Should().BeFalse();
        context.Errors.Should().ContainSingle(e => e.Contains("Rate limit exceeded"));
        _loggerMock.Object.LogInformation("Completed token bucket strategy test for user {UserId}; next called: {NextCalled}", context.UserId, nextCalled6);
    }

    /// <summary>
    /// Tests that rate limiting works with SlidingWindowStrategy.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_WithSlidingWindowStrategy_WorksCorrectly()
    {
        _loggerMock.Object.LogInformation("Starting sliding window strategy test with request limit {RequestLimit} and window minutes {WindowMinutes}", 3, 1);

        // Arrange
        var slidingWindowStrategy = new SlidingWindowStrategy(3, TimeSpan.FromMinutes(1));
        var middleware = new RateLimitingMiddleware(
            _commandServiceMock.Object,
            _configuration,
            slidingWindowStrategy,
            _loggerMock.Object
        );

        var context = new ExecutionContext
        {
            UserId = 444,
            ChatId = 456,
            User = new BotUser { TelegramId = 444, FirstName = "User" },
            IsValid = true
        };

        // First 3 requests should pass
        for (int i = 0; i < 3; i++)
        {
            var nextCalled = false;
            Task<ExecutionContext> Next(ExecutionContext ctx)
            {
                nextCalled = true;
                return Task.FromResult(ctx);
            }
            var result = await middleware.ProcessAsync(context, Next, CancellationToken.None);
            nextCalled.Should().BeTrue();
        }

        // Fourth request should be blocked
        var nextCalled4 = false;
        Task<ExecutionContext> Next4(ExecutionContext ctx)
        {
            nextCalled4 = true;
            return Task.FromResult(ctx);
        }

        // Act
        _loggerMock.Object.LogWarning("Submitting request {RequestNumber} after sliding window limit is exhausted for user {UserId}", 4, context.UserId);
        var result4 = await middleware.ProcessAsync(context, Next4, CancellationToken.None);

        // Assert
        result4.Should().BeSameAs(context);
        nextCalled4.Should().BeFalse();
        context.Errors.Should().ContainSingle(e => e.Contains("Rate limit exceeded"));
        _loggerMock.Object.LogInformation("Completed sliding window strategy test for user {UserId}; next called: {NextCalled}", context.UserId, nextCalled4);
    }

    /// <summary>
    /// Tests middleware priority.
    /// </summary>
    [Fact]
    public void Priority_ReturnsCorrectValue()
    {
        _loggerMock.Object.LogInformation("Starting middleware priority test with expected priority {ExpectedPriority}", 20);

        // Arrange
        var middleware = new RateLimitingMiddleware(
            _commandServiceMock.Object,
            _configuration,
            _rateLimitingStrategy,
            _loggerMock.Object
        );

        // Act & Assert
        middleware.Priority.Should().Be(20);
        _loggerMock.Object.LogInformation("Completed middleware priority test with actual priority {ActualPriority}", middleware.Priority);
    }
}
