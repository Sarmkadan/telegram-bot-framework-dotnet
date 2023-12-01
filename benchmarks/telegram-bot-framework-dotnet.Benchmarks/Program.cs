using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TelegramBotFramework.Configuration;
using TelegramBotFramework.Models;
using TelegramBotFramework.Services;

namespace TelegramBotFramework.Benchmarks;

/// <summary>
/// A benchmark class for measuring the performance of the Telegram Bot Framework.
/// </summary>
[MemoryDiagnoser]
public class BotBenchmarks
{
    /// <summary>
    /// The service provider used to resolve dependencies.
    /// </summary>
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// The bot orchestrator used to process user messages and retrieve user sessions.
    /// </summary>
    private readonly IBotOrchestrator _botOrchestrator;

    /// <summary>
    /// The user ID used for benchmarking.
    /// </summary>
    private readonly long _userId = 12345;

    /// <summary>
    /// The chat ID used for benchmarking.
    /// </summary>
    private readonly long _chatId = 67890;

    /// <summary>
    /// Initializes a new instance of the <see cref="BotBenchmarks"/> class.
    /// </summary>
    public BotBenchmarks()
    {
        var services = new ServiceCollection();
        var config = new BotConfiguration
        {
            BotToken = "test-token",
            BotUsername = "test-bot"
        };
        
        services.AddTelegramBotFramework(config);
        
        // Mocking logging to be silent for benchmarks
        services.AddLogging(builder => builder.AddFilter("TelegramBotFramework", Microsoft.Extensions.Logging.LogLevel.None));
        
        _serviceProvider = services.BuildServiceProvider();
        _botOrchestrator = _serviceProvider.GetRequiredService<IBotOrchestrator>();
    }

    /// <summary>
    /// Sets up the benchmark by ensuring a session exists for the specified user ID.
    /// </summary>
    [IterationSetup]
    public void Setup()
    {
        // Make sure a session exists
        try
        {
            _botOrchestrator.GetUserSessionAsync(_userId).GetAwaiter().GetResult();
        }
        catch
        {
            _botOrchestrator.ProcessUserMessageAsync(_userId, _chatId, "/start", "TestUser").GetAwaiter().GetResult();
        }
    }

    /// <summary>
    /// Measures the performance of processing a user message.
    /// </summary>
    /// <returns>The execution context of the processed message.</returns>
    [Benchmark]
    public async Task<TelegramBotFramework.Models.ExecutionContext> ProcessMessageBenchmark()
    {
        return await _botOrchestrator.ProcessUserMessageAsync(_userId, _chatId, "/echo", "TestUser");
    }

    /// <summary>
    /// Measures the performance of retrieving a user session.
    /// </summary>
    /// <returns>The user session for the specified user ID.</returns>
    [Benchmark]
    public async Task<TelegramBotFramework.Models.UserSession> GetUserSessionBenchmark()
    {
        return await _botOrchestrator.GetUserSessionAsync(_userId);
    }

    /// <summary>
    /// Measures the performance of ending a user session.
    /// </summary>
    /// <returns>A boolean indicating whether the session was ended successfully.</returns>
    [Benchmark]
    public async Task<bool> EndUserSessionBenchmark()
    {
        return await _botOrchestrator.EndUserSessionAsync(_userId);
    }
}

/// <summary>
/// The main program class.
/// </summary>
public class Program
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    /// <param name="args">The command-line arguments.</param>
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<BotBenchmarks>();
    }
}
