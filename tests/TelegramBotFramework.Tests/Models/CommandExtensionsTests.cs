using FluentAssertions;
using TelegramBotFramework.Models;
using Xunit;

namespace TelegramBotFramework.Tests.Models;

/// <summary>
/// Provides extension methods for testing command-related functionality in the Telegram Bot Framework.
/// Contains unit tests for verifying the behavior of command extensions and their methods.
/// </summary>
public class CommandExtensionsTests
{
    /// <summary>
    /// Tests that <see cref="Command.HasParameters()"/> returns true when the command has parameters.
    /// Verifies the extension method correctly identifies commands with parameters.
    /// </summary>
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

    /// <summary>
    /// Tests that <see cref="Command.HasParameters()"/> returns false when the command has no parameters.
    /// Verifies the extension method correctly handles null parameter collections.
    /// </summary>
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

    /// <summary>
    /// Tests that <see cref="Command.GetPrimaryPattern()"/> returns the command name when present.
    /// Verifies the extension method correctly extracts the primary pattern from a command.
    /// </summary>
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

    /// <summary>
    /// Tests that <see cref="Command.IsStandardCommand()"/> returns true for standard commands.
    /// Verifies the extension method correctly identifies commands of standard type.
    /// </summary>
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

    /// <summary>
    /// Tests that <see cref="Command.GetFormattedString()"/> generates a non-empty formatted string for commands with details.
    /// Verifies the extension method produces output for commands containing name, type, description, and metadata.
    /// </summary>
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
