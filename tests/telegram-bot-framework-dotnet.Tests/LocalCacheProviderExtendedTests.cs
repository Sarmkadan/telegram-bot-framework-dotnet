using System;
using System.Threading.Tasks;
using TelegramBotFramework.Caching;
using Xunit;

namespace TelegramBotFramework.Tests;

public class LocalCacheProviderExtendedTests
{
    private readonly LocalCacheProvider _cache;

    public LocalCacheProviderExtendedTests()
    {
        _cache = new LocalCacheProvider();
    }

    [Fact]
    public async Task SetAsync_GetAsync_ReturnsStoredValue()
    {
        await _cache.SetAsync("key1", "value1");
        var result = await _cache.GetAsync<string>("key1");
        Assert.Equal("value1", result);
    }

    [Fact]
    public async Task GetAsync_NonExistentKey_ReturnsDefault()
    {
        var result = await _cache.GetAsync<string>("nonexistent");
        Assert.Null(result);
    }

    [Fact]
    public async Task ExistsAsync_ReturnsTrueForExistingKey()
    {
        await _cache.SetAsync("key2", "value2");
        Assert.True(await _cache.ExistsAsync("key2"));
    }

    [Fact]
    public async Task RemoveAsync_KeyDoesNotExistInCache()
    {
        await _cache.SetAsync("key3", "value3");
        await _cache.RemoveAsync("key3");
        Assert.False(await _cache.ExistsAsync("key3"));
    }

    [Fact]
    public async Task GetOrCreateAsync_CreatesValueIfNotExists()
    {
        var result = await _cache.GetOrCreateAsync("key4", () => Task.FromResult("value4"));
        Assert.Equal("value4", result);
        Assert.Equal("value4", await _cache.GetAsync<string>("key4"));
    }

    [Fact]
    public async Task FlushAsync_ResetsStatistics()
    {
        await _cache.SetAsync("key5", "value5");
        await _cache.GetAsync<string>("key5");
        await _cache.FlushAsync();
        var stats = await _cache.GetStatisticsAsync();
        Assert.Equal(0, stats.ItemCount);
        Assert.Equal(0, stats.HitCount);
        Assert.Equal(0, stats.SetCount);
    }

    [Fact]
    public async Task GetAsync_ExpiredItem_ReturnsDefault()
    {
        await _cache.SetAsync("key6", "value6", TimeSpan.FromMilliseconds(50));
        await Task.Delay(150);
        var result = await _cache.GetAsync<string>("key6");
        Assert.Null(result);
        Assert.False(await _cache.ExistsAsync("key6"));
    }
}
