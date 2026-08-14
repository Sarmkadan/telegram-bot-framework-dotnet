using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TelegramBotFramework.Caching;
using Xunit;

namespace TelegramBotFramework.Tests.Caching;

public class DistributedCacheProviderTests
{
    private class TestableCacheProvider : DistributedCacheProvider
    {
        private readonly Dictionary<string, string> _storage = new();

        public TestableCacheProvider() : base(null) { }

        protected override Task<string?> GetValueAsync(string key) =>
            Task.FromResult(_storage.TryGetValue(key, out var val) ? val : null);

        protected override Task SetValueAsync(string key, string value, TimeSpan? expiration)
        {
            _storage[key] = value;
            return Task.CompletedTask;
        }

        protected override Task RemoveValueAsync(string key)
        {
            _storage.Remove(key);
            return Task.CompletedTask;
        }

        protected override Task<bool> KeyExistsAsync(string key) =>
            Task.FromResult(_storage.ContainsKey(key));

        protected override Task FlushAllAsync()
        {
            _storage.Clear();
            return Task.CompletedTask;
        }

        protected override Task<CacheStatistics> GetStatsAsync() =>
            Task.FromResult(new CacheStatistics());
    }

    private record TestData(int Id, string Name);

    [Fact]
    public async Task SetAndGetAsync_RoundTripsValue()
    {
        var provider = new TestableCacheProvider();
        var data = new TestData(1, "Test");

        await provider.SetAsync("key", data);
        var result = await provider.GetAsync<TestData>("key");

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Test", result.Name);
    }

    [Fact]
    public async Task GetAsync_ReturnsNull_WhenKeyDoesNotExist()
    {
        var provider = new TestableCacheProvider();
        var result = await provider.GetAsync<TestData>("missing");
        Assert.Null(result);
    }

    [Fact]
    public async Task RemoveAsync_RemovesKey()
    {
        var provider = new TestableCacheProvider();
        await provider.SetAsync("key", new TestData(1, "Test"));
        
        await provider.RemoveAsync("key");
        
        Assert.False(await provider.ExistsAsync("key"));
    }

    [Fact]
    public async Task ExistsAsync_ReturnsTrue_WhenKeyExists()
    {
        var provider = new TestableCacheProvider();
        await provider.SetAsync("key", new TestData(1, "Test"));
        
        var exists = await provider.ExistsAsync("key");
        
        Assert.True(exists);
    }

    [Fact]
    public async Task GetOrCreateAsync_CreatesAndCaches_WhenNotExists()
    {
        var provider = new TestableCacheProvider();
        var data = new TestData(2, "New");

        var result = await provider.GetOrCreateAsync("key", () => Task.FromResult(data));
        
        Assert.Equal(2, result.Id);
        Assert.True(await provider.ExistsAsync("key"));
    }

    [Fact]
    public async Task GetOrCreateAsync_ReturnsCached_WhenExists()
    {
        var provider = new TestableCacheProvider();
        await provider.SetAsync("key", new TestData(3, "Cached"));

        var result = await provider.GetOrCreateAsync("key", () => Task.FromResult(new TestData(99, "Factory")));
        
        Assert.Equal(3, result.Id);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Operations_HandleNullOrWhiteSpaceKey_Gracefully(string? key)
    {
        var provider = new TestableCacheProvider();

        // Should not throw
        await provider.GetAsync<TestData>(key);
        await provider.SetAsync(key, new TestData(1, "Test"));
        await provider.RemoveAsync(key);
        var exists = await provider.ExistsAsync(key);
        
        Assert.False(exists);
    }
}
