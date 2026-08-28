#nullable enable
using System;
using System.Collections.Generic;
using FluentAssertions;
using TelegramBotFramework.Formatters;
using TelegramBotFramework.Models;
using Xunit;

namespace TelegramBotFramework.Tests;

/// <summary>
/// Tests for <see cref="MessageFormatter"/>.
/// </summary>
public sealed class MessageFormatterTests : IMessageFormatterTests
{
    private static Message CreateMessage(
        long userId = 123,
        string content = "Test content",
        DateTime? createdAt = null,
        bool isEdited = false)
    {
        return new Message
        {
            UserId = userId,
            Content = content,
            CreatedAt = createdAt ?? new DateTime(2023, 1, 1, 12, 34, 56, DateTimeKind.Utc),
            IsEdited = isEdited
        };
    }

    [Fact]
    public void EscapeMarkdown_EscapesAllSpecialCharacters()
    {
        // Arrange
        var special = "_*[]()~`\\";
        var message = CreateMessage(content: $"Hello {special} World");

        // Act
        var formatted = MessageFormatter.FormatAsMarkdown(message);

        // Assert
        // All special characters should be escaped with a backslash
        foreach (var ch in new[] { '_', '*', '[', ']', '(', ')', '~', '`', '\\' })
        {
            formatted.Should().Contain($"\\{ch}");
        }
    }

    [Fact]
    public void EscapeHtml_EscapesAllSpecialCharacters()
    {
        // Arrange
        var special = "<>&'\"";
        var message = CreateMessage(content: $"Hello {special} World");

        // Act
        var formatted = MessageFormatter.FormatAsHtml(message);

        // Assert
        formatted.Should().Contain("&lt;")
                 .And.Contain("&gt;")
                 .And.Contain("&amp;")
                 .And.Contain("&#39;")
                 .And.Contain("&quot;");
    }

    [Fact]
    public void FormatAsPlainText_IncludesTimestampAndEditedFlag()
    {
        // Arrange
        var created = new DateTime(2023, 5, 10, 14, 30, 0, DateTimeKind.Utc);
        var message = CreateMessage(userId: 42, content: "Plain text", createdAt: created, isEdited: true);

        // Act
        var result = MessageFormatter.FormatAsPlainText(message);

        // Assert
        result.Should().Contain($"[{created:yyyy-MM-dd HH:mm:ss}] 42:");
        result.Should().Contain("Plain text");
        result.Should().Contain("(Edited)");
    }

    [Fact]
    public void FormatAsMarkdown_ProducesCorrectTemplate()
    {
        // Arrange
        var created = new DateTime(2023, 5, 10, 14, 30, 0, DateTimeKind.Utc);
        var message = CreateMessage(userId: 42, content: "Markdown *content*", createdAt: created, isEdited: false);

        // Act
        var result = MessageFormatter.FormatAsMarkdown(message);

        // Assert
        var escapeMethod = typeof(MessageFormatter)
            .GetMethod("EscapeMarkdown", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!;
        var expectedUserId = escapeMethod.Invoke(null, new object[] { "42" }) as string;

        result.Should().StartWith($"**[{created:HH:mm}]** _{expectedUserId}_: ");
        result.Should().Contain("Markdown \\*content\\*");
        result.Should().NotContain("_(edited)_");
    }

    [Fact]
    public void TruncateForPreview_LongMessage_IsTruncatedWithEllipsis()
    {
        // Arrange
        var longText = new string('a', 150);
        var message = CreateMessage(content: longText);

        // Act
        var preview = MessageFormatter.TruncateForPreview(message, maxLength: 100);

        // Assert
        preview.Length.Should().Be(101); // 100 chars + ellipsis
        preview.Should().EndWith("…");
        preview.Substring(0, 100).Should().Be(new string('a', 100));
    }

    [Fact]
    public void FormatAsConversation_MarkdownTrue_CombinesMultipleMessages()
    {
        // Arrange
        var msg1 = CreateMessage(userId: 1, content: "First", createdAt: new DateTime(2023, 1, 1, 10, 0, 0, DateTimeKind.Utc));
        var msg2 = CreateMessage(userId: 2, content: "Second", createdAt: new DateTime(2023, 1, 1, 10, 5, 0, DateTimeKind.Utc));
        var messages = new List<Message> { msg1, msg2 };

        // Act
        var result = MessageFormatter.FormatAsConversation(messages, markdown: true);

        // Assert
        // Both messages should appear, each followed by a blank line
        var parts = result.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        parts.Should().Contain(part => part.Contains("First"));
        parts.Should().Contain(part => part.Contains("Second"));
    }

    [Fact]
    public void FormatAsPlainText_EmptyContent_ProducesLinesWithoutException()
    {
        // Arrange
        var message = CreateMessage(content: string.Empty);

        // Act
        var result = MessageFormatter.FormatAsPlainText(message);

        // Assert
        result.Should().Contain($"{message.UserId}:");
        result.Should().EndWith("\n"); // At least one newline after the empty content line
    }
}
