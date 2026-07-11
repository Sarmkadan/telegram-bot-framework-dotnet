using FluentAssertions;
using TelegramBotFramework.Models;
using Xunit;

namespace TelegramBotFramework.Tests.Models;

public class CommandExtensionsTests
{
    [Fact]
    public void HasParameters_CommandHasParameters_ReturnsTrue()
    {
        // Arrange
        var command = new Command { Parameters = new[] { new CommandParameter() } };

        // Act
        var result = command.HasParameters();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void HasParameters_CommandHasNoParameters_ReturnsFalse()
    {
        // Arrange
        var command = new Command { Parameters = null };

        // Act
        var result = command.HasParameters();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GetPrimaryPattern_CommandHasName_ReturnsName()
    {
        // Arrange
        var command = new Command { Name = "test" };

        // Act
        var result = command.GetPrimaryPattern();

        // Assert
        result.Should().Be("test");
    }

    [Fact]
    public void IsStandardCommand_CommandIsStandard_ReturnsTrue()
    {
        // Arrange
        var command = new Command { Type = CommandType.Standard };

        // Act
        var result = command.IsStandardCommand();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void GetFormattedString_CommandHasDetails_ReturnsFormattedString()
    {
        // Arrange
        var command = new Command
        {
            Name = "test",
            Type = CommandType.Standard,
            Description = "Test command",
            CreatedAt = DateTime.Now,
            ExecutionCount = 1
        };

        // Act
        var result = command.GetFormattedString();

        // Assert
        result.Should().NotBeEmpty();
    }
}
