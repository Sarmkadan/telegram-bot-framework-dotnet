namespace TelegramBotFramework.Tests.Models;

/// <summary>
/// Defines the contract for command extension tests.
/// </summary>
public interface ICommandExtensionsTests
{
    void HasParameters_CommandHasParameters_ReturnsTrue();
    void HasParameters_CommandHasNoParameters_ReturnsFalse();
    void GetPrimaryPattern_CommandHasName_ReturnsName();
    void IsStandardCommand_CommandIsStandard_ReturnsTrue();
    void GetFormattedString_CommandHasDetails_ReturnsFormattedString();
}