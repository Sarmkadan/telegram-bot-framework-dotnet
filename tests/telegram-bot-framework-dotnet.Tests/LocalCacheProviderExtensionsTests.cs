using Xunit;
using FluentAssertions;
using TelegramBotFramework.Caching;

namespace TelegramBotFramework.Tests;

public sealed class LocalCacheProviderExtensionsTests
{
    private readonly LocalCacheProvider _provider = new();

    [Fact]
    public async Task TryGetAsync_ExistingKey_ReturnsSuccessAndValue()
    {
        const string key = "testKey";
        const string value = "testValue";
        await _provider.SetAsync(key, value);

        var result = await _provider.TryGetAsync<string>(key);

        result.Success.Should().BeTrue();
        result.Value.Should().Be(value);
    }

    [Fact]
    public async Task TryGetAsync_NonExistingKey_ReturnsFailure()
    {
        var result = await _provider.TryGetAsync<string>("missingKey");

        result.Success.Should().BeFalse();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task GetOrCreateAsync_ExistingKey_ReturnsExistingValue()
    {
        const string key = "testKey";
        const string existingValue = "existing";
        await _provider.SetAsync(key, existingValue);

        var result = await _provider.GetOrCreateAsync(key, () => "new");

        result.Should().Be(existingValue);
    }

    [Fact]
    public async Task GetOrCreateAsync_NewKey_ReturnsCreatedValue()
    {
        const string key = "testKey";
        const string newValue = "new";

        var result = await _provider.GetOrCreateAsync(key, () => newValue);

        result.Should().Be(newValue);
        (await _provider.GetAsync<string>(key)).Should().Be(newValue);
    }

    [Fact]
    public async Task GetManyAsync_ExistingKeys_ReturnsAllValues()
    {
        await _provider.SetAsync("key1", "val1");
        await _provider.SetAsync("key2", "val2");
        var keys = new[] { "key1", "key2", "key3" };

        var result = await _provider.GetManyAsync<string>(keys);

        result.Should().HaveCount(3);
        result["key1"].Should().Be("val1");
        result["key2"].Should().Be("val2");
        result["key3"].Should().BeNull();
    }

    [Fact]
    public async Task SetManyAsync_MultipleValues_SetsAll()
    {
        var values = new Dictionary<string, string> { { "k1", "v1" }, { "k2", "v2" } };

        await _provider.SetManyAsync(values);

        (await _provider.GetAsync<string>("k1")).Should().Be("v1");
        (await _provider.GetAsync<string>("k2")).Should().Be("v2");
    }

    [Fact]
    public async Task RemoveManyAsync_MultipleKeys_RemovesAll()
    {
        await _provider.SetAsync("k1", "v1");
        await _provider.SetAsync("k2", "v2");

        await _provider.RemoveManyAsync(new[] { "k1", "k2" });

        (await _provider.GetAsync<string>("k1")).Should().BeNull();
        (await _provider.GetAsync<string>("k2")).Should().BeNull();
    }
}
