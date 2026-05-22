#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using TelegramBotFramework.Models;
using TelegramBotFramework.Repositories;
using Xunit;

namespace TelegramBotFramework.Tests;

public sealed class BotUserTests
{
    [Fact]
    public void GetDisplayName_WithFirstAndLastName_ReturnsFullName()
    {
        var user = new BotUser { TelegramId = 1, FirstName = "John", LastName = "Doe" };

        var name = user.GetDisplayName();

        name.Should().Be("John Doe");
    }

    [Fact]
    public void GetDisplayName_WithoutLastName_ReturnsFirstNameOnly()
    {
        var user = new BotUser { TelegramId = 1, FirstName = "Alice" };

        var name = user.GetDisplayName();

        name.Should().Be("Alice");
    }

    [Fact]
    public void Validate_WithNonPositiveTelegramId_ThrowsInvalidOperationException()
    {
        var user = new BotUser { TelegramId = 0, FirstName = "Test" };

        var act = () => user.Validate();

        act.Should().Throw<InvalidOperationException>().WithMessage("*TelegramId*");
    }

    [Fact]
    public void Validate_WithEmptyFirstName_ThrowsInvalidOperationException()
    {
        var user = new BotUser { TelegramId = 999, FirstName = "  " };

        var act = () => user.Validate();

        act.Should().Throw<InvalidOperationException>().WithMessage("*FirstName*");
    }

    [Fact]
    public void UpdateActivity_IncrementsMessagesCount()
    {
        var user = new BotUser { TelegramId = 1, FirstName = "Test" };
        var before = user.MessagesCount;

        user.UpdateActivity();

        user.MessagesCount.Should().Be(before + 1);
    }

    [Fact]
    public void SetMetadata_AndGetMetadata_RoundTripsValue()
    {
        var user = new BotUser { TelegramId = 1, FirstName = "Test" };

        user.SetMetadata("plan", "premium");

        user.GetMetadata("plan").Should().Be("premium");
    }

    [Fact]
    public void GetMetadata_WhenKeyNotPresent_ReturnsNull()
    {
        var user = new BotUser { TelegramId = 1, FirstName = "Test" };

        user.GetMetadata("missing").Should().BeNull();
    }

    [Fact]
    public void SetMetadata_OverwritesExistingKey()
    {
        var user = new BotUser { TelegramId = 1, FirstName = "Test" };
        user.SetMetadata("tier", "free");

        user.SetMetadata("tier", "pro");

        user.GetMetadata("tier").Should().Be("pro");
    }
}

public sealed class CommandTests
{
    [Fact]
    public void CanExecuteBy_AdminCommandAndUserRole_ReturnsFalse()
    {
        var command = new Command { Name = "/ban", HandlerType = "Handler", RequiresAdmin = true, IsEnabled = true };

        command.CanExecuteBy(UserRole.User).Should().BeFalse();
    }

    [Fact]
    public void CanExecuteBy_AdminCommandAndModeratorRole_ReturnsFalse()
    {
        var command = new Command { Name = "/ban", HandlerType = "Handler", RequiresAdmin = true, IsEnabled = true };

        command.CanExecuteBy(UserRole.Moderator).Should().BeFalse();
    }

    [Fact]
    public void CanExecuteBy_AdminCommandAndAdminRole_ReturnsTrue()
    {
        var command = new Command { Name = "/ban", HandlerType = "Handler", RequiresAdmin = true, IsEnabled = true };

        command.CanExecuteBy(UserRole.Administrator).Should().BeTrue();
    }

    [Fact]
    public void CanExecuteBy_WhenCommandIsDisabled_ReturnsFalseForAnyRole()
    {
        var command = new Command { Name = "/start", HandlerType = "Handler", IsEnabled = false };

        command.CanExecuteBy(UserRole.Owner).Should().BeFalse();
    }

    [Fact]
    public void RecordExecution_IncrementsExecutionCount()
    {
        var command = new Command { Name = "/test", HandlerType = "Handler" };

        command.RecordExecution();

        command.ExecutionCount.Should().Be(1);
    }

    [Fact]
    public void RecordExecution_CalledMultipleTimes_AccumulatesCount()
    {
        var command = new Command { Name = "/test", HandlerType = "Handler" };

        command.RecordExecution();
        command.RecordExecution();
        command.RecordExecution();

        command.ExecutionCount.Should().Be(3);
    }

    [Fact]
    public void IsRateLimited_WhenExecutionsAtLimit_ReturnsTrue()
    {
        var command = new Command { Name = "/flood", HandlerType = "Handler", RateLimitPerMinute = 10 };

        command.IsRateLimited(10).Should().BeTrue();
    }

    [Fact]
    public void IsRateLimited_WhenExecutionsBelowLimit_ReturnsFalse()
    {
        var command = new Command { Name = "/flood", HandlerType = "Handler", RateLimitPerMinute = 10 };

        command.IsRateLimited(9).Should().BeFalse();
    }

    [Fact]
    public void IsRateLimited_WhenNoLimitConfigured_ReturnsFalseRegardlessOfCount()
    {
        var command = new Command { Name = "/open", HandlerType = "Handler", RateLimitPerMinute = null };

        command.IsRateLimited(9999).Should().BeFalse();
    }

    [Fact]
    public void GetCommandPatterns_WithAlias_ReturnsBothNameAndAlias()
    {
        var command = new Command { Name = "/start", HandlerType = "Handler", Alias = "/go" };

        var patterns = command.GetCommandPatterns().ToList();

        patterns.Should().HaveCount(2);
        patterns.Should().ContainInOrder("/start", "/go");
    }

    [Fact]
    public void GetCommandPatterns_WithoutAlias_ReturnsOnlyName()
    {
        var command = new Command { Name = "/help", HandlerType = "Handler" };

        var patterns = command.GetCommandPatterns().ToList();

        patterns.Should().HaveCount(1);
        patterns[0].Should().Be("/help");
    }

    [Fact]
    public void Validate_StandardCommandMissingLeadingSlash_ThrowsInvalidOperationException()
    {
        var command = new Command { Name = "start", HandlerType = "Handler", Type = CommandType.Standard };

        var act = () => command.Validate();

        act.Should().Throw<InvalidOperationException>().WithMessage("*/*");
    }

    [Fact]
    public void Validate_CommandWithEmptyName_ThrowsInvalidOperationException()
    {
        var command = new Command { Name = "", HandlerType = "Handler" };

        var act = () => command.Validate();

        act.Should().Throw<InvalidOperationException>().WithMessage("*name*");
    }
}

public sealed class UserSessionTests
{
    [Fact]
    public void IsExpired_WhenExpiresAtIsInThePast_ReturnsTrue()
    {
        var session = new UserSession
        {
            SessionId = "abc123",
            UserId = 1,
            ChatId = 100,
            ExpiresAt = DateTime.UtcNow.AddMinutes(-5)
        };

        session.IsExpired().Should().BeTrue();
    }

    [Fact]
    public void IsExpired_WhenNoExpiresAtSet_ReturnsFalse()
    {
        var session = new UserSession { SessionId = "abc123", UserId = 1, ChatId = 100 };

        session.IsExpired().Should().BeFalse();
    }

    [Fact]
    public void IsExpired_WhenExpiresAtIsInTheFuture_ReturnsFalse()
    {
        var session = new UserSession
        {
            SessionId = "abc123",
            UserId = 1,
            ChatId = 100,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        session.IsExpired().Should().BeFalse();
    }

    [Fact]
    public void UpdateActivity_CalledTwice_IncrementsInteractionCountToTwo()
    {
        var session = new UserSession { SessionId = "s1", UserId = 1, ChatId = 1 };

        session.UpdateActivity();
        session.UpdateActivity();

        session.InteractionCount.Should().Be(2);
    }

    [Fact]
    public void SetContextData_AndGetContextData_StoresAndRetrieves()
    {
        var session = new UserSession { SessionId = "s1", UserId = 1, ChatId = 1 };

        session.SetContextData("order_id", "ORD-42");

        session.GetContextData("order_id").Should().Be("ORD-42");
    }

    [Fact]
    public void RemoveContextData_WhenKeyExists_ReturnsTrueAndRemovesEntry()
    {
        var session = new UserSession { SessionId = "s1", UserId = 1, ChatId = 1 };
        session.SetContextData("temp", "value");

        var removed = session.RemoveContextData("temp");

        removed.Should().BeTrue();
        session.GetContextData("temp").Should().BeNull();
    }

    [Fact]
    public void ClearContextData_AfterSettingMultipleKeys_EmptiesAllData()
    {
        var session = new UserSession { SessionId = "s1", UserId = 1, ChatId = 1 };
        session.SetContextData("k1", "v1");
        session.SetContextData("k2", "v2");

        session.ClearContextData();

        session.GetContextData("k1").Should().BeNull();
        session.GetContextData("k2").Should().BeNull();
    }

    [Fact]
    public void AddCommandToHistory_When55CommandsAdded_CapsAtFiftyEntries()
    {
        var session = new UserSession { SessionId = "s1", UserId = 1, ChatId = 1 };

        for (int i = 0; i < 55; i++)
            session.AddCommandToHistory($"/cmd{i}");

        session.GetCommandHistory().Count().Should().Be(50);
    }

    [Fact]
    public void Validate_WhenSessionIdIsEmpty_ThrowsInvalidOperationException()
    {
        var session = new UserSession { SessionId = "", UserId = 1, ChatId = 1 };

        var act = () => session.Validate();

        act.Should().Throw<InvalidOperationException>().WithMessage("*SessionId*");
    }

    [Fact]
    public void Validate_WhenUserIdIsZero_ThrowsInvalidOperationException()
    {
        var session = new UserSession { SessionId = "s1", UserId = 0, ChatId = 1 };

        var act = () => session.Validate();

        act.Should().Throw<InvalidOperationException>().WithMessage("*UserId*");
    }
}

public sealed class MenuTests
{
    [Fact]
    public void AddButton_IncreasesButtonCount()
    {
        var menu = new Menu { Id = "main", Title = "Main Menu" };

        menu.AddButton(new MenuButton { Label = "Option 1", CallbackData = "opt1" });

        menu.Buttons.Should().HaveCount(1);
    }

    [Fact]
    public void RemoveButton_ByCallbackData_RemovesCorrectButtonAndLeavesOthers()
    {
        var menu = new Menu { Id = "main", Title = "Main" };
        menu.AddButton(new MenuButton { Label = "A", CallbackData = "a" });
        menu.AddButton(new MenuButton { Label = "B", CallbackData = "b" });

        var removed = menu.RemoveButton("a");

        removed.Should().BeTrue();
        menu.Buttons.Should().HaveCount(1);
        menu.Buttons[0].CallbackData.Should().Be("b");
    }

    [Fact]
    public void RemoveButton_WhenCallbackDataNotFound_ReturnsFalse()
    {
        var menu = new Menu { Id = "main", Title = "Main" };
        menu.AddButton(new MenuButton { Label = "A", CallbackData = "a" });

        menu.RemoveButton("nonexistent").Should().BeFalse();
    }

    [Fact]
    public void GetButton_WithMatchingCallbackData_ReturnsCorrectButton()
    {
        var menu = new Menu { Id = "m", Title = "T" };
        menu.AddButton(new MenuButton { Label = "Settings", CallbackData = "settings" });
        menu.AddButton(new MenuButton { Label = "Help", CallbackData = "help" });

        var button = menu.GetButton("settings");

        button.Should().NotBeNull();
        button!.Label.Should().Be("Settings");
    }

    [Fact]
    public void GetButton_WhenCallbackDataNotFound_ReturnsNull()
    {
        var menu = new Menu { Id = "m", Title = "T" };

        menu.GetButton("does-not-exist").Should().BeNull();
    }

    [Fact]
    public void GetArrangedButtons_WithFiveButtonsAndMaxTwoPerRow_ProducesThreeRows()
    {
        var menu = new Menu { Id = "m", Title = "T", MaxButtonsPerRow = 2 };
        for (int i = 1; i <= 5; i++)
            menu.AddButton(new MenuButton { Label = $"B{i}", CallbackData = $"b{i}" });

        var rows = menu.GetArrangedButtons();

        rows.Should().HaveCount(3);
        rows[0].Should().HaveCount(2);
        rows[1].Should().HaveCount(2);
        rows[2].Should().HaveCount(1);
    }

    [Fact]
    public void GetArrangedButtons_WithExactMultipleOfMaxPerRow_ProducesEvenRows()
    {
        var menu = new Menu { Id = "m", Title = "T", MaxButtonsPerRow = 3 };
        for (int i = 1; i <= 6; i++)
            menu.AddButton(new MenuButton { Label = $"B{i}", CallbackData = $"b{i}" });

        var rows = menu.GetArrangedButtons();

        rows.Should().HaveCount(2);
        rows.Should().AllSatisfy(r => r.Should().HaveCount(3));
    }

    [Fact]
    public void Validate_WithNoButtons_ThrowsInvalidOperationException()
    {
        var menu = new Menu { Id = "empty", Title = "Empty" };

        var act = () => menu.Validate();

        act.Should().Throw<InvalidOperationException>().WithMessage("*button*");
    }

    [Fact]
    public void Validate_WithEmptyId_ThrowsInvalidOperationException()
    {
        var menu = new Menu { Id = "", Title = "T" };
        menu.AddButton(new MenuButton { Label = "X", CallbackData = "x" });

        var act = () => menu.Validate();

        act.Should().Throw<InvalidOperationException>().WithMessage("*Id*");
    }
}

public sealed class InMemoryUserRepositoryTests
{
    private static BotUser CreateUser(long telegramId, string firstName, string? username = null,
        UserStatus status = UserStatus.Active) =>
        new()
        {
            TelegramId = telegramId,
            FirstName = firstName,
            Username = username,
            Status = status
        };

    [Fact]
    public async Task CreateAsync_ValidUser_StoresAndReturnsUser()
    {
        var repo = new InMemoryUserRepository();
        var user = CreateUser(1001, "Alice");

        var result = await repo.CreateAsync(user).ConfigureAwait(false);

        result.Should().NotBeNull();
        result.TelegramId.Should().Be(1001);
    }

    [Fact]
    public async Task GetByIdAsync_WhenUserExists_ReturnsUser()
    {
        var repo = new InMemoryUserRepository();
        await repo.CreateAsync(CreateUser(2002, "Bob")).ConfigureAwait(false);

        var result = await repo.GetByIdAsync(2002).ConfigureAwait(false);

        result.Should().NotBeNull();
        result!.FirstName.Should().Be("Bob");
    }

    [Fact]
    public async Task GetByIdAsync_WhenUserDoesNotExist_ReturnsNull()
    {
        var repo = new InMemoryUserRepository();

        var result = await repo.GetByIdAsync(9999).ConfigureAwait(false);

        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WhenUserExists_ReturnsTrueAndRemovesEntry()
    {
        var repo = new InMemoryUserRepository();
        await repo.CreateAsync(CreateUser(3003, "Carol")).ConfigureAwait(false);

        var deleted = await repo.DeleteAsync(3003).ConfigureAwait(false);

        deleted.Should().BeTrue();
        (await repo.GetByIdAsync(3003)).Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WhenUserDoesNotExist_ReturnsFalse()
    {
        var repo = new InMemoryUserRepository();

        var deleted = await repo.DeleteAsync(99999).ConfigureAwait(false);

        deleted.Should().BeFalse();
    }

    [Fact]
    public async Task GetByStatusAsync_FiltersUsersByStatus()
    {
        var repo = new InMemoryUserRepository();
        await repo.CreateAsync(CreateUser(1, "Active1")).ConfigureAwait(false);
        await repo.CreateAsync(CreateUser(2, "Banned", status: UserStatus.Banned)).ConfigureAwait(false);
        await repo.CreateAsync(CreateUser(3, "Active2")).ConfigureAwait(false);

        var banned = await repo.GetByStatusAsync(UserStatus.Banned).ConfigureAwait(false);

        banned.Should().HaveCount(1);
        banned[0].FirstName.Should().Be("Banned");
    }

    [Fact]
    public async Task SearchAsync_ByPartialFirstName_ReturnsAllMatches()
    {
        var repo = new InMemoryUserRepository();
        await repo.CreateAsync(CreateUser(1, "Alexander")).ConfigureAwait(false);
        await repo.CreateAsync(CreateUser(2, "Alex")).ConfigureAwait(false);
        await repo.CreateAsync(CreateUser(3, "Bobby")).ConfigureAwait(false);

        var results = await repo.SearchAsync("alex").ConfigureAwait(false);

        results.Should().HaveCount(2);
    }

    [Fact]
    public async Task CountAsync_ReturnsCorrectUserCount()
    {
        var repo = new InMemoryUserRepository();
        await repo.CreateAsync(CreateUser(1, "One")).ConfigureAwait(false);
        await repo.CreateAsync(CreateUser(2, "Two")).ConfigureAwait(false);
        await repo.CreateAsync(CreateUser(3, "Three")).ConfigureAwait(false);

        var count = await repo.CountAsync().ConfigureAwait(false);

        count.Should().Be(3);
    }
}