#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace TelegramBotFramework.Services;

/// <summary>
/// Provides extension methods for <see cref="BroadcastResult"/> to simplify common operations
/// and provide additional functionality for working with broadcast results.
/// </summary>
public static class BroadcastResultExtensions
{
    /// <summary>
    /// Determines whether the broadcast operation completed with any failures.
    /// </summary>
    /// <param name="result">The broadcast result to check.</param>
    /// <returns><see langword="true"/> if there were any failures; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="result"/> is <see langword="null"/>.</exception>
    public static bool HasFailures(this BroadcastResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return result.FailedCount > 0;
    }

    /// <summary>
    /// Gets the percentage of successfully processed chats (0-100).
    /// </summary>
    /// <param name="result">The broadcast result.</param>
    /// <returns>The success percentage as a value between 0 and 100.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="result"/> is <see langword="null"/>.</exception>
    public static double GetSuccessPercentage(this BroadcastResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return result.TotalChats > 0
            ? (double)result.SuccessCount / result.TotalChats * 100
            : 0;
    }

    /// <summary>
    /// Gets a combined list of all chat IDs (both successful and failed) in the order they were processed.
    /// </summary>
    /// <param name="result">The broadcast result.</param>
    /// <returns>An enumerable containing all chat IDs.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="result"/> is <see langword="null"/>.</exception>
    public static IEnumerable<long> GetAllChatIds(this BroadcastResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        foreach (var chatId in result.SuccessfulChatIds)
        {
            yield return chatId;
        }

        foreach (var failure in result.Failures)
        {
            yield return failure.ChatId;
        }
    }

    /// <summary>
    /// Gets a dictionary mapping chat IDs to their status for quick lookup.
    /// </summary>
    /// <param name="result">The broadcast result.</param>
    /// <returns>A dictionary where keys are chat IDs and values indicate success (<see langword="true"/>) or failure (<see langword="false"/>).</returns>
    /// <exception cref="ArgumentNullException"><paramref name="result"/> is <see langword="null"/>.</exception>
    public static IReadOnlyDictionary<long, bool> GetChatStatusMap(this BroadcastResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var dictionary = new Dictionary<long, bool>(result.TotalChats);

        foreach (var chatId in result.SuccessfulChatIds)
        {
            dictionary[chatId] = true;
        }

        foreach (var failure in result.Failures)
        {
            dictionary[failure.ChatId] = false;
        }

        return dictionary;
    }
}