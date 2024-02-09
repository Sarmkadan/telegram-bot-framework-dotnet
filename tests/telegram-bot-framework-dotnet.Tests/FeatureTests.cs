#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using TelegramBotFramework.ConversationFlow;
using TelegramBotFramework.Events;
using TelegramBotFramework.Integration;
using TelegramBotFramework.Keyboard;
using TelegramBotFramework.Services;
using Xunit;

namespace TelegramBotFramework.Tests;

// =============================================================================
// Feature 1: InlineKeyboardBuilder tests
// =============================================================================

/// <summary>
/// Provides unit tests for the <see cref="TelegramBotFramework.Keyboard.InlineKeyboardBuilder"/> class.
/// Tests various scenarios for building inline keyboard markup including different button types,
/// automatic row wrapping, manual row control, and error conditions.
/// </summary>
public sealed class InlineKeyboardBuilderTests
{
	/// <summary>
	/// Tests that building a keyboard with a single callback button creates a markup with one row and one button.
	/// Verifies the button text, callback data, and button type are correctly set.
	/// </summary>
	[Fact]
	public void Build_WithSingleCallbackButton_CreatesOneRowOneButton()
	{
		var markup = InlineKeyboardBuilder.Create()
			.AddButton("Click me", "click")
			.Build();

		markup.RowCount.Should().Be(1);
	markup.TotalButtonCount.Should().Be(1);
	markup.InlineKeyboard[0][0].Text.Should().Be("Click me");
	markup.InlineKeyboard[0][0].CallbackData.Should().Be("click");
	markup.InlineKeyboard[0][0].Type.Should().Be(InlineButtonType.Callback);
	}

	/// <summary>
	/// Tests that adding a URL button creates a button with the correct type and URL.
	/// Verifies the button type is set to Url and the URL is correctly assigned.
	/// </summary>
	[Fact]
	public void Build_WithUrlButton_SetsTypeAndUrl()
	{
		var markup = InlineKeyboardBuilder.Create()
			.AddUrlButton("Visit", "https://example.com")
			.Build();

		var btn = markup.InlineKeyboard[0][0];
	btn.Type.Should().Be(InlineButtonType.Url);
	btn.Url.Should().Be("https://example.com");
	btn.CallbackData.Should().BeNull();
	}

	/// <summary>
	/// Tests that adding a switch inline button creates a button with the correct type and inline query.
	/// Verifies the button type is set to SwitchInline and the switch inline query is correctly assigned.
	/// </summary>
	[Fact]
	public void Build_WithSwitchInlineButton_SetsTypeAndQuery()
	{
		var markup = InlineKeyboardBuilder.Create()
			.AddSwitchInlineButton("Search", "my query")
			.Build();

		var btn = markup.InlineKeyboard[0][0];
	btn.Type.Should().Be(InlineButtonType.SwitchInline);
	btn.SwitchInlineQuery.Should().Be("my query");
	}

	/// <summary>
	/// Tests that the keyboard builder automatically wraps buttons into multiple rows when the maximum buttons per row is exceeded.
	/// Verifies that buttons are distributed across multiple rows based on the maxButtonsPerRow configuration.
	/// </summary>
	[Fact]
	public void Build_AutoWrapsButtonsAtMaxPerRow()
	{
		var markup = InlineKeyboardBuilder.Create(maxButtonsPerRow: 2)
			.AddButton("A", "a")
			.AddButton("B", "b")
			.AddButton("C", "c")
			.Build();

		markup.RowCount.Should().Be(2);
	markup.InlineKeyboard[0].Count.Should().Be(2);
	markup.InlineKeyboard[1].Count.Should().Be(1);
	}

	/// <summary>
	/// Tests that calling NewRow() forces a row break before the maximum buttons per row is reached.
	/// Verifies that NewRow() creates a new row regardless of the current button count.
	/// </summary>
	[Fact]
	public void NewRow_ForcesRowBreakBeforeMaxReached()
	{
		var markup = InlineKeyboardBuilder.Create(maxButtonsPerRow: 3)
			.AddButton("A", "a")
			.NewRow()
			.AddButton("B", "b")
			.Build();

		markup.RowCount.Should().Be(2);
	markup.InlineKeyboard[0][0].Text.Should().Be("A");
	markup.InlineKeyboard[1][0].Text.Should().Be("B");
	}

	/// <summary>
	/// Tests that the ToButtonLabels() method returns a two-dimensional array of button labels.
	/// Verifies the method correctly converts the keyboard markup into a label array.
	/// </summary>
	[Fact]
	public void ToButtonLabels_ReturnsTwoDimensionalLabelArray()
	{
		var markup = InlineKeyboardBuilder.Create(maxButtonsPerRow: 2)
			.AddButton("Yes", "yes")
			.AddButton("No", "no")
			.Build();

		var labels = markup.ToButtonLabels();

		labels.Should().HaveCount(1);
	labels[0].Should().BeEquivalentTo(new[] { "Yes", "No" });
	}

	/// <summary>
	/// Tests that the ToMenu() method converts an inline keyboard markup to a menu model.
	/// Verifies the menu is created with the correct ID, title, and buttons with proper properties.
	/// </summary>
	[Fact]
	public void ToMenu_ConvertsMarkupToMenuModel()
	{
		var menu = InlineKeyboardBuilder.Create()
			.AddButton("Help", "help")
			.AddUrlButton("Docs", "https://docs.example.com")
			.ToMenu("main_menu", "Main Menu");

		menu.Id.Should().Be("main_menu");
	menu.Title.Should().Be("Main Menu");
	menu.Buttons.Should().HaveCount(2);
	menu.Buttons[0].CallbackData.Should().Be("help");
	menu.Buttons[1].Url.Should().Be("https://docs.example.com");
	menu.Buttons[1].Action.Should().Be(Models.ButtonAction.OpenUrl);
	}

	/// <summary>
	/// Tests that attempting to build a keyboard with no buttons throws an InvalidOperationException.
	/// Verifies the builder enforces the requirement for at least one button.
	/// </summary>
	[Fact]
	public void Build_WithNoButtons_ThrowsInvalidOperationException()
	{
		var act = () => InlineKeyboardBuilder.Create().Build();

		act.Should().Throw<InvalidOperationException>()
			.WithMessage("*empty keyboard*");
	}

	/// <summary>
	/// Tests that adding a button with callback data exceeding 64 bytes throws an ArgumentException.
	/// Verifies the builder enforces the 64-byte limit on callback data.
	/// </summary>
	[Fact]
	public void AddButton_WithCallbackDataExceeding64Bytes_ThrowsArgumentException()
	{
		var longData = new string('x', 65);

		var act = () => InlineKeyboardBuilder.Create().AddButton("Test", longData);

		act.Should().Throw<ArgumentException>()
			.WithMessage("*64*byte*");
	}

	/// <summary>
	/// Tests that adding a button with empty text throws an ArgumentException.
	/// Verifies the builder enforces non-empty button text.
	/// </summary>
	[Fact]
	public void AddButton_WithEmptyText_ThrowsArgumentException()
	{
		var act = () => InlineKeyboardBuilder.Create().AddButton("", "data");

		act.Should().Throw<ArgumentException>();
	}
}

// =============================================================================
// Feature 2: Conversation state persistence tests
// =============================================================================

public sealed class InMemoryConversationStateStoreTests
{
	private readonly InMemoryConversationStateStore _store = new();

	private static UserFlowState MakeState(long userId, FlowStateStatus status = FlowStateStatus.Active)
	=> new()
	{
		StateId = Guid.NewGuid().ToString("N"),
		FlowId = "test-flow",
		UserId = userId,
		ChatId = 100L,
		CurrentStepId = "step1",
		Status = status,
		StartedAt = DateTime.UtcNow,
		LastActivityAt = DateTime.UtcNow
	};

	[Fact]
	public async Task SaveAndLoad_RoundTrip_ReturnsPersistedState()
	{
		var state = MakeState(42L);
		await _store.SaveStateAsync(state);

		var loaded = await _store.LoadStateAsync(42L);

		loaded.Should().NotBeNull();
	loaded!.StateId.Should().Be(state.StateId);
	loaded.FlowId.Should().Be("test-flow");
	}

	[Fact]
	public async Task LoadStateAsync_WhenNoState_ReturnsNull()
	{
		var loaded = await _store.LoadStateAsync(999L);

		loaded.Should().BeNull();
	}

	[Fact]
	public async Task DeleteStateAsync_RemovesPersistedState()
	{
		var state = MakeState(10L);
		await _store.SaveStateAsync(state);

		await _store.DeleteStateAsync(10L);

		var loaded = await _store.LoadStateAsync(10L);
	loaded.Should().BeNull();
	}

	[Fact]
	public async Task LoadAllActiveStatesAsync_ReturnsOnlyActiveAndWaiting()
	{
		await _store.SaveStateAsync(MakeState(1L, FlowStateStatus.Active));
		await _store.SaveStateAsync(MakeState(2L, FlowStateStatus.WaitingForInput));
		await _store.SaveStateAsync(MakeState(3L, FlowStateStatus.Completed));
		await _store.SaveStateAsync(MakeState(4L, FlowStateStatus.Aborted));

		var active = await _store.LoadAllActiveStatesAsync();

		active.Should().HaveCount(2);
	active.Select(s => s.UserId).Should().BeEquivalentTo(new[] { 1L, 2L });
	}

	[Fact]
	public async Task SaveStateAsync_Overwrites_ExistingEntry()
	{
		var state = MakeState(7L);
		await _store.SaveStateAsync(state);

		state.CurrentStepId = "step2";
		await _store.SaveStateAsync(state);

		var loaded = await _store.LoadStateAsync(7L);
	loaded!.CurrentStepId.Should().Be("step2");
	}

	[Fact]
	public async Task DeleteStateAsync_OnNonExistentUser_DoesNotThrow()
	{
		var act = async () => await _store.DeleteStateAsync(9999L);
	await act.Should().NotThrowAsync();
	}
}

public sealed class FileConversationStateStoreTests : IDisposable
{
	private readonly string _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
	private readonly FileConversationStateStore _store;

	public FileConversationStateStoreTests()
	{
		_store = new FileConversationStateStore(_tempDir);
	}

	public void Dispose()
	{
	if (Directory.Exists(_tempDir))
	Directory.Delete(_tempDir, recursive: true);
	}

	private static UserFlowState MakeState(long userId, FlowStateStatus status = FlowStateStatus.Active)
	=> new()
	{
		StateId = Guid.NewGuid().ToString("N"),
		FlowId = "file-flow",
		UserId = userId,
		ChatId = 200L,
		CurrentStepId = "step1",
		Status = status,
		StartedAt = DateTime.UtcNow,
		LastActivityAt = DateTime.UtcNow
	};

	[Fact]
	public async Task SaveAndLoad_RoundTrip_PersistsToFile()
	{
		var state = MakeState(55L);
		await _store.SaveStateAsync(state);

		File.Exists(Path.Combine(_tempDir, "55.json")).Should().BeTrue();

		var loaded = await _store.LoadStateAsync(55L);
	loaded.Should().NotBeNull();
	loaded!.StateId.Should().Be(state.StateId);
	loaded.FlowId.Should().Be("file-flow");
	}

	[Fact]
	public async Task DeleteStateAsync_RemovesFile()
	{
		var state = MakeState(66L);
		await _store.SaveStateAsync(state);

		await _store.DeleteStateAsync(66L);

		File.Exists(Path.Combine(_tempDir, "66.json")).Should().BeFalse();
	var loaded = await _store.LoadStateAsync(66L);
	loaded.Should().BeNull();
	}

	[Fact]
	public async Task LoadAllActiveStatesAsync_RestoresActiveFromDisk()
	{
		await _store.SaveStateAsync(MakeState(100L, FlowStateStatus.Active));
		await _store.SaveStateAsync(MakeState(101L, FlowStateStatus.WaitingForInput));
		await _store.SaveStateAsync(MakeState(102L, FlowStateStatus.Completed));

		// Create a fresh store pointing at the same directory (simulating restart)
		var freshStore = new FileConversationStateStore(_tempDir);
		var active = await freshStore.LoadAllActiveStatesAsync();

		active.Should().HaveCount(2);
	}
}

// =============================================================================
// Feature 3: WebhookService & WebhookHandler tests
// =============================================================================

public sealed class WebhookHandlerTests
{
	private readonly WebhookHandler _handler = new();

	[Fact]
	public async Task ProcessUpdateAsync_WithValidMessageJson_ParsesChatAndUserId()
	{
		const string json = """
		{
			"update_id": 123456,
			"message": {
				"message_id": 1,
				"from": { "id": 42, "first_name": "Alice" },
				"chat": { "id": 999 },
				"date": 1700000000,
				"text": "hello"
			}
		}
	""";

		var update = await _handler.ProcessUpdateAsync(json);

		update.Should().NotBeNull();
	update!.UpdateId.Should().Be(123456L);
	update.MessageType.Should().Be(UpdateType.Message);
	update.Message!.UserId.Should().Be(42L);
	update.Message.ChatId.Should().Be(999L);
	update.Message.Text.Should().Be("hello");
	}

	[Fact]
	public async Task ProcessUpdateAsync_WithCallbackQueryJson_ParsesCallbackData()
	{
		const string json = """
		{
			"update_id": 789,
			"callback_query": {
				"id": "cq-id",
				"data": "confirm",
				"message": {
					"message_id": 2,
					"from": { "id": 10, "first_name": "Bob" },
					"chat": { "id": 20 },
					"date": 1700000001
				}
			}
		}
	""";

		var update = await _handler.ProcessUpdateAsync(json);

		update.Should().NotBeNull();
	update!.MessageType.Should().Be(UpdateType.CallbackQuery);
	update.CallbackData.Should().Be("confirm");
	update.CallbackQueryId.Should().Be("cq-id");
	}

	[Fact]
	public async Task ProcessUpdateAsync_WithEmptyBody_ReturnsNull()
	{
		var update = await _handler.ProcessUpdateAsync(string.Empty);
	update.Should().BeNull();
	}

	[Fact]
	public async Task ProcessUpdateAsync_WithInvalidJson_ReturnsNull()
	{
		var update = await _handler.ProcessUpdateAsync("not-json");
	update.Should().BeNull();
	}

	[Fact]
	public void ValidateWebhookRequest_WithMatchingSignature_ReturnsTrue()
	{
		const string payload = "{\"update_id\":1}";
		const string secretKey = "my-secret";
		var signature = Utilities.CryptoUtility.ComputeHmacSHA256(payload, secretKey);

		var valid = _handler.ValidateWebhookRequest(payload, signature, secretKey);

		valid.Should().BeTrue();
	}

	[Fact]
	public void ValidateWebhookRequest_WithWrongSignature_ReturnsFalse()
	{
		const string payload = "{\"update_id\":1}";

		var valid = _handler.ValidateWebhookRequest(payload, "wrong-sig", "my-secret");

		valid.Should().BeFalse();
	}

	[Fact]
	public void ValidateWebhookRequest_WithNoSecretConfigured_ReturnsTrue()
	{
		var valid = _handler.ValidateWebhookRequest("{}", "any-sig", secretKey: null);
	valid.Should().BeTrue();
	}
}

public sealed class WebhookOptionsTests
{
	[Fact]
	public void Validate_WithValidHttpsUrl_DoesNotThrow()
	{
		var options = new WebhookOptions { Url = "https://mybot.example.com/api/webhook" };
		var act = () => options.Validate();
	act.Should().NotThrow();
	}

	[Fact]
	public void Validate_WithEmptyUrl_ThrowsInvalidOperationException()
	{
		var options = new WebhookOptions { Url = "" };
		var act = () => options.Validate();
	act.Should().Throw<InvalidOperationException>().WithMessage("*Url*");
	}

	[Fact]
	public void Validate_WithInvalidMaxConnections_ThrowsInvalidOperationException()
	{
		var options = new WebhookOptions { Url = "https://example.com", MaxConnections = 200 };
		var act = () => options.Validate();
	act.Should().Throw<InvalidOperationException>().WithMessage("*MaxConnections*");
	}
}
