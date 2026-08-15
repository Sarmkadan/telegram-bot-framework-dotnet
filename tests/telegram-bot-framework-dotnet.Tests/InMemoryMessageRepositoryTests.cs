using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TelegramBotFramework.Models;
using TelegramBotFramework.Repositories;
using Xunit;

namespace TelegramBotFramework.Tests.Repositories;

public class InMemoryMessageRepositoryTests
{
    private readonly InMemoryMessageRepository _repository;

    public InMemoryMessageRepositoryTests()
    {
        _repository = new InMemoryMessageRepository();
    }

    private Message CreateTestMessage(long userId = 1, long chatId = 10, string content = "Hello")
    {
        return new Message
        {
            UserId = userId,
            ChatId = chatId,
            Content = content
        };
    }

    [Fact]
    public async Task CreateAsync_ValidMessage_AddsMessage()
    {
        var message = CreateTestMessage();
        var created = await _repository.CreateAsync(message);

        Assert.NotEqual(0, created.MessageId);
        Assert.Equal(message.Content, created.Content);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingMessage_ReturnsMessage()
    {
        var message = CreateTestMessage();
        var created = await _repository.CreateAsync(message);

        var retrieved = await _repository.GetByIdAsync(created.MessageId);
        Assert.NotNull(retrieved);
        Assert.Equal(created.MessageId, retrieved!.MessageId);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingMessage_ReturnsNull()
    {
        var retrieved = await _repository.GetByIdAsync(999);
        Assert.Null(retrieved);
    }

    [Fact]
    public async Task UpdateAsync_ExistingMessage_UpdatesMessage()
    {
        var message = CreateTestMessage();
        var created = await _repository.CreateAsync(message);

        created.Content = "Updated";
        await _repository.UpdateAsync(created);

        var retrieved = await _repository.GetByIdAsync(created.MessageId);
        Assert.NotNull(retrieved);
        Assert.Equal("Updated", retrieved!.Content);
    }

    [Fact]
    public async Task DeleteAsync_ExistingMessage_ReturnsTrueAndRemovesMessage()
    {
        var message = CreateTestMessage();
        var created = await _repository.CreateAsync(message);

        var deleted = await _repository.DeleteAsync(created.MessageId);

        Assert.True(deleted);
        Assert.Null(await _repository.GetByIdAsync(created.MessageId));
    }

    [Fact]
    public async Task ExistsAsync_ExistingMessage_ReturnsTrue()
    {
        var message = CreateTestMessage();
        var created = await _repository.CreateAsync(message);

        var exists = await _repository.ExistsAsync(created.MessageId);
        Assert.True(exists);
    }

    [Fact]
    public async Task CountAsync_MessagesExist_ReturnsCorrectCount()
    {
        await _repository.CreateAsync(CreateTestMessage());
        await _repository.CreateAsync(CreateTestMessage(userId: 2));

        var count = await _repository.CountAsync();
        Assert.Equal(2, count);
    }

    [Fact]
    public async Task GetByUserIdAsync_ExistingMessages_ReturnsFilteredMessages()
    {
        await _repository.CreateAsync(CreateTestMessage(userId: 1));
        await _repository.CreateAsync(CreateTestMessage(userId: 2));

        var messages = await _repository.GetByUserIdAsync(1);
        Assert.Single(messages);
        Assert.Equal(1, messages[0].UserId);
    }
}
