using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TelegramBotFramework.Attributes;
using TelegramBotFramework.Integration;

namespace TelegramBotFramework.Services;

/// <summary>
/// Scans the loaded assemblies for <see cref="CommandAttribute"/> decorations,
/// builds a list of <see cref="BotCommand"/> objects and pushes them to Telegram
/// via <see cref="ITelegramApiClient.SetMyCommandsAsync"/>.
/// </summary>
public sealed class CommandMenuSyncService
{
    private readonly ITelegramApiClient _apiClient;

    public CommandMenuSyncService(ITelegramApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    /// <summary>
    /// Collects all commands defined with <c>CommandAttribute</c> and synchronises
    /// them with Telegram.
    /// </summary>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>True if the Telegram API accepted the command list.</returns>
    public async Task<bool> SyncAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var commands = CollectCommands();
        return await _apiClient.SetMyCommandsAsync(commands, cancellationToken);
    }

    private static IReadOnlyList<BotCommand> CollectCommands()
    {
        // Scan all loaded assemblies for types that have the CommandAttribute.
        var commandTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => t.GetCustomAttribute<CommandAttribute>() != null);

        var botCommands = new List<BotCommand>();

        foreach (var type in commandTypes)
        {
            var attr = type.GetCustomAttribute<CommandAttribute>();
            if (attr is null)
                continue; // safety, should not happen

            // Assume CommandAttribute exposes Name and Description properties.
            var name = attr.Name?.Trim() ?? string.Empty;
            var description = attr.Description?.Trim() ?? string.Empty;

            if (!string.IsNullOrEmpty(name))
            {
                botCommands.Add(new BotCommand(name, description));
            }
        }

        return botCommands;
    }
}
