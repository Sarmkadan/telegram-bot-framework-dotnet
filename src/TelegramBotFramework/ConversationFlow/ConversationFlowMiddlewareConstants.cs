#nullable enable
namespace TelegramBotFramework.ConversationFlow.Middleware
{
    /// <summary>
    /// Constants used by <see cref="ConversationFlowMiddleware"/>.
    /// </summary>
    internal static class ConversationFlowMiddlewareConstants
    {
        /// <summary>
        /// Pipeline execution priority of the conversation flow middleware. Higher values run earlier.
        /// </summary>
        public const int Priority = 85;
    }
}