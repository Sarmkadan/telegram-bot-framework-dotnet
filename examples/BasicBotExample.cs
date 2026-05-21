#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TelegramBotFramework.Models;
using TelegramBotFramework.Services;

namespace TelegramBotFramework.Examples
{
    /// <summary>
    /// Basic bot example demonstrating command registration and simple message handling.
    /// This example shows the fundamental patterns for building a Telegram bot using the framework.
    /// </summary>
public sealed class BasicBotExample
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BasicBotExample> _logger;
        private readonly ICommandService _commandService;
        private readonly IUserService _userService;
        private readonly IMessageService _messageService;

        public BasicBotExample(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _logger = serviceProvider.GetRequiredService<ILogger<BasicBotExample>>();
            _commandService = serviceProvider.GetRequiredService<ICommandService>();
            _userService = serviceProvider.GetRequiredService<IUserService>();
            _messageService = serviceProvider.GetRequiredService<IMessageService>();
        }

        public async Task RunAsync()
        {
            _logger.LogInformation("Starting BasicBotExample");

            try
            {
                // Register basic commands
                await RegisterStartCommandAsync().ConfigureAwait(false);
                await RegisterHelpCommandAsync().ConfigureAwait(false);
                await RegisterEchoCommandAsync().ConfigureAwait(false);

                _logger.LogInformation("Bot is running. Commands registered: /start, /help, /echo");

                // Simulate incoming message
                await HandleIncomingMessageAsync(123456789, 123456789, "/start").ConfigureAwait(false);
                await HandleIncomingMessageAsync(123456789, 123456789, "Hello bot!").ConfigureAwait(false);
                await HandleIncomingMessageAsync(123456789, 123456789, "/echo Test message").ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in BasicBotExample");
                throw;
            }
        }

        private async Task RegisterStartCommandAsync()
        {
            var command = new Command
            {
                Name = "/start",
                Description = "Welcome message and bot introduction",
                HandlerType = "StartCommandHandler",
                Type = CommandType.Standard,
                IsEnabled = true,
                RequiresAdmin = false,
                RateLimitPerMinute = 60,
                Parameters = new List<CommandParameter>()
            };

            await _commandService.RegisterCommandAsync(command).ConfigureAwait(false);
            _logger.LogInformation("Registered /start command");
        }

        private async Task RegisterHelpCommandAsync()
        {
            var command = new Command
            {
                Name = "/help",
                Description = "Display available commands and usage information",
                HandlerType = "HelpCommandHandler",
                Type = CommandType.Standard,
                IsEnabled = true,
                RequiresAdmin = false,
                RateLimitPerMinute = 60,
                Parameters = new List<CommandParameter>()
            };

            await _commandService.RegisterCommandAsync(command).ConfigureAwait(false);
            _logger.LogInformation("Registered /help command");
        }

        private async Task RegisterEchoCommandAsync()
        {
            var command = new Command
            {
                Name = "/echo",
                Description = "Echo back the provided text",
                HandlerType = "EchoCommandHandler",
                Type = CommandType.Standard,
                IsEnabled = true,
                RequiresAdmin = false,
                RateLimitPerMinute = 30,
                Parameters = new List<CommandParameter>
                {
                    new CommandParameter
                    {
                        Name = "text",
                        Type = "string",
                        IsRequired = true,
                        Description = "Text to echo"
                    }
                }
            };

            await _commandService.RegisterCommandAsync(command).ConfigureAwait(false);
            _logger.LogInformation("Registered /echo command");
        }

        private async Task HandleIncomingMessageAsync(long userId, long chatId, string content)
        {
            _logger.LogInformation("Handling message: {Content}", content);

            try
            {
                // Get or create user
                var user = await _userService.GetOrCreateUserAsync(userId, "User", "Test").ConfigureAwait(false);
                _logger.LogInformation("User {UserId} retrieved/created", user.Id);

                // Process message
                var message = new Message
                {
                    UserId = userId,
                    ChatId = chatId,
                    Content = content,
                    Type = MessageType.Text,
                    Metadata = new Dictionary<string, object>
                    {
                        { "source", "direct" }
                    }
                };

                var result = await _messageService.ProcessIncomingMessageAsync(message).ConfigureAwait(false);
                _logger.LogInformation("Message processed with status: {Status}", result.Status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message");
            }
        }
    }
}