namespace TelegramBotFramework.Tests.Models;

/// <summary>
/// Interface for the tests of <see cref="TelegramBotFrameworkDotnetOptionsExtensions"/>.
/// </summary>
public interface ITelegramBotFrameworkDotnetOptionsExtensionsTests
{
    void Validate_OptionsAreValid_DoesNotThrow();
    void Validate_OptionsAreInvalid_ThrowsInvalidOperationException();
    void GetSessionTimeout_OptionsHaveSessionTimeout_ReturnsTimeSpan();
    void GetMessageProcessingTimeout_OptionsHaveMessageProcessingTimeout_ReturnsTimeSpan();
    void HasDatabaseConfigured_OptionsHaveDatabaseConnectionString_ReturnsTrue();
}