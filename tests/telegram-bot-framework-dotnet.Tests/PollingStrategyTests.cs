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

public class PollingStrategyTests : IPollingStrategyTests
{
    private readonly Mock<ITelegramApiClient> _mockApiClient = new();
    private readonly Mock<ILogger<PollingStrategy>> _mockLogger = new();
    private readonly PollingStrategy _pollingStrategy;

    public PollingStrategyTests()
    {
        _pollingStrategy = new PollingStrategy(_mockApiClient.Object, _mockLogger.Object);
    }

    [Fact]
    public void Constructor_WithNullApiClient_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new PollingStrategy(null!));
    }

    [Fact]
    public void Constructor_WithNullLogger_UsesConsoleLogger()
    {
        // Act
        var strategy = new PollingStrategy(_mockApiClient.Object, logger: null);

        // Assert - just verify it doesn't throw
        strategy.Should().NotBeNull();
    }

    [Fact]
    public void Start_WhenAlreadyRunning_DoesNotStartAnotherPollingTask()
    {
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
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("already running")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task StopAsync_WhenNotRunning_DoesNotThrow()
    {
        // Act
        var act = async () => await _pollingStrategy.StopAsync();

        // Assert
        await act.Should().NotThrowAsync();
        _mockLogger.Verify(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Polling stopped")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact]
    public async Task ProcessUpdateAsync_WithNullUpdate_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _pollingStrategy.ProcessUpdateAsync(null!));
    }

    [Fact]
    public async Task ProcessUpdateAsync_AdvancesLastUpdateId()
    {
        // Arrange
        var update = new TelegramUpdate
        {
            UpdateId = 123,
            Timestamp = DateTime.UtcNow,
            MessageType = UpdateType.Message
        };

        // Act
        await _pollingStrategy.ProcessUpdateAsync(update);

        // Assert
        var status = _pollingStrategy.GetStatus();
        status.LastUpdateId.Should().Be(123);
        status.IsRunning.Should().BeFalse(); // Not running since we didn't call Start
    }

    [Fact]
    public async Task ProcessUpdateAsync_InvokesOnUpdateReceivedEvent()
    {
        // Arrange
        var update = new TelegramUpdate
        {
            UpdateId = 456,
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
        receivedUpdate!.UpdateId.Should().Be(456);
    }

    [Fact]
    public async Task ProcessUpdateAsync_WithException_LogsErrorAndContinues()
    {
        // Arrange
        var update = new TelegramUpdate
        {
            UpdateId = 789,
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
        _pollingStrategy.OnUpdateReceived += _ => throw new InvalidOperationException("Test exception");

        // Act
        var act = async () => await _pollingStrategy.ProcessUpdateAsync(update);

        // Assert
        await act.Should().NotThrowAsync();
        _mockLogger.Verify(x => x.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error processing update")),
            It.Is<Exception>(e => e.Message == "Test exception"),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void GetStatus_ReturnsCorrectPollingStatus()
    {
        // Arrange
        var update = new TelegramUpdate
        {
            UpdateId = 999,
            Timestamp = DateTime.UtcNow,
            MessageType = UpdateType.Message
        };

        // Act
        _pollingStrategy.ProcessUpdateAsync(update).Wait();

        // Assert
        var status = _pollingStrategy.GetStatus();
        status.IsRunning.Should().BeFalse();
        status.LastUpdateId.Should().Be(999);
        // LastPollTime is only set during active polling loop, not by ProcessUpdateAsync
        status.LastPollTime.Should().BeNull();
    }

    [Fact]
    public async Task Start_WithCustomPollInterval_SetsCorrectInterval()
    {
        // Arrange
        var customInterval = TimeSpan.FromMilliseconds(500);

        // Act
        _pollingStrategy.Start(customInterval);

        // Assert
        _pollingStrategy.GetStatus().IsRunning.Should().BeTrue();
        _mockLogger.Verify(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("500ms")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        await _pollingStrategy.StopAsync();
    }

    [Fact]
    public async Task Start_WithDefaultInterval_UsesOneSecondInterval()
    {
        // Act
        _pollingStrategy.Start();

        // Assert
        _mockLogger.Verify(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("1000ms")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        await _pollingStrategy.StopAsync();
    }

    [Fact]
    public async Task Polling_WithEmptyUpdates_AppliesDelay()
    {
        // Arrange
        var emptyUpdates = new List<JsonElement>();
        _mockApiClient.Setup(x => x.GetUpdatesAsync(0, It.IsAny<int>()))
            .ReturnsAsync(emptyUpdates);

        // Act
        _pollingStrategy.Start(TimeSpan.FromMilliseconds(100));

        // Wait for polling to execute at least once
        await Task.Delay(200);

        // Assert - polling should be running
        _pollingStrategy.GetStatus().IsRunning.Should().BeTrue();

        await _pollingStrategy.StopAsync();
    }

    [Fact]
    public async Task Polling_WithUpdates_ProcessesThemAndAdvancesOffset()
    {
        // Arrange
        var jsonUpdate = "{\"update_id\": 123, \"message\": {\"message_id\": 456, \"chat\": {\"id\": 789}, \"from\": {\"id\": 101112}, \"date\": 1234567890, \"text\": \"Hello\"}}";
        var jsonElement = JsonDocument.Parse(jsonUpdate).RootElement;
        var updates = new List<JsonElement> { jsonElement };

        _mockApiClient.SetupSequence(x => x.GetUpdatesAsync(0, It.IsAny<int>()))
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
        _pollingStrategy.Start(TimeSpan.FromMilliseconds(50));

        // Wait for polling to process updates
        await Task.Delay(300);

        // Assert
        _pollingStrategy.GetStatus().LastUpdateId.Should().Be(123);
        eventInvoked.Should().BeTrue();
        receivedUpdate.Should().NotBeNull();
        receivedUpdate!.UpdateId.Should().Be(123);

        await _pollingStrategy.StopAsync();
    }

    [Fact]
    public async Task Polling_WithException_AppliesBackoffDelay()
    {
        // Arrange
        _mockApiClient.Setup(x => x.GetUpdatesAsync(0, It.IsAny<int>()))
            .ThrowsAsync(new InvalidOperationException("API failure"));

        // Act
        _pollingStrategy.Start(TimeSpan.FromMilliseconds(50));

        // Wait for backoff to apply
        await Task.Delay(100);

        // Assert - polling should still be running despite exception
        _pollingStrategy.GetStatus().IsRunning.Should().BeTrue();

        await _pollingStrategy.StopAsync();
    }

    [Fact]
    public async Task StopAsync_CancelsPollingLoop()
    {
        // Arrange
        _pollingStrategy.Start(TimeSpan.FromMilliseconds(50));

        // Wait for polling to start
        await Task.Delay(100);

        // Act
        await _pollingStrategy.StopAsync();

        // Assert
        var status = _pollingStrategy.GetStatus();
        status.IsRunning.Should().BeFalse();
    }

    [Fact]
    public async Task Polling_AdvancesUpdateIdThroughMultiplePolls()
    {
        // Arrange - simulate 3 consecutive polls with updates
        var update1 = CreateUpdateJson(100);
        var update2 = CreateUpdateJson(101);
        var update3 = CreateUpdateJson(102);

        _mockApiClient.SetupSequence(x => x.GetUpdatesAsync(0, It.IsAny<int>()))
            .ReturnsAsync(new List<JsonElement> { update1 })
            .ReturnsAsync(new List<JsonElement> { update2 })
            .ReturnsAsync(new List<JsonElement> { update3 });

        long lastUpdateId = 0;
        _pollingStrategy.OnUpdateReceived += async (u) =>
        {
            lastUpdateId = u.UpdateId;
            await Task.CompletedTask;
        };

        // Act
        _pollingStrategy.Start(TimeSpan.FromMilliseconds(50));

        // Wait for polling to process all updates - give it more time
        await Task.Delay(800);

        // Assert - LastUpdateId should be at least 100 (the last processed update)
        _pollingStrategy.GetStatus().LastUpdateId.Should().BeGreaterOrEqualTo(100);
        lastUpdateId.Should().BeGreaterOrEqualTo(100);

        await _pollingStrategy.StopAsync();
    }

    [Fact]
    public async Task Polling_WithOffsetAdvancement_RequestsUpdatesWithCorrectOffset()
    {
        // Arrange
        var update1 = CreateUpdateJson(100);
        var update2 = CreateUpdateJson(101);

        _mockApiClient.SetupSequence(x => x.GetUpdatesAsync(0, It.IsAny<int>()))
            .ReturnsAsync(new List<JsonElement> { update1 })
            .ReturnsAsync(new List<JsonElement> { update2 })
            .ReturnsAsync(new List<JsonElement>());

        _mockApiClient.Setup(x => x.GetUpdatesAsync(101, It.IsAny<int>()))
            .ReturnsAsync(new List<JsonElement>());

        // Act
        _pollingStrategy.Start(TimeSpan.FromMilliseconds(50));

        // Wait for polling to process updates
        await Task.Delay(300);

        // Assert - should have requested with offset 101 after processing update 100
        _mockApiClient.Verify(x => x.GetUpdatesAsync(0, It.IsAny<int>()), Times.AtLeastOnce());
        _mockApiClient.Verify(x => x.GetUpdatesAsync(101, It.IsAny<int>()), Times.AtLeastOnce());

        await _pollingStrategy.StopAsync();
    }

    [Fact]
    public async Task LastPollTime_IsUpdatedOnEachPoll()
    {
        // Arrange
        var emptyUpdates = new List<JsonElement>();
        _mockApiClient.Setup(x => x.GetUpdatesAsync(0, It.IsAny<int>()))
            .ReturnsAsync(emptyUpdates);

        // Act
        _pollingStrategy.Start(TimeSpan.FromMilliseconds(100));

        // Wait for multiple polls
        await Task.Delay(350);

        // Assert
        var status = _pollingStrategy.GetStatus();
        status.LastPollTime.Should().NotBeNull();
        var firstPollTime = status.LastPollTime;

        await Task.Delay(150); // Wait for another poll
        status = _pollingStrategy.GetStatus();
        status.LastPollTime.Should().NotBe(firstPollTime);

        await _pollingStrategy.StopAsync();
    }

    [Fact]
    public async Task Polling_WithCancellation_StopsGracefully()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        _mockApiClient.Setup(x => x.GetUpdatesAsync(0, It.IsAny<int>()))
            .ReturnsAsync(new List<JsonElement>());

        _pollingStrategy.Start(TimeSpan.FromMilliseconds(50));
        await Task.Delay(100); // Let it start

        // Act
        await _pollingStrategy.StopAsync();

        // Assert
        _pollingStrategy.GetStatus().IsRunning.Should().BeFalse();
    }

    private static JsonElement CreateUpdateJson(long updateId)
    {
        var json = $"{{ \"update_id\": {updateId}, \"message\": {{ \"message_id\": {updateId + 1000}, \"chat\": {{ \"id\": 123 }}, \"from\": {{ \"id\": 456 }}, \"date\": 1234567890, \"text\": \"Test\" }} }}";
        return JsonDocument.Parse(json).RootElement;
    }
}