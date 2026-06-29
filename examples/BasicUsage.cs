// =============================================================================
// Basic usage of the Telegram Bot Framework
// =============================================================================

using Microsoft.Extensions.DependencyInjection;
using TelegramBotFramework.Models;
using TelegramBotFramework.Services;

namespace TelegramBotFramework.Examples
{
    public class BasicUsage
    {
        private readonly ICommandService _commandService;

        public BasicUsage(IServiceProvider serviceProvider)
        {
            _commandService = serviceProvider.GetRequiredService<ICommandService>();
        }

        public async Task RegisterMinimalCommandAsync()
        {
            // A simple command registration
            var command = new Command
            {
                Name = "/hello",
                Description = "Says hello back",
                HandlerType = "HelloCommandHandler",
                IsEnabled = true
            };

            await _commandService.RegisterCommandAsync(command);
        }
    }
}
