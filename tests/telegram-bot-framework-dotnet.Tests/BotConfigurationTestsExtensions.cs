#nullable enable

using FluentAssertions;
using TelegramBotFramework.Models;
using Xunit;

namespace TelegramBotFramework.Tests;

/// <summary>
/// Provides extension methods for testing <see cref="BotConfiguration"/> instances.
/// </summary>
public static class BotConfigurationTestsExtensions
{
	/// <summary>
	/// Creates a valid configuration with default values that passes validation.
	/// </summary>
	/// <param name="_">Discard parameter for fluent syntax.</param>
	/// <returns>A new <see cref="BotConfiguration"/> instance with valid default values.</returns>
	public static BotConfiguration CreateValidConfiguration(this BotConfiguration _) => new BotConfiguration
	{
		BotToken = "test-token-123",
		BotUsername = "TestBot",
		SessionTimeoutMinutes = 30,
		MaxConcurrentRequests = 10
	};

	/// <summary>
	/// Sets the owner ID on the configuration.
	/// </summary>
	/// <param name="config">The configuration instance.</param>
	/// <param name="ownerId">The owner ID to set.</param>
	/// <returns>The configuration instance for method chaining.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="config"/> is <see langword="null"/>.</exception>
	public static BotConfiguration WithOwnerId(this BotConfiguration config, long ownerId)
	{
		ArgumentNullException.ThrowIfNull(config);
		config.OwnerId = ownerId;
		return config;
	}

	/// <summary>
	/// Sets the admin IDs on the configuration.
	/// </summary>
	/// <param name="config">The configuration instance.</param>
	/// <param name="adminIds">The admin IDs to set.</param>
	/// <returns>The configuration instance for method chaining.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="config"/> is <see langword="null"/>.</exception>
	public static BotConfiguration WithAdminIds(this BotConfiguration config, params long[] adminIds)
	{
		ArgumentNullException.ThrowIfNull(config);
		ArgumentNullException.ThrowIfNull(adminIds);
		config.AdminIds = new List<long>(adminIds);
		return config;
	}

	/// <summary>
	/// Sets custom settings on the configuration.
	/// </summary>
	/// <param name="config">The configuration instance.</param>
	/// <param name="customSettings">The custom settings to set. If <see langword="null"/>, creates a new dictionary.</param>
	/// <returns>The configuration instance for method chaining.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="config"/> is <see langword="null"/>.</exception>
	public static BotConfiguration WithCustomSettings(this BotConfiguration config, Dictionary<string, string>? customSettings = null)
	{
		ArgumentNullException.ThrowIfNull(config);
		config.CustomSettings = customSettings ?? new Dictionary<string, string>();
		return config;
	}

	/// <summary>
	/// Asserts that the configuration is valid.
	/// </summary>
	/// <param name="config">The configuration instance to validate.</param>
	/// <exception cref="ArgumentNullException"><paramref name="config"/> is <see langword="null"/>.</exception>
	public static void ShouldBeValid(this BotConfiguration config)
	{
		ArgumentNullException.ThrowIfNull(config);
		var result = config.Validate();
		result.Should().BeTrue();
	}

	/// <summary>
	/// Asserts that the configuration validation throws the specified exception.
	/// </summary>
	/// <param name="config">The configuration instance to validate.</param>
	/// <param name="expectedMessage">The expected exception message.</param>
	/// <exception cref="ArgumentNullException"><paramref name="config"/> is <see langword="null"/>.</exception>
	public static void ShouldThrowValidationException(this BotConfiguration config, string expectedMessage)
	{
		ArgumentNullException.ThrowIfNull(config);
		ArgumentNullException.ThrowIfNull(expectedMessage);
		config.Invoking(c => c.Validate())
			.Should().Throw<InvalidOperationException>()
			.WithMessage(expectedMessage);
	}

	/// <summary>
	/// Enables webhook on the configuration.
	/// </summary>
	/// <param name="config">The configuration instance.</param>
	/// <param name="webhookUrl">The webhook URL. If <see langword="null"/>, uses a default value.</param>
	/// <param name="webhookSecret">The webhook secret. If <see langword="null"/>, uses a default value.</param>
	/// <returns>The configuration instance for method chaining.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="config"/> is <see langword="null"/>.</exception>
	public static BotConfiguration WithWebhookEnabled(this BotConfiguration config, string? webhookUrl = null, string? webhookSecret = null)
	{
		ArgumentNullException.ThrowIfNull(config);
		config.EnableWebhook = true;
		config.WebhookUrl = webhookUrl ?? "https://example.com/webhook";
		config.WebhookSecret = webhookSecret ?? "secret123";
		return config;
	}

	/// <summary>
	/// Disables rate limiting on the configuration.
	/// </summary>
	/// <param name="config">The configuration instance.</param>
	/// <returns>The configuration instance for method chaining.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="config"/> is <see langword="null"/>.</exception>
	public static BotConfiguration WithRateLimitingDisabled(this BotConfiguration config)
	{
		ArgumentNullException.ThrowIfNull(config);
		config.EnableRateLimiting = false;
		return config;
	}

	/// <summary>
	/// Sets the session timeout on the configuration.
	/// </summary>
	/// <param name="config">The configuration instance.</param>
	/// <param name="minutes">The session timeout in minutes.</param>
	/// <returns>The configuration instance for method chaining.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="config"/> is <see langword="null"/>.</exception>
	public static BotConfiguration WithSessionTimeout(this BotConfiguration config, int minutes)
	{
		ArgumentNullException.ThrowIfNull(config);
		config.SessionTimeoutMinutes = minutes;
		return config;
	}

	/// <summary>
	/// Sets the maximum concurrent requests on the configuration.
	/// </summary>
	/// <param name="config">The configuration instance.</param>
	/// <param name="maxRequests">The maximum concurrent requests.</param>
	/// <returns>The configuration instance for method chaining.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="config"/> is <see langword="null"/>.</exception>
	public static BotConfiguration WithMaxConcurrentRequests(this BotConfiguration config, int maxRequests)
	{
		ArgumentNullException.ThrowIfNull(config);
		config.MaxConcurrentRequests = maxRequests;
		return config;
	}

	/// <summary>
	/// Asserts that the specified user ID is an admin.
	/// </summary>
	/// <param name="config">The configuration instance.</param>
	/// <param name="userId">The user ID to check.</param>
	/// <exception cref="ArgumentNullException"><paramref name="config"/> is <see langword="null"/>.</exception>
	public static void ShouldBeAdmin(this BotConfiguration config, long userId)
	{
		ArgumentNullException.ThrowIfNull(config);
		config.IsAdmin(userId).Should().BeTrue();
	}

	/// <summary>
	/// Asserts that the specified user ID is not an admin.
	/// </summary>
	/// <param name="config">The configuration instance.</param>
	/// <param name="userId">The user ID to check.</param>
	/// <exception cref="ArgumentNullException"><paramref name="config"/> is <see langword="null"/>.</exception>
	public static void ShouldNotBeAdmin(this BotConfiguration config, long userId)
	{
		ArgumentNullException.ThrowIfNull(config);
		config.IsAdmin(userId).Should().BeFalse();
	}

	/// <summary>
	/// Asserts that the session timeout matches the expected value.
	/// </summary>
	/// <param name="config">The configuration instance.</param>
	/// <param name="expected">The expected timeout value.</param>
	/// <exception cref="ArgumentNullException"><paramref name="config"/> is <see langword="null"/>.</exception>
	public static void SessionTimeoutShouldBe(this BotConfiguration config, TimeSpan expected)
	{
		ArgumentNullException.ThrowIfNull(config);
		config.GetSessionTimeout().Should().Be(expected);
	}

	/// <summary>
	/// Disables logging on the configuration.
	/// </summary>
	/// <param name="config">The configuration instance.</param>
	/// <returns>The configuration instance for method chaining.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="config"/> is <see langword="null"/>.</exception>
	public static BotConfiguration WithLoggingDisabled(this BotConfiguration config)
	{
		ArgumentNullException.ThrowIfNull(config);
		config.EnableLogging = false;
		config.LogLevel = LogLevel.Info;
		return config;
	}

	/// <summary>
	/// Sets the localization language on the configuration.
	/// </summary>
	/// <param name="config">The configuration instance.</param>
	/// <param name="language">The language code to set.</param>
	/// <returns>The configuration instance for method chaining.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="config"/> is <see langword="null"/>.</exception>
	/// <exception cref="ArgumentException"><paramref name="language"/> is <see langword="null"/> or whitespace.</exception>
	public static BotConfiguration WithLocalizationLanguage(this BotConfiguration config, string language)
	{
		ArgumentNullException.ThrowIfNull(config);
		ArgumentException.ThrowIfNullOrWhiteSpace(language);
		config.LocalizationLanguage = language;
		return config;
	}
}