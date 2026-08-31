#nullable enable

using FluentAssertions;
using TelegramBotFramework.ConversationFlow;
using Xunit;

public sealed class InMemoryConversationStateStoreExtensionsTests
{
    [Fact]
    public async Task TryLoadAndHasState_ExistingAndMissingUsers_ReturnExpectedResults()
    {
        var store = new InMemoryConversationStateStore();
        var state = CreateState(1);
        await store.SaveStateAsync(state);

        (await store.TryLoadStateAsync(1)).Should().BeSameAs(state);
        (await store.TryLoadStateAsync(2)).Should().BeNull();
        (await store.HasStateAsync(1)).Should().BeTrue();
        (await store.HasStateAsync(2)).Should().BeFalse();

        await FluentActions.Invoking(() => store.TryLoadStateAsync(0))
            .Should().ThrowAsync<ArgumentOutOfRangeException>();
        await FluentActions.Invoking(() => store.HasStateAsync(-1))
            .Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task UpdateStateStatus_ExistingAndMissingUsers_UpdatesOrReturnsNull()
    {
        var store = new InMemoryConversationStateStore();
        var state = CreateState(1);
        await store.SaveStateAsync(state);

        var updated = await store.UpdateStateStatusAsync(1, FlowStateStatus.Suspended);

        updated.Should().BeSameAs(state);
        updated!.Status.Should().Be(FlowStateStatus.Suspended);
        (await store.LoadStateAsync(1))!.Status.Should().Be(FlowStateStatus.Suspended);
        (await store.UpdateStateStatusAsync(2, FlowStateStatus.Completed)).Should().BeNull();
    }

    [Fact]
    public async Task GetActiveAndRemoveTerminalStates_UseActiveStateEnumeration()
    {
        var store = new InMemoryConversationStateStore();
        var active = CreateState(1, FlowStateStatus.Active);
        var waiting = CreateState(2, FlowStateStatus.WaitingForInput);
        var completed = CreateState(3, FlowStateStatus.Completed);
        await SaveAsync(store, active, waiting, completed);

        var states = await store.GetActiveStatesAsync();
        var removed = await store.RemoveTerminalStatesAsync();

        states.Should().HaveCount(2).And.Contain(active).And.Contain(waiting);
        removed.Should().Be(0);
        store.GetStateCount().Should().Be(3);
        (await new InMemoryConversationStateStore().GetActiveStatesAsync()).Should().BeEmpty();
    }

    [Fact]
    public async Task TouchStateAndGetStateCount_ExistingAndMissingUsers_ReturnExpectedResults()
    {
        var store = new InMemoryConversationStateStore();
        var before = DateTime.UtcNow.AddMinutes(-5);
        var state = CreateState(1, lastActivityAt: before);
        await store.SaveStateAsync(state);

        (await store.TouchStateAsync(1)).Should().BeTrue();
        state.LastActivityAt.Should().BeAfter(before);
        (await store.TouchStateAsync(2)).Should().BeFalse();
        store.GetStateCount().Should().Be(1);
    }

    [Fact]
    public async Task FindStateById_ActiveMissingAndInvalidIds_ReturnExpectedResults()
    {
        var store = new InMemoryConversationStateStore();
        var active = CreateState(1);
        var terminal = CreateState(2, FlowStateStatus.Completed);
        await SaveAsync(store, active, terminal);

        (await store.FindStateByIdAsync(active.StateId)).Should().BeSameAs(active);
        (await store.FindStateByIdAsync(terminal.StateId)).Should().BeNull();
        (await store.FindStateByIdAsync("missing")).Should().BeNull();
        await FluentActions.Invoking(() => store.FindStateByIdAsync(null!))
            .Should().ThrowAsync<ArgumentNullException>();
        await FluentActions.Invoking(() => store.FindStateByIdAsync(string.Empty))
            .Should().ThrowAsync<ArgumentException>();
        await FluentActions.Invoking(() => store.FindStateByIdAsync(" "))
            .Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task RemoveStaleStates_StrictCutoff_RemovesOnlyOlderActiveStates()
    {
        var cutoff = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var store = new InMemoryConversationStateStore();
        await SaveAsync(
            store,
            CreateState(1, lastActivityAt: cutoff.AddTicks(-1)),
            CreateState(2, lastActivityAt: cutoff),
            CreateState(3, lastActivityAt: cutoff.AddTicks(1)),
            CreateState(4, FlowStateStatus.Failed, cutoff.AddDays(-1)));

        (await store.RemoveStaleStatesAsync(cutoff)).Should().Be(1);
        (await store.LoadStateAsync(1)).Should().BeNull();
        store.GetStateCount().Should().Be(3);
    }

    [Fact]
    public async Task PublicMethods_NullStore_ThrowArgumentNullException()
    {
        InMemoryConversationStateStore store = null!;

        await Assert.ThrowsAsync<ArgumentNullException>(() => store.TryLoadStateAsync(1));
        await Assert.ThrowsAsync<ArgumentNullException>(() => store.HasStateAsync(1));
        await Assert.ThrowsAsync<ArgumentNullException>(() => store.UpdateStateStatusAsync(1, FlowStateStatus.Active));
        await Assert.ThrowsAsync<ArgumentNullException>(() => store.GetActiveStatesAsync());
        await Assert.ThrowsAsync<ArgumentNullException>(() => store.RemoveTerminalStatesAsync());
        await Assert.ThrowsAsync<ArgumentNullException>(() => store.TouchStateAsync(1));
        Assert.Throws<ArgumentNullException>(() => store.GetStateCount());
        await Assert.ThrowsAsync<ArgumentNullException>(() => store.FindStateByIdAsync("id"));
        await Assert.ThrowsAsync<ArgumentNullException>(() => store.RemoveStaleStatesAsync(DateTime.UtcNow));
    }

    private static UserFlowState CreateState(
        long userId,
        FlowStateStatus status = FlowStateStatus.Active,
        DateTime? lastActivityAt = null) => new()
    {
        StateId = $"state-{userId}",
        FlowId = "flow",
        UserId = userId,
        ChatId = userId,
        CurrentStepId = "step",
        Status = status,
        StartedAt = DateTime.UtcNow,
        LastActivityAt = lastActivityAt ?? DateTime.UtcNow
    };

    private static async Task SaveAsync(
        InMemoryConversationStateStore store,
        params UserFlowState[] states)
    {
        foreach (var state in states)
        {
            await store.SaveStateAsync(state);
        }
    }
}
