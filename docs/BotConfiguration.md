# BotConfiguration
The `BotConfiguration` type is used to configure the settings of a Telegram bot, including authentication, database connections, logging, and other operational parameters. It provides a centralized way to manage the bot's behavior, allowing for customization and flexibility in its deployment and usage.

## API
The `BotConfiguration` type exposes the following public members:
* `BotToken`: a string representing the bot's authentication token.
* `BotUsername`: a string representing the bot's username.
* `OwnerId`: a nullable long integer representing the owner's ID.
* `DatabaseConnectionString`: a string representing the connection string to the database.
* `SessionTimeoutMinutes`: an integer representing the session timeout in minutes.
* `MessageProcessingTimeoutSeconds`: an integer representing the message processing timeout in seconds.
* `EnableLogging`: a boolean indicating whether logging is enabled.
* `LogLevel`: an enumeration of type `LogLevel` representing the logging level.
* `MaxConcurrentRequests`: an integer representing the maximum number of concurrent requests.
* `EnableWebhook`: a boolean indicating whether the webhook is enabled.
* `ApiKey`: a nullable string representing the API key.
* `WebhookUrl`: a nullable string representing the webhook URL.
* `WebhookSecret`: a nullable string representing the webhook secret.
* `CustomSettings`: a dictionary of strings representing custom settings.
* `AdminIds`: a list of long integers representing the admin IDs.
* `EnableRateLimiting`: a boolean indicating whether rate limiting is enabled.
* `RateLimitPerMinute`: an integer representing the rate limit per minute.
* `LocalizationLanguage`: a nullable string representing the localization language.
* `Validate`: a boolean indicating whether validation is enabled.
* `GetCustomSetting`: a nullable string representing a custom setting, taking a string parameter representing the setting key.

## Usage
Here are two examples of using the `BotConfiguration` type in C#:
```csharp
// Example 1: Creating a new BotConfiguration instance
var config = new BotConfiguration
{
    BotToken = "123456:ABC-DEF1234ghIkl-zyx57W2v1u123ew11",
    BotUsername = "mybot",
    OwnerId = 123456789,
    DatabaseConnectionString = "Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;",
    SessionTimeoutMinutes = 30,
    MessageProcessingTimeoutSeconds = 10,
    EnableLogging = true,
    LogLevel = LogLevel.Debug,
    MaxConcurrentRequests = 10,
    EnableWebhook = true,
    ApiKey = "myapikey",
    WebhookUrl = "https://example.com/webhook",
    WebhookSecret = "mywebhooksecret",
    CustomSettings = new Dictionary<string, string> { { "setting1", "value1" } },
    AdminIds = new List<long> { 123456789 },
    EnableRateLimiting = true,
    RateLimitPerMinute = 100,
    LocalizationLanguage = "en-US",
    Validate = true
};

// Example 2: Retrieving a custom setting
var customSetting = config.GetCustomSetting("setting1");
Console.WriteLine(customSetting); // Output: value1
```

## Notes
When using the `BotConfiguration` type, consider the following edge cases and thread-safety remarks:
* The `BotToken` and `ApiKey` properties should be kept secure, as they grant access to the bot's functionality.
* The `DatabaseConnectionString` property should be properly formatted to avoid connection issues.
* The `EnableLogging` and `LogLevel` properties can impact performance, so they should be used judiciously.
* The `MaxConcurrentRequests` property can help prevent overload, but it should be set according to the bot's expected usage.
* The `EnableWebhook` and `WebhookUrl` properties require a properly configured webhook to function correctly.
* The `CustomSettings` dictionary can be used to store arbitrary data, but it should be used sparingly to avoid cluttering the configuration.
* The `AdminIds` list should be populated with the IDs of trusted users to ensure proper access control.
* The `EnableRateLimiting` and `RateLimitPerMinute` properties can help prevent abuse, but they should be set according to the bot's expected usage.
* The `LocalizationLanguage` property can impact the bot's behavior, so it should be set according to the target audience.
* The `Validate` property can help ensure data integrity, but it should be used judiciously to avoid performance impacts.
* The `GetCustomSetting` method can throw a `KeyNotFoundException` if the specified setting is not found in the `CustomSettings` dictionary.
* The `BotConfiguration` type is not thread-safe by default, so it should be properly synchronized when accessed from multiple threads.
