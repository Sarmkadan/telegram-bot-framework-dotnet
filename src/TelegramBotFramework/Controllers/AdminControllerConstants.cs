namespace TelegramBotFramework.Controllers;

/// <summary>
/// Constants for AdminController.
/// </summary>
internal static class AdminControllerConstants
{
    /// <summary>
    /// Internal server error message.
    /// </summary>
    public const string InternalServerErrorMessage = "Internal server error";

    /// <summary>
    /// Route template for commands with name parameter.
    /// </summary>
    public const string CommandsRouteTemplate = "commands/{commandName}";

    /// <summary>
    /// Format string for user not found message.
    /// </summary>
    public const string UserNotFoundFormat = "User {0} not found";

    /// <summary>
    /// Format string for administrator not found message.
    /// </summary>
    public const string AdministratorNotFoundFormat = "Administrator {0} not found";

    /// <summary>
    /// Format string for command not found message.
    /// </summary>
    public const string CommandNotFoundFormat = "Command {0} not found";
}
