using TelegramBotFramework.Models;

namespace TelegramBotFramework.Benchmarks;

public interface IBotBenchmarks
{
    void Setup();
    Task<TelegramBotFramework.Models.ExecutionContext> ProcessMessageBenchmark(CancellationToken cancellationToken);
    Task<UserSession> GetUserSessionBenchmark(CancellationToken cancellationToken);
    Task<bool> EndUserSessionBenchmark(CancellationToken cancellationToken);
}