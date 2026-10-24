#nullable enable

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using TelegramBotFramework.Integration;
using Xunit;

namespace TelegramBotFramework.Tests;

public class TelegramApiClientTests
{
    private const string ValidToken = "123456789:abcdefghijklmnopqrstuvwxyzA";
    private const long ChatId = 123;

    [Fact]
    public async Task SendMessageAsync_TextExceedsTelegramLimit_ThrowsArgumentException()
    {
        var client = new TelegramApiClient(ValidToken);
        var text = new string('a', 4097);

        Func<Task> act = () => client.SendMessageAsync(ChatId, text);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Message text cannot exceed 4096 characters*")
            .WithParameterName("text");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-a-token")]
    [InlineData("123456789:tooshort")]
    public void Constructor_EmptyOrMalformedToken_ThrowsArgumentException(string? token)
    {
        Action act = () => new TelegramApiClient(token!);

        act.Should().Throw<ArgumentException>().WithParameterName("botToken");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task EditMessageAsync_NonPositiveMessageId_ThrowsArgumentException(int messageId)
    {
        var client = new TelegramApiClient(ValidToken);

        Func<Task> act = () => client.EditMessageAsync(ChatId, messageId, "updated");

        await act.Should().ThrowAsync<ArgumentException>().WithParameterName("messageId");
    }

    [Fact]
    public async Task SendMessageWithButtonsAsync_NullOrBlankLabels_ThrowsExpectedExceptions()
    {
        var client = new TelegramApiClient(ValidToken);

        Func<Task> nullLabels = () => client.SendMessageWithButtonsAsync(ChatId, "Pick one", null!);
        Func<Task> blankLabel = () => client.SendMessageWithButtonsAsync(ChatId, "Pick one", new[] { new[] { "Yes", " " } });

        await nullLabels.Should().ThrowAsync<ArgumentNullException>().WithParameterName("buttonLabels");
        await blankLabel.Should().ThrowAsync<ArgumentException>().WithParameterName("buttonLabels");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(11)]
    public async Task SendPollAsync_OptionCountOutOfRange_ThrowsArgumentException(int count)
    {
        var client = new TelegramApiClient(ValidToken);
        var options = new string[count];
        Array.Fill(options, "option");

        Func<Task> act = () => client.SendPollAsync(ChatId, "Question?", options);

        await act.Should().ThrowAsync<ArgumentException>().WithParameterName("options");
    }

    [Fact]
    public async Task SendMediaGroupAsync_NullOrSingleItem_ThrowsExpectedExceptions()
    {
        var client = new TelegramApiClient(ValidToken);
        var single = new List<MediaGroupItem> { new(MediaType.Photo, "file-id") };

        Func<Task> nullItems = () => client.SendMediaGroupAsync(ChatId, null!);
        Func<Task> tooFew = () => client.SendMediaGroupAsync(ChatId, single);

        await nullItems.Should().ThrowAsync<ArgumentNullException>().WithParameterName("items");
        await tooFew.Should().ThrowAsync<ArgumentException>().WithParameterName("items");
    }

    [Fact]
    public async Task SendMessageAsync_CancelledToken_ThrowsOperationCanceledException()
    {
        var client = new TelegramApiClient(ValidToken);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        Func<Task> act = () => client.SendMessageAsync(ChatId, "Hello", cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task OpenCircuit_ValidArguments_ShortCircuitWithoutCallingTelegram()
    {
        // Arrange: a breaker that opens on the first failure and stays open for an hour,
        // so every call returns its failure value before any HTTP request is made.
        var breaker = new TelegramCircuitBreaker(new CircuitBreakerOptions
        {
            FailureThreshold = 1,
            OpenDuration = TimeSpan.FromHours(1)
        });
        await breaker.RecordFailureAsync();
        var client = new TelegramApiClient(ValidToken, circuitBreaker: breaker);
        var media = new List<MediaGroupItem> { new(MediaType.Photo, "first"), new(MediaType.Photo, "second") };

        // Act
        var sent = await client.SendMessageAsync(ChatId, "Hello");
        var buttons = await client.SendMessageWithButtonsAsync(ChatId, "Pick", new[] { new[] { "Yes" } });
        var edited = await client.EditMessageAsync(ChatId, 1, "Updated");
        var deleted = await client.DeleteMessageAsync(ChatId, 1);
        var poll = await client.SendPollAsync(ChatId, "Question?", new[] { "Yes", "No" });
        var group = await client.SendMediaGroupAsync(ChatId, media);
        var me = await client.GetMeAsync();
        var callback = await client.AnswerCallbackQueryAsync("callback-id");
        var webhook = await client.SetWebhookAsync("https://example.com/hook");

        // Assert
        sent.Should().BeFalse();
        buttons.Should().BeFalse();
        edited.Should().BeFalse();
        deleted.Should().BeFalse();
        poll.Should().BeNull();
        group.Should().BeEmpty();
        me.Should().BeNull();
        callback.Should().BeFalse();
        webhook.Should().BeFalse();
        breaker.State.Should().Be(CircuitState.Open);
    }
}
