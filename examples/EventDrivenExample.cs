#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TelegramBotFramework.Events;
using TelegramBotFramework.Models;
using TelegramBotFramework.Services;

namespace TelegramBotFramework.Examples
{
    /// <summary>
    /// Event-driven architecture example demonstrating pub-sub pattern for decoupled communication.
    /// Shows how to publish and subscribe to framework events.
    /// </summary>
public sealed class EventDrivenExample
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EventDrivenExample> _logger;
        private readonly IEventBus _eventBus;
        private readonly IMessageService _messageService;

        public EventDrivenExample(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _logger = serviceProvider.GetRequiredService<ILogger<EventDrivenExample>>();
            _eventBus = serviceProvider.GetRequiredService<IEventBus>();
            _messageService = serviceProvider.GetRequiredService<IMessageService>();
        }

        public async Task RunAsync()
        {
            _logger.LogInformation("Starting EventDrivenExample");

            try
            {
                // Subscribe to events
                await SubscribeToEventsAsync().ConfigureAwait(false);

                // Simulate events
                await SimulateMessageFlowAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in EventDrivenExample");
                throw;
            }
        }

        private async Task SubscribeToEventsAsync()
        {
            _logger.LogInformation("Subscribing to events");

            // Subscribe to message received events
            _eventBus.Subscribe<MessageReceivedEvent>(async evt =>
            {
                _logger.LogInformation("📨 Event: Message received from user {UserId}: {Content}",
                    evt.UserId, evt.MessageContent);

                await HandleMessageReceivedAsync(evt).ConfigureAwait(false);
            });

            // Subscribe to command executed events
            _eventBus.Subscribe<CommandExecutedEvent>(async evt =>
            {
                _logger.LogInformation("⚡ Event: Command executed - {CommandName} by user {UserId}, Success: {Success}",
                    evt.CommandName, evt.UserId, evt.Success);

                await HandleCommandExecutedAsync(evt).ConfigureAwait(false);
            });

            // Subscribe to bot state changed events
            _eventBus.Subscribe<BotStateChangedEvent>(async evt =>
            {
                _logger.LogInformation("🔄 Event: Bot state changed from {OldState} to {NewState}",
                    evt.OldState, evt.NewState);

                await HandleBotStateChangedAsync(evt).ConfigureAwait(false);
            });

            _logger.LogInformation("Event subscriptions registered");
        }

        private async Task SimulateMessageFlowAsync()
        {
            _logger.LogInformation("Simulating message flow with events");

            var userId = 123456789L;
            var chatId = 123456789L;

            // Simulate message processing
            var message = new Message
            {
                UserId = userId,
                ChatId = chatId,
                Content = "Hello bot!",
                Type = MessageType.Text
            };

            _logger.LogInformation("Processing message: {Content}", message.Content);

            // This will trigger MessageReceivedEvent
            var processed = await _messageService.ProcessIncomingMessageAsync(message).ConfigureAwait(false);

            _logger.LogInformation("Message processing complete");

            // Simulate another message (command)
            var commandMessage = new Message
            {
                UserId = userId,
                ChatId = chatId,
                Content = "/start",
                Type = MessageType.Command
            };

            var commandProcessed = await _messageService.ProcessIncomingMessageAsync(commandMessage).ConfigureAwait(false);

            // Simulate bot state change
            await PublishBotStateChangeAsync("Idle", "Processing").ConfigureAwait(false);
            await Task.Delay(500).ConfigureAwait(false);
            await PublishBotStateChangeAsync("Processing", "Ready").ConfigureAwait(false);
        }

        private async Task HandleMessageReceivedAsync(MessageReceivedEvent evt)
        {
            _logger.LogInformation("Handler: Processing message from user {UserId}", evt.UserId);

            // Custom logic for handling received messages
            await Task.Delay(50).ConfigureAwait(false);

            _logger.LogInformation("Handler: Message processing complete");
        }

        private async Task HandleCommandExecutedAsync(CommandExecutedEvent evt)
        {
            _logger.LogInformation("Handler: Recording command execution - {CommandName}", evt.CommandName);

            // Custom logic for command tracking, logging, etc.
            await Task.Delay(50).ConfigureAwait(false);

            if (evt.Success)
            {
                _logger.LogInformation("Handler: Command executed successfully");
            }
            else
            {
                _logger.LogWarning("Handler: Command execution failed");
            }
        }

        private async Task HandleBotStateChangedAsync(BotStateChangedEvent evt)
        {
            _logger.LogInformation("Handler: Notified of state change");

            // Custom logic for state change handling
            await Task.Delay(50).ConfigureAwait(false);

            _logger.LogInformation("Handler: State change handled");
        }

        private async Task PublishBotStateChangeAsync(string oldState, string newState)
        {
            var evt = new BotStateChangedEvent
            {
                CorrelationId = Guid.NewGuid().ToString(),
                OldState = oldState,
                NewState = newState,
                Timestamp = DateTime.UtcNow
            };

            await _eventBus.PublishAsync(evt).ConfigureAwait(false);
        }
    }
}