#nullable enable

using FluentAssertions;
using Moq;
using TelegramBotFramework.ConversationFlow;
using Xunit;

public sealed class ConversationFlowEngineExtensionsTests
{
    [Theory]
    [InlineData(FlowStateStatus.Active, true)]
    [InlineData(FlowStateStatus.WaitingForInput, true)]
    [InlineData(FlowStateStatus.Completed, false)]
    public async Task HasActiveFlowAsync_MatchingFlow_ReturnsResultForStatus(
        FlowStateStatus status,
        bool expected)
    {
        var engine = CreateEngine(CreateState(status: status));

        var result = await engine.Object.HasActiveFlowAsync(long.MaxValue, "checkout");

        result.Should().Be(expected);
    }

    [Fact]
    public async Task HasActiveFlowAsync_MissingOrDifferentFlow_ReturnsFalse()
    {
        var missingEngine = CreateEngine(null);
        var differentEngine = CreateEngine(CreateState(flowId: "registration"));

        (await missingEngine.Object.HasActiveFlowAsync(0, "checkout")).Should().BeFalse();
        (await differentEngine.Object.HasActiveFlowAsync(0, "checkout")).Should().BeFalse();
    }

    [Fact]
    public async Task GetCurrentStepIdAsync_StateAndNoState_ReturnStepIdAndNull()
    {
        var activeEngine = CreateEngine(CreateState(currentStepId: "confirm"));
        var missingEngine = CreateEngine(null);

        (await activeEngine.Object.GetCurrentStepIdAsync(long.MinValue)).Should().Be("confirm");
        (await missingEngine.Object.GetCurrentStepIdAsync(long.MinValue)).Should().BeNull();
    }

    [Fact]
    public async Task GetVariableAsync_PresentMissingAndEmptyVariables_ReturnExpectedValues()
    {
        var populatedEngine = CreateEngine(CreateState(variables: new() { ["name"] = "Ada" }));
        var emptyEngine = CreateEngine(CreateState(variables: new()));

        (await populatedEngine.Object.GetVariableAsync(42, "name")).Should().Be("Ada");
        (await populatedEngine.Object.GetVariableAsync(42, "missing")).Should().BeNull();
        (await emptyEngine.Object.GetVariableAsync(42, "name")).Should().BeNull();
    }

    [Fact]
    public async Task GetActiveFlowAsync_ActiveState_ReturnsAssociatedDefinition()
    {
        var state = CreateState();
        var flow = new FlowDefinition
        {
            FlowId = "checkout",
            Name = "Checkout",
            InitialStepId = "start",
            Steps = []
        };
        var engine = CreateEngine(state);
        engine.Setup(x => x.GetFlowAsync("checkout", It.IsAny<CancellationToken>()))
            .ReturnsAsync(flow);

        var result = await engine.Object.GetActiveFlowAsync(7);

        result.Should().BeSameAs(flow);
    }

    [Fact]
    public async Task GetActiveFlowAsync_NoActiveState_ReturnsNullWithoutFlowLookup()
    {
        var engine = CreateEngine(null);

        (await engine.Object.GetActiveFlowAsync(7)).Should().BeNull();
        engine.Verify(
            x => x.GetFlowAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task PublicMethods_InvalidArguments_ThrowExpectedExceptions()
    {
        IConversationFlowEngine nullEngine = null!;
        var engine = CreateEngine(null);

        await Assert.ThrowsAsync<ArgumentNullException>(() => nullEngine.HasActiveFlowAsync(1, "flow"));
        await Assert.ThrowsAsync<ArgumentNullException>(() => nullEngine.GetCurrentStepIdAsync(1));
        await Assert.ThrowsAsync<ArgumentNullException>(() => nullEngine.GetVariableAsync(1, "name"));
        await Assert.ThrowsAsync<ArgumentNullException>(() => nullEngine.GetActiveFlowAsync(1));
        await Assert.ThrowsAsync<ArgumentNullException>(() => engine.Object.HasActiveFlowAsync(1, null!));
        await Assert.ThrowsAsync<ArgumentException>(() => engine.Object.HasActiveFlowAsync(1, " "));
        await Assert.ThrowsAsync<ArgumentNullException>(() => engine.Object.GetVariableAsync(1, null!));
        await Assert.ThrowsAsync<ArgumentException>(() => engine.Object.GetVariableAsync(1, string.Empty));
    }

    private static Mock<IConversationFlowEngine> CreateEngine(UserFlowState? state)
    {
        var engine = new Mock<IConversationFlowEngine>();
        engine.Setup(x => x.GetActiveFlowStateAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(state);
        return engine;
    }

    private static UserFlowState CreateState(
        string flowId = "checkout",
        string currentStepId = "start",
        FlowStateStatus status = FlowStateStatus.Active,
        Dictionary<string, string>? variables = null) => new()
    {
        StateId = "state-1",
        FlowId = flowId,
        UserId = 1,
        ChatId = 1,
        CurrentStepId = currentStepId,
        Status = status,
        StartedAt = DateTime.UtcNow,
        LastActivityAt = DateTime.UtcNow,
        Variables = variables ?? new()
    };
}
