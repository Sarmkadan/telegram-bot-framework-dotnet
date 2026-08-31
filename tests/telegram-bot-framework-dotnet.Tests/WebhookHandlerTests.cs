using FluentAssertions;
using TelegramBotFramework.Integration;
using Xunit;

namespace TelegramBotFramework.Tests.Integration;

public class WebhookHandlerTests
{
    [Fact]
    public void Constructor_InitializesPublicPropertiesWithDefaultValues()
    {
        var handler = new WebhookHandler();

        handler.UpdateId.Should().Be(0);
        handler.MessageType.Should().Be(UpdateType.Message);
        handler.Timestamp.Should().Be(default);
        handler.Message.Should().BeNull();
        handler.CallbackData.Should().BeNull();
        handler.CallbackQueryId.Should().BeNull();
        handler.InlineQuery.Should().BeNull();
    }

    [Fact]
    public void PublicProperties_RoundTripAssignedValues()
    {
        var timestamp = new DateTime(2026, 8, 31, 12, 30, 0, DateTimeKind.Utc);
        var message = new TelegramMessage { MessageId = long.MaxValue, Text = string.Empty };
        var handler = new WebhookHandler
        {
            UpdateId = long.MaxValue,
            MessageType = UpdateType.CallbackQuery,
            Timestamp = timestamp,
            Message = message,
            CallbackData = string.Empty,
            CallbackQueryId = "callback-id",
            InlineQuery = "query"
        };

        handler.UpdateId.Should().Be(long.MaxValue);
        handler.MessageType.Should().Be(UpdateType.CallbackQuery);
        handler.Timestamp.Should().Be(timestamp);
        handler.Message.Should().BeSameAs(message);
        handler.CallbackData.Should().BeEmpty();
        handler.CallbackQueryId.Should().Be("callback-id");
        handler.InlineQuery.Should().Be("query");
    }

    [Fact]
    public async Task ProcessUpdateAsync_ValidMessage_ParsesMessageAndEmptyEntities()
    {
        const string json = """
            {
              "update_id": 42,
              "message": {
                "message_id": 7,
                "chat": { "id": -100 },
                "from": { "id": 99 },
                "date": 0,
                "text": "hello",
                "entities": []
              }
            }
            """;
        var before = DateTime.UtcNow;

        var update = await new WebhookHandler().ProcessUpdateAsync(json);

        update.Should().NotBeNull();
        update!.UpdateId.Should().Be(42);
        update.MessageType.Should().Be(UpdateType.Message);
        update.Timestamp.Should().BeOnOrAfter(before).And.BeOnOrBefore(DateTime.UtcNow);
        update.Message.Should().NotBeNull();
        update.Message!.MessageId.Should().Be(7);
        update.Message.ChatId.Should().Be(-100);
        update.Message.UserId.Should().Be(99);
        update.Message.Timestamp.Should().Be(DateTime.UnixEpoch);
        update.Message.Text.Should().Be("hello");
        update.Message.Entities.Should().BeEmpty();
    }

    [Fact]
    public async Task ProcessUpdateAsync_CallbackQuery_ParsesFieldsAndTruncatesValuesOverBoundary()
    {
        var callbackId = new string('i', WebhookHandlerConstants.MaxCallbackDataLength + 1);
        var callbackData = new string('d', WebhookHandlerConstants.MaxCallbackDataLength);
        var json = $$"""
            {
              "update_id": 43,
              "callback_query": {
                "id": "{{callbackId}}",
                "data": "{{callbackData}}"
              }
            }
            """;

        var update = await new WebhookHandler().ProcessUpdateAsync(json);

        update.Should().NotBeNull();
        update!.MessageType.Should().Be(UpdateType.CallbackQuery);
        update.CallbackQueryId.Should().Be(callbackId[..WebhookHandlerConstants.MaxCallbackDataLength]);
        update.CallbackData.Should().Be(callbackData);
        update.Message.Should().BeNull();
    }

    [Fact]
    public async Task ProcessUpdateAsync_EditedMessageAndInlineQuery_ParsesBothUpdateTypes()
    {
        const string editedJson = """
            {
              "update_id": 44,
              "edited_message": {
                "message_id": 8,
                "chat": { "id": 10 },
                "from": { "id": 11 },
                "date": 1,
                "edit_date": 2
              }
            }
            """;
        const string inlineJson = """
            {
              "update_id": 45,
              "inline_query": { "query": "find this" }
            }
            """;
        var handler = new WebhookHandler();

        var editedUpdate = await handler.ProcessUpdateAsync(editedJson);
        var inlineUpdate = await handler.ProcessUpdateAsync(inlineJson);

        editedUpdate!.MessageType.Should().Be(UpdateType.EditedMessage);
        editedUpdate.Message!.EditedTimestamp.Should().Be(DateTime.UnixEpoch.AddSeconds(2));
        inlineUpdate!.MessageType.Should().Be(UpdateType.InlineQuery);
        inlineUpdate.InlineQuery.Should().Be("find this");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not json")]
    [InlineData("[]")]
    [InlineData("{\"update_id\":1}")]
    public async Task ProcessUpdateAsync_InvalidOrUnsupportedInput_ReturnsNull(string? json)
    {
        var result = await new WebhookHandler().ProcessUpdateAsync(json!);

        result.Should().BeNull();
    }

    [Fact]
    public async Task ProcessUpdateAsync_CanceledToken_ThrowsOperationCanceledException()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var act = () => new WebhookHandler().ProcessUpdateAsync("{}", cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Theory]
    [InlineData(null, null, true)]
    [InlineData("anything", "", true)]
    [InlineData(null, "configured", false)]
    [InlineData("", "configured", false)]
    [InlineData("configured", "configured", true)]
    [InlineData("wrong", "configured", false)]
    public void ValidateSecretToken_VariousInputs_ReturnsExpectedResult(
        string? header,
        string? configuredSecret,
        bool expected)
    {
        var result = new WebhookHandler().ValidateSecretToken(header, configuredSecret);

        result.Should().Be(expected);
    }
}
