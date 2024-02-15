#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace TelegramBotFramework.ConversationFlow;

/// <summary>
/// File-system-backed implementation of <see cref="IConversationStateStore"/>.
/// Each user's active <see cref="UserFlowState"/> is serialised to a JSON file named
/// <c>{userId}.json</c> inside a configurable directory.
/// State survives process restarts and can be shared between instances that mount
/// the same directory (e.g., a shared NFS volume or container bind-mount).
/// </summary>
/// <remarks>
/// This implementation is suited for low-to-medium traffic bots running on a single host.
/// For high-concurrency or multi-node deployments consider a database-backed store.
/// </remarks>
public sealed class FileConversationStateStore : IConversationStateStore, IDisposable
{
    private readonly string _directory;
    private readonly ILogger<FileConversationStateStore> _logger;
    private readonly SemaphoreSlim _fileLock = new(1, 1);
    private bool _disposed;

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented          = true,
        PropertyNameCaseInsensitive = true,
        Converters             = { new JsonStringEnumConverter() }
    };

    /// <summary>
    /// Initialises a new instance of <see cref="FileConversationStateStore"/>.
    /// </summary>
    /// <param name="directory">
    /// Path to the directory where state files are written.
    /// The directory is created automatically if it does not exist.
    /// </param>
    /// <param name="logger">Optional logger; a no-op logger is used when omitted.</param>
    public FileConversationStateStore(
        string directory,
        ILogger<FileConversationStateStore>? logger = null)
    {
        if (string.IsNullOrWhiteSpace(directory))
            throw new ArgumentException("Directory path cannot be empty.", nameof(directory));

        _directory = directory;
        _logger    = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<FileConversationStateStore>.Instance;

        Directory.CreateDirectory(_directory);
    }

    /// <inheritdoc/>
    public async Task SaveStateAsync(UserFlowState state, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(state);

        var path = GetFilePath(state.UserId);
        await _fileLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var json = JsonSerializer.Serialize(state, SerializerOptions);
            await File.WriteAllTextAsync(path, json, cancellationToken).ConfigureAwait(false);
            _logger.LogDebug("Persisted flow state for UserId {UserId} to {Path}", state.UserId, path);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to persist flow state for UserId {UserId}", state.UserId);
            throw;
        }
        finally
        {
            _fileLock.Release();
        }
    }

    /// <inheritdoc/>
    public async Task<UserFlowState?> LoadStateAsync(long userId, CancellationToken cancellationToken = default)
    {
        var path = GetFilePath(userId);
        if (!File.Exists(path))
            return null;

        await _fileLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var json  = await File.ReadAllTextAsync(path, cancellationToken).ConfigureAwait(false);
            var state = JsonSerializer.Deserialize<UserFlowState>(json, SerializerOptions);
            _logger.LogDebug("Loaded flow state for UserId {UserId} from {Path}", userId, path);
            return state;
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Corrupted state file for UserId {UserId} at {Path} — deleting", userId, path);
            TryDeleteFile(path);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load flow state for UserId {UserId}", userId);
            return null;
        }
        finally
        {
            _fileLock.Release();
        }
    }

    /// <inheritdoc/>
    public async Task DeleteStateAsync(long userId, CancellationToken cancellationToken = default)
    {
        var path = GetFilePath(userId);
        if (!File.Exists(path))
            return;

        await _fileLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            File.Delete(path);
            _logger.LogDebug("Deleted flow state file for UserId {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete flow state for UserId {UserId}", userId);
        }
        finally
        {
            _fileLock.Release();
        }
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<UserFlowState>> LoadAllActiveStatesAsync(CancellationToken cancellationToken = default)
    {
        var files  = Directory.GetFiles(_directory, "*.json");
        var result = new List<UserFlowState>(files.Length);

        foreach (var file in files)
        {
            if (!long.TryParse(Path.GetFileNameWithoutExtension(file), out var userId))
                continue;

            var state = await LoadStateAsync(userId, cancellationToken).ConfigureAwait(false);
            if (state is { Status: FlowStateStatus.Active or FlowStateStatus.WaitingForInput })
                result.Add(state);
        }

        _logger.LogInformation("Restored {Count} active flow states from disk", result.Count);
        return result.AsReadOnly();
    }

    public string GetFilePath(long userId) =>
        Path.Combine(_directory, $"{userId}.json");

    private static void TryDeleteFile(string path)
    {
        try { File.Delete(path); }
        catch { /* best-effort */ }
    }

    /// <summary>
    /// Releases the internal synchronisation primitive used to guard file access.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        _fileLock.Dispose();
        _disposed = true;
    }
}
