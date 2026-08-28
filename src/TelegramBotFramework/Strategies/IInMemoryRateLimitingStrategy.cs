#nullable enable

namespace TelegramBotFramework.Strategies;

/// <summary>
/// Interface for in-memory rate limiting strategy.
/// </summary>
public interface IInMemoryRateLimitingStrategy
{
    bool IsRequestAllowed(string identifier);
    int GetRemainingRequests(string identifier);
    Task<bool> IsActionAllowedAsync(string key, int limit, TimeSpan interval, CancellationToken cancellationToken = default);
}