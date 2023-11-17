using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TelegramBotFramework.Configuration;
using TelegramBotFramework.Models;
using TelegramBotFramework.Services;

namespace TelegramBotFramework.Benchmarks;

[MemoryDiagnoser]
public class BotBenchmarks
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IBotOrchestrator _botOrchestrator;
    private readonly long _userId = 12345;
    private readonly long _chatId = 67890;

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

    [Benchmark]
    public async Task<TelegramBotFramework.Models.ExecutionContext> ProcessMessageBenchmark()
    {
        return await _botOrchestrator.ProcessUserMessageAsync(_userId, _chatId, "/echo", "TestUser");
    }

    [Benchmark]
    public async Task<TelegramBotFramework.Models.UserSession> GetUserSessionBenchmark()
    {
        return await _botOrchestrator.GetUserSessionAsync(_userId);
    }

    [Benchmark]
    public async Task<bool> EndUserSessionBenchmark()
    {
        return await _botOrchestrator.EndUserSessionAsync(_userId);
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<BotBenchmarks>();
    }
}
