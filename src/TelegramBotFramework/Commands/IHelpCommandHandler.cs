#nullable enable
namespace TelegramBotFramework.Commands;

/// <summary>
/// Interface for the built-in handler for the /help command.
/// </summary>
public interface IHelpCommandHandler
{
    /// <summary>
    /// Handles the /help command.
    /// </summary>
    /// <param name="context">The execution context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The execution context.</returns>
    Task<Models.ExecutionContext> HandleAsync(
        Models.ExecutionContext context,
        System.Threading.CancellationToken cancellationToken = default);

    /// <summary>
    /// Builds the formatted help message by inspecting all registered handlers.
    /// </summary>
    /// <returns>The formatted help text.</returns>
    string BuildHelpText();
}