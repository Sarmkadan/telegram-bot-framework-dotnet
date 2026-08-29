using FluentAssertions;
using TelegramBotFramework.Models;
using Xunit;

namespace TelegramBotFramework.Tests.Models;

/// <summary>
/// Tests for the <see cref="TelegramBotFrameworkDotnetOptionsExtensions"/> extension methods.
/// </summary>
public class TelegramBotFrameworkDotnetOptionsExtensionsTests : ITelegramBotFrameworkDotnetOptionsExtensionsTests
{
    /// <summary>
    /// Verifies that calling <see cref="TelegramBotFrameworkDotnetOptionsExtensions.Validate(TelegramBotFramework.Models.TelegramBotFrameworkDotnetOptions)"/>
    /// with valid options does not throw an exception.
    /// </summary>
    [Fact]
    public void Validate_OptionsAreValid_DoesNotThrow()
    {
        // Arrange
        var options = new TelegramBotFrameworkDotnetOptions
        {
            BotToken = "test",
            BotUsername = "test"
        };

        // Act and Assert
        options.Validate();
    }

    /// <summary>
    /// Verifies that calling <see cref="TelegramBotFrameworkDotnetOptionsExtensions.Validate(TelegramBotFramework.Models.TelegramBotFrameworkDotnetOptions)"/>
    /// with invalid options throws an <see cref="InvalidOperationException"/>.
    /// </summary>
    [Fact]
    public void Validate_OptionsAreInvalid_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = new TelegramBotFrameworkDotnetOptions
        {
            BotToken = "",
            BotUsername = ""
        };

        // Act and Assert
        Assert.Throws<InvalidOperationException>(() => options.Validate());
    }

    /// <summary>
    /// Verifies that <see cref="TelegramBotFrameworkDotnetOptionsExtensions.GetSessionTimeout(TelegramBotFramework.Models.TelegramBotFrameworkDotnetOptions)"/>
    /// returns a <see cref="TimeSpan"/> representing the configured session timeout.
    /// </summary>
    [Fact]
    public void GetSessionTimeout_OptionsHaveSessionTimeout_ReturnsTimeSpan()
    {
        // Arrange
        var options = new TelegramBotFrameworkDotnetOptions { SessionTimeoutMinutes = 30 };

        // Act
        var result = options.GetSessionTimeout();

        // Assert
        result.Should().Be(TimeSpan.FromMinutes(30));
    }

    /// <summary>
    /// Verifies that <see cref="TelegramBotFrameworkDotnetOptionsExtensions.GetMessageProcessingTimeout(TelegramBotFramework.Models.TelegramBotFrameworkDotnetOptions)"/>
    /// returns a <see cref="TimeSpan"/> representing the configured message processing timeout.
    /// </summary>
    [Fact]
    public void GetMessageProcessingTimeout_OptionsHaveMessageProcessingTimeout_ReturnsTimeSpan()
    {
        // Arrange
        var options = new TelegramBotFrameworkDotnetOptions { MessageProcessingTimeoutSeconds = 30 };

        // Act
        var result = options.GetMessageProcessingTimeout();

        // Assert
        result.Should().Be(TimeSpan.FromSeconds(30));
    }

    /// <summary>
    /// Verifies that <see cref="TelegramBotFrameworkDotnetOptionsExtensions.HasDatabaseConfigured(TelegramBotFramework.Models.TelegramBotFrameworkDotnetOptions)"/>
    /// returns <c>true</c> when a database connection string is configured.
    /// </summary>
    [Fact]
    public void HasDatabaseConfigured_OptionsHaveDatabaseConnectionString_ReturnsTrue()
    {
        // Arrange
        var options = new TelegramBotFrameworkDotnetOptions { DatabaseConnectionString = "test" };

        // Act
        var result = options.HasDatabaseConfigured();

        // Assert
        result.Should().BeTrue();
    }
}
