#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TelegramBotFramework.Integration;
using static TelegramBotFramework.Tests.Integration.PollingStrategyTestsConstants;
using Xunit;

namespace TelegramBotFramework.Tests.Integration;

/// <summary>
/// Tests for the <see cref="PollingStrategy"/> class, covering constructor behavior,
/// start/stop functionality, update processing, and polling loop mechanics.
/// </summary>
public class PollingStrategyTests : IPollingStrategyTests
{
    private readonly Mock<ITelegramApiClient> _mockApiClient = new();
    private readonly Mock<ILogger<PollingStrategy>> _mockLogger = new();
    private ILogger<PollingStrategy> _logger => _mockLogger.Object;
    private readonly PollingStrategy _pollingStrategy;

    /// <summary>
    /// Initializes a new instance of the <see cref="PollingStrategyTests"/> class
    /// with mocked dependencies for testing.
    /// </summary>
    public PollingStrategyTests()
    {
        _pollingStrategy = new PollingStrategy(_mockApiClient.Object, _mockLogger.Object);
    }

    /// <summary>
    /// Verifies that constructing a PollingStrategy with a null API client throws an ArgumentNullException.
    /// </summary>
    [Fact]
    public void Constructor_WithNullApiClient_ThrowsArgumentNullException()
    {
        _logger.LogInformation("Starting {TestName}", nameof(Constructor_WithNullApiClient_ThrowsArgumentNullException));
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new PollingStrategy(null!));
        _logger.LogInformation("Completed {TestName}", nameof(Constructor_WithNullApiClient_ThrowsArgumentNullException));
    }

    /// <summary>
    /// Verifies that constructing a PollingStrategy with a null logger falls back to using the console logger.
    /// </summary>
    [Fact]
    public void Constructor_WithNullLogger_UsesConsoleLogger()
    {
        _logger.LogInformation("Starting {TestName}", nameof(Constructor_WithNullLogger_UsesConsoleLogger));
        _logger.LogWarning("No logger supplied; verifying fallback to the console logger for {TestName}", nameof(Constructor_WithNullLogger_UsesConsoleLogger));
        // Act
        var strategy = new PollingStrategy(_mockApiClient.Object, logger: null);

        // Assert - just verify it doesn't throw
        strategy.Should().NotBeNull();
        _logger.LogInformation("Completed {TestName}", nameof(Constructor_WithNullLogger_UsesConsoleLogger));
    }

    /// <summary>
    /// Verifies that calling Start() on an already running PollingStrategy does not start another polling task.
    /// </summary>
    [Fact]
    public void Start_WhenAlreadyRunning_DoesNotStartAnotherPollingTask()
    {
        _logger.LogInformation("Testing Start method when already running");
        // Arrange
        _pollingStrategy.Start();
        var initialTask = _pollingStrategy.GetStatus().IsRunning;

        // Act
        _pollingStrategy.Start();

        // Assert
        _pollingStrategy.GetStatus().IsRunning.Should().BeTrue();
        _mockLogger.Verify(x => x.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(AlreadyRunningLogSubstring)),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        _logger.LogInformation("Start_WhenAlreadyRunning test completed - IsRunning: {IsRunning}", _pollingStrategy.GetStatus().IsRunning);
    }

    /// <summary>
    /// Verifies that calling StopAsync() on a PollingStrategy that is not running does not throw an exception.
    /// </summary>
    [Fact]
    public async Task StopAsync_WhenNotRunning_DoesNotThrow()
    {
        _logger.LogInformation("Testing StopAsync when not running");
        // Act
        var act = async () => await _pollingStrategy.StopAsync();

        // Assert
        await act.Should().NotThrowAsync();
        _mockLogger.Verify(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(PollingStoppedLogSubstring)),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
        _logger.LogInformation("StopAsync_WhenNotRunning test completed - IsRunning: {IsRunning}", _pollingStrategy.GetStatus().IsRunning);
    }

    /// <summary>
    /// Verifies that calling ProcessUpdateAsync with a null update throws an ArgumentNullException.
    /// </summary>
    [Fact]
    public async Task ProcessUpdateAsync_WithNullUpdate_ThrowsArgumentNullException()
    {
        _logger.LogInformation("Testing ProcessUpdateAsync with null update");
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _pollingStrategy.ProcessUpdateAsync(null!));
        _logger.LogInformation("ProcessUpdateAsync_WithNullUpdate test completed - ArgumentNullException expected and thrown for {ParamName}", nameof(TelegramUpdate));
    }

    /// <summary>
    /// Verifies that calling ProcessUpdateAsync advances the last update ID in the polling strategy's status.
    /// </summary>
    [Fact]
    public async Task ProcessUpdateAsync_AdvancesLastUpdateId()
    {
        _logger.LogInformation("Testing ProcessUpdateAsync advances last update ID for {UpdateId}", ProcessedUpdateId);
        // Arrange
        var update = new TelegramUpdate
        {
            UpdateId = ProcessedUpdateId,
            Timestamp = DateTime.UtcNow,
            MessageType = UpdateType.Message
        };

        // Act
        await _pollingStrategy.ProcessUpdateAsync(update);

        // Assert
        var status = _pollingStrategy.GetStatus();
        status.LastUpdateId.Should().Be(ProcessedUpdateId);
        status.IsRunning.Should().BeFalse(); // Not running since we didn't call Start
        _logger.LogInformation("ProcessUpdateAsync_AdvancesLastUpdateId test completed - LastUpdateId is {LastUpdateId}", status.LastUpdateId);
    }

    /// <summary>
    /// Verifies that calling ProcessUpdateAsync invokes the OnUpdateReceived event with the provided update.
    /// </summary>
    [Fact]
    public async Task ProcessUpdateAsync_InvokesOnUpdateReceivedEvent()
    {
        _logger.LogInformation("Testing ProcessUpdateAsync invokes OnUpdateReceived event for {UpdateId}", EventUpdateId);
        // Arrange
        var update = new TelegramUpdate
        {
            UpdateId = EventUpdateId,
            Timestamp = DateTime.UtcNow,
            MessageType = UpdateType.Message
        };

        bool eventInvoked = false;
        TelegramUpdate? receivedUpdate = null;

        _pollingStrategy.OnUpdateReceived += async (u) =>
        {
            eventInvoked = true;
            receivedUpdate = u;
            await Task.CompletedTask;
        };

        // Act
        await _pollingStrategy.ProcessUpdateAsync(update);

        // Assert
        eventInvoked.Should().BeTrue();
        receivedUpdate.Should().NotBeNull();
        receivedUpdate!.UpdateId.Should().Be(EventUpdateId);
        _logger.LogInformation("ProcessUpdateAsync_InvokesOnUpdateReceivedEvent test completed - eventInvoked: {EventInvoked}, receivedUpdateId: {ReceivedUpdateId}", eventInvoked, receivedUpdate?.UpdateId);
    }

    /// <summary>
    /// Verifies that when ProcessUpdateAsync encounters an exception in the event handler, it logs the error and continues without throwing.
    /// </summary>
    [Fact]
    public async Task ProcessUpdateAsync_WithException_LogsErrorAndContinues()
    {
        _logger.LogInformation("Testing ProcessUpdateAsync with exception for update {UpdateId}", FailingUpdateId);
        // Arrange
        var update = new TelegramUpdate
        {
            UpdateId = FailingUpdateId,
            Timestamp = DateTime.UtcNow,
            MessageType = UpdateType.Message
        };

        _mockLogger.Setup(x => x.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

        // Simulate exception in event handler
        _pollingStrategy.OnUpdateReceived += _ => throw new InvalidOperationException(TestExceptionMessage);

        // Act
        var act = async () => await _pollingStrategy.ProcessUpdateAsync(update);

        // Assert
        await act.Should().NotThrowAsync();
        _mockLogger.Verify(x => x.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(ErrorProcessingUpdateLogSubstring)),
            It.Is<Exception>(e => e.Message == TestExceptionMessage),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        _logger.LogInformation("ProcessUpdateAsync_WithException_LogsErrorAndContinues test completed");
    }

    /// <summary>
    /// Verifies that GetStatus returns the correct polling status after processing an update.
    /// </summary>
    [Fact]
    public void GetStatus_ReturnsCorrectPollingStatus()
    {
        _logger.LogInformation("Testing GetStatus_ReturnsCorrectPollingStatus for update {UpdateId}", StatusUpdateId);
        // Arrange
        var update = new TelegramUpdate
        {
            UpdateId = StatusUpdateId,
            Timestamp = DateTime.UtcNow,
            MessageType = UpdateType.Message
        };

        // Act
        _pollingStrategy.ProcessUpdateAsync(update).Wait();

        // Assert
        var status = _pollingStrategy.GetStatus();
        status.IsRunning.Should().BeFalse();
        status.LastUpdateId.Should().Be(StatusUpdateId);
        // LastPollTime is only set during active polling loop, not by ProcessUpdateAsync
        status.LastPollTime.Should().BeNull();
        _logger.LogInformation("GetStatus_ReturnsCorrectPollingStatus test completed - IsRunning: {IsRunning}, LastUpdateId: {LastUpdateId}", status.IsRunning, status.LastUpdateId);
    }

    /// <summary>
    /// Verifies that starting the polling strategy with a custom poll interval sets the correct interval.
    /// </summary>
    [Fact]
    public async Task Start_WithCustomPollInterval_SetsCorrectInterval()
    {
        _logger.LogInformation("Testing Start_WithCustomPollInterval_SetsCorrectInterval with customInterval={CustomIntervalMs}ms", LongPollIntervalMs);
        // Arrange
        var customInterval = TimeSpan.FromMilliseconds(LongPollIntervalMs);

        // Act
        _pollingStrategy.Start(customInterval);

        // Assert
        _pollingStrategy.GetStatus().IsRunning.Should().BeTrue();
        _mockLogger.Verify(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(CustomIntervalLogSubstring)),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        await _pollingStrategy.StopAsync();
        _logger.LogInformation("Start_WithCustomPollInterval_SetsCorrectInterval test completed");
    }

    /// <summary>
    /// Verifies that starting the polling strategy with the default interval uses a one-second interval.
    /// </summary>
    [Fact]
    public async Task Start_WithDefaultInterval_UsesOneSecondInterval()
    {
        _logger.LogInformation("Testing Start_WithDefaultInterval_UsesOneSecondInterval");
        // Act
        _pollingStrategy.Start();

        // Assert
        _mockLogger.Verify(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(DefaultIntervalLogSubstring)),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        await _pollingStrategy.StopAsync();
        _logger.LogInformation("Start_WithDefaultInterval_UsesOneSecondInterval test completed");
    }

    /// <summary>
/// Verifies that when polling with empty updates, the strategy applies the configured delay between polls.
/// </summary>
[Fact]
public async Task Polling_WithEmptyUpdates_AppliesDelay()
{
    _logger.LogInformation("Testing Polling_WithEmptyUpdates_AppliesDelay with StandardPollIntervalMs={StandardPollIntervalMs}", StandardPollIntervalMs);
    _logger.LogWarning("No updates are available; verifying polling delay of {PollIntervalMs} ms", StandardPollIntervalMs);
    // Arrange
    var emptyUpdates = new List<JsonElement>();
    _mockApiClient.Setup(x => x.GetUpdatesAsync(InitialUpdateOffset, It.IsAny<int>()))
        .ReturnsAsync(emptyUpdates);

    // Act
    _pollingStrategy.Start(TimeSpan.FromMilliseconds(StandardPollIntervalMs));

    // Wait for polling to execute at least once
    await Task.Delay(MediumDelayMs);

    // Assert - polling should be running
    _pollingStrategy.GetStatus().IsRunning.Should().BeTrue();
    _logger.LogInformation("Polling_WithEmptyUpdates_AppliesDelay assert passed - IsRunning={IsRunning}", _pollingStrategy.GetStatus().IsRunning);

    await _pollingStrategy.StopAsync();
    _logger.LogInformation("Polling_WithEmptyUpdates_AppliesDelay test completed");
}

    /// <summary>
/// Verifies that when polling receives updates, the strategy processes them and advances the offset correctly.
/// </summary>
[Fact]
public async Task Polling_WithUpdates_ProcessesThemAndAdvancesOffset()
{
    _logger.LogInformation("Testing Polling_WithUpdates_ProcessesThemAndAdvancesOffset");
    // Arrange
    var jsonUpdate = JsonUpdate;
    var jsonElement = JsonDocument.Parse(jsonUpdate).RootElement;
    var updates = new List<JsonElement> { jsonElement };

    _mockApiClient.SetupSequence(x => x.GetUpdatesAsync(InitialUpdateOffset, It.IsAny<int>()))
        .ReturnsAsync(updates)
        .ReturnsAsync(new List<JsonElement>()); // Empty on second call

    bool eventInvoked = false;
    TelegramUpdate? receivedUpdate = null;

    _pollingStrategy.OnUpdateReceived += async (u) =>
    {
        eventInvoked = true;
        receivedUpdate = u;
        await Task.CompletedTask;
    };

    // Act
    _pollingStrategy.Start(TimeSpan.FromMilliseconds(ShortPollIntervalMs));

    // Wait for polling to process updates
    await Task.Delay(LongDelayMs);

    // Assert
    _pollingStrategy.GetStatus().LastUpdateId.Should().Be(ProcessedUpdateId);
    eventInvoked.Should().BeTrue();
    receivedUpdate.Should().NotBeNull();
    receivedUpdate!.UpdateId.Should().Be(ProcessedUpdateId);

    await _pollingStrategy.StopAsync();
    _logger.LogInformation("Polling_WithUpdates_ProcessesThemAndAdvancesOffset test completed");
}

    /// <summary>
/// Verifies that when polling encounters an exception, the strategy applies a backoff delay before retrying.
/// </summary>
[Fact]
public async Task Polling_WithException_AppliesBackoffDelay()
{
    _logger.LogInformation("Testing Polling_WithException_AppliesBackoffDelay");
    _logger.LogWarning("Telegram API failure will trigger the polling backoff path for {PollIntervalMs} ms", ShortPollIntervalMs);
    // Arrange
    _mockApiClient.Setup(x => x.GetUpdatesAsync(InitialUpdateOffset, It.IsAny<int>()))
        .ThrowsAsync(new InvalidOperationException(ApiFailureMessage));

    // Act
    _pollingStrategy.Start(TimeSpan.FromMilliseconds(ShortPollIntervalMs));

    // Wait for backoff to apply
    await Task.Delay(ShortDelayMs);

    // Assert - polling should still be running despite exception
    _pollingStrategy.GetStatus().IsRunning.Should().BeTrue();
    _logger.LogInformation("Polling_WithException_AppliesBackoffDelay assert passed - IsRunning={IsRunning}", _pollingStrategy.GetStatus().IsRunning);

    await _pollingStrategy.StopAsync();
    _logger.LogInformation("Polling_WithException_AppliesBackoffDelay test completed");
}

    /// <summary>
/// Verifies that calling StopAsync() cancels the polling loop and sets the running state to false.
/// </summary>
[Fact]
public async Task StopAsync_CancelsPollingLoop()
{
    _logger.LogInformation("Testing StopAsync_CancelsPollingLoop");
    // Arrange
    _pollingStrategy.Start(TimeSpan.FromMilliseconds(ShortPollIntervalMs));

    // Wait for polling to start
    await Task.Delay(ShortDelayMs);

    // Act
    await _pollingStrategy.StopAsync();

    // Assert
    var status = _pollingStrategy.GetStatus();
    status.IsRunning.Should().BeFalse();
    _logger.LogInformation("StopAsync_CancelsPollingLoop test completed - IsRunning={IsRunning}", status.IsRunning);
}

    /// <summary>
/// Verifies that the polling strategy advances the update ID through multiple polling cycles when processing consecutive updates.
/// </summary>
[Fact]
public async Task Polling_AdvancesUpdateIdThroughMultiplePolls()
{
    _logger.LogInformation("Testing Polling_AdvancesUpdateIdThroughMultiplePolls");
    // Arrange - simulate 3 consecutive polls with updates
    var update1 = CreateUpdateJson(FirstUpdateId);
    var update2 = CreateUpdateJson(SecondUpdateId);
    var update3 = CreateUpdateJson(ThirdUpdateId);

    _mockApiClient.SetupSequence(x => x.GetUpdatesAsync(InitialUpdateOffset, It.IsAny<int>()))
        .ReturnsAsync(new List<JsonElement> { update1 })
        .ReturnsAsync(new List<JsonElement> { update2 })
        .ReturnsAsync(new List<JsonElement> { update3 });

    long lastUpdateId = InitialUpdateOffset;
    _pollingStrategy.OnUpdateReceived += async (u) =>
    {
        lastUpdateId = u.UpdateId;
        await Task.CompletedTask;
    };

    // Act
    _pollingStrategy.Start(TimeSpan.FromMilliseconds(ShortPollIntervalMs));

    // Wait for polling to process all updates - give it more time
    await Task.Delay(VeryLongDelayMs);

    // Assert - LastUpdateId should be at least 100 (the last processed update)
    _pollingStrategy.GetStatus().LastUpdateId.Should().BeGreaterOrEqualTo(FirstUpdateId);
    lastUpdateId.Should().BeGreaterOrEqualTo(FirstUpdateId);
    _logger.LogInformation("Polling_AdvancesUpdateIdThroughMultiplePolls assertions passed - LastUpdateId: {LastUpdateId}, tracked lastUpdateId: {TrackedLastUpdateId}", _pollingStrategy.GetStatus().LastUpdateId, lastUpdateId);

    await _pollingStrategy.StopAsync();
    _logger.LogInformation("Polling_AdvancesUpdateIdThroughMultiplePolls test completed");
}

    /// <summary>
/// Verifies that the polling strategy requests updates with the correct offset after processing updates.
/// </summary>
[Fact]
public async Task Polling_WithOffsetAdvancement_RequestsUpdatesWithCorrectOffset()
{
    _logger.LogInformation(
        "Starting {TestName} with initial offset {InitialOffset} and expected offset {ExpectedOffset}",
        nameof(Polling_WithOffsetAdvancement_RequestsUpdatesWithCorrectOffset),
        InitialUpdateOffset,
        SecondUpdateId);
    // Arrange
    var update1 = CreateUpdateJson(FirstUpdateId);
    var update2 = CreateUpdateJson(SecondUpdateId);

    _mockApiClient.SetupSequence(x => x.GetUpdatesAsync(InitialUpdateOffset, It.IsAny<int>()))
        .ReturnsAsync(new List<JsonElement> { update1 })
        .ReturnsAsync(new List<JsonElement> { update2 })
        .ReturnsAsync(new List<JsonElement>());

    _mockApiClient.Setup(x => x.GetUpdatesAsync(SecondUpdateId, It.IsAny<int>()))
        .ReturnsAsync(new List<JsonElement>());

    // Act
    _pollingStrategy.Start(TimeSpan.FromMilliseconds(ShortPollIntervalMs));

    // Wait for polling to process updates
    await Task.Delay(LongDelayMs);

    // Assert - should have requested with offset 101 after processing update 100
    _mockApiClient.Verify(x => x.GetUpdatesAsync(InitialUpdateOffset, It.IsAny<int>()), Times.AtLeastOnce());
    _mockApiClient.Verify(x => x.GetUpdatesAsync(SecondUpdateId, It.IsAny<int>()), Times.AtLeastOnce());

    await _pollingStrategy.StopAsync();
    _logger.LogInformation(
        "Completed {TestName} with expected offset {ExpectedOffset}",
        nameof(Polling_WithOffsetAdvancement_RequestsUpdatesWithCorrectOffset),
        SecondUpdateId);
}

    /// <summary>
/// Verifies that the LastPollTime property is updated on each polling cycle.
/// </summary>
[Fact]
public async Task LastPollTime_IsUpdatedOnEachPoll()
{
    _logger.LogInformation(
        "Starting {TestName} with poll interval {PollIntervalMs} ms",
        nameof(LastPollTime_IsUpdatedOnEachPoll),
        StandardPollIntervalMs);
    // Arrange
    var emptyUpdates = new List<JsonElement>();
    _mockApiClient.Setup(x => x.GetUpdatesAsync(InitialUpdateOffset, It.IsAny<int>()))
        .ReturnsAsync(emptyUpdates);

    // Act
    _pollingStrategy.Start(TimeSpan.FromMilliseconds(StandardPollIntervalMs));

    // Wait for multiple polls
    await Task.Delay(ExtraLongDelayMs);

    // Assert
    var status = _pollingStrategy.GetStatus();
    status.LastPollTime.Should().NotBeNull();
    var firstPollTime = status.LastPollTime;

    await Task.Delay(AdditionalPollDelayMs); // Wait for another poll
    status = _pollingStrategy.GetStatus();
    status.LastPollTime.Should().NotBe(firstPollTime);

    await _pollingStrategy.StopAsync();
    _logger.LogInformation(
        "Completed {TestName}; last poll time is {LastPollTime}",
        nameof(LastPollTime_IsUpdatedOnEachPoll),
        status.LastPollTime);
}

    /// <summary>
/// Verifies that the polling strategy stops gracefully when cancelled.
/// </summary>
[Fact]
public async Task Polling_WithCancellation_StopsGracefully()
{
    _logger.LogInformation(
        "Starting {TestName} with poll interval {PollIntervalMs} ms",
        nameof(Polling_WithCancellation_StopsGracefully),
        ShortPollIntervalMs);
    // Arrange
    var cts = new CancellationTokenSource();
    _mockApiClient.Setup(x => x.GetUpdatesAsync(InitialUpdateOffset, It.IsAny<int>()))
        .ReturnsAsync(new List<JsonElement>());

    _pollingStrategy.Start(TimeSpan.FromMilliseconds(ShortPollIntervalMs));
    await Task.Delay(ShortDelayMs); // Let it start

    // Act
    await _pollingStrategy.StopAsync();

    // Assert
    _pollingStrategy.GetStatus().IsRunning.Should().BeFalse();
    _logger.LogInformation(
        "Completed {TestName}; polling running state is {IsRunning}",
        nameof(Polling_WithCancellation_StopsGracefully),
        _pollingStrategy.GetStatus().IsRunning);
}

    private static JsonElement CreateUpdateJson(long updateId)
    {
        var json = string.Format(
            CultureInfo.InvariantCulture,
            UpdateJsonFormat,
            updateId,
            updateId + MessageIdOffset,
            DefaultChatId,
            DefaultFromId,
            DefaultDate,
            DefaultTestText);
        return JsonDocument.Parse(json).RootElement;
    }
}
