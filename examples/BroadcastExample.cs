#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using Microsoft.Extensions.DependencyInjection;
using TelegramBotFramework.Services;

namespace TelegramBotFramework.Examples;

/// <summary>
/// Example demonstrating the usage of BroadcastService for sending messages to multiple chats.
/// </summary>
public static class BroadcastExample
{
    /// <summary>
    /// Demonstrates basic broadcast functionality.
    /// </summary>
    public static async Task RunBasicBroadcastAsync()
    {
        // Setup DI container
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddBroadcastService(options =>
        {
            options.MessagesPerSecond = 10; // Send 10 messages per second
            options.MaxConcurrency = 3;    // Max 3 concurrent sends
            options.MaxRetryAttempts = 2;   // Retry failed messages twice
        });

        var serviceProvider = services.BuildServiceProvider();
        var broadcastService = serviceProvider.GetRequiredService<IBroadcastService>();

        // List of chat IDs to broadcast to
        var chatIds = new long[] { 123456789L, 987654321L, 555555555L, 111111111L };
        var messageText = "Hello from BroadcastService! This is a broadcast message.";

        // Define progress callback
        Task ProgressCallback(BroadcastProgress progress)
        {
            Console.WriteLine($"Progress: {progress.ProgressPercentage}% - " +
                $"Sent: {progress.SuccessCount}, Failed: {progress.FailedCount}");
            return Task.CompletedTask;
        }

        // Execute broadcast
        var result = await broadcastService.BroadcastAsync(
            chatIds: chatIds,
            messageText: messageText,
            options: null,
            progressCallback: ProgressCallback);

        Console.WriteLine($"Broadcast completed: {result.SuccessCount} succeeded, {result.FailedCount} failed");

        if (result.Failures.Any())
        {
            Console.WriteLine("\nFailed chats:");
            foreach (var failure in result.Failures)
            {
                Console.WriteLine($"  Chat {failure.ChatId}: {failure.ErrorMessage}");
            }
        }
    }

    /// <summary>
    /// Demonstrates broadcast with custom message formatting.
    /// </summary>
    public static async Task RunCustomMessageBroadcastAsync()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddBroadcastService();

        var serviceProvider = services.BuildServiceProvider();
        var broadcastService = serviceProvider.GetRequiredService<IBroadcastService>();

        var chatIds = new long[] { 111111111L, 222222222L, 333333333L };
        var messageText = "Important announcement";

        // Custom message formatter that adds chat-specific information
        Task ProgressCallback(BroadcastProgress progress)
        {
            Console.WriteLine($"Progress: {progress.ProgressPercentage}%");
            return Task.CompletedTask;
        }

        var result = await broadcastService.BroadcastAsync(
            chatIds: chatIds,
            messageText: messageText,
            options: new BroadcastOptions
            {
                MessageFormatter = (text, chatId) => $"[{chatId}] {text}"
            },
            progressCallback: ProgressCallback);

        Console.WriteLine($"Broadcast with custom formatting: {result.SuccessCount} succeeded");
    }

    /// <summary>
    /// Demonstrates broadcast to users with cancellation support.
    /// </summary>
    public static async Task RunBroadcastToUsersAsync()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddBroadcastService();

        var serviceProvider = services.BuildServiceProvider();
        var broadcastService = serviceProvider.GetRequiredService<IBroadcastService>();

        // Simulated users (in real usage, these would come from UserService)
        var users = new Models.BotUser[]
        {
            new Models.BotUser { TelegramId = 111111111L, FirstName = "User", LastName = "One" },
            new Models.BotUser { TelegramId = 222222222L, FirstName = "User", LastName = "Two" },
            new Models.BotUser { TelegramId = 333333333L, FirstName = "User", LastName = "Three" }
        };

        var cts = new CancellationTokenSource();
        var messageText = "Welcome to our service!";

        // Start broadcast in background
        var broadcastTask = broadcastService.BroadcastToUsersAsync(
            users: users,
            messageText: messageText,
            options: new BroadcastOptions { MessagesPerSecond = 5 },
            progressCallback: async progress =>
            {
                Console.WriteLine($"User broadcast progress: {progress.ProgressPercentage}%");

                // Example: cancel after 30% progress
                if (progress.ProgressPercentage >= 30)
                {
                    Console.WriteLine("Cancelling broadcast...");
                    cts.Cancel();
                }
            },
            cancellationToken: cts.Token);

        var result = await broadcastTask;
        Console.WriteLine($"User broadcast result: {result.SuccessCount} succeeded, {result.FailedCount} failed");
    }

    /// <summary>
    /// Demonstrates getting rate limit statistics.
    /// </summary>
    public static void ShowRateLimitStats()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddBroadcastService(options =>
        {
            options.MessagesPerSecond = 20;
            options.MaxConcurrency = 5;
        });

        var serviceProvider = services.BuildServiceProvider();
        var broadcastService = serviceProvider.GetRequiredService<IBroadcastService>();

        var stats = broadcastService.GetRateLimitStats();
        Console.WriteLine($"Rate limit stats:");
        Console.WriteLine($"  Configured rate: {stats.MessagesPerSecond} msg/s");
        Console.WriteLine($"  Max concurrency: {stats.MaxConcurrency}");
        Console.WriteLine($"  Current concurrency: {stats.CurrentConcurrency}");
        Console.WriteLine($"  Total sent: {stats.TotalMessagesSent}");
        Console.WriteLine($"  Total failed: {stats.TotalMessagesFailed}");
        Console.WriteLine($"  Avg rate: {stats.AverageMessagesPerSecond:F2} msg/s");
    }
}
