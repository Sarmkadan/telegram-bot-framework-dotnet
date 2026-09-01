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
        _logger.LogInformation("Starting roundtrip test for user {UserId}", 12345);
        try
        {
            // Arrange
            var originalState = new UserFlowState
            {
                StateId = Guid.NewGuid().ToString(),
                FlowId = FileConversationStateStoreTestsConstants.TestFlowId,
                UserId = 12345,
                ChatId = 67890,
                CurrentStepId = FileConversationStateStoreTestsConstants.TestStepId,
                Status = FlowStateStatus.Active,
                StartedAt = DateTime.UtcNow,
                LastActivityAt = DateTime.UtcNow,
                Variables = new Dictionary<string, string> { { FileConversationStateStoreTestsConstants.TestVariableName, FileConversationStateStoreTestsConstants.TestVariableValue } },
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
            loadedState.StartedAt.Should().BeCloseTo(originalState.StartedAt, FileConversationStateStoreTestsConstants.AssertionTimeout);
            loadedState.LastActivityAt.Should().BeCloseTo(originalState.LastActivityAt, FileConversationStateStoreTestsConstants.AssertionTimeout);
            loadedState.CompletedAt.Should().Be(originalState.CompletedAt);
            loadedState.Variables.Should().BeEquivalentTo(originalState.Variables);
            loadedState.History.Should().HaveCount(1);
            loadedState.History[0].StepId.Should().Be(FileConversationStateStoreTestsConstants.TestStepId);

            _logger.LogInformation("Completed roundtrip test for user {UserId} successfully", 12345);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during roundtrip test for user {UserId}", 12345);
            throw;
        }
    }

    /// <summary>
    /// Tests that LoadStateAsync returns null when the state file does not exist.
    /// </summary>
    [Fact]
    public async Task LoadStateAsync_MissingState_ReturnsNull()
    {
        _logger.LogInformation("Testing LoadStateAsync for non-existent user {UserId}", 99999L);
        try
        {
            // Arrange
            var nonExistentUserId = 99999L;

            // Act
            var loadedState = await _store.LoadStateAsync(nonExistentUserId);

            // Assert
            loadedState.Should().BeNull();

            _logger.LogInformation("LoadStateAsync correctly returned null for non-existent user {UserId}", 99999L);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during LoadStateAsync test for non-existent user {UserId}", 99999L);
            throw;
        }
    }

    /// <summary>
    /// Tests that DeleteStateAsync removes the state file.
    /// </summary>
    [Fact]
    public async Task DeleteStateAsync_RemovesFile()
    {
        _logger.LogInformation("Testing DeleteStateAsync for user {UserId}", 54321);
        try
        {
            // Arrange
            var state = new UserFlowState
            {
                StateId = Guid.NewGuid().ToString(),
                FlowId = FileConversationStateStoreTestsConstants.TestFlowId,
                UserId = 54321,
                ChatId = 54321,
                CurrentStepId = FileConversationStateStoreTestsConstants.TestStepId,
                Status = FlowStateStatus.Active,
                StartedAt = DateTime.UtcNow,
                LastActivityAt = DateTime.UtcNow
            };

            await _store.SaveStateAsync(state);
            var filePath = Path.Combine(_tempDirectory, $"{state.UserId}{FileConversationStateStoreTestsConstants.JsonFileExtension}");
            File.Exists(filePath).Should().BeTrue("State file should exist before deletion");

            // Act
            await _store.DeleteStateAsync(state.UserId);

            // Assert
            File.Exists(filePath).Should().BeFalse("State file should be deleted after DeleteStateAsync");

            // Second call should not throw
            await _store.DeleteStateAsync(state.UserId);

            _logger.LogInformation("DeleteStateAsync test completed successfully for user {UserId}", 54321);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during DeleteStateAsync test for user {UserId}", 54321);
            throw;
        }
    }

    /// <summary>
    /// Tests that LoadStateAsync handles corrupted JSON files gracefully by deleting them and returning null.
    /// </summary>
    [Fact]
    public async Task LoadStateAsync_CorruptedFile_DeletesFileAndReturnsNull()
    {
        _logger.LogInformation("Testing LoadStateAsync with corrupted file for user {UserId}", 11111L);
        try
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

            _logger.LogInformation("LoadStateAsync correctly handled corrupted file for user {UserId}", 11111L);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during LoadStateAsync corrupted file test for user {UserId}", 11111L);
            throw;
        }
    }

    /// <summary>
    /// Tests that LoadStateAsync handles empty files gracefully by deleting them and returning null.
    /// </summary>
    [Fact]
    public async Task LoadStateAsync_EmptyFile_DeletesFileAndReturnsNull()
    {
        _logger.LogInformation("Testing LoadStateAsync with empty file for user {UserId}", 22222L);
        try
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

            _logger.LogInformation("LoadStateAsync correctly handled empty file for user {UserId}", 22222L);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during LoadStateAsync empty file test for user {UserId}", 22222L);
            throw;
        }
    }

    /// <summary>
    /// Tests that LoadStateAsync handles files with invalid UserFlowState structure gracefully by deleting them and returning null.
    /// </summary>
    [Fact]
    public async Task LoadStateAsync_InvalidStructure_DeletesFileAndReturnsNull()
    {
        _logger.LogInformation("Testing LoadStateAsync with invalid structure file for user {UserId}", 33333L);
        try
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

            _logger.LogInformation("LoadStateAsync correctly handled invalid structure file for user {UserId}", 33333L);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during LoadStateAsync invalid structure test for user {UserId}", 33333L);
            throw;
        }
    }

    /// <summary>
    /// Tests that LoadAllActiveStatesAsync returns only active states.
    /// </summary>
    [Fact]
    public async Task LoadAllActiveStatesAsync_ReturnsOnlyActiveStates()
    {
        _logger.LogInformation("Testing LoadAllActiveStatesAsync with mixed state statuses");
        try
        {
            // Arrange
            var activeState1 = new UserFlowState
            {
                StateId = Guid.NewGuid().ToString(),
                FlowId = FileConversationStateStoreTestsConstants.TestFlowId,
                UserId = 10001,
                ChatId = 10001,
                CurrentStepId = FileConversationStateStoreTestsConstants.TestStepId,
                Status = FlowStateStatus.Active,
                StartedAt = DateTime.UtcNow,
                LastActivityAt = DateTime.UtcNow
            };

            var activeState2 = new UserFlowState
            {
                StateId = Guid.NewGuid().ToString(),
                FlowId = FileConversationStateStoreTestsConstants.TestFlowId,
                UserId = 10002,
                ChatId = 10002,
                CurrentStepId = FileConversationStateStoreTestsConstants.TestStepId,
                Status = FlowStateStatus.WaitingForInput,
                StartedAt = DateTime.UtcNow,
                LastActivityAt = DateTime.UtcNow
            };

            var completedState = new UserFlowState
            {
                StateId = Guid.NewGuid().ToString(),
                FlowId = FileConversationStateStoreTestsConstants.TestFlowId,
                UserId = 10003,
                ChatId = 10003,
                CurrentStepId = FileConversationStateStoreTestsConstants.TestStepId,
                Status = FlowStateStatus.Completed,
                StartedAt = DateTime.UtcNow,
                LastActivityAt = DateTime.UtcNow,
                CompletedAt = DateTime.UtcNow
            };

            var suspendedState = new UserFlowState
            {
                StateId = Guid.NewGuid().ToString(),
                FlowId = FileConversationStateStoreTestsConstants.TestFlowId,
                UserId = 10004,
                ChatId = 10004,
                CurrentStepId = FileConversationStateStoreTestsConstants.TestStepId,
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

            _logger.LogInformation("LoadAllActiveStatesAsync test completed successfully. Found {Count} active states", allStates.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during LoadAllActiveStatesAsync test");
            throw;
        }
    }

    /// <summary>
    /// Tests that LoadAllActiveStatesAsync returns an empty list when no state files exist.
    /// </summary>
    [Fact]
    public async Task LoadAllActiveStatesAsync_NoFiles_ReturnsEmptyList()
    {
        _logger.LogInformation("Testing LoadAllActiveStatesAsync with no state files");
        try
        {
            // Act
            var allStates = await _store.LoadAllActiveStatesAsync();

            // Assert
            allStates.Should().BeEmpty();

            _logger.LogInformation("LoadAllActiveStatesAsync correctly returned empty list when no files exist");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during LoadAllActiveStatesAsync test with no files");
            throw;
        }
    }

    /// <summary>
    /// Tests that SaveStateAsync throws ArgumentNullException when state is null.
    /// </summary>
    [Fact]
    public async Task SaveStateAsync_NullState_ThrowsArgumentNullException()
    {
        _logger.LogInformation("Testing SaveStateAsync with null state");
        try
        {
            // Act
            Func<Task> act = async () => await _store.SaveStateAsync(null!);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>();

            _logger.LogInformation("SaveStateAsync correctly threw ArgumentNullException for null state");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during SaveStateAsync null state test");
            throw;
        }
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
        _logger.LogInformation("Testing constructor with invalid directory: '{Directory}'", invalidDirectory ?? "null");
        try
        {
            // Act
            Action act = () => new FileConversationStateStore(invalidDirectory!);

            // Assert
            act.Should().Throw<ArgumentException>();

            _logger.LogInformation("Constructor correctly threw ArgumentException for invalid directory: '{Directory}'", invalidDirectory ?? "null");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during constructor test with invalid directory: '{Directory}'", invalidDirectory ?? "null");
            throw;
        }
    }

    /// <summary>
    /// Tests that GetFilePath returns correct path for user ID.
    /// </summary>
    [Fact]
    public void GetFilePath_ReturnsCorrectPath()
    {
        _logger.LogInformation("Testing GetFilePath for user {UserId}", 123456789L);
        try
        {
            // Arrange
            var userId = 123456789L;
            var expectedPath = Path.Combine(_tempDirectory, $"{userId}{FileConversationStateStoreTestsConstants.JsonFileExtension}");

            // Act
            var actualPath = _store.GetFilePath(userId);

            // Assert
            actualPath.Should().Be(expectedPath);

            _logger.LogInformation("GetFilePath returned expected path: {Path}", actualPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during GetFilePath test for user {UserId}", 123456789L);
            throw;
        }
    }

    /// <summary>
    /// Tests that Dispose can be called multiple times without error.
    /// </summary>
    [Fact]
    public void Dispose_MultipleTimes_DoesNotThrow()
    {
        _logger.LogInformation("Testing Dispose multiple times");
        try
        {
            // Act
            _store.Dispose();
            Action act = () => _store.Dispose();

            // Assert
            act.Should().NotThrow();

            _logger.LogInformation("Dispose multiple times test completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during Dispose multiple times test");
            throw;
        }
    }

    /// <summary>
    /// Tests that constructor throws ArgumentException when directory is null or whitespace (async overload).
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Constructor_InvalidDirectory_ThrowsArgumentExceptionAsync(string? invalidDirectory)
    {
        _logger.LogInformation("Testing constructor with invalid directory (async): '{Directory}'", invalidDirectory ?? "null");
        try
        {
            // Act
            Func<Task> act = async () => { var store = new FileConversationStateStore(invalidDirectory!); store.Dispose(); };

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();

            _logger.LogInformation("Constructor correctly threw ArgumentException for invalid directory (async): '{Directory}'", invalidDirectory ?? "null");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during constructor test (async) with invalid directory: '{Directory}'", invalidDirectory ?? "null");
            throw;
        }
    }

    /// <summary>
    /// Tests that Dispose can be called multiple times without error (async overload).
    /// </summary>
    [Fact]
    public async Task Dispose_MultipleTimes_DoesNotThrowAsync()
    {
        _logger.LogInformation("Testing Dispose multiple times (async)");
        try
        {
            // Act
            _store.Dispose();
            Func<Task> act = async () => { _store.Dispose(); };

            // Assert
            await act.Should().NotThrowAsync();

            _logger.LogInformation("Dispose multiple times (async) test completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during Dispose multiple times (async) test");
            throw;
        }
    }
}