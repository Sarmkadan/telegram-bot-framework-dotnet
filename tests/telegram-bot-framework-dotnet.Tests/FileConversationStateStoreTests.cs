#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using TelegramBotFramework.ConversationFlow;
using Xunit;

/// <summary>
/// Tests for the FileConversationStateStore class.
/// </summary>
public sealed class FileConversationStateStoreTests : IDisposable, IFileConversationStateStoreTests
{
    private readonly string _tempDirectory;
    private readonly FileConversationStateStore _store;
    private readonly ILogger<FileConversationStateStore> _logger;

    public FileConversationStateStoreTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDirectory);

        // Use a minimal logger for testing
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Warning);
        });
        _logger = loggerFactory.CreateLogger<FileConversationStateStore>();
        _store = new FileConversationStateStore(_tempDirectory, _logger);
    }

    public void Dispose()
    {
        _store.Dispose();
        try
        {
            Directory.Delete(_tempDirectory, true);
        }
        catch
        {
            // Best effort cleanup
        }
    }

    /// <summary>
    /// Tests that SaveStateAsync and LoadStateAsync work correctly in a roundtrip scenario.
    /// </summary>
    [Fact]
    public async Task SaveStateAsync_LoadStateAsync_Roundtrip_ReturnsSameState()
    {
        // Arrange
        var originalState = new UserFlowState
        {
            StateId = Guid.NewGuid().ToString(),
            FlowId = "test_flow",
            UserId = 12345,
            ChatId = 67890,
            CurrentStepId = "step_1",
            Status = FlowStateStatus.Active,
            StartedAt = DateTime.UtcNow,
            LastActivityAt = DateTime.UtcNow,
            Variables = new Dictionary<string, string> { { "test_var", "test_value" } },
            History = new List<FlowStepHistory> { new() { StepId = "step_1", EnteredAt = DateTime.UtcNow } }
        };

        // Act
        await _store.SaveStateAsync(originalState);
        var loadedState = await _store.LoadStateAsync(originalState.UserId);

        // Assert
        loadedState.Should().NotBeNull();
        loadedState!.StateId.Should().Be(originalState.StateId);
        loadedState.FlowId.Should().Be(originalState.FlowId);
        loadedState.UserId.Should().Be(originalState.UserId);
        loadedState.ChatId.Should().Be(originalState.ChatId);
        loadedState.CurrentStepId.Should().Be(originalState.CurrentStepId);
        loadedState.Status.Should().Be(originalState.Status);
        loadedState.StartedAt.Should().BeCloseTo(originalState.StartedAt, TimeSpan.FromSeconds(1));
        loadedState.LastActivityAt.Should().BeCloseTo(originalState.LastActivityAt, TimeSpan.FromSeconds(1));
        loadedState.CompletedAt.Should().Be(originalState.CompletedAt);
        loadedState.Variables.Should().BeEquivalentTo(originalState.Variables);
        loadedState.History.Should().HaveCount(1);
        loadedState.History[0].StepId.Should().Be("step_1");
    }

    /// <summary>
    /// Tests that LoadStateAsync returns null when the state file does not exist.
    /// </summary>
    [Fact]
    public async Task LoadStateAsync_MissingState_ReturnsNull()
    {
        // Arrange
        var nonExistentUserId = 99999L;

        // Act
        var loadedState = await _store.LoadStateAsync(nonExistentUserId);

        // Assert
        loadedState.Should().BeNull();
    }

    /// <summary>
    /// Tests that DeleteStateAsync removes the state file.
    /// </summary>
    [Fact]
    public async Task DeleteStateAsync_RemovesFile()
    {
        // Arrange
        var state = new UserFlowState
        {
            StateId = Guid.NewGuid().ToString(),
            FlowId = "test_flow",
            UserId = 54321,
            ChatId = 54321,
            CurrentStepId = "step_1",
            Status = FlowStateStatus.Active,
            StartedAt = DateTime.UtcNow,
            LastActivityAt = DateTime.UtcNow
        };

        await _store.SaveStateAsync(state);
        var filePath = Path.Combine(_tempDirectory, $"{state.UserId}.json");
        File.Exists(filePath).Should().BeTrue("State file should exist before deletion");

        // Act
        await _store.DeleteStateAsync(state.UserId);

        // Assert
        File.Exists(filePath).Should().BeFalse("State file should be deleted after DeleteStateAsync");

        // Second call should not throw
        await _store.DeleteStateAsync(state.UserId);
    }

    /// <summary>
    /// Tests that LoadStateAsync handles corrupted JSON files gracefully by deleting them and returning null.
    /// </summary>
    [Fact]
    public async Task LoadStateAsync_CorruptedFile_DeletesFileAndReturnsNull()
    {
        // Arrange
        var userId = 11111L;
        var filePath = Path.Combine(_tempDirectory, $"{userId}.json");

        // Write corrupted JSON
        await File.WriteAllTextAsync(filePath, "{ invalid json {{{");
        File.Exists(filePath).Should().BeTrue("Corrupted file should exist before load attempt");

        // Act
        var loadedState = await _store.LoadStateAsync(userId);

        // Assert
        loadedState.Should().BeNull("Corrupted state should return null");
        File.Exists(filePath).Should().BeFalse("Corrupted file should be deleted after load attempt");
    }

    /// <summary>
    /// Tests that LoadStateAsync handles empty files gracefully by deleting them and returning null.
    /// </summary>
    [Fact]
    public async Task LoadStateAsync_EmptyFile_DeletesFileAndReturnsNull()
    {
        // Arrange
        var userId = 22222L;
        var filePath = Path.Combine(_tempDirectory, $"{userId}.json");

        // Write empty file
        await File.WriteAllTextAsync(filePath, string.Empty);
        File.Exists(filePath).Should().BeTrue("Empty file should exist before load attempt");

        // Act
        var loadedState = await _store.LoadStateAsync(userId);

        // Assert
        loadedState.Should().BeNull("Empty state should return null");
        File.Exists(filePath).Should().BeFalse("Empty file should be deleted after load attempt");
    }

    /// <summary>
    /// Tests that LoadStateAsync handles files with invalid UserFlowState structure gracefully by deleting them and returning null.
    /// </summary>
    [Fact]
    public async Task LoadStateAsync_InvalidStructure_DeletesFileAndReturnsNull()
    {
        // Arrange
        var userId = 33333L;
        var filePath = Path.Combine(_tempDirectory, $"{userId}.json");

        // Write JSON with missing required properties
        await File.WriteAllTextAsync(filePath, JsonSerializer.Serialize(new { SomeProperty = "value" }));
        File.Exists(filePath).Should().BeTrue("Invalid structure file should exist before load attempt");

        // Act
        var loadedState = await _store.LoadStateAsync(userId);

        // Assert
        loadedState.Should().BeNull("Invalid structure should return null");
        File.Exists(filePath).Should().BeFalse("Invalid structure file should be deleted after load attempt");
    }

    /// <summary>
    /// Tests that LoadAllActiveStatesAsync returns only active states.
    /// </summary>
    [Fact]
    public async Task LoadAllActiveStatesAsync_ReturnsOnlyActiveStates()
    {
        // Arrange
        var activeState1 = new UserFlowState
        {
            StateId = Guid.NewGuid().ToString(),
            FlowId = "test_flow",
            UserId = 10001,
            ChatId = 10001,
            CurrentStepId = "step_1",
            Status = FlowStateStatus.Active,
            StartedAt = DateTime.UtcNow,
            LastActivityAt = DateTime.UtcNow
        };

        var activeState2 = new UserFlowState
        {
            StateId = Guid.NewGuid().ToString(),
            FlowId = "test_flow",
            UserId = 10002,
            ChatId = 10002,
            CurrentStepId = "step_1",
            Status = FlowStateStatus.WaitingForInput,
            StartedAt = DateTime.UtcNow,
            LastActivityAt = DateTime.UtcNow
        };

        var completedState = new UserFlowState
        {
            StateId = Guid.NewGuid().ToString(),
            FlowId = "test_flow",
            UserId = 10003,
            ChatId = 10003,
            CurrentStepId = "step_1",
            Status = FlowStateStatus.Completed,
            StartedAt = DateTime.UtcNow,
            LastActivityAt = DateTime.UtcNow,
            CompletedAt = DateTime.UtcNow
        };

        var suspendedState = new UserFlowState
        {
            StateId = Guid.NewGuid().ToString(),
            FlowId = "test_flow",
            UserId = 10004,
            ChatId = 10004,
            CurrentStepId = "step_1",
            Status = FlowStateStatus.Suspended,
            StartedAt = DateTime.UtcNow,
            LastActivityAt = DateTime.UtcNow
        };

        await _store.SaveStateAsync(activeState1);
        await _store.SaveStateAsync(activeState2);
        await _store.SaveStateAsync(completedState);
        await _store.SaveStateAsync(suspendedState);

        // Act
        var allStates = await _store.LoadAllActiveStatesAsync();

        // Assert
        allStates.Should().HaveCount(2, "Should return only Active and WaitingForInput states");
        allStates.Should().ContainSingle(s => s.UserId == 10001 && s.Status == FlowStateStatus.Active);
        allStates.Should().ContainSingle(s => s.UserId == 10002 && s.Status == FlowStateStatus.WaitingForInput);
        allStates.Should().NotContain(s => s.UserId == 10003, "Completed state should not be returned");
        allStates.Should().NotContain(s => s.UserId == 10004, "Suspended state should not be returned");
    }

    /// <summary>
    /// Tests that LoadAllActiveStatesAsync returns an empty list when no state files exist.
    /// </summary>
    [Fact]
    public async Task LoadAllActiveStatesAsync_NoFiles_ReturnsEmptyList()
    {
        // Act
        var allStates = await _store.LoadAllActiveStatesAsync();

        // Assert
        allStates.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that SaveStateAsync throws ArgumentNullException when state is null.
    /// </summary>
    [Fact]
    public async Task SaveStateAsync_NullState_ThrowsArgumentNullException()
    {
        // Act
        Func<Task> act = async () => await _store.SaveStateAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    /// <summary>
    /// Tests that constructor throws ArgumentException when directory is null or whitespace.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_InvalidDirectory_ThrowsArgumentException(string? invalidDirectory)
    {
        // Act
        Action act = () => new FileConversationStateStore(invalidDirectory!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Tests that GetFilePath returns correct path for user ID.
    /// </summary>
    [Fact]
    public void GetFilePath_ReturnsCorrectPath()
    {
        // Arrange
        var userId = 123456789L;
        var expectedPath = Path.Combine(_tempDirectory, $"{userId}.json");

        // Act
        var actualPath = _store.GetFilePath(userId);

        // Assert
        actualPath.Should().Be(expectedPath);
    }

    /// <summary>
    /// Tests that Dispose can be called multiple times without error.
    /// </summary>
    [Fact]
    public void Dispose_MultipleTimes_DoesNotThrow()
    {
        // Act
        _store.Dispose();
        Action act = () => _store.Dispose();

        // Assert
        act.Should().NotThrow();
    }
}