# ScheduledMessageService

The `ScheduledMessageService` provides functionality for scheduling messages to be sent at specific times or after delays. It's designed to work seamlessly with the Telegram Bot Framework and integrates with the dependency injection system.


## Features

- Schedule messages to be sent at a specific `DateTimeOffset`
- Schedule messages with a `TimeSpan` delay
- Automatic retry mechanism for failed message sends (up to 3 attempts by default)
- Cancel scheduled messages before they are sent
- Track scheduled message status and history
- In-memory storage (persistent storage can be added as needed)
- Thread-safe implementation with proper locking

## Installation

The `ScheduledMessageService` is automatically registered when you call `AddTelegramBotFramework()` in your DI setup. No additional installation is required.


## Usage


### Basic Setup

```csharp
using Microsoft.Extensions.DependencyInjection;
using TelegramBotFramework.Configuration;
using TelegramBotFramework.Services;

// Register services
var services = new ServiceCollection();
services.AddTelegramBotFramework(botConfig);

// Build service provider
var serviceProvider = services.BuildServiceProvider();

// Get the scheduled message service
var scheduledMessageService = serviceProvider.GetRequiredService<IScheduledMessageService>();
```

### Scheduling Messages


#### Schedule for Specific Time

```csharp
// Schedule a message to be sent at 9 AM tomorrow
var messageId = await scheduledMessageService.ScheduleMessageAsync(
    chatId: 123456789L,
    text: "Good morning! This is your daily reminder.",
    sendAt: DateTimeOffset.UtcNow.Date.AddDays(1).AddHours(9)
);

Console.WriteLine($"Message scheduled with ID: {messageId}");
```


#### Schedule with Delay

```csharp
// Schedule a message to be sent in 30 minutes
var messageId = await scheduledMessageService.ScheduleMessageAsync(
    chatId: 123456789L,
    text: "This is a delayed notification.",
    delay: TimeSpan.FromMinutes(30)
);
```

### Managing Scheduled Messages


#### Cancel a Scheduled Message

```csharp
var cancelled = scheduledMessageService.CancelScheduledMessage(messageId);
if (cancelled)
{
    Console.WriteLine("Message successfully cancelled");
}
```

#### Get All Scheduled Messages

```csharp
var allMessages = scheduledMessageService.GetAllScheduledMessages();
foreach (var message in allMessages)
{
    Console.WriteLine($"ID: {message.Id}, Chat: {message.ChatId}, " +
                     $"Scheduled: {message.ScheduledTime}, Status: " +
                     $"{(message.IsSent ? "Sent" : message.IsCancelled ? "Cancelled" : "Pending")}");
}
```

#### Get Messages for a Specific Chat

```csharp
var chatMessages = scheduledMessageService.GetScheduledMessagesForChat(123456789L);
Console.WriteLine($"Found {chatMessages.Count()} scheduled messages for this chat");
```

#### Get a Specific Scheduled Message

```csharp
var message = scheduledMessageService.GetScheduledMessage(messageId);
if (message != null)
{
    Console.WriteLine($"Message status: {message.IsSent}");
}
```

## ScheduledMessage Class

The `ScheduledMessage` class contains the following properties:

- `Id` (string): Unique identifier for the scheduled message
- `ChatId` (long): The chat identifier where the message will be sent
- `Text` (string): The message content
- `ScheduledTime` (DateTimeOffset): When the message is scheduled to be sent
- `CreatedAt` (DateTimeOffset): When the message was scheduled
- `IsCancelled` (bool): Whether the message has been cancelled
- `IsSent` (bool): Whether the message has been successfully sent
- `SentAt` (DateTimeOffset?): When the message was sent (null if not sent)
- `ErrorMessage` (string?): Error message if sending failed
- `NextAttemptTime` (DateTimeOffset?): Next retry time for failed messages
- `AttemptCount` (int): Number of send attempts made

## Error Handling and Retries

The service automatically handles failed message sends with retry logic:

- First attempt: Immediately
- Second attempt: After 30 seconds (if first fails)
- Third attempt: After another 30 seconds (if second fails)
- If all attempts fail, the message is marked with an error

You can customize the retry behavior by modifying the `_defaultRetryDelay` and `_maxRetryAttempts` fields in the `ScheduledMessageService` class.

## Thread Safety

The service is fully thread-safe with proper locking mechanisms:
- All public methods are thread-safe
- Internal collections are protected by locks
- Timers are properly disposed

## Disposal

Always call `Dispose()` when you're done with the service to clean up timers and resources:

```csharp
scheduledMessageService.Dispose();
```

Or use it in a `using` statement:

```csharp
using var scheduledMessageService = serviceProvider.GetRequiredService<IScheduledMessageService>();
// Use the service...
```

## Integration with Other Services


The `ScheduledMessageService` works well with other framework services:

```csharp
// Combine with MessageService
var messageService = serviceProvider.GetRequiredService<IMessageService>();

// Schedule a message
var messageId = await scheduledMessageService.ScheduleMessageAsync(chatId, text, sendAt);

// Later, retrieve information about the scheduled message
var scheduledMsg = scheduledMessageService.GetScheduledMessage(messageId);
```

## Example: Daily Reminder Bot

```csharp
// Schedule daily reminders
public async Task ScheduleDailyReminders(IScheduledMessageService scheduler, long chatId, string reminderText, TimeSpan reminderTime)
{
    // Schedule for today
    var todayTime = DateTimeOffset.UtcNow.Date.Add(reminderTime);
    var messageId = await scheduler.ScheduleMessageAsync(
        chatId: chatId,
        text: reminderText,
        sendAt: todayTime > DateTimeOffset.UtcNow ? todayTime : todayTime.AddDays(1)
    );
    
    // Schedule for every day after
    while (true)
    {
        await Task.Delay(TimeSpan.FromDays(1));
        await scheduler.ScheduleMessageAsync(
            chatId: chatId,
            text: reminderText,
            sendAt: DateTimeOffset.UtcNow.Date.AddDays(1).Add(reminderTime)
        );
    }
}
```

## Performance Considerations

- Each scheduled message uses a `System.Threading.Timer`
- Memory usage is proportional to the number of scheduled messages
- For production with many scheduled messages, consider:
  - Implementing persistent storage
  - Adding cleanup for old/completed messages
  - Using a database-backed scheduler

## API Reference

### IScheduledMessageService Interface

```csharp
public interface IScheduledMessageService : IDisposable
{
    Task<string> ScheduleMessageAsync(long chatId, string text, DateTimeOffset sendAt, CancellationToken cancellationToken = default);
    Task<string> ScheduleMessageAsync(long chatId, string text, TimeSpan delay, CancellationToken cancellationToken = default);
    bool CancelScheduledMessage(string messageId);
    IEnumerable<ScheduledMessage> GetAllScheduledMessages();
    ScheduledMessage? GetScheduledMessage(string messageId);
    IEnumerable<ScheduledMessage> GetScheduledMessagesForChat(long chatId);
}
```

## Troubleshooting

### Messages not being sent

1. Verify the chat ID is correct
2. Check that the Telegram Bot API client is properly configured
3. Ensure the bot has permissions to send messages to the chat
4. Check logs for error messages

### Scheduled time in the past

The service validates that scheduled times are in the future and throws an `ArgumentException` if not.

### High memory usage

If you're scheduling many messages, consider:
- Implementing cleanup logic to remove completed messages
- Using persistent storage instead of in-memory
- Limiting the number of concurrent scheduled messages

## Extending the Service

You can extend the `ScheduledMessageService` by:

1. Creating a derived class and overriding virtual methods
2. Adding new methods to the interface
3. Implementing persistent storage
4. Adding custom retry policies
5. Adding notification callbacks when messages are sent

## See Also

- [MessageService](MessageService.md) - For processing incoming messages
- [TelegramApiClient](ITelegramApiClient.md) - The underlying API client
- [DependencyInjectionSetup](DependencyInjectionSetup.md) - For service registration
