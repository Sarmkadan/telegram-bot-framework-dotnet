#nullable enable

namespace TelegramBotFramework.Tests;

public interface IBotFrameworkExceptionTests
{
    void BotFrameworkException_ShouldSetPropertiesCorrectly();
    void CommandExceptions_ShouldSetPropertiesCorrectly();
    void PermissionAndSessionExceptions_ShouldSetPropertiesCorrectly();
    void UserAndRateLimitExceptions_ShouldSetPropertiesCorrectly();
    void ConfigurationAndDuplicateUpdateExceptions_ShouldSetPropertiesCorrectly();
}