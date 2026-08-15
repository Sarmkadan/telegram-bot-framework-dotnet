using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TelegramBotFramework.Models;
using TelegramBotFramework.Repositories;
using Xunit;

namespace TelegramBotFramework.Tests.Repositories;

public class InMemoryUserRepositoryTests
{
    private readonly InMemoryUserRepository _repository;

    public InMemoryUserRepositoryTests()
    {
        _repository = new InMemoryUserRepository();
    }

    private BotUser CreateTestUser(long id = 1, string firstName = "John")
    {
        return new BotUser
        {
            TelegramId = id,
            FirstName = firstName
        };
    }

    [Fact]
    public async Task CreateAsync_ValidUser_AddsUser()
    {
        var user = CreateTestUser();
        await _repository.CreateAsync(user);

        var retrieved = await _repository.GetByIdAsync(user.TelegramId);
        Assert.NotNull(retrieved);
        Assert.Equal(user.TelegramId, retrieved!.TelegramId);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingUser_ReturnsUser()
    {
        var user = CreateTestUser(123, "Jane");
        await _repository.CreateAsync(user);

        var retrieved = await _repository.GetByIdAsync(123);
        Assert.NotNull(retrieved);
        Assert.Equal("Jane", retrieved!.FirstName);
    }

    [Fact]
    public async Task UpdateAsync_ExistingUser_UpdatesUser()
    {
        var user = CreateTestUser(1, "John");
        await _repository.CreateAsync(user);
        
        user.FirstName = "JohnUpdated";
        await _repository.UpdateAsync(user);

        var retrieved = await _repository.GetByIdAsync(1);
        Assert.NotNull(retrieved);
        Assert.Equal("JohnUpdated", retrieved!.FirstName);
    }

    [Fact]
    public async Task DeleteAsync_ExistingUser_RemovesUser()
    {
        var user = CreateTestUser(1, "John");
        await _repository.CreateAsync(user);
        
        var deleted = await _repository.DeleteAsync(1);
        
        Assert.True(deleted);
        var retrieved = await _repository.GetByIdAsync(1);
        Assert.Null(retrieved);
    }

    [Fact]
    public async Task ExistsAsync_ExistingUser_ReturnsTrue()
    {
        var user = CreateTestUser(1, "John");
        await _repository.CreateAsync(user);
        
        var exists = await _repository.ExistsAsync(1);
        Assert.True(exists);
    }

    [Fact]
    public async Task GetByStatusAsync_UsersExist_ReturnsFilteredUsers()
    {
        var user1 = CreateTestUser(1, "John");
        user1.Status = UserStatus.Active;
        await _repository.CreateAsync(user1);

        var user2 = CreateTestUser(2, "Jane");
        user2.Status = UserStatus.Inactive;
        await _repository.CreateAsync(user2);
        
        var activeUsers = await _repository.GetByStatusAsync(UserStatus.Active);
        
        Assert.Single(activeUsers);
        Assert.Equal(1, activeUsers[0].TelegramId);
    }
}
