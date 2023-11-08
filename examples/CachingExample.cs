// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TelegramBotFramework.Caching;
using TelegramBotFramework.Services;

namespace TelegramBotFramework.Examples
{
    /// <summary>
    /// Caching example demonstrating performance optimization techniques using cache providers,
    /// cache invalidation patterns, and TTL management.
    /// </summary>
    public class CachingExample
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CachingExample> _logger;
        private readonly ICacheProvider _cacheProvider;
        private readonly IUserService _userService;

        public CachingExample(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _logger = serviceProvider.GetRequiredService<ILogger<CachingExample>>();
            _cacheProvider = serviceProvider.GetRequiredService<ICacheProvider>();
            _userService = serviceProvider.GetRequiredService<IUserService>();
        }

        public async Task RunAsync()
        {
            _logger.LogInformation("Starting CachingExample");

            try
            {
                await DemonstrateCacheOperationsAsync();
                await DemonstrateCacheExpirationAsync();
                await DemonstrateCachingPatterns();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CachingExample");
                throw;
            }
        }

        private async Task DemonstrateCacheOperationsAsync()
        {
            _logger.LogInformation("--- Cache Basic Operations ---");

            const string userKey = "user:123";
            var userData = new { Id = 123, Name = "John", Email = "john@example.com" };

            // Set value in cache
            await _cacheProvider.SetAsync(userKey, userData, TimeSpan.FromHours(1));
            _logger.LogInformation("Set value in cache: {Key}", userKey);

            // Get value from cache
            var cachedUser = await _cacheProvider.GetAsync(userKey);
            if (cachedUser != null)
            {
                _logger.LogInformation("Retrieved from cache: {Value}", cachedUser);
            }

            // Check if key exists
            var exists = await _cacheProvider.ExistsAsync(userKey);
            _logger.LogInformation("Key exists in cache: {Exists}", exists);

            // Remove value from cache
            await _cacheProvider.RemoveAsync(userKey);
            _logger.LogInformation("Removed value from cache: {Key}", userKey);

            // Verify removal
            var afterRemoval = await _cacheProvider.GetAsync(userKey);
            _logger.LogInformation("After removal, cache contains value: {HasValue}", afterRemoval != null);
        }

        private async Task DemonstrateCacheExpirationAsync()
        {
            _logger.LogInformation("--- Cache Expiration Management ---");

            const string tempKey = "temp_session:456";
            var sessionData = new { SessionId = "456", CreatedAt = DateTime.UtcNow };

            // Set with short TTL
            await _cacheProvider.SetAsync(tempKey, sessionData, TimeSpan.FromSeconds(2));
            _logger.LogInformation("Set temporary cache with 2 second TTL: {Key}", tempKey);

            // Verify exists
            var exists1 = await _cacheProvider.ExistsAsync(tempKey);
            _logger.LogInformation("Immediately after set, cache exists: {Exists}", exists1);

            // Wait for expiration
            await Task.Delay(3000);
            var exists2 = await _cacheProvider.ExistsAsync(tempKey);
            _logger.LogInformation("After 3 seconds (past TTL), cache exists: {Exists}", exists2);
        }

        private async Task DemonstrateCachingPatterns()
        {
            _logger.LogInformation("--- Caching Patterns ---");

            // Pattern 1: Cache-Aside (Get or Create)
            await DemonstrateCacheAsidePatternAsync();

            // Pattern 2: Bulk cache operations
            await DemonstrateBulkCacheOperationsAsync();

            // Pattern 3: Cache invalidation
            await DemonstrateCacheInvalidationAsync();
        }

        private async Task DemonstrateCacheAsidePatternAsync()
        {
            _logger.LogInformation("Pattern: Cache-Aside (Get or Create)");

            const string userId = "789";
            const string cacheKey = "user:profile:789";

            // Simulate function that gets user from database
            async Task<object> GetUserFromDatabaseAsync()
            {
                _logger.LogInformation("  [DB] Fetching user {UserId} from database", userId);
                await Task.Delay(100); // Simulate DB latency
                return new { Id = userId, Name = "Jane Doe", Email = "jane@example.com" };
            }

            // First call - hits database
            var user1 = await _cacheProvider.GetOrCreateAsync(
                cacheKey,
                GetUserFromDatabaseAsync,
                TimeSpan.FromMinutes(5)
            );
            _logger.LogInformation("  First call result: {Value}", user1);

            // Second call - hits cache
            var user2 = await _cacheProvider.GetOrCreateAsync(
                cacheKey,
                GetUserFromDatabaseAsync,
                TimeSpan.FromMinutes(5)
            );
            _logger.LogInformation("  Second call result (from cache): {Value}", user2);
        }

        private async Task DemonstrateBulkCacheOperationsAsync()
        {
            _logger.LogInformation("Pattern: Bulk Cache Operations");

            var cacheData = new Dictionary<string, object>
            {
                { "setting:theme", "dark" },
                { "setting:language", "en" },
                { "setting:timezone", "UTC" }
            };

            // Set multiple values
            foreach (var kvp in cacheData)
            {
                await _cacheProvider.SetAsync(kvp.Key, kvp.Value, TimeSpan.FromHours(24));
            }
            _logger.LogInformation("  Set {Count} values in cache", cacheData.Count);

            // Retrieve multiple values
            foreach (var key in cacheData.Keys)
            {
                var value = await _cacheProvider.GetAsync(key);
                _logger.LogInformation("  {Key}: {Value}", key, value);
            }

            // Clear all
            foreach (var key in cacheData.Keys)
            {
                await _cacheProvider.RemoveAsync(key);
            }
            _logger.LogInformation("  Cleared {Count} values from cache", cacheData.Count);
        }

        private async Task DemonstrateCacheInvalidationAsync()
        {
            _logger.LogInformation("Pattern: Cache Invalidation");

            const string userCacheKey = "user:stats:999";
            var stats = new { Views = 100, Clicks = 50, Conversions = 10 };

            // Set initial value
            await _cacheProvider.SetAsync(userCacheKey, stats, TimeSpan.FromHours(1));
            _logger.LogInformation("  Cached user stats: {Stats}", stats);

            // Simulate update
            _logger.LogInformation("  [Updating stats in database...]");
            await Task.Delay(50);

            // Invalidate cache to reflect new data
            await _cacheProvider.RemoveAsync(userCacheKey);
            _logger.LogInformation("  Invalidated cache after update");

            // Next access will reload from source
            var exists = await _cacheProvider.ExistsAsync(userCacheKey);
            _logger.LogInformation("  Cache exists after invalidation: {Exists}", exists);
        }
    }
}
