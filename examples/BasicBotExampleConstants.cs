#nullable enable

namespace TelegramBotFramework.Examples
{
    internal static class BasicBotExampleConstants
    {
        public const string StartingLogMessage = "Starting BasicBotExample";
        public const string RunningLogMessage = "Bot is running. Commands registered: /start, /help, /echo";
        public const string ErrorLogMessage = "Error in BasicBotExample";
        public const string StartCommandName = "/start";
        public const string StartCommandDescription = "Welcome message and bot introduction";
        public const string StartCommandHandlerType = "StartCommandHandler";
        public const string StartCommandRegisteredLogMessage = "Registered /start command";
        public const string HelpCommandName = "/help";
        public const string HelpCommandDescription = "Display available commands and usage information";
        public const string HelpCommandHandlerType = "HelpCommandHandler";
        public const string HelpCommandRegisteredLogMessage = "Registered /help command";
        public const string EchoCommandName = "/echo";
        public const string EchoCommandDescription = "Echo back the provided text";
        public const string EchoCommandHandlerType = "EchoCommandHandler";
        public const string EchoCommandRegisteredLogMessage = "Registered /echo command";
        public const string EchoParameterName = "text";
        public const string EchoParameterType = "string";
        public const string EchoParameterDescription = "Text to echo";
        public const string SampleMessage = "Hello bot!";
        public const string SampleEchoMessage = "/echo Test message";
        public const string HandlingMessageLogTemplate = "Handling message: {Content}";
        public const string DefaultFirstName = "User";
        public const string DefaultLastName = "Test";
        public const string UserRetrievedLogTemplate = "User {UserId} retrieved/created";
        public const string MetadataSourceKey = "source";
        public const string MetadataSourceDirect = "direct";
        public const string MessageProcessedLogTemplate = "Message processed with status: {Status}";
        public const string MessageProcessingErrorLogMessage = "Error processing message";
        public const long SampleUserId = 123456789;
        public const long SampleChatId = 123456789;
        public const int StandardCommandRateLimitPerMinute = 60;
        public const int EchoCommandRateLimitPerMinute = 30;
    }
}
