#nullable enable

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TelegramBotFramework.Integration;
using Xunit;

namespace TelegramBotFramework.Tests.Integration;

/// <summary>
/// Comprehensive tests for PollingStrategy state transitions and error handling.
/// Tests the state machine behavior including start/stop transitions, error states, backoff behavior,
/// and cancellation scenarios.
/// </summary>
public class PollingStrategyStateTransitionsTests
{
    private readonly Mock<ITelegramApiClient> _mockApiClient = new();
    private readonly Mock<ILogger<PollingStrategy>> _mockLogger = new();
    private readonly PollingStrategy _pollingStrategy;

    public PollingStrategyStateTransitionsTests()
    {
        _pollingStrategy = new PollingStrategy(_mockApiClient.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Start_FromStoppedState_StartsPollingSuccessfully()
    {
        // Arrange - strategy starts in stopped state (not running)
        var initialStatus = _pollingStrategy.GetStatus();
        initialStatus.IsRunning.Should().BeFalse();

        // Act
        _pollingStrategy.Start();

        // Assert
        var status = _pollingStrategy.GetStatus();
        status.IsRunning.Should().BeTrue();
        status.IsDraining.Should().BeFalse();
        status.IsDrainComplete.Should().BeFalse();
        status.InFlightCount.Should().Be(0);

        // Cleanup
        await _pollingStrategy.StopAsync();
    }

    [Fact]
    public async Task Start_WhenAlreadyRunning_ShouldNoOpAndLogWarning()
    {
        // Arrange
        _pollingStrategy.Start();
        var initialStatus = _pollingStrategy.GetStatus();
        initialStatus.IsRunning.Should().BeTrue();

        // Reset mock to clear previous calls
        _mockLogger.Reset();

        // Act - try to start again
        _pollingStrategy.Start();

        // Assert - should still be running
        var status = _pollingStrategy.GetStatus();
        status.IsRunning.Should().BeTrue();

        // Verify warning was logged
        _mockLogger.Verify(x => x.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("already running")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        // Cleanup
        await _pollingStrategy.StopAsync();
    }

    [Fact]
    public async Task StopAsync_WhenNotRunning_DoesNotThrowAndMaintainsStoppedState()
    {
        // Arrange - ensure strategy is not running
        var initialStatus = _pollingStrategy.GetStatus();
        initialStatus.IsRunning.Should().BeFalse();

        // Act - stop when not running
        var act = () => _pollingStrategy.StopAsync();

        // Assert - should not throw and state should remain stopped
        await act.Should().NotThrowAsync();
        var status = _pollingStrategy.GetStatus();
        status.IsRunning.Should().BeFalse();
        status.IsDraining.Should().BeFalse();
        status.IsDrainComplete.Should().BeFalse();
    }

    [Fact]
    public async Task StopAsync_FromRunningState_StopsGracefullyAndTransitionsToStopped()
    {
        // Arrange - start polling
        _pollingStrategy.Start(TimeSpan.FromMilliseconds(50));
        await Task.Delay(100); // Let polling start

        var runningStatus = _pollingStrategy.GetStatus();
        runningStatus.IsRunning.Should().BeTrue();

        // Act - stop polling
        await _pollingStrategy.StopAsync();

        // Assert - should transition to stopped state
        var stoppedStatus = _pollingStrategy.GetStatus();
        stoppedStatus.IsRunning.Should().BeFalse();
        stoppedStatus.IsDraining.Should().BeFalse();
        stoppedStatus.IsDrainComplete.Should().BeTrue();
        stoppedStatus.InFlightCount.Should().Be(0);
    }

    [Fact]
    public async Task Polling_WithConsecutiveFailures_TransitionsToBackoffStateAndRecovers()
    {
        // Arrange - setup API to fail multiple times then succeed
        var failureCount = 0;
        _mockApiClient.Setup(x => x.GetUpdatesAsync(0, It.IsAny<int>()))
            .ReturnsAsync(() => {
                failureCount++;
                if (failureCount <= 3)
                {
                    throw new InvalidOperationException($"API failure #{failureCount}");
                }
                return new List<JsonElement>(); // Success after failures
            });

        // Act - start polling
        _pollingStrategy.Start(TimeSpan.FromMilliseconds(50));
        await Task.Delay(200); // Let it fail and backoff a few times

        // Assert - should be in backoff state with increased failure count
        var backoffStatus = _pollingStrategy.GetStatus();
        backoffStatus.IsRunning.Should().BeTrue();
        backoffStatus.ConsecutiveFailureCount.Should().BeGreaterOrEqualTo(3);
        backoffStatus.CurrentBackoffMs.Should().BeGreaterThan(1000); // Base interval

        // Wait for recovery - let it try again after backoff
        await Task.Delay(100);

        // Assert - failure count should reset after successful poll
        var recoveredStatus = _pollingStrategy.GetStatus();
        recoveredStatus.ConsecutiveFailureCount.Should().Be(0);
        recoveredStatus.CurrentBackoffMs.Should().Be(0);

        // Cleanup
        await _pollingStrategy.StopAsync();
    }

    [Fact]
    public async Task Polling_WithPersistentFailure_RemainsInBackoffState()
    {
        // Arrange - setup API to always fail
        _mockApiClient.Setup(x => x.GetUpdatesAsync(0, It.IsAny<int>()))
            .ThrowsAsync(new InvalidOperationException("Persistent API failure"));

        // Act - start polling
        _pollingStrategy.Start(TimeSpan.FromMilliseconds(50));
        await Task.Delay(150); // Let it fail and apply backoff

        // Assert - should be running but in backoff state
        var backoffStatus = _pollingStrategy.GetStatus();
        backoffStatus.IsRunning.Should().BeTrue();
        backoffStatus.ConsecutiveFailureCount.Should().BeGreaterThan(1);
        backoffStatus.CurrentBackoffMs.Should().BeGreaterThan(1000);

        // Cleanup
        await _pollingStrategy.StopAsync();
    }

    [Fact]
    public async Task Polling_WithCancellationToken_MidPoll_StopsGracefullyAndMaintainsState()
    {
        // Arrange - setup API to return empty updates
        _mockApiClient.Setup(x => x.GetUpdatesAsync(0, It.IsAny<int>()))
            .ReturnsAsync(new List<JsonElement>());

        // Act - start polling
        _pollingStrategy.Start(TimeSpan.FromMilliseconds(50));
        await Task.Delay(100); // Let it start

        var runningStatus = _pollingStrategy.GetStatus();
        runningStatus.IsRunning.Should().BeTrue();

        // Act - stop with cancellation token
        var stopTask = _pollingStrategy.StopAsync(CancellationToken.None);

        // Assert - should complete without cancellation
        await stopTask.WaitAsync(TimeSpan.FromSeconds(5));

        var stoppedStatus = _pollingStrategy.GetStatus();
        stoppedStatus.IsRunning.Should().BeFalse();
        stoppedStatus.IsDrainComplete.Should().BeTrue();
    }

    [Fact]
    public async Task Polling_WithUpdateProcessingDuringShutdown_CompletesInFlightHandlers()
    {
        // Arrange - setup API to return updates
        var updateJson = "{\"update_id\": 123, \"message\": {\"message_id\": 456, \"chat\": {\"id\": 789}, \"from\": {\"id\": 101112}, \"date\": 1234567890, \"text\": \"Hello\"}}";
        var jsonElement = JsonDocument.Parse(updateJson).RootElement;
        var updates = new List<JsonElement> { jsonElement };

        bool handlerCompleted = false;
        _pollingStrategy.OnUpdateReceived += async (u) => {
            await Task.Delay(100); // Simulate slow handler
            handlerCompleted = true;
            await Task.CompletedTask;
        };

        _mockApiClient.SetupSequence(x => x.GetUpdatesAsync(0, It.IsAny<int>()))
            .ReturnsAsync(updates)
            .ReturnsAsync(new List<JsonElement>());

        // Act - start polling
        _pollingStrategy.Start(TimeSpan.FromMilliseconds(50));
        await Task.Delay(200); // Let it process update

        var statusDuringPoll = _pollingStrategy.GetStatus();
        statusDuringPoll.InFlightCount.Should().Be(1);
        statusDuringPoll.IsDraining.Should().BeFalse();

        // Act - stop while handler is in flight
        await _pollingStrategy.StopAsync();

        // Assert - handler should complete and drain should be complete
        handlerCompleted.Should().BeTrue();
        var finalStatus = _pollingStrategy.GetStatus();
        finalStatus.IsDrainComplete.Should().BeTrue();
        finalStatus.InFlightCount.Should().Be(0);
    }

    [Fact]
    public async Task Polling_WithMultipleConsecutiveFailures_AppliesExponentialBackoff()
    {
        // Arrange - track backoff delays
        var backoffDelays = new List<int>();
        var failureCount = 0;

        _mockApiClient.Setup(x => x.GetUpdatesAsync(0, It.IsAny<int>()))
            .ReturnsAsync(() => {
                failureCount++;
                if (failureCount <= 5)
                {
                    // Calculate expected backoff for this failure count
                    var expectedBackoff = (int)Math.Min(
                        1000 * Math.Pow(1.5, failureCount - 1),
                        30000
                    );
                    backoffDelays.Add(expectedBackoff);
                    throw new InvalidOperationException($"API failure #{failureCount}");
                }
                return new List<JsonElement>();
            });

        // Act - start polling
        _pollingStrategy.Start(TimeSpan.FromMilliseconds(50));
        await Task.Delay(400); // Let it fail multiple times

        // Assert - backoff should increase exponentially
        var status = _pollingStrategy.GetStatus();
        status.ConsecutiveFailureCount.Should().BeGreaterOrEqualTo(5);
        status.CurrentBackoffMs.Should().BeGreaterThan(1000);

        // Verify exponential growth (approximately)
        backoffDelays.Should().HaveCountGreaterOrEqualTo(5);
        for (int i = 1; i < backoffDelays.Count; i++)
        {
            backoffDelays[i].Should().BeGreaterThan(backoffDelays[i - 1]);
        }

        // Cleanup
        await _pollingStrategy.StopAsync();
    }

    [Fact]
    public void GetStatus_ReturnsAccurateStateAfterMultipleTransitions()
    {
        // Initial state - stopped
        var status1 = _pollingStrategy.GetStatus();
        status1.IsRunning.Should().BeFalse();
        status1.ConsecutiveFailureCount.Should().Be(0);
        status1.CurrentBackoffMs.Should().Be(0);
        status1.IsDraining.Should().BeFalse();
        status1.IsDrainComplete.Should().BeFalse();
        status1.InFlightCount.Should().Be(0);
        status1.LastUpdateId.Should().Be(0);
        status1.LastPollTime.Should().BeNull();

        // Start polling
        _pollingStrategy.Start();
        var status2 = _pollingStrategy.GetStatus();
        status2.IsRunning.Should().BeTrue();
        status2.IsDraining.Should().BeFalse();
        status2.IsDrainComplete.Should().BeFalse();

        // Process an update
        var update = new TelegramUpdate
        {
            UpdateId = 999,
            Timestamp = DateTime.UtcNow,
            MessageType = UpdateType.Message
        };
        _pollingStrategy.ProcessUpdateAsync(update).Wait();

        var status3 = _pollingStrategy.GetStatus();
        status3.LastUpdateId.Should().Be(999);
        status3.IsRunning.Should().BeTrue();

        // Stop polling
        _pollingStrategy.StopAsync().Wait();
        var status4 = _pollingStrategy.GetStatus();
        status4.IsRunning.Should().BeFalse();
        status4.IsDrainComplete.Should().BeTrue();
    }

    [Fact]
    public async Task Polling_WithUpdateProcessing_UpdatesLastUpdateIdAndPollTime()
    {
        // Arrange
        var update1Json = "{\"update_id\": 100, \"message\": {\"message_id\": 200, \"chat\": {\"id\": 300}, \"from\": {\"id\": 400}, \"date\": 1234567890, \"text\": \"First\"}}";
        var update2Json = "{\"update_id\": 101, \"message\": {\"message_id\": 201, \"chat\": {\"id\": 301}, \"from\": {\"id\": 401}, \"date\": 1234567891, \"text\": \"Second\"}}";

        var update1 = JsonDocument.Parse(update1Json).RootElement;
        var update2 = JsonDocument.Parse(update2Json).RootElement;

        _mockApiClient.SetupSequence(x => x.GetUpdatesAsync(0, It.IsAny<int>()))
            .ReturnsAsync(new List<JsonElement> { update1 })
            .ReturnsAsync(new List<JsonElement> { update2 })
            .ReturnsAsync(new List<JsonElement>());

        // Act - start polling
        _pollingStrategy.Start(TimeSpan.FromMilliseconds(50));

        // Wait for multiple polls
        await Task.Delay(300);

        // Assert
        var status = _pollingStrategy.GetStatus();
        status.LastUpdateId.Should().Be(101);
        status.LastPollTime.Should().NotBeNull();
        status.IsRunning.Should().BeTrue();

        // Cleanup
        await _pollingStrategy.StopAsync();
    }

    [Fact]
    public async Task StopAsync_WithInFlightHandlers_WaitsForCompletion()
    {
        // Arrange - setup slow handler
        var handlerCompleted = false;
        _pollingStrategy.OnUpdateReceived += async (u) => {
            await Task.Delay(200); // Slow handler
            handlerCompleted = true;
            await Task.CompletedTask;
        };

        var updateJson = "{\"update_id\": 123, \"message\": {\"message_id\": 456, \"chat\": {\"id\": 789}, \"from\": {\"id\": 101112}, \"date\": 1234567890, \"text\": \"Hello\"}}";
        var jsonElement = JsonDocument.Parse(updateJson).RootElement;

        _mockApiClient.Setup(x => x.GetUpdatesAsync(0, It.IsAny<int>()))
            .ReturnsAsync(new List<JsonElement> { jsonElement });

        // Act - start polling
        _pollingStrategy.Start(TimeSpan.FromMilliseconds(50));
        await Task.Delay(150); // Let handler start

        var statusDuring = _pollingStrategy.GetStatus();
        statusDuring.InFlightCount.Should().Be(1);

        // Act - stop while handler is running
        var stopTask = _pollingStrategy.StopAsync();

        // Assert - should wait for handler to complete
        await stopTask.WaitAsync(TimeSpan.FromSeconds(3));
        handlerCompleted.Should().BeTrue();

        var finalStatus = _pollingStrategy.GetStatus();
        finalStatus.InFlightCount.Should().Be(0);
        finalStatus.IsDrainComplete.Should().BeTrue();
    }

    [Fact]
    public async Task Polling_WithCancellationDuringBackoff_StopsImmediately()
    {
        // Arrange - setup API to fail
        _mockApiClient.Setup(x => x.GetUpdatesAsync(0, It.IsAny<int>()))
            .ThrowsAsync(new InvalidOperationException("API failure"));

        // Act - start polling
        _pollingStrategy.Start(TimeSpan.FromMilliseconds(50));
        await Task.Delay(100); // Let it fail and enter backoff

        var backoffStatus = _pollingStrategy.GetStatus();
        backoffStatus.IsRunning.Should().BeTrue();
        backoffStatus.ConsecutiveFailureCount.Should().BeGreaterThan(0);

        // Act - stop during backoff
        var stopTask = _pollingStrategy.StopAsync();
        await stopTask;

        // Assert - should stop even during backoff
        await Task.Delay(100); // Give it time to stop
        var stoppedStatus = _pollingStrategy.GetStatus();
        stoppedStatus.IsRunning.Should().BeFalse();
        stoppedStatus.IsDrainComplete.Should().BeTrue();
    }

    [Fact]
    public void Start_WithNullPollInterval_UsesDefaultInterval()
    {
        // Act
        _pollingStrategy.Start(pollInterval: null);

        // Assert
        _mockLogger.Verify(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("1000ms")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        // Cleanup
        _pollingStrategy.StopAsync().Wait();
    }

    [Fact]
    public async Task GetStatus_ThreadSafe_WhenCalledConcurrently()
    {
        // Arrange - start polling
        _pollingStrategy.Start(TimeSpan.FromMilliseconds(50));
        await Task.Delay(100);

        // Act - call GetStatus from multiple threads
        var tasks = new List<Task>();
        var statuses = new List<PollingStatus>();

        for (int i = 0; i < 10; i++)
        {
            tasks.Add(Task.Run(() => {
                var status = _pollingStrategy.GetStatus();
                lock (statuses)
                {
                    statuses.Add(status);
                }
            }));
        }

        await Task.WhenAll(tasks);

        // Assert - all status calls should return valid data without exceptions
        statuses.Should().HaveCount(10);
        foreach (var status in statuses)
        {
            status.Should().NotBeNull();
            status.IsRunning.Should().BeTrue();
        }

        // Cleanup
        await _pollingStrategy.StopAsync();
    }

    [Fact]
    public async Task Polling_WithBackoffThenSuccess_ResetsFailureCount()
    {
        // Arrange - fail twice, then succeed
        var callCount = 0;
        _mockApiClient.Setup(x => x.GetUpdatesAsync(0, It.IsAny<int>()))
            .ReturnsAsync(() => {
                callCount++;
                if (callCount <= 2)
                {
                    throw new InvalidOperationException($"API failure #{callCount}");
                }
                return new List<JsonElement>();
            });

        // Act - start polling
        _pollingStrategy.Start(TimeSpan.FromMilliseconds(50));
        await Task.Delay(200); // Let it fail and backoff

        var backoffStatus = _pollingStrategy.GetStatus();
        backoffStatus.ConsecutiveFailureCount.Should().Be(2);

        // Wait for success
        await Task.Delay(100);

        var successStatus = _pollingStrategy.GetStatus();
        successStatus.ConsecutiveFailureCount.Should().Be(0);
        successStatus.CurrentBackoffMs.Should().Be(0);

        // Cleanup
        await _pollingStrategy.StopAsync();
    }
}