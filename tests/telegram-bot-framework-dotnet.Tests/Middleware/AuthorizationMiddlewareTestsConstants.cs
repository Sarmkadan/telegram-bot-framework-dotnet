#nullable enable

namespace TelegramBotFramework.Middleware.Tests;

internal static class AuthorizationMiddlewareTestsConstants
{
    public const long InvalidUserId = 0;
    public const long RegularUserId = 123;
    public const long TestChatId = 456;
    public const long ModeratorUserId = 500;
    public const long AdminUserId = 999;
    public const int ExpectedMiddlewarePriority = 30;

    public const string AdminCommandName = "/admincommand";
    public const string AdminCommandDescription = "Admin command";
    public const string AdminCommandHandlerType = "AdminHandler";
    public const string RegularUserFirstName = "Regular";
    public const string AdminUserFirstName = "Admin";
    public const string UnauthorizedCommandErrorFragment = "not authorized to execute command";
}
