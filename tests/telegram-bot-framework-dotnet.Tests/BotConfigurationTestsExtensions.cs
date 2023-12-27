#nullable enable

using FluentAssertions;
using TelegramBotFramework.Models;
using Xunit;

namespace TelegramBotFramework.Tests;

public static class BotConfigurationTestsExtensions
{
    /// <summary>
    /// Creates a valid configuration with default values that passes validation.
    /// </summary>
    public static BotConfiguration CreateValidConfiguration(this BotConfiguration _) => new BotConfiguration
    {
        BotToken = "test-token-123",
        BotUsername = "TestBot",
        SessionTimeoutMinutes = 30,
        MaxConcurrentRequests = 10
    };

    /// <summary>
    /// Creates a configuration with the specified owner ID set.
    /// </summary>
    public static BotConfiguration WithOwnerId(this BotConfiguration config, long ownerId)
    {
        config.OwnerId = ownerId;
        return config;
    }

    /// <summary>
    /// Creates a configuration with the specified admin IDs.
    /// </summary>
    public static BotConfiguration WithAdminIds(this BotConfiguration config, params long[] adminIds)
    {
        config.AdminIds = new List<long>(adminIds);
        return config;
    }

    /// <summary>
    /// Creates a configuration with custom settings initialized.
    /// </summary>
    public static BotConfiguration WithCustomSettings(this BotConfiguration config, Dictionary<string, string>? customSettings = null)
    {
        config.CustomSettings = customSettings ?? new Dictionary<string, string>();
        return config;
    }

    /// <summary>
    /// Asserts that the configuration is valid.
    /// </summary>
    public static void ShouldBeValid(this BotConfiguration config)
    {
        var result = config.Validate();
        result.Should().BeTrue();
    }

    /// <summary>
    /// Asserts that the configuration validation throws the specified exception.
    /// </summary>
    public static void ShouldThrowValidationException(this BotConfiguration config, string expectedMessage)
    {
        config.Invoking(c => c.Validate())
            .Should().Throw<InvalidOperationException>()
            .WithMessage(expectedMessage);
    }

    /// <summary>
    /// Creates a configuration with webhook enabled.
    /// </summary>
    public static BotConfiguration WithWebhookEnabled(this BotConfiguration config, string? webhookUrl = null, string? webhookSecret = null)
    {
        config.EnableWebhook = true;
        config.WebhookUrl = webhookUrl ?? "https://example.com/webhook";
        config.WebhookSecret = webhookSecret ?? "secret123";
        return config;
    }

    /// <summary>
    /// Creates a configuration with rate limiting disabled.
    /// </summary>
    public static BotConfiguration WithRateLimitingDisabled(this BotConfiguration config)
    {
        config.EnableRateLimiting = false;
        return config;
    }

    /// <summary>
    /// Creates a configuration with the specified session timeout.
    /// </summary>
    public static BotConfiguration WithSessionTimeout(this BotConfiguration config, int minutes)
    {
        config.SessionTimeoutMinutes = minutes;
        return config;
    }

    /// <summary>
    /// Creates a configuration with the specified max concurrent requests.
    /// </summary>
    public static BotConfiguration WithMaxConcurrentRequests(this BotConfiguration config, int maxRequests)
    {
        config.MaxConcurrentRequests = maxRequests;
        return config;
    }

    /// <summary>
    /// Asserts that the specified user ID is an admin.
    /// </summary>
    public static void ShouldBeAdmin(this BotConfiguration config, long userId)
    {
        config.IsAdmin(userId).Should().BeTrue();
    }

    /// <summary>
    /// Asserts that the specified user ID is not an admin.
    /// </summary>
    public static void ShouldNotBeAdmin(this BotConfiguration config, long userId)
    {
        config.IsAdmin(userId).Should().BeFalse();
    }

    /// <summary>
    /// Asserts that the session timeout matches the expected value.
    /// </summary>
    public static void SessionTimeoutShouldBe(this BotConfiguration config, TimeSpan expected)
    {
        config.GetSessionTimeout().Should().Be(expected);
    }

    /// <summary>
    /// Creates a configuration with logging disabled.
    /// </summary>
    public static BotConfiguration WithLoggingDisabled(this BotConfiguration config)
    {
        config.EnableLogging = false;
        config.LogLevel = LogLevel.Info;
        return config;
    }

    /// <summary>
    /// Creates a configuration with the specified localization language.
    /// </summary>
    public static BotConfiguration WithLocalizationLanguage(this BotConfiguration config, string language)
    {
        config.LocalizationLanguage = language;
        return config;
    }
}