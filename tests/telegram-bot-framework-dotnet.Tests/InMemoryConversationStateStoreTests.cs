using TelegramBotFramework.ConversationFlow;
using Xunit;

namespace TelegramBotFramework.Tests.ConversationFlow;

public sealed class InMemoryConversationStateStoreTests
{
    private readonly InMemoryConversationStateStore _store = new();

    [Fact]
    public async Task SaveStateAsync_ThenLoadStateAsync_ReturnsSameState()
    {
        var state = CreateState(42);

        await _store.SaveStateAsync(state);

        var loaded = await _store.LoadStateAsync(state.UserId);
        Assert.Same(state, loaded);
        Assert.Equal(1, _store.Count);
    }

    [Fact]
    public async Task SaveStateAsync_WithExistingUserId_ReplacesState()
    {
        var original = CreateState(42, FlowStateStatus.Active, "first");
        var replacement = CreateState(42, FlowStateStatus.WaitingForInput, "second");
        await _store.SaveStateAsync(original);

        await _store.SaveStateAsync(replacement);

        Assert.Same(replacement, await _store.LoadStateAsync(42));
        Assert.Equal(1, _store.Count);
    }

    [Fact]
    public async Task SaveStateAsync_WithNullState_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _store.SaveStateAsync(null!));
    }

    [Theory]
    [InlineData(long.MinValue)]
    [InlineData(0L)]
    [InlineData(long.MaxValue)]
    public async Task LoadStateAsync_WithMissingBoundaryUserId_ReturnsNull(long userId)
    {
        Assert.Null(await _store.LoadStateAsync(userId));
    }

    [Fact]
    public async Task DeleteStateAsync_RemovesState()
    {
        await _store.SaveStateAsync(CreateState(long.MaxValue));

        await _store.DeleteStateAsync(long.MaxValue);

        Assert.Null(await _store.LoadStateAsync(long.MaxValue));
        Assert.Equal(0, _store.Count);
    }

    [Fact]
    public async Task DeleteStateAsync_WithMissingUser_DoesNotThrow()
    {
        var exception = await Record.ExceptionAsync(
            () => _store.DeleteStateAsync(long.MinValue));

        Assert.Null(exception);
    }

    [Fact]
    public async Task LoadAllActiveStatesAsync_ReturnsOnlyActiveAndWaitingStates()
    {
        var active = CreateState(1, FlowStateStatus.Active);
        var waiting = CreateState(2, FlowStateStatus.WaitingForInput);
        await _store.SaveStateAsync(active);
        await _store.SaveStateAsync(waiting);
        await _store.SaveStateAsync(CreateState(3, FlowStateStatus.Suspended));
        await _store.SaveStateAsync(CreateState(4, FlowStateStatus.Completed));

        var states = await _store.LoadAllActiveStatesAsync();

        Assert.Equal(2, states.Count);
        Assert.Contains(active, states);
        Assert.Contains(waiting, states);
    }

    [Fact]
    public async Task LoadAllActiveStatesAsync_WithEmptyStore_ReturnsEmptyList()
    {
        var states = await _store.LoadAllActiveStatesAsync();

        Assert.Empty(states);
    }

    private static UserFlowState CreateState(
        long userId,
        FlowStateStatus status = FlowStateStatus.Active,
        string currentStepId = "step") => new()
    {
        StateId = Guid.NewGuid().ToString("N"),
        FlowId = "flow",
        UserId = userId,
        ChatId = 100,
        CurrentStepId = currentStepId,
        Status = status,
        StartedAt = DateTime.UtcNow,
        LastActivityAt = DateTime.UtcNow
    };
}
