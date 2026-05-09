#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Commands;

/// <summary>
/// Built-in handler for the /help command.
/// Scans all registered <see cref="ICommandHandler"/> implementations, reads
/// their <see cref="Attributes.CommandAttribute"/> metadata, and returns a
/// formatted list of available commands.
/// </summary>
[Attributes.Command("help", Description = "Show available commands")]
public sealed class HelpCommandHandler : ICommandHandler
{
    private readonly IEnumerable<ICommandHandler> _handlers;
    private readonly Microsoft.Extensions.Logging.ILogger<HelpCommandHandler> _logger;

    public HelpCommandHandler(
        IEnumerable<ICommandHandler> handlers,
        Microsoft.Extensions.Logging.ILogger<HelpCommandHandler> logger)
    {
        _handlers = handlers ?? throw new ArgumentNullException(nameof(handlers));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public Task<Models.ExecutionContext> HandleAsync(
        Models.ExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        var helpText = BuildHelpText();
        context.SetState("help_text", helpText);

        _logger.LogInformation("Help command executed for user {UserId}", context.UserId);

        return Task.FromResult(context);
    }

    /// <summary>
    /// Builds the formatted help message by inspecting all registered handlers.
    /// </summary>
    public string BuildHelpText()
    {
        var lines = new System.Text.StringBuilder();
        lines.AppendLine("Available commands:");
        lines.AppendLine();

        foreach (var handler in _handlers.OrderBy(GetCommandName))
        {
            var attr = handler.GetType()
                .GetCustomAttributes(typeof(Attributes.CommandAttribute), inherit: false)
                .OfType<Attributes.CommandAttribute>()
                .FirstOrDefault();

            if (attr is null)
                continue;

            var name = $"/{attr.Name}";
            if (!string.IsNullOrWhiteSpace(attr.Description))
                lines.AppendLine($"{name} — {attr.Description}");
            else
                lines.AppendLine(name);
        }

        return lines.ToString().TrimEnd();
    }

    private static string GetCommandName(ICommandHandler handler)
    {
        var attr = handler.GetType()
            .GetCustomAttributes(typeof(Attributes.CommandAttribute), inherit: false)
            .OfType<Attributes.CommandAttribute>()
            .FirstOrDefault();

        return attr?.Name ?? string.Empty;
    }
}
