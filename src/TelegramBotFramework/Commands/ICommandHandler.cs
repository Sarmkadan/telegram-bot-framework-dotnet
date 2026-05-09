#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Commands;

/// <summary>
/// Defines a handler for a bot command. Decorate implementations with
/// <see cref="Attributes.CommandAttribute"/> to supply the command name and
/// an optional description that will appear in the auto-generated /help output.
/// </summary>
public interface ICommandHandler
{
    /// <summary>
    /// Executes the command within the given execution context.
    /// </summary>
    Task<Models.ExecutionContext> HandleAsync(
        Models.ExecutionContext context,
        CancellationToken cancellationToken = default);
}
