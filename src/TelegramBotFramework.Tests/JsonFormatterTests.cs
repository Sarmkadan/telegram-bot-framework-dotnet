#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using TelegramBotFramework.Formatters;
using TelegramBotFramework.Models;
using Xunit;

namespace TelegramBotFramework.Tests;

public sealed class JsonFormatterTests
{
    [Fact]
    public void Constructor_WithPrettyTrue_EnablesIndentation()
    {
        var formatter = new JsonFormatter(pretty: true);
        var json = formatter.Format(new { Name = "Bob" });

        // When WriteIndented is true the output contains line‑breaks.
        Assert.Contains(Environment.NewLine, json);
    }

    [Fact]
    public void Format_SingleObject_ReturnsValidJson()
    {
        var formatter = new JsonFormatter();
        var payload = new { Name = "Alice", Age = 30 };
        var json = formatter.Format(payload);

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.Equal("Alice", root.GetProperty("name").GetString());
        Assert.Equal(30, root.GetProperty("age").GetInt32());
    }

    [Fact]
    public void Format_NullObject_ReturnsLiteralNull()
    {
        var formatter = new JsonFormatter();
        string json = formatter.Format<object?>(null);

        Assert.Equal("null", json);
    }

    [Fact]
    public void Format_Collection_WrapsItemsAndCount()
    {
        var formatter = new JsonFormatter();
        var data = new[] { 1, 2, 3 };
        var json = formatter.Format(data);

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var items = root.GetProperty("items");
        Assert.Equal(3, items.GetArrayLength());
        Assert.Equal(1, items[0].GetInt32());
        Assert.Equal(2, items[1].GetInt32());
        Assert.Equal(3, items[2].GetInt32());

        Assert.Equal(3, root.GetProperty("count").GetInt32());
    }

    [Fact]
    public void Format_EmptyCollection_ReturnsCountZero()
    {
        var formatter = new JsonFormatter();
        var empty = Enumerable.Empty<string>();
        var json = formatter.Format(empty);

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.Equal(0, root.GetProperty("count").GetInt32());
        Assert.Empty(root.GetProperty("items").EnumerateArray());
    }

    [Fact]
    public void FormatError_IncludesAllProvidedFields()
    {
        var formatter = new JsonFormatter();
        var json = formatter.FormatError("ERR001", "Something went wrong", "details here");

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.Equal("ERR001", root.GetProperty("error").GetString());
        Assert.Equal("Something went wrong", root.GetProperty("message").GetString());
        Assert.Equal("details here", root.GetProperty("details").GetString());
        Assert.True(root.TryGetProperty("timestamp", out _));
    }

    [Fact]
    public void FormatMessage_ProducesExpectedJsonStructure()
    {
        var formatter = new JsonFormatter();

        var message = new Message
        {
            MessageId = 123,
            Content = "Hello world",
            UserId = 42,
            ChatId = 1001,
            CreatedAt = new DateTime(2023, 1, 1, 12, 0, 0, DateTimeKind.Utc),
            IsEdited = false,
            Type = MessageType.Text   // assuming an enum called MessageType with a Text value
        };

        var json = formatter.FormatMessage(message);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.Equal(123, root.GetProperty("id").GetInt32());
        Assert.Equal("Hello world", root.GetProperty("content").GetString());
        Assert.Equal(42, root.GetProperty("userId").GetInt32());
        Assert.Equal(1001, root.GetProperty("chatId").GetInt32());
        Assert.Equal("2023-01-01T12:00:00Z", root.GetProperty("createdAt").GetString());
        Assert.False(root.GetProperty("isEdited").GetBoolean());
        Assert.Equal("Text", root.GetProperty("type").GetString());
    }

    [Fact]
    public void FormatMessages_WrapsMultipleMessages()
    {
        var formatter = new JsonFormatter();

        var messages = new[]
        {
            new Message
            {
                MessageId = 1,
                Content = "First",
                UserId = 10,
                ChatId = 20,
                CreatedAt = DateTime.UtcNow,
                IsEdited = false,
                Type = MessageType.Text
            },
            new Message
            {
                MessageId = 2,
                Content = "Second",
                UserId = 11,
                ChatId = 21,
                CreatedAt = DateTime.UtcNow,
                IsEdited = true,
                Type = MessageType.Text
            }
        };

        var json = formatter.FormatMessages(messages);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var msgs = root.GetProperty("messages");
        Assert.Equal(2, msgs.GetArrayLength());
        Assert.Equal(2, root.GetProperty("count").GetInt32());

        Assert.Equal(1, msgs[0].GetProperty("id").GetInt32());
        Assert.Equal("First", msgs[0].GetProperty("content").GetString());

        Assert.Equal(2, msgs[1].GetProperty("id").GetInt32());
        Assert.Equal("Second", msgs[1].GetProperty("content").GetString());
    }
}
