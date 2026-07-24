using System;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using TelegramBotFramework.Events;
using TelegramBotFramework.Tests;

namespace TelegramBotFramework.Tests
{
    public static class EventPublisherTestsExtensions
    {
        public static EventPublisherTests WithCorrelationId(this EventPublisherTests tests, string correlationId)
        {
            ArgumentNullException.ThrowIfNull(tests);
            ArgumentException.ThrowIfNullOrEmpty(correlationId);

            var publisherField = typeof(EventPublisherTests).GetField("_publisher", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("_publisher field not found in EventPublisherTests");
            var publisher = (EventPublisher)publisherField.GetValue(tests)!;
            publisher.WithCorrelationId(correlationId);
            return tests;
        }

        public static async Task PublishAndAssertMessageReceivedAsync(
            this EventPublisherTests tests,
            long chatId,
            long userId,
            string messageText,
            string? correlationId = null)
        {
            ArgumentNullException.ThrowIfNull(tests);
            ArgumentException.ThrowIfNullOrEmpty(messageText);

            var publisherField = typeof(EventPublisherTests).GetField("_publisher", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("_publisher field not found in EventPublisherTests");
            var publisher = (EventPublisher)publisherField.GetValue(tests)!;
            var eventBusMockField = typeof(EventPublisherTests).GetField("_eventBusMock", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("_eventBusMock field not found in EventPublisherTests");
            var eventBusMock = (Mock<IEventBus>)eventBusMockField.GetValue(tests)!;

            if (correlationId != null)
                publisher.WithCorrelationId(correlationId);

            await publisher.PublishMessageReceivedAsync(chatId, userId, messageText);

            eventBusMock.Verify(x => x.PublishAsync(It.Is<MessageReceivedEvent>(e =>
                e.ChatId == chatId &&
                e.UserId == userId &&
                e.MessageText == messageText &&
                (correlationId == null || e.CorrelationId == correlationId) &&
                e.EventType == nameof(MessageReceivedEvent))), Times.Once);
        }

        public static async Task PublishAndAssertCommandExecutedAsync(
            this EventPublisherTests tests,
            string commandName,
            long userId,
            string? arguments,
            bool success,
            string? errorMessage = null,
            string? correlationId = null)
        {
            ArgumentNullException.ThrowIfNull(tests);
            ArgumentException.ThrowIfNullOrEmpty(commandName);

            var publisherField = typeof(EventPublisherTests).GetField("_publisher", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("_publisher field not found in EventPublisherTests");
            var publisher = (EventPublisher)publisherField.GetValue(tests)!;
            var eventBusMockField = typeof(EventPublisherTests).GetField("_eventBusMock", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("_eventBusMock field not found in EventPublisherTests");
            var eventBusMock = (Mock<IEventBus>)eventBusMockField.GetValue(tests)!;

            if (correlationId != null)
                publisher.WithCorrelationId(correlationId);

            await publisher.PublishCommandExecutedAsync(commandName, userId, arguments, success, errorMessage);

            eventBusMock.Verify(x => x.PublishAsync(It.Is<CommandExecutedEvent>(e =>
                e.CommandName == commandName &&
                e.UserId == userId &&
                e.Arguments == arguments &&
                e.Success == success &&
                e.ErrorMessage == errorMessage &&
                (correlationId == null || e.CorrelationId == correlationId) &&
                e.EventType == nameof(CommandExecutedEvent))), Times.Once);
        }
    }
}