// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedVariable

using System;
using TelegramBotFramework.Formatters;
using Xunit;

namespace TelegramBotFramework.Tests;

public class XmlFormatterJsonExtensionsTests
{
    [Fact]
    public void ToJson_WithNullFormatter_ThrowsArgumentNullException()
    {
        XmlFormatter? formatter = null;
        Assert.Throws<ArgumentNullException>(() => formatter!.ToJson());
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, true)]
    public void ToJson_ProducesCorrectJson(bool pretty, bool indented)
    {
        var formatter = new XmlFormatter(pretty);
        var json = formatter.ToJson(indented);

        // The JSON must contain the "pretty" property with the correct value.
        var expectedFragment = $"\"pretty\":{pretty.ToString().ToLower()}";
        Assert.Contains(expectedFragment, json);

        // When indented is true, the JSON should contain line breaks or spaces.
        if (indented)
        {
            Assert.Contains("\n", json);
        }
        else
        {
            Assert.DoesNotContain("\n", json);
        }
    }

    [Theory]
    [InlineData("{\"pretty\":true}", true)]
    [InlineData("{\"pretty\":false}", false)]
    public void FromJson_ValidJson_ReturnsFormatterWithCorrectPretty(string json, bool expectedPretty)
    {
        var formatter = XmlFormatterJsonExtensions.FromJson(json);
        Assert.NotNull(formatter);
        Assert.Equal(expectedPretty, formatter!.GetPretty());
    }

    [Fact]
    public void FromJson_InvalidJson_ReturnsNull()
    {
        const string invalidJson = "{ this is not valid json }";
        var formatter = XmlFormatterJsonExtensions.FromJson(invalidJson);
        Assert.Null(formatter);
    }

    [Fact]
    public void FromJson_NullOrEmptyString_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => XmlFormatterJsonExtensions.FromJson(null!));
        Assert.Throws<ArgumentException>(() => XmlFormatterJsonExtensions.FromJson(string.Empty));
    }

    [Theory]
    [InlineData("{\"pretty\":true}", true)]
    [InlineData("{\"pretty\":false}", false)]
    public void TryFromJson_ValidJson_ReturnsTrueAndFormatter(string json, bool expectedPretty)
    {
        var result = XmlFormatterJsonExtensions.TryFromJson(json, out var formatter);
        Assert.True(result);
        Assert.NotNull(formatter);
        Assert.Equal(expectedPretty, formatter!.GetPretty());
    }

    [Fact]
    public void TryFromJson_InvalidJson_ReturnsFalseAndNull()
    {
        const string invalidJson = "{ not a json }";
        var result = XmlFormatterJsonExtensions.TryFromJson(invalidJson, out var formatter);
        Assert.False(result);
        Assert.Null(formatter);
    }

    [Fact]
    public void TryFromJson_NullOrEmptyString_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => XmlFormatterJsonExtensions.TryFromJson(null!, out _));
        Assert.Throws<ArgumentException>(() => XmlFormatterJsonExtensions.TryFromJson(string.Empty, out _));
    }
}
