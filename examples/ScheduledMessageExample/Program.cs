using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TelegramBotFramework.Configuration;
using TelegramBotFramework.Integration;
using TelegramBotFramework.Models;
using TelegramBotFramework.Services;

// Example demonstrating how to use ScheduledMessageService
var services = new ServiceCollection();

// Create bot configuration (in production, use environment variables or config file)
var botConfig = new BotConfiguration
{
    BotToken = "YOUR_BOT_TOKEN_HERE",
    BotUsername = "YOUR_BOT_USERNAME_HERE",
    LogLevel = LogLevel.Info
};

// Register services
services.AddTelegramBotFramework(botConfig);

// Build service provider
var serviceProvider = services.BuildServiceProvider();

// Get the scheduled message service
var scheduledMessageService = serviceProvider.GetRequiredService<IScheduledMessageService>();

Console.WriteLine("Telegram Bot Framework - Scheduled Message Example");
Console.WriteLine("================================================\n");

// Example 1: Schedule a message to be sent at a specific time
Console.WriteLine("Example 1: Schedule message for specific time");
var messageId1 = scheduledMessageService.ScheduleMessageAsync(
    chatId: 123456789L,
    text: "🎉 Good morning! This is your scheduled reminder.",
    sendAt: DateTimeOffset.UtcNow.AddSeconds(5)
).Result;

Console.WriteLine($"✓ Scheduled message 1 (ID: {messageId1}) for 5 seconds from now");

// Example 2: Schedule a message with a delay
Console.WriteLine("\nExample 2: Schedule message with delay");
var messageId2 = scheduledMessageService.ScheduleMessageAsync(
    chatId: 123456789L,
    text: "⏰ This message will be sent in 10 seconds.",
    delay: TimeSpan.FromSeconds(10)
).Result;

Console.WriteLine($"✓ Scheduled message 2 (ID: {messageId2}) for 10 seconds from now");

// Example 3: Schedule multiple messages
Console.WriteLine("\nExample 3: Schedule multiple messages");
var messages = new[]
{
    "📅 Daily update at 9 AM",
    "📊 Weekly statistics at 10 AM",
    "🔔 Notification at 3 PM"
};

var scheduledIds = new List<string>();
foreach (var message in messages)
{
    var messageId = scheduledMessageService.ScheduleMessageAsync(
        chatId: 123456789L,
        text: message,
        sendAt: DateTimeOffset.UtcNow.AddMinutes(1)
    ).Result;
    scheduledIds.Add(messageId);
    Console.WriteLine($"✓ Scheduled message: '{message}' (ID: {messageId})");
}

// Example 4: List all scheduled messages
Console.WriteLine("\nExample 4: List all scheduled messages");
var allMessages = scheduledMessageService.GetAllScheduledMessages().ToList();
Console.WriteLine($"Total scheduled messages: {allMessages.Count}");
foreach (var msg in allMessages)
{
    Console.WriteLine($"  - ID: {msg.Id}, Chat: {msg.ChatId}, " +
                     $"Scheduled: {msg.ScheduledTime:yyyy-MM-dd HH:mm:ss}, " +
                     $"Status: {(msg.IsSent ? "Sent" : msg.IsCancelled ? "Cancelled" : "Pending")}");
}

// Example 5: Cancel a scheduled message
Console.WriteLine("\nExample 5: Cancel a scheduled message");
var messageId3 = scheduledMessageService.ScheduleMessageAsync(
    chatId: 123456789L,
    text: "This message will be cancelled",
    sendAt: DateTimeOffset.UtcNow.AddMinutes(5)
).Result;

Console.WriteLine($"✓ Scheduled message 3 (ID: {messageId3})");
Console.WriteLine($"✓ Cancelling message 3...");
var cancelled = scheduledMessageService.CancelScheduledMessage(messageId3);
Console.WriteLine($"✓ Cancellation successful: {cancelled}");

// Example 6: Get messages for a specific chat
Console.WriteLine("\nExample 6: Get messages for chat 123456789");
var chatMessages = scheduledMessageService.GetScheduledMessagesForChat(123456789L);
Console.WriteLine($"Messages for chat 123456789: {chatMessages.Count()}");

// Wait for scheduled messages to be sent
Console.WriteLine("\n⏳ Waiting for scheduled messages to be sent...");
Console.WriteLine("Press Ctrl+C to exit");

// Keep the example running
try
{
    await Task.Delay(Timeout.Infinite, new CancellationTokenSource().Token);
}
catch (TaskCanceledException)
{
    Console.WriteLine("\nExample completed.");
}
finally
{
    // Cleanup
    scheduledMessageService.Dispose();
}