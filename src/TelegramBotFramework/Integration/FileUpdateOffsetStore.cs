#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace TelegramBotFramework.Integration;

using System.IO;

/// <summary>
/// A file-based implementation of <see cref="IUpdateOffsetStore"/> that persists the last processed update offset to disk.
/// This implementation is suitable for production scenarios where the application may restart.
/// </summary>
public sealed class FileUpdateOffsetStore : IUpdateOffsetStore
{
    private readonly string _filePath;
    private long _lastOffset = 0;
    private readonly object _lock = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="FileUpdateOffsetStore"/> class.
    /// </summary>
    /// <param name="filePath">The path to the file where the offset will be stored.</param>
    /// <exception cref="ArgumentNullException">Thrown when filePath is null.</exception>
    /// <exception cref="ArgumentException">Thrown when filePath is empty or whitespace.</exception>
    public FileUpdateOffsetStore(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path cannot be null or whitespace.", nameof(filePath));
        }

        _filePath = filePath;
        LoadFromFile();
    }

    /// <summary>
    /// Gets the last processed update offset.
    /// </summary>
    /// <returns>The last processed update offset, or 0 if no updates have been processed.</returns>
    public long GetLastOffset()
    {
        lock (_lock)
        {
            return _lastOffset;
        }
    }

    /// <summary>
    /// Sets the last processed update offset.
    /// </summary>
    /// <param name="offset">The update offset to store.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when offset is negative.</exception>
    public Task SetLastOffset(long offset)
    {
        if (offset < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(offset), "Offset cannot be negative");
        }

        lock (_lock)
        {
            _lastOffset = offset;
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Persists any pending changes to the offset store.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task PersistAsync()
    {
        lock (_lock)
        {
            try
            {
                // Ensure directory exists
                var directory = Path.GetDirectoryName(_filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Write the offset to file
                File.WriteAllText(_filePath, _lastOffset.ToString());
            }
            catch (IOException ex)
            {
                // Log error but don't throw - persistence is best-effort
                // In a real application, you might want to log this
                Console.Error.WriteLine($"Failed to persist offset to {_filePath}: {ex.Message}");
            }

            return Task.CompletedTask;
        }
    }

    private void LoadFromFile()
    {
        try
        {
            if (File.Exists(_filePath))
            {
                var content = File.ReadAllText(_filePath).Trim();
                if (long.TryParse(content, out var offset))
                {
                    _lastOffset = offset;
                }
            }
        }
        catch (IOException ex)
        {
            // Log error but don't throw - we'll use default value of 0
            Console.Error.WriteLine($"Failed to load offset from {_filePath}: {ex.Message}");
        }
    }
}