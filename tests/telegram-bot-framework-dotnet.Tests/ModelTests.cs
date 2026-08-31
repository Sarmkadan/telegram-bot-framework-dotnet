#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using FluentAssertions;
using TelegramBotFramework.Models;
using TelegramBotFramework.Repositories;
using Xunit;

namespace TelegramBotFramework.Tests;

/// <summary>
/// Unit tests for the <see cref="BotUser"/> class.
/// </summary>
public sealed class BotUserTests : IBotUserTests
{
	/// <summary>
	/// Tests that GetDisplayName returns the full name when both first and last names are provided.
	/// </summary>
	[Fact]
	public void GetDisplayName_WithFirstAndLastName_ReturnsFullName()
	{
		var user = new BotUser { TelegramId = BotUserTestsConstants.DefaultTelegramId, FirstName = "John", LastName = "Doe" };

		var name = user.GetDisplayName();

		name.Should().Be("John Doe");
	}

	/// <summary>
	/// Tests that GetDisplayName returns only the first name when last name is not provided.
	/// </summary>
	[Fact]
	public void GetDisplayName_WithoutLastName_ReturnsFirstNameOnly()
	{
		var user = new BotUser { TelegramId = BotUserTestsConstants.DefaultTelegramId, FirstName = "Alice" };

		var name = user.GetDisplayName();

		name.Should().Be("Alice");
	}

	/// <summary>
	/// Tests that Validate throws InvalidOperationException when TelegramId is not positive.
	/// </summary>
	[Fact]
	public void Validate_WithNonPositiveTelegramId_ThrowsInvalidOperationException()
	{
		var user = new BotUser { TelegramId = 0, FirstName = BotUserTestsConstants.TestFirstName };

		var act = () => user.Validate();

		act.Should().Throw<InvalidOperationException>().WithMessage("*TelegramId*");
	}

	/// <summary>
	/// Tests that Validate throws InvalidOperationException when FirstName is empty or whitespace.
	/// </summary>
	[Fact]
	public void Validate_WithEmptyFirstName_ThrowsInvalidOperationException()
	{
		var user = new BotUser { TelegramId = 999, FirstName = " " };

		var act = () => user.Validate();

		act.Should().Throw<InvalidOperationException>().WithMessage("*FirstName*");
	}

	/// <summary>
	/// Tests that UpdateActivity increments the MessagesCount property by 1.
	/// </summary>
	[Fact]
	public void UpdateActivity_IncrementsMessagesCount()
	{
		var user = new BotUser { TelegramId = BotUserTestsConstants.DefaultTelegramId, FirstName = BotUserTestsConstants.TestFirstName };
		var before = user.MessagesCount;

		user.UpdateActivity();

		user.MessagesCount.Should().Be(before + BotUserTestsConstants.SingleItemCount);
	}

	/// <summary>
	/// Tests that SetMetadata stores a value and GetMetadata retrieves the same value.
	/// </summary>
	[Fact]
	public void SetMetadata_AndGetMetadata_RoundTripsValue()
	{
		var user = new BotUser { TelegramId = BotUserTestsConstants.DefaultTelegramId, FirstName = BotUserTestsConstants.TestFirstName };

		user.SetMetadata("plan", "premium");

		user.GetMetadata("plan").Should().Be("premium");
	}

	/// <summary>
	/// Tests that GetMetadata returns null when the requested metadata key does not exist.
	/// </summary>
	[Fact]
	public void GetMetadata_WhenKeyNotPresent_ReturnsNull()
	{
		var user = new BotUser { TelegramId = BotUserTestsConstants.DefaultTelegramId, FirstName = BotUserTestsConstants.TestFirstName };

		user.GetMetadata("missing").Should().BeNull();
	}

	/// <summary>
	/// Tests that SetMetadata overwrites existing metadata values for the same key.
	/// </summary>
	[Fact]
	public void SetMetadata_OverwritesExistingKey()
	{
		var user = new BotUser { TelegramId = BotUserTestsConstants.DefaultTelegramId, FirstName = BotUserTestsConstants.TestFirstName };
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
		var command = new Command { Name = BotUserTestsConstants.BanCommandName, HandlerType = BotUserTestsConstants.CommandHandlerType, RequiresAdmin = true, IsEnabled = true };

		command.CanExecuteBy(UserRole.User).Should().BeFalse();
	}

	[Fact]
	public void CanExecuteBy_AdminCommandAndModeratorRole_ReturnsFalse()
	{
		var command = new Command { Name = BotUserTestsConstants.BanCommandName, HandlerType = BotUserTestsConstants.CommandHandlerType, RequiresAdmin = true, IsEnabled = true };

		command.CanExecuteBy(UserRole.Moderator).Should().BeFalse();
	}

	[Fact]
	public void CanExecuteBy_AdminCommandAndAdminRole_ReturnsTrue()
	{
		var command = new Command { Name = BotUserTestsConstants.BanCommandName, HandlerType = BotUserTestsConstants.CommandHandlerType, RequiresAdmin = true, IsEnabled = true };

		command.CanExecuteBy(UserRole.Administrator).Should().BeTrue();
	}

	[Fact]
	public void CanExecuteBy_WhenCommandIsDisabled_ReturnsFalseForAnyRole()
	{
		var command = new Command { Name = BotUserTestsConstants.StartCommandName, HandlerType = BotUserTestsConstants.CommandHandlerType, IsEnabled = false };

		command.CanExecuteBy(UserRole.Owner).Should().BeFalse();
	}

	[Fact]
	public void RecordExecution_IncrementsExecutionCount()
	{
		var command = new Command { Name = BotUserTestsConstants.TestCommandName, HandlerType = BotUserTestsConstants.CommandHandlerType };

		command.RecordExecution();

		command.ExecutionCount.Should().Be(BotUserTestsConstants.SingleItemCount);
	}

	[Fact]
	public void RecordExecution_CalledMultipleTimes_AccumulatesCount()
	{
		var command = new Command { Name = BotUserTestsConstants.TestCommandName, HandlerType = BotUserTestsConstants.CommandHandlerType };

		command.RecordExecution();
		command.RecordExecution();
		command.RecordExecution();

		command.ExecutionCount.Should().Be(BotUserTestsConstants.ThreeItemCount);
	}

	[Fact]
	public void IsRateLimited_WhenExecutionsAtLimit_ReturnsTrue()
	{
		var command = new Command { Name = BotUserTestsConstants.FloodCommandName, HandlerType = BotUserTestsConstants.CommandHandlerType, RateLimitPerMinute = BotUserTestsConstants.RateLimitPerMinute };

		command.IsRateLimited(BotUserTestsConstants.RateLimitPerMinute).Should().BeTrue();
	}

	[Fact]
	public void IsRateLimited_WhenExecutionsBelowLimit_ReturnsFalse()
	{
		var command = new Command { Name = BotUserTestsConstants.FloodCommandName, HandlerType = BotUserTestsConstants.CommandHandlerType, RateLimitPerMinute = BotUserTestsConstants.RateLimitPerMinute };

		command.IsRateLimited(BotUserTestsConstants.ExecutionsBelowRateLimit).Should().BeFalse();
	}

	[Fact]
	public void IsRateLimited_WhenNoLimitConfigured_ReturnsFalseRegardlessOfCount()
	{
		var command = new Command { Name = "/open", HandlerType = BotUserTestsConstants.CommandHandlerType, RateLimitPerMinute = null };

		command.IsRateLimited(BotUserTestsConstants.UnlimitedExecutionCount).Should().BeFalse();
	}

	[Fact]
	public void GetCommandPatterns_WithAlias_ReturnsBothNameAndAlias()
	{
		var command = new Command { Name = BotUserTestsConstants.StartCommandName, HandlerType = BotUserTestsConstants.CommandHandlerType, Aliases = new List<string> { "/go" } };

		var patterns = command.GetCommandPatterns().ToList();

		patterns.Should().HaveCount(BotUserTestsConstants.TwoItemCount);
		patterns.Should().ContainInOrder(BotUserTestsConstants.StartCommandName, "/go");
	}

	[Fact]
	public void GetCommandPatterns_WithoutAlias_ReturnsOnlyName()
	{
		var command = new Command { Name = "/help", HandlerType = BotUserTestsConstants.CommandHandlerType };

		var patterns = command.GetCommandPatterns().ToList();

		patterns.Should().HaveCount(BotUserTestsConstants.SingleItemCount);
		patterns[0].Should().Be("/help");
	}

	[Fact]
	public void Validate_StandardCommandMissingLeadingSlash_ThrowsInvalidOperationException()
	{
		var command = new Command { Name = "start", HandlerType = BotUserTestsConstants.CommandHandlerType, Type = CommandType.Standard };

		var act = () => command.Validate();

		act.Should().Throw<InvalidOperationException>().WithMessage("*/*");
	}

	[Fact]
	public void Validate_CommandWithEmptyName_ThrowsInvalidOperationException()
	{
		var command = new Command { Name = "", HandlerType = BotUserTestsConstants.CommandHandlerType };

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
			SessionId = BotUserTestsConstants.SessionId,
			UserId = BotUserTestsConstants.DefaultUserId,
			ChatId = BotUserTestsConstants.SessionChatId,
			ExpiresAt = DateTime.UtcNow.AddMinutes(BotUserTestsConstants.PastExpirationMinutes)
		};

		session.IsExpired().Should().BeTrue();
	}

	[Fact]
	public void IsExpired_WhenNoExpiresAtSet_ReturnsFalse()
	{
		var session = new UserSession { SessionId = BotUserTestsConstants.SessionId, UserId = BotUserTestsConstants.DefaultUserId, ChatId = BotUserTestsConstants.SessionChatId };

		session.IsExpired().Should().BeFalse();
	}

	[Fact]
	public void IsExpired_WhenExpiresAtIsInTheFuture_ReturnsFalse()
	{
		var session = new UserSession
		{
			SessionId = BotUserTestsConstants.SessionId,
			UserId = BotUserTestsConstants.DefaultUserId,
			ChatId = BotUserTestsConstants.SessionChatId,
			ExpiresAt = DateTime.UtcNow.AddHours(BotUserTestsConstants.FutureExpirationHours)
		};

		session.IsExpired().Should().BeFalse();
	}

	[Fact]
	public void UpdateActivity_CalledTwice_IncrementsInteractionCountToTwo()
	{
		var session = new UserSession { SessionId = BotUserTestsConstants.ShortSessionId, UserId = BotUserTestsConstants.DefaultUserId, ChatId = BotUserTestsConstants.DefaultChatId };

		session.UpdateActivity();
		session.UpdateActivity();

		session.InteractionCount.Should().Be(BotUserTestsConstants.TwoItemCount);
	}

	[Fact]
	public void SetContextData_AndGetContextData_StoresAndRetrieves()
	{
		var session = new UserSession { SessionId = BotUserTestsConstants.ShortSessionId, UserId = BotUserTestsConstants.DefaultUserId, ChatId = BotUserTestsConstants.DefaultChatId };

		session.SetContextData("order_id", "ORD-42");

		session.GetContextData("order_id").Should().Be("ORD-42");
	}

	[Fact]
	public void RemoveContextData_WhenKeyExists_ReturnsTrueAndRemovesEntry()
	{
		var session = new UserSession { SessionId = BotUserTestsConstants.ShortSessionId, UserId = BotUserTestsConstants.DefaultUserId, ChatId = BotUserTestsConstants.DefaultChatId };
		session.SetContextData("temp", "value");

		var removed = session.RemoveContextData("temp");

		removed.Should().BeTrue();
		session.GetContextData("temp").Should().BeNull();
	}

	[Fact]
	public void ClearContextData_AfterSettingMultipleKeys_EmptiesAllData()
	{
		var session = new UserSession { SessionId = BotUserTestsConstants.ShortSessionId, UserId = BotUserTestsConstants.DefaultUserId, ChatId = BotUserTestsConstants.DefaultChatId };
		session.SetContextData(BotUserTestsConstants.FirstContextKey, "v1");
		session.SetContextData(BotUserTestsConstants.SecondContextKey, "v2");

		session.ClearContextData();

		session.GetContextData(BotUserTestsConstants.FirstContextKey).Should().BeNull();
		session.GetContextData(BotUserTestsConstants.SecondContextKey).Should().BeNull();
	}

	[Fact]
	public void AddCommandToHistory_When55CommandsAdded_CapsAtFiftyEntries()
	{
		var session = new UserSession { SessionId = BotUserTestsConstants.ShortSessionId, UserId = BotUserTestsConstants.DefaultUserId, ChatId = BotUserTestsConstants.DefaultChatId };

		for (int i = 0; i < BotUserTestsConstants.CommandsAddedBeyondHistoryLimit; i++)
			session.AddCommandToHistory(string.Format(BotUserTestsConstants.CommandHistoryFormat, i));

		session.GetCommandHistory().Count().Should().Be(BotUserTestsConstants.CommandHistoryLimit);
	}

	[Fact]
	public void Validate_WhenSessionIdIsEmpty_ThrowsInvalidOperationException()
	{
		var session = new UserSession { SessionId = "", UserId = BotUserTestsConstants.DefaultUserId, ChatId = BotUserTestsConstants.DefaultChatId };

		var act = () => session.Validate();

		act.Should().Throw<InvalidOperationException>().WithMessage("*SessionId*");
	}

	[Fact]
	public void Validate_WhenUserIdIsZero_ThrowsInvalidOperationException()
	{
		var session = new UserSession { SessionId = BotUserTestsConstants.ShortSessionId, UserId = 0, ChatId = BotUserTestsConstants.DefaultChatId };

		var act = () => session.Validate();

		act.Should().Throw<InvalidOperationException>().WithMessage("*UserId*");
	}
}

public sealed class MenuTests
{
	[Fact]
	public void AddButton_IncreasesButtonCount()
	{
		var menu = new Menu { Id = BotUserTestsConstants.MainMenuId, Title = "Main Menu" };

		menu.AddButton(new MenuButton { Label = "Option 1", CallbackData = "opt1" });

		menu.Buttons.Should().HaveCount(BotUserTestsConstants.SingleItemCount);
	}

	[Fact]
	public void RemoveButton_ByCallbackData_RemovesCorrectButtonAndLeavesOthers()
	{
		var menu = new Menu { Id = BotUserTestsConstants.MainMenuId, Title = "Main" };
		menu.AddButton(new MenuButton { Label = "A", CallbackData = "a" });
		menu.AddButton(new MenuButton { Label = "B", CallbackData = "b" });

		var removed = menu.RemoveButton("a");

		removed.Should().BeTrue();
		menu.Buttons.Should().HaveCount(BotUserTestsConstants.SingleItemCount);
		menu.Buttons[0].CallbackData.Should().Be("b");
	}

	[Fact]
	public void RemoveButton_WhenCallbackDataNotFound_ReturnsFalse()
	{
		var menu = new Menu { Id = BotUserTestsConstants.MainMenuId, Title = "Main" };
		menu.AddButton(new MenuButton { Label = "A", CallbackData = "a" });

		menu.RemoveButton("nonexistent").Should().BeFalse();
	}

	[Fact]
	public void GetButton_WithMatchingCallbackData_ReturnsCorrectButton()
	{
		var menu = new Menu { Id = BotUserTestsConstants.CompactMenuId, Title = BotUserTestsConstants.CompactMenuTitle };
		menu.AddButton(new MenuButton { Label = "Settings", CallbackData = "settings" });
		menu.AddButton(new MenuButton { Label = "Help", CallbackData = "help" });

		var button = menu.GetButton("settings");

		button.Should().NotBeNull();
		button!.Label.Should().Be("Settings");
	}

	[Fact]
	public void GetButton_WhenCallbackDataNotFound_ReturnsNull()
	{
		var menu = new Menu { Id = BotUserTestsConstants.CompactMenuId, Title = BotUserTestsConstants.CompactMenuTitle };

		menu.GetButton("does-not-exist").Should().BeNull();
	}

	[Fact]
	public void GetArrangedButtons_WithFiveButtonsAndMaxTwoPerRow_ProducesThreeRows()
	{
		var menu = new Menu { Id = BotUserTestsConstants.CompactMenuId, Title = BotUserTestsConstants.CompactMenuTitle, MaxButtonsPerRow = BotUserTestsConstants.TwoButtonsPerRow };
		for (int i = BotUserTestsConstants.SingleItemCount; i <= BotUserTestsConstants.FiveButtonCount; i++)
			menu.AddButton(new MenuButton { Label = $"B{i}", CallbackData = $"b{i}" });

		var rows = menu.GetArrangedButtons();

		rows.Should().HaveCount(BotUserTestsConstants.ThreeItemCount);
		rows[0].Should().HaveCount(BotUserTestsConstants.TwoItemCount);
		rows[1].Should().HaveCount(BotUserTestsConstants.TwoItemCount);
		rows[2].Should().HaveCount(BotUserTestsConstants.SingleItemCount);
	}

	[Fact]
	public void GetArrangedButtons_WithExactMultipleOfMaxPerRow_ProducesEvenRows()
	{
		var menu = new Menu { Id = BotUserTestsConstants.CompactMenuId, Title = BotUserTestsConstants.CompactMenuTitle, MaxButtonsPerRow = BotUserTestsConstants.ThreeButtonsPerRow };
		for (int i = BotUserTestsConstants.SingleItemCount; i <= BotUserTestsConstants.SixButtonCount; i++)
			menu.AddButton(new MenuButton { Label = $"B{i}", CallbackData = $"b{i}" });

		var rows = menu.GetArrangedButtons();

		rows.Should().HaveCount(BotUserTestsConstants.TwoItemCount);
		rows.Should().AllSatisfy(r => r.Should().HaveCount(BotUserTestsConstants.ThreeItemCount));
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
		var menu = new Menu { Id = "", Title = BotUserTestsConstants.CompactMenuTitle };
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
		var user = CreateUser(BotUserTestsConstants.CreatedUserTelegramId, "Alice");

		var result = await repo.CreateAsync(user).ConfigureAwait(false);

		result.Should().NotBeNull();
		result.TelegramId.Should().Be(BotUserTestsConstants.CreatedUserTelegramId);
	}

	[Fact]
	public async Task GetByIdAsync_WhenUserExists_ReturnsUser()
	{
		var repo = new InMemoryUserRepository();
		await repo.CreateAsync(CreateUser(BotUserTestsConstants.ExistingUserTelegramId, "Bob")).ConfigureAwait(false);

		var result = await repo.GetByIdAsync(BotUserTestsConstants.ExistingUserTelegramId).ConfigureAwait(false);

		result.Should().NotBeNull();
		result!.FirstName.Should().Be("Bob");
	}

	[Fact]
	public async Task GetByIdAsync_WhenUserDoesNotExist_ReturnsNull()
	{
		var repo = new InMemoryUserRepository();

		var result = await repo.GetByIdAsync(BotUserTestsConstants.MissingUserTelegramId).ConfigureAwait(false);

		result.Should().BeNull();
	}

	[Fact]
	public async Task DeleteAsync_WhenUserExists_ReturnsTrueAndRemovesEntry()
	{
		var repo = new InMemoryUserRepository();
		await repo.CreateAsync(CreateUser(BotUserTestsConstants.DeletedUserTelegramId, "Carol")).ConfigureAwait(false);

		var deleted = await repo.DeleteAsync(BotUserTestsConstants.DeletedUserTelegramId).ConfigureAwait(false);

		deleted.Should().BeTrue();
		(await repo.GetByIdAsync(BotUserTestsConstants.DeletedUserTelegramId)).Should().BeNull();
	}

	[Fact]
	public async Task DeleteAsync_WhenUserDoesNotExist_ReturnsFalse()
	{
		var repo = new InMemoryUserRepository();

		var deleted = await repo.DeleteAsync(BotUserTestsConstants.MissingDeletedUserTelegramId).ConfigureAwait(false);

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

		banned.Should().HaveCount(BotUserTestsConstants.SingleItemCount);
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

		results.Should().HaveCount(BotUserTestsConstants.TwoItemCount);
	}

	[Fact]
	public async Task CountAsync_ReturnsCorrectUserCount()
	{
		var repo = new InMemoryUserRepository();
		await repo.CreateAsync(CreateUser(1, "One")).ConfigureAwait(false);
		await repo.CreateAsync(CreateUser(2, "Two")).ConfigureAwait(false);
		await repo.CreateAsync(CreateUser(3, "Three")).ConfigureAwait(false);

		var count = await repo.CountAsync().ConfigureAwait(false);

		count.Should().Be(BotUserTestsConstants.ThreeItemCount);
	}
}
