#nullable enable
namespace TelegramBotFramework.Caching;

using System.Threading.Tasks;

/// <summary>
/// Interface for local cache provider.
/// </summary>
public interface ILocalCacheProvider
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
    Task RemoveAsync(string key);
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
    Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null);
    Task FlushAsync();
    Task<CacheStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default);
}