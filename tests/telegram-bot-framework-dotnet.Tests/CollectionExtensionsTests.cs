#nullable enable

using FluentAssertions;
using TelegramBotFramework.Utilities;
using Xunit;

namespace TelegramBotFramework.Tests;

public class CollectionExtensionsTests
{
    [Fact]
    public void GetOrDefault_ValidIndex_ReturnsItem()
    {
        var list = new List<string> { "a", "b", "c" };
        list.GetOrDefault(1).Should().Be("b");
    }

    [Fact]
    public void GetOrDefault_InvalidIndex_ReturnsDefaultValue()
    {
        var list = new List<string> { "a", "b", "c" };
        list.GetOrDefault(5, "default").Should().Be("default");
    }

    [Fact]
    public void IsNullOrEmpty_NullOrEmpty_ReturnsTrue()
    {
        ((IEnumerable<string>?)null).IsNullOrEmpty().Should().BeTrue();
        new List<string>().IsNullOrEmpty().Should().BeTrue();
    }

    [Fact]
    public void HasItems_HasItems_ReturnsTrue()
    {
        new List<string> { "a" }.HasItems().Should().BeTrue();
    }

    [Fact]
    public void HasItems_EmptyOrNull_ReturnsFalse()
    {
        new List<string>().HasItems().Should().BeFalse();
        ((IEnumerable<string>?)null).HasItems().Should().BeFalse();
    }

    [Fact]
    public void Shuffle_ValidSource_ReturnsShuffledItems()
    {
        var source = new List<int> { 1, 2, 3, 4, 5 };
        var shuffled = TelegramBotFramework.Utilities.CollectionExtensions.Shuffle(source).ToList();
        shuffled.Should().HaveCount(5);
        shuffled.Should().BeEquivalentTo(source);
    }

    [Fact]
    public void AddRange_ValidSource_AddsAllItems()
    {
        var collection = new List<int> { 1, 2 };
        var itemsToAdd = new List<int> { 3, 4 };
        collection.AddRange(itemsToAdd);
        collection.Should().HaveCount(4);
        collection.Should().Contain(new[] { 1, 2, 3, 4 });
    }

    [Fact]
    public void ToDictionarySafe_DuplicateKeys_KeepsFirstOccurrence()
    {
        var source = new List<KeyValuePair<string, int>>
        {
            new("a", 1),
            new("a", 2)
        };
        var dict = source.ToDictionarySafe(x => x.Key, x => x.Value);
        dict.Should().HaveCount(1);
        dict["a"].Should().Be(1);
    }

    [Fact]
    public void ForEach_ValidSource_ExecutesAction()
    {
        var list = new List<int> { 1, 2, 3 };
        var sum = 0;
        ((IEnumerable<int>)list).ForEach(x => sum += x).ToList(); // Consume the enumerable
        sum.Should().Be(6);
    }
}
