namespace TelegramBotFramework.Tests
{
    public interface ICommandServiceTests
    {
        Task GetCommandAsync_WhenExists_ReturnsCommand();
        Task GetCommandAsync_WhenDoesNotExist_ReturnsNull();
        Task ExecuteCommandAsync_WhenCommandIsDisabled_AddsErrorToContext();
        Task ExecuteCommandAsync_WithInsufficientPermissions_AddsErrorToContext();
        Task IsCommandRateLimitedAsync_WhenExceedsLimit_ReturnsTrue();
        Task IsCommandRateLimitedAsync_WhenWithinLimit_ReturnsFalse();
        Task RegisterCommandAsync_WithInvalidCommand_ThrowsException();
    }
}