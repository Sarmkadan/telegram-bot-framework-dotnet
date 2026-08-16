using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using TelegramBotFramework.Formatters;
using TelegramBotFramework.Models;
using Xunit;

namespace TelegramBotFramework.Tests;

public sealed class XmlFormatterTests
{
    [Fact]
    public void Constructor_SetsPrettyFlag_Correctly()
    {
        var prettyFormatter = new XmlFormatter(true);
        Assert.True(prettyFormatter.GetPretty());

        var compactFormatter = new XmlFormatter(false);
        Assert.False(compactFormatter.GetPretty());
    }

    [Fact]
    public void Format_SingleObject_ReturnsValidXml()
    {
        var formatter = new XmlFormatter();
        var data = new TestData { Name = "Alice", Age = 30 };

        string result = formatter.Format(data);
        XElement root = XElement.Parse(result);

        Assert.Equal("TestData", root.Name.LocalName);
        Assert.Equal("Alice", root.Element("Name")?.Value);
        Assert.Equal("30", root.Element("Age")?.Value);
    }

    [Fact]
    public void Format_NullInput_ReturnsEmptyRootElement()
    {
        var formatter = new XmlFormatter();
        TestData? data = null;

        string result = formatter.Format(data);
        XElement root = XElement.Parse(result);

        Assert.Equal("TestData", root.Name.LocalName);
        Assert.Empty(root.Elements());
    }

    [Fact]
    public void Format_Collection_ReturnsItemsWrapper()
    {
        var formatter = new XmlFormatter();
        var data = new List<int> { 10, 20, 30 };

        string result = formatter.Format(data);
        XElement root = XElement.Parse(result);

        Assert.Equal("items", root.Name.LocalName);
        Assert.Equal(3, root.Elements("item").Count());
    }

    [Fact]
    public void Format_EmptyCollection_ReturnsEmptyItems()
    {
        var formatter = new XmlFormatter();
        var data = Enumerable.Empty<string>();

        string result = formatter.Format(data);
        XElement root = XElement.Parse(result);

        Assert.Equal("items", root.Name.LocalName);
        Assert.Empty(root.Elements());
    }

    [Fact]
    public void FormatError_ReturnsCorrectStructure()
    {
        var formatter = new XmlFormatter();
        string result = formatter.FormatError("ERR_500", "Server Error", "Retry later");
        XElement root = XElement.Parse(result);

        Assert.Equal("error", root.Name.LocalName);
        Assert.Equal("ERR_500", root.Element("code")?.Value);
        Assert.Equal("Server Error", root.Element("message")?.Value);
        Assert.Equal("Retry later", root.Element("details")?.Value);
        Assert.NotNull(root.Element("timestamp"));
    }

    [Fact]
    public void FormatMessage_MapsFieldsCorrectly()
    {
        var formatter = new XmlFormatter();
        var message = new Message
        {
            MessageId = 123,
            Content = "Hello World",
            UserId = 1,
            ChatId = 99,
            Type = MessageType.Text,
            CreatedAt = DateTime.UtcNow
        };

        string result = formatter.FormatMessage(message);
        XElement root = XElement.Parse(result);

        Assert.Equal("message", root.Name.LocalName);
        Assert.Equal("123", root.Element("id")?.Value);
        Assert.Equal("Hello World", root.Element("content")?.Value);
        Assert.Equal("1", root.Element("userId")?.Value);
        Assert.Equal("99", root.Element("chatId")?.Value);
        Assert.Equal("Text", root.Element("type")?.Value);
    }

    [Fact]
    public void FormatMessages_IncludesCountAttribute()
    {
        var formatter = new XmlFormatter();
        var messages = new List<Message>
        {
            new Message { MessageId = 1, Content = "A", UserId = 1, ChatId = 1, Type = MessageType.Text, CreatedAt = DateTime.UtcNow },
            new Message { MessageId = 2, Content = "B", UserId = 1, ChatId = 1, Type = MessageType.Text, CreatedAt = DateTime.UtcNow }
        };

        string result = formatter.FormatMessages(messages);
        XElement root = XElement.Parse(result);

        Assert.Equal("messages", root.Name.LocalName);
        Assert.Equal("2", root.Attribute("count")?.Value);
        Assert.Equal(2, root.Elements("message").Count());
    }

    private sealed class TestData
    {
        public string? Name { get; set; }
        public int Age { get; set; }
    }
}
