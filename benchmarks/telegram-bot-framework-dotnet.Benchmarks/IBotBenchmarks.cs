using TelegramBotFramework.Models;

namespace TelegramBotFramework.Benchmarks;

public interface IBotBenchmarks
{
    void Setup();
    Task<TelegramBotFramework.Models.ExecutionContext> ProcessMessageBenchmark();
    Task<UserSession> GetUserSessionBenchmark();
    Task<bool> EndUserSessionBenchmark();
}