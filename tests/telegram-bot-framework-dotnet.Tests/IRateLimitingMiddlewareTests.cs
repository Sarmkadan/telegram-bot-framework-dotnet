#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// Interface for RateLimitingMiddlewareTests
// =============================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
using TelegramBotFramework.Models;

namespace TelegramBotFramework.Middleware.Tests;

/// <summary>
/// Interface for RateLimitingMiddlewareTests.
/// </summary>
public interface IRateLimitingMiddlewareTests
{
    Task ProcessAsync_WhenRateLimitingDisabled_PassesToNext();
    Task ProcessAsync_WhenContextInvalid_PassesToNext();
    Task ProcessAsync_WhenUserNull_LogsWarningAndPassesToNext();
    Task ProcessAsync_WhenUserIsAdmin_BypassesRateLimit();
    Task ProcessAsync_WhenUnderRateLimit_PassesToNext();
    Task ProcessAsync_WhenOverRateLimit_BlocksAndAddsError();
    Task ProcessAsync_DifferentUsersLimitedIndependently();
    Task ProcessAsync_WithTokenBucketStrategy_WorksCorrectly();
    Task ProcessAsync_WithSlidingWindowStrategy_WorksCorrectly();
    void Priority_ReturnsCorrectValue();
}