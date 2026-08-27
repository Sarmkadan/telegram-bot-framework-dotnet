#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Models;

/// <summary>
/// Interface for an inline query received from a Telegram user.
/// </summary>
public interface IInlineQuery
{
    /// <summary>Unique identifier supplied by Telegram for this query.</summary>
    string QueryId { get; set; }

    /// <summary>Telegram user ID that submitted the query.</summary>
    long UserId { get; set; }

    /// <summary>Raw text of the query (may be empty for open-ended queries).</summary>
    string Query { get; set; }

    /// <summary>Pagination offset supplied by Telegram; empty on the initial request.</summary>
    string Offset { get; set; }

    /// <summary>Current processing status of this query.</summary>
    InlineQueryStatus Status { get; set; }

    /// <summary>UTC timestamp when the query was received.</summary>
    DateTime ReceivedAt { get; set; }

    /// <summary>UTC timestamp when the query was answered; null if not yet answered.</summary>
    DateTime? AnsweredAt { get; set; }

    /// <summary>Arbitrary metadata attached to this query instance.</summary>
    Dictionary<string, object>? Metadata { get; set; }

    /// <summary>Sets a metadata entry by key.</summary>
    void SetMetadata(string key, object value);

    /// <summary>Gets a metadata entry by key, or null if not present.</summary>
    object? GetMetadata(string key);

    /// <summary>
    /// Validates required fields.
    /// </summary>
    bool Validate();

    /// <summary>Returns processing duration in milliseconds, or -1 if the query has not been answered yet.</summary>
    long GetProcessingDurationMs();
}