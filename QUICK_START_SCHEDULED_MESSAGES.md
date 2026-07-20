# Quick Start: ScheduledMessageService

This guide shows you how to quickly start using the ScheduledMessageService in your Telegram bot.

## 1. Basic Setup

The service is automatically registered when you set up the framework:

```csharp
using Microsoft.Extensions.DependencyInjection;
using TelegramBotFramework.Configuration;
using TelegramBotFramework.Services;

// Configure your bot
var botConfig = new BotConfiguration
{
    BotToken = "YOUR_BOT_TOKEN",
    BotUsername = "YOUR_BOT_USERNAME",
    LogLevel = LogLevel.Info
};

// Register services
var services = new ServiceCollection();
services.AddTelegramBotFramework(botConfig);

// Build service provider
var serviceProvider = services.BuildServiceProvider();

// Get the scheduled message service
var scheduledMessageService = serviceProvider.GetRequiredService<IScheduledMessageService>();
```

## 2. Schedule a Message (Quick Examples)

### Schedule for a specific time
```csharp
// Send tomorrow at 9 AM
var messageId = await scheduledMessageService.ScheduleMessageAsync(
    chatId: 123456789L,
    text: "Good morning! Here's your daily update.",
    sendAt: DateTimeOffset.UtcNow.Date.AddDays(1).AddHours(9)
);
```

### Schedule with a delay
```csharp
// Send in 30 minutes
var messageId = await scheduledMessageService.ScheduleMessageAsync(
    chatId: 123456789L,
    text: "Reminder: Meeting in 30 minutes",
    delay: TimeSpan.FromMinutes(30)
);
```

### Schedule immediately (with delay)
```csharp
// Send in 5 seconds (good for testing)
var messageId = await scheduledMessageService.ScheduleMessageAsync(
    chatId: 123456789L,
    text: "Test message",
    delay: TimeSpan.FromSeconds(5)
);
```

## 3. Manage Scheduled Messages

### Cancel a scheduled message
```csharp
bool cancelled = scheduledMessageService.CancelScheduledMessage(messageId);
if (cancelled)
{
    Console.WriteLine("Message cancelled successfully!");
}
```

### List all scheduled messages
```csharp
var allMessages = scheduledMessageService.GetAllScheduledMessages();
foreach (var msg in allMessages)
{
    Console.WriteLine($"ID: {msg.Id}");
    Console.WriteLine($"  Chat: {msg.ChatId}");
    Console.WriteLine($"  Scheduled: {msg.ScheduledTime}");
    Console.WriteLine($"  Status: {(msg.IsSent ? "Sent" : msg.IsCancelled ? "Cancelled" : "Pending")}");
}
```

### Get messages for a specific chat
```csharp
var chatMessages = scheduledMessageService.GetScheduledMessagesForChat(123456789L);
Console.WriteLine($"Found {chatMessages.Count()} scheduled messages for this chat");
```

## 4. Complete Example: Daily Reminder Bot

```csharp
using Microsoft.Extensions.DependencyInjection;
using TelegramBotFramework.Configuration;
using TelegramBotFramework.Services;

public class DailyReminderBot
{
    private readonly IScheduledMessageService _scheduler;
    private readonly long _userChatId;
    
    public DailyReminderBot(string botToken, string botUsername, long userChatId)
    {
        var botConfig = new BotConfiguration
        {
            BotToken = botToken,
            BotUsername = botUsername,
            LogLevel = LogLevel.Info
        };
        
        var services = new ServiceCollection();
        services.AddTelegramBotFramework(botConfig);
        var serviceProvider = services.BuildServiceProvider();
        
        _scheduler = serviceProvider.GetRequiredService<IScheduledMessageService>();
        _userChatId = userChatId;
    }
    
    public async Task StartDailyReminders(string reminderText, TimeSpan reminderTime)
    {
        // Schedule first reminder
        await ScheduleNextReminder(reminderText, reminderTime);
        
        // Keep scheduling future reminders
        while (true)
        {
            await Task.Delay(TimeSpan.FromDays(1));
            await ScheduleNextReminder(reminderText, reminderTime);
        }
    }
    
    private async Task ScheduleNextReminder(string text, TimeSpan timeOfDay)
    {
        var nextReminderTime = DateTimeOffset.UtcNow.Date.Add(timeOfDay);
        
        // If time already passed today, schedule for tomorrow
        if (nextReminderTime < DateTimeOffset.UtcNow)
        {
            nextReminderTime = nextReminderTime.AddDays(1);
        }
        
        await _scheduler.ScheduleMessageAsync(
            chatId: _userChatId,
            text: text,
            sendAt: nextReminderTime
        );
    }
}

// Usage:
var bot = new DailyReminderBot("YOUR_TOKEN", "YOUR_BOT", 123456789L);
await bot.StartDailyReminders("🌞 Good morning! Here's your daily update.", TimeSpan.FromHours(9));
```

## 5. Common Patterns

### One-time notification
```csharp
// Send a reminder in 2 hours
var messageId = await scheduler.ScheduleMessageAsync(
    chatId: userId,
    text: "Don't forget to check your messages!",
    delay: TimeSpan.FromHours(2)
);
```

### Weekly digest
```csharp
// Send every Monday at 8 AM
var messageId = await scheduler.ScheduleMessageAsync(
    chatId: userId,
    text: "📊 Here's your weekly digest...",
    sendAt: DateTimeOffset.UtcNow.Date.AddDays(7 - (int)DateTimeOffset.UtcNow.DayOfWeek)
                   .AddHours(8)
);
```

### Event reminder
```csharp
// Send reminder 1 hour before an event
var eventTime = DateTimeOffset.UtcNow.AddHours(3);
var reminderTime = eventTime.AddHours(-1);

var messageId = await scheduler.ScheduleMessageAsync(
    chatId: userId,
    text: "🚨 Event starting in 1 hour!",
    sendAt: reminderTime
);
```

## 6. Important Notes

### Error Handling
The service automatically retries failed sends up to 3 times with 30-second intervals.

### Thread Safety
All operations are thread-safe - you can schedule/cancel from multiple threads.

### Resource Cleanup
Always dispose the service when done:
```csharp
scheduledMessageService.Dispose();
```

Or use it in a using statement:
```csharp
using var scheduler = serviceProvider.GetRequiredService<IScheduledMessageService>();
// Use scheduler...
```

### Validation
The service validates inputs:
- ChatId must be positive (> 0)
- Text cannot be null or empty
- Send time must be in the future

### Logging
All operations are logged via ILogger. For debugging, set LogLevel to Debug.

## 7. Need Help?

Check the full documentation:
- **docs/ScheduledMessageService.md** - Complete API reference
- **IMPLEMENTATION_SUMMARY.md** - Technical details
- **examples/ScheduledMessageExample/Program.cs** - Working examples

## Quick Checklist

- [ ] Added `services.AddTelegramBotFramework(botConfig)` to DI setup
- [ ] Injected `IScheduledMessageService`
- [ ] Using `await scheduler.ScheduleMessageAsync(...)` to schedule messages
- [ ] Using `scheduler.CancelScheduledMessage(id)` to cancel
- [ ] Disposing the service when done

That's it! You're ready to schedule messages in your Telegram bot. 🎉
