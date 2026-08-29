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
            _logger.LogInformation(BasicBotExampleConstants.StartingLogMessage);

            try
            {
                // Register basic commands
                await RegisterStartCommandAsync().ConfigureAwait(false);
                await RegisterHelpCommandAsync().ConfigureAwait(false);
                await RegisterEchoCommandAsync().ConfigureAwait(false);

                _logger.LogInformation(BasicBotExampleConstants.RunningLogMessage);

                // Simulate incoming message
                await HandleIncomingMessageAsync(BasicBotExampleConstants.SampleUserId, BasicBotExampleConstants.SampleChatId, BasicBotExampleConstants.StartCommandName).ConfigureAwait(false);
                await HandleIncomingMessageAsync(BasicBotExampleConstants.SampleUserId, BasicBotExampleConstants.SampleChatId, BasicBotExampleConstants.SampleMessage).ConfigureAwait(false);
                await HandleIncomingMessageAsync(BasicBotExampleConstants.SampleUserId, BasicBotExampleConstants.SampleChatId, BasicBotExampleConstants.SampleEchoMessage).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, BasicBotExampleConstants.ErrorLogMessage);
                throw;
            }
        }

        private async Task RegisterStartCommandAsync()
        {
            var command = new Command
            {
                Name = BasicBotExampleConstants.StartCommandName,
                Description = BasicBotExampleConstants.StartCommandDescription,
                HandlerType = BasicBotExampleConstants.StartCommandHandlerType,
                Type = CommandType.Standard,
                IsEnabled = true,
                RequiresAdmin = false,
                RateLimitPerMinute = BasicBotExampleConstants.StandardCommandRateLimitPerMinute,
                Parameters = new List<CommandParameter>()
            };

            await _commandService.RegisterCommandAsync(command).ConfigureAwait(false);
            _logger.LogInformation(BasicBotExampleConstants.StartCommandRegisteredLogMessage);
        }

        private async Task RegisterHelpCommandAsync()
        {
            var command = new Command
            {
                Name = BasicBotExampleConstants.HelpCommandName,
                Description = BasicBotExampleConstants.HelpCommandDescription,
                HandlerType = BasicBotExampleConstants.HelpCommandHandlerType,
                Type = CommandType.Standard,
                IsEnabled = true,
                RequiresAdmin = false,
                RateLimitPerMinute = BasicBotExampleConstants.StandardCommandRateLimitPerMinute,
                Parameters = new List<CommandParameter>()
            };

            await _commandService.RegisterCommandAsync(command).ConfigureAwait(false);
            _logger.LogInformation(BasicBotExampleConstants.HelpCommandRegisteredLogMessage);
        }

        private async Task RegisterEchoCommandAsync()
        {
            var command = new Command
            {
                Name = BasicBotExampleConstants.EchoCommandName,
                Description = BasicBotExampleConstants.EchoCommandDescription,
                HandlerType = BasicBotExampleConstants.EchoCommandHandlerType,
                Type = CommandType.Standard,
                IsEnabled = true,
                RequiresAdmin = false,
                RateLimitPerMinute = BasicBotExampleConstants.EchoCommandRateLimitPerMinute,
                Parameters = new List<CommandParameter>
                {
                    new CommandParameter
                    {
                        Name = BasicBotExampleConstants.EchoParameterName,
                        Type = BasicBotExampleConstants.EchoParameterType,
                        IsRequired = true,
                        Description = BasicBotExampleConstants.EchoParameterDescription
                    }
                }
            };

            await _commandService.RegisterCommandAsync(command).ConfigureAwait(false);
            _logger.LogInformation(BasicBotExampleConstants.EchoCommandRegisteredLogMessage);
        }

        private async Task HandleIncomingMessageAsync(long userId, long chatId, string content)
        {
            _logger.LogInformation(BasicBotExampleConstants.HandlingMessageLogTemplate, content);

            try
            {
                // Get or create user
                var user = await _userService.GetOrCreateUserAsync(userId, BasicBotExampleConstants.DefaultFirstName, BasicBotExampleConstants.DefaultLastName).ConfigureAwait(false);
                _logger.LogInformation(BasicBotExampleConstants.UserRetrievedLogTemplate, user.Id);

                // Process message
                var message = new Message
                {
                    UserId = userId,
                    ChatId = chatId,
                    Content = content,
                    Type = MessageType.Text,
                    Metadata = new Dictionary<string, object>
                    {
                        { BasicBotExampleConstants.MetadataSourceKey, BasicBotExampleConstants.MetadataSourceDirect }
                    }
                };

                var result = await _messageService.ProcessIncomingMessageAsync(message).ConfigureAwait(false);
                _logger.LogInformation(BasicBotExampleConstants.MessageProcessedLogTemplate, result.Status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, BasicBotExampleConstants.MessageProcessingErrorLogMessage);
            }
        }
    }
}
