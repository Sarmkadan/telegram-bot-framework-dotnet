#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// Tests for InMemoryRateLimitingStrategy class
// =============================================================================

using FluentAssertions;
using Xunit;

namespace TelegramBotFramework.Strategies.Tests;

/// <summary>
/// Tests for the InMemoryRateLimitingStrategy class.
/// </summary>
public sealed class InMemoryRateLimitingStrategyTests : IInMemoryRateLimitingStrategyTests
{
    /// <summary>
    /// Tests that requests exactly at the window boundary are properly expired.
    /// This is a regression test for the boundary condition bug where requests
    /// at exactly the window edge (e.g., exactly 1 minute old) were not removed,
    /// allowing bursts at window edges to exceed the rate limit.
    /// </summary>
    [Fact]
    public void IsRequestAllowed_RequestsAtWindowBoundaryAreExpired()
    {
        // Arrange
        var strategy = new InMemoryRateLimitingStrategy();
        var identifier = "test-user";

        // Fill the rate limit
        for (int i = 0; i < 30; i++)
        {
            strategy.IsRequestAllowed(identifier).Should().BeTrue();
        }

        // 31st request should be blocked
        strategy.IsRequestAllowed(identifier).Should().BeFalse();

        // Wait for requests to expire (exactly 1 minute)
        Thread.Sleep(TimeSpan.FromMinutes(1));

        // After waiting exactly 1 minute, all previous requests should be expired
        // and new requests should be allowed again
        strategy.IsRequestAllowed(identifier).Should().BeTrue();
    }

    /// <summary>
    /// Tests that requests just inside the window are not expired.
    /// </summary>
    [Fact]
    public void IsRequestAllowed_RequestsInsideWindowAreNotExpired()
    {
        // Arrange
        var strategy = new InMemoryRateLimitingStrategy();
        var identifier = "test-user";

        // Make 30 requests to fill the limit
        for (int i = 0; i < 30; i++)
        {
            strategy.IsRequestAllowed(identifier).Should().BeTrue();
        }

        // 31st request should be blocked
        strategy.IsRequestAllowed(identifier).Should().BeFalse();

        // Wait for 59 seconds (just inside the 1 minute window)
        Thread.Sleep(TimeSpan.FromSeconds(59));

        // Request should still be blocked (not expired yet)
        strategy.IsRequestAllowed(identifier).Should().BeFalse();
    }

    /// <summary>
    /// Tests that requests just outside the window are expired.
    /// </summary>
    [Fact]
    public void IsRequestAllowed_RequestsOutsideWindowAreExpired()
    {
        // Arrange
        var strategy = new InMemoryRateLimitingStrategy();
        var identifier = "test-user";

        // Make 30 requests to fill the limit
        for (int i = 0; i < 30; i++)
        {
            strategy.IsRequestAllowed(identifier).Should().BeTrue();
        }

        // 31st request should be blocked
        strategy.IsRequestAllowed(identifier).Should().BeFalse();

        // Wait for 61 seconds (just outside the 1 minute window)
        Thread.Sleep(TimeSpan.FromSeconds(61));

        // Request should be expired and new request should be allowed
        strategy.IsRequestAllowed(identifier).Should().BeTrue();
    }

    /// <summary>
    /// Tests that GetRemainingRequests correctly handles boundary conditions.
    /// </summary>
    [Fact]
    public void GetRemainingRequests_HandlesBoundaryConditionsCorrectly()
    {
        // Arrange
        var strategy = new InMemoryRateLimitingStrategy();
        var identifier = "test-user";

        // Fill the rate limit
        for (int i = 0; i < 30; i++)
        {
            strategy.IsRequestAllowed(identifier);
        }

        // Should have 0 remaining
        strategy.GetRemainingRequests(identifier).Should().Be(0);

        // Wait for requests to expire (exactly 1 minute)
        Thread.Sleep(TimeSpan.FromMinutes(1));

        // After waiting exactly 1 minute, all previous requests should be expired
        // and we should have 30 remaining again
        strategy.GetRemainingRequests(identifier).Should().Be(30);
    }

    /// <summary>
    /// Tests that IsActionAllowedAsync handles boundary conditions correctly.
    /// </summary>
    [Fact]
    public async Task IsActionAllowedAsync_HandlesBoundaryConditionsCorrectly()
    {
        // Arrange
        var strategy = new InMemoryRateLimitingStrategy();
        var key = "test-key";
        var limit = 10;
        var interval = TimeSpan.FromSeconds(30);

        // Fill the rate limit
        for (int i = 0; i < limit; i++)
        {
            (await strategy.IsActionAllowedAsync(key, limit, interval)).Should().BeTrue();
        }

        // Should be blocked
        (await strategy.IsActionAllowedAsync(key, limit, interval)).Should().BeFalse();

        // Wait for requests to expire (exactly 30 seconds)
        await Task.Delay(TimeSpan.FromSeconds(30));

        // After waiting exactly the interval, all previous requests should be expired
        // and new requests should be allowed again
        (await strategy.IsActionAllowedAsync(key, limit, interval)).Should().BeTrue();
    }

    /// <summary>
    /// Tests that different identifiers are limited independently.
    /// </summary>
    [Fact]
    public void IsRequestAllowed_DifferentIdentifiersLimitedIndependently()
    {
        // Arrange
        var strategy = new InMemoryRateLimitingStrategy();
        var identifier1 = "user1";
        var identifier2 = "user2";

        // Fill identifier1's limit
        for (int i = 0; i < 30; i++)
        {
            strategy.IsRequestAllowed(identifier1).Should().BeTrue();
        }

        // identifier1 should be blocked
        strategy.IsRequestAllowed(identifier1).Should().BeFalse();

        // identifier2 should still be able to make requests
        for (int i = 0; i < 30; i++)
        {
            strategy.IsRequestAllowed(identifier2).Should().BeTrue();
        }
    }
}
