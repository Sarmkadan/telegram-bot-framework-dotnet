using TelegramBotFramework.Integration;

namespace TelegramBotFramework.Events
{
    public interface IEventPublisher
    {
        EventPublisher WithCorrelationId(string correlationId);
        Task PublishMessageReceivedAsync(long chatId, long userId, string? messageText);
        Task PublishCommandExecutedAsync(string commandName, long userId, string? arguments, bool success, string? errorMessage = null);
        Task PublishBotStateChangedAsync(string previousState, string newState, string? reason = null);
        Task PublishAsync<TEvent>(TEvent @event) where TEvent : class, IEvent;
    }
}