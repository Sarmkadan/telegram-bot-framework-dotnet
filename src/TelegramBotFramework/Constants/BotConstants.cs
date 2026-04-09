// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Constants;

/// <summary>
/// Core bot framework constants.
/// </summary>
public static class BotConstants
{
    // Command prefixes and delimiters
    public const string CommandPrefix = "/";
    public const string CommandParameterDelimiter = " ";
    public const string CommandParamSeparator = ":";

    // Session and context keys
    public const string CurrentMenuContextKey = "current_menu";
    public const string UserStateContextKey = "user_state";
    public const string CommandHistoryContextKey = "command_history";
    public const string SessionLanguageContextKey = "language";
    public const string LastCommandContextKey = "last_command";

    // Message types and statuses
    public const string TextMessageType = "text";
    public const string CallbackMessageType = "callback";
    public const string DocumentMessageType = "document";

    // Default values
    public const int DefaultSessionTimeoutMinutes = 30;
    public const int DefaultMessageTimeoutSeconds = 10;
    public const int DefaultRateLimitPerMinute = 30;
    public const int DefaultMaxConcurrentRequests = 10;

    // Error messages
    public const string CommandNotFoundMessage = "❌ Command not found. Type /help for available commands.";
    public const string InsufficientPermissionsMessage = "❌ You don't have permission to execute this command.";
    public const string SessionExpiredMessage = "⏰ Your session has expired. Please start again.";
    public const string RateLimitExceededMessage = "⏱️ You're sending requests too fast. Please wait a moment.";
    public const string CommandExecutionErrorMessage = "❌ An error occurred while executing the command.";
    public const string GenericErrorMessage = "❌ An unexpected error occurred. Please try again later.";

    // Success messages
    public const string CommandExecutedSuccessfullyMessage = "✅ Command executed successfully.";
    public const string MenuDisplayedMessage = "📋 Menu displayed.";
    public const string SettingsSavedMessage = "✅ Settings saved successfully.";

    // Metadata keys
    public const string UserExecutionTimeKey = "execution_time_ms";
    public const string CommandHandlerTypeKey = "handler_type";
    public const string ErrorStackTraceKey = "stack_trace";
    public const string RequestIdKey = "request_id";

    // Cache keys
    public const string UserCacheKeyPrefix = "user_";
    public const string SessionCacheKeyPrefix = "session_";
    public const string CommandCacheKeyPrefix = "command_";
    public const string MenuCacheKeyPrefix = "menu_";

    // Timeouts and delays
    public const int CommandExecutionTimeoutMs = 30000;
    public const int DatabaseQueryTimeoutSeconds = 15;
    public const int WebhookTimeoutSeconds = 30;

    // Special command names
    public const string StartCommand = "start";
    public const string HelpCommand = "help";
    public const string CancelCommand = "cancel";
    public const string BackCommand = "back";
    public const string MenuCommand = "menu";
    public const string SettingsCommand = "settings";
    public const string StatusCommand = "status";
}

/// <summary>
/// HTTP and API related constants.
/// </summary>
public static class ApiConstants
{
    public const string TelegramApiBaseUrl = "https://api.telegram.org/bot";
    public const string ContentTypeJson = "application/json";
    public const string ContentTypeForm = "application/x-www-form-urlencoded";

    public const int DefaultApiTimeoutSeconds = 30;
    public const int MaxRetries = 3;
    public const int RetryDelayMilliseconds = 1000;
}

/// <summary>
/// Database and storage constants.
/// </summary>
public static class StorageConstants
{
    public const string DefaultDatabaseName = "TelegramBot";
    public const int DefaultConnectionPoolSize = 10;
    public const int ConnectionTimeoutSeconds = 30;

    public const string UsersTableName = "Users";
    public const string CommandsTableName = "Commands";
    public const string MessagesTableName = "Messages";
    public const string SessionsTableName = "Sessions";
    public const string MenusTableName = "Menus";
}

/// <summary>
/// Localization and formatting constants.
/// </summary>
public static class LocalizationConstants
{
    public const string DefaultLanguage = "en";
    public const string EnglishLanguageCode = "en";
    public const string UkrainianLanguageCode = "uk";

    public const string DateTimeFormatFull = "yyyy-MM-dd HH:mm:ss";
    public const string DateTimeFormatShort = "yyyy-MM-dd";
    public const string TimeFormatShort = "HH:mm";
}
