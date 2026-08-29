#nullable enable

using System.Threading;
using System.Threading.Tasks;
using TelegramBotFramework.Models;

namespace TelegramBotFramework.Middleware.Tests;

/// <summary>
/// Interface for AuthorizationMiddlewareTests to enable mocking and dependency injection.
/// </summary>
public interface IAuthorizationMiddlewareTests
{
    Task ProcessAsync_WhenContextInvalid_PassesToNext();
    Task ProcessAsync_WhenUserNull_LogsWarningAndPassesToNext();
    Task ProcessAsync_WhenUserIsRegularAndNoCommand_PassesThrough();
    Task ProcessAsync_WhenUserIsAdminAndNoCommand_PassesThrough();
    Task ProcessAsync_WhenRegularUserTriesAdminCommand_BlocksAndAddsError();
    Task ProcessAsync_WhenAdminUserExecutesAdminCommand_PassesThrough();
    Task ProcessAsync_WhenModeratorTriesAdminCommand_BlocksAndAddsError();
    Task ProcessAsync_WhenUserHasAdminRoleExecutesAdminCommand_PassesThrough();
    Task ProcessAsync_WhenUserWithoutCommand_ExecutesRegularCommands();
    void Priority_ReturnsCorrectValue();
    void Constructor_WhenCommandServiceNull_Throws();
    void Constructor_WhenUserServiceNull_Throws();
    void Constructor_WhenLoggerNull_Throws();
}