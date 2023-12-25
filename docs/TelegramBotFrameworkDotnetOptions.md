# TelegramBotFrameworkDotnetOptions

The `TelegramBotFrameworkDotnetOptions` class provides a centralized configuration schema for initializing and managing instances of the `telegram-bot-framework-dotnet` library. It encapsulates essential settings such as authentication tokens, connectivity parameters, performance constraints, and feature flags, ensuring that bot behavior remains consistent and configurable across different environments.

## API

*   **`BotToken`** (string): The authentication token obtained from BotFather to authorize bot API requests.
*   **`BotUsername`** (string): The registered username of the bot, used for identification and command parsing.
*   **`DatabaseConnectionString`** (string?): An optional connection string for persistent storage, enabling state management and session persistence. If not provided, certain framework features relying on state may be disabled.
*   **`SessionTimeoutMinutes`** (int): The duration in minutes after which an inactive user session is considered expired.
*   **`MessageProcessingTimeoutSeconds`** (int): The maximum allowable time in seconds for the bot to process an incoming update. If processing exceeds this limit, the request may be terminated.
*   **`MaxConcurrentRequests`** (int): The maximum number of concurrent API requests or message processing tasks permitted to ensure resource stability.
*   **`EnableLogging`** (bool): A toggle to enable or disable framework-level diagnostic logging.
*   **`EnableRateLimiting`** (bool): A toggle to enable or disable internal rate limiting mechanisms to comply with API usage policies.
*   **`RateLimitPerMinute`** (int): The number of allowed requests or interactions per minute when rate limiting is enabled.

## Usage

### Example 1: Configuration via `IOptions` in ASP.NET Core
```csharp
// appsettings.json
{
  "TelegramBot": {
    "BotToken": "123456:ABC-DEF1234ghIkl-zyx57W2v1u123ew11",
    "BotUsername": "MyCoolBot",
    "EnableLogging": true,
    "MaxConcurrentRequests": 10
  }
}

// Program.cs
builder.Services.Configure<TelegramBotFrameworkDotnetOptions>(
    builder.Configuration.GetSection("TelegramBot"));
```

### Example 2: Programmatic Configuration
```csharp
var options = new TelegramBotFrameworkDotnetOptions
{
    BotToken = "123456:ABC-DEF1234ghIkl-zyx57W2v1u123ew11",
    BotUsername = "MyCoolBot",
    DatabaseConnectionString = "Server=myServer;Database=myDB;...",
    SessionTimeoutMinutes = 30,
    MessageProcessingTimeoutSeconds = 5,
    MaxConcurrentRequests = 5,
    EnableLogging = true,
    EnableRateLimiting = true,
    RateLimitPerMinute = 20
};
```

## Notes

*   **Thread Safety**: While the `TelegramBotFrameworkDotnetOptions` class allows public mutation of its properties, it is standard practice to treat configuration objects as immutable once the application has completed its startup and dependency injection registration phase. Modifying these properties at runtime from multiple threads may lead to inconsistent behavior depending on how the framework consumes these options.
*   **Edge Cases**:
    *   If `BotToken` is missing or invalid, the framework will fail to authenticate with the Telegram API upon startup.
    *   If `DatabaseConnectionString` is null or invalid, functionality that relies on stateful storage (such as persistent sessions) will not operate correctly; it is recommended to validate this string during application startup if stateful features are required.
    *   Setting `MaxConcurrentRequests` or `RateLimitPerMinute` to excessively low values may result in message processing backlogs or frequent throttling by the Telegram API.
