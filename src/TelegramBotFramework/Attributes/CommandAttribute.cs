#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Attributes;

/// <summary>
/// Marks a class as a handler for a specific bot command.
/// </summary>
/// <example>
/// <code>
/// [Command("start")]
/// public class StartCommand : ICommandHandler
/// {
///     public Task HandleAsync(Update update, CancellationToken cancellationToken)
///     {
///         // Handle the /start command
///         return Task.CompletedTask;
///     }
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class CommandAttribute : Attribute, ICommandAttribute
{
    /// <summary>
    /// The command name (e.g. "start", "help"). Leading slash is optional.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Optional human-readable description shown in the /help listing.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Optional list of aliases for this command (e.g. ["start", "begin"]).
    /// </summary>
    public string[] Aliases { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandAttribute"/> class.
    /// </summary>
    /// <param name="name">The command name, with or without a leading slash.</param>
    /// <exception cref="ArgumentException"><paramref name="name"/> is null, empty, or consists only of white-space characters.</exception>
    public CommandAttribute(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Command name must not be empty.", nameof(name));

        Name = name.TrimStart('/');
    }

    /// <inheritdoc />
    public override string ToString() => $"CommandAttribute {{ Description = {Description}, Aliases = {Aliases} }}";
}
