#nullable enable

using System;
using System.Threading.Tasks;
using FluentAssertions;
using TelegramBotFramework.Integration;
using Xunit;

namespace TelegramBotFramework.Tests;

public class TelegramApiClientTests
{
    [Fact]
    public async Task SendMessageAsync_TextExceedsTelegramLimit_ThrowsArgumentException()
    {
        var client = new TelegramApiClient("123456789:abcdefghijklmnopqrstuvwxyzA");
        var text = new string('a', 4097);

        Func<Task> act = () => client.SendMessageAsync(123, text);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Message text cannot exceed 4096 characters*")
            .WithParameterName("text");
    }
}
