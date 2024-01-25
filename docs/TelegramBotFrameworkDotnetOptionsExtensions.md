# TelegramBotFrameworkDotnetOptionsExtensions

Extension methods for configuring and validating `TelegramBotFrameworkOptions` in .NET applications using the `telegram-bot-framework-dotnet` library. These utilities assist with session management, message processing timeouts, and database configuration checks.

## API

### `Validate`
Validates the provided `TelegramBotFrameworkOptions` instance to ensure required properties are configured correctly. Throws an `ArgumentException` if validation fails.

```csharp
public static void Validate(TelegramBotFrameworkOptions options)
```

**Parameters**
- `options`: The options instance to validate.

**Throws**
- `ArgumentException`: When required properties are missing or invalid.

---

### `GetSessionTimeout`
Retrieves the session timeout duration from the provided options. Returns the configured timeout or a default value if not specified.

```csharp
public static TimeSpan GetSessionTimeout(TelegramBotFrameworkOptions options)
```

**Parameters**
- `options`: The options instance containing the session timeout configuration.

**Returns**
- `TimeSpan`: The session timeout duration.

---

### `GetMessageProcessingTimeout`
Retrieves the message processing timeout duration from the provided options. Returns the configured timeout or a default value if not specified.

```csharp
public static TimeSpan GetMessageProcessingTimeout(TelegramBotFrameworkOptions options)
```

**Parameters**
- `options`: The options instance containing the message processing timeout configuration.

**Returns**
- `TimeSpan`: The message processing timeout duration.

---
### `HasDatabaseConfigured`
Checks whether a database provider has been configured in the options.

```csharp
public static bool HasDatabaseConfigured(TelegramBotFrameworkOptions options)
```

**Parameters**
- `options`: The options instance to check for database configuration.

**Returns**
- `bool`: `true` if a database provider is configured; otherwise, `false`.

## Usage

### Basic Configuration and Validation
```csharp
var options = new TelegramBotFrameworkOptions
{
    SessionTimeout = TimeSpan.FromMinutes(30),
    MessageProcessingTimeout = TimeSpan.FromSeconds(10),
    DatabaseProvider = new SqliteDatabaseProvider("connection_string")
};

TelegramBotFrameworkDotnetOptionsExtensions.Validate(options);
bool hasDatabase = TelegramBotFrameworkDotnetOptionsExtensions.HasDatabaseConfigured(options);
TimeSpan sessionTimeout = TelegramBotFrameworkDotnetOptionsExtensions.GetSessionTimeout(options);
```

### Retrieving Timeouts with Fallback
```csharp
var minimalOptions = new TelegramBotFrameworkOptions();

TimeSpan sessionTimeout = TelegramBotFrameworkDotnetOptionsExtensions.GetSessionTimeout(minimalOptions);
TimeSpan messageTimeout = TelegramBotFrameworkDotnetOptionsExtensions.GetMessageProcessingTimeout(minimalOptions);

Console.WriteLine($"Session Timeout: {sessionTimeout.TotalSeconds}s");
Console.WriteLine($"Message Timeout: {messageTimeout.TotalSeconds}s");
```

## Notes

- **Thread Safety**: All methods are thread-safe as they only read from the `TelegramBotFrameworkOptions` instance and perform no state modifications.
- **Default Values**: When timeouts are not explicitly configured, `GetSessionTimeout` and `GetMessageProcessingTimeout` return `TimeSpan.Zero`. Ensure your application handles this case appropriately.
- **Validation Order**: Call `Validate` before using other extension methods to avoid runtime issues with uninitialized properties.
- **Database Check**: `HasDatabaseConfigured` returns `false` if `DatabaseProvider` is `null` or not properly initialized.
