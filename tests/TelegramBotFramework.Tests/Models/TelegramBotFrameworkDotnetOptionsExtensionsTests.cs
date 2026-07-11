using FluentAssertions;
using TelegramBotFramework.Models;
using Xunit;

namespace TelegramBotFramework.Tests.Models;

public class TelegramBotFrameworkDotnetOptionsExtensionsTests
{
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
