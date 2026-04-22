#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Models;

/// <summary>
/// Represents an inline query received from a Telegram user.
/// </summary>
public sealed class InlineQuery
{
    /// <summary>Unique identifier supplied by Telegram for this query.</summary>
    public string QueryId { get; set; } = string.Empty;

    /// <summary>Telegram user ID that submitted the query.</summary>
    public long UserId { get; set; }

    /// <summary>Raw text of the query (may be empty for open-ended queries).</summary>
    public string Query { get; set; } = string.Empty;

    /// <summary>Pagination offset supplied by Telegram; empty on the initial request.</summary>
    public string Offset { get; set; } = string.Empty;

    /// <summary>Current processing status of this query.</summary>
    public InlineQueryStatus Status { get; set; } = InlineQueryStatus.Pending;

    /// <summary>UTC timestamp when the query was received.</summary>
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;

    /// <summary>UTC timestamp when the query was answered; null if not yet answered.</summary>
    public DateTime? AnsweredAt { get; set; }

    /// <summary>Arbitrary metadata attached to this query instance.</summary>
    public Dictionary<string, object>? Metadata { get; set; }

    /// <summary>Sets a metadata entry by key.</summary>
    public void SetMetadata(string key, object value)
    {
        Metadata ??= new Dictionary<string, object>();
        Metadata[key] = value;
    }

    /// <summary>Gets a metadata entry by key, or null if not present.</summary>
    public object? GetMetadata(string key) =>
        Metadata?.TryGetValue(key, out var value) == true ? value : null;

    /// <summary>
    /// Validates required fields.
    /// </summary>
    public bool Validate()
    {
        if (string.IsNullOrWhiteSpace(QueryId))
            throw new InvalidOperationException("QueryId is required");

        if (UserId <= 0)
            throw new InvalidOperationException("UserId must be positive");

        return true;
    }

    /// <summary>Returns processing duration in milliseconds, or -1 if the query has not been answered yet.</summary>
    public long GetProcessingDurationMs() =>
        AnsweredAt.HasValue
            ? (long)(AnsweredAt.Value - ReceivedAt).TotalMilliseconds
            : -1;
}

/// <summary>
/// A single result item returned in response to an inline query.
/// </summary>
public sealed class InlineQueryResult
{
    /// <summary>Unique 16-character identifier within the result set.</summary>
    public string ResultId { get; set; } = Guid.NewGuid().ToString("N")[..16];

    /// <summary>Content type of this result.</summary>
    public InlineQueryResultType Type { get; set; } = InlineQueryResultType.Article;

    /// <summary>Title displayed in the results list.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Short description rendered below the title.</summary>
    public string? Description { get; set; }

    /// <summary>Message text sent to the chat when this result is selected.</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>Optional URL used to display a thumbnail preview.</summary>
    public string? ThumbnailUrl { get; set; }

    /// <summary>Opaque payload forwarded to the bot for routing or analytics.</summary>
    public string? CustomPayload { get; set; }

    /// <summary>UTC timestamp when this result was generated.</summary>
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Validates required fields.
    /// </summary>
    public bool Validate()
    {
        if (string.IsNullOrWhiteSpace(Title))
            throw new InvalidOperationException("Title is required");

        if (string.IsNullOrWhiteSpace(Content))
            throw new InvalidOperationException("Content is required");

        return true;
    }
}

/// <summary>
/// A paginated slice of inline query results, ready to be forwarded to the Telegram API.
/// </summary>
public sealed class PagedInlineQueryResult
{
    /// <summary>Results on the current page.</summary>
    public IList<InlineQueryResult> Results { get; set; } = new List<InlineQueryResult>();

    /// <summary>Total number of matching results across all pages.</summary>
    public int TotalCount { get; set; }

    /// <summary>Current page number (1-based).</summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>Maximum number of results per page.</summary>
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// Telegram-compatible offset value to pass with <c>answerInlineQuery</c> to request the next
    /// page; empty string when no further pages exist.
    /// </summary>
    public string NextOffset { get; set; } = string.Empty;

    /// <summary>Whether additional pages are available.</summary>
    public bool HasNextPage => !string.IsNullOrEmpty(NextOffset);

    /// <summary>Total number of pages.</summary>
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
}

/// <summary>Processing status of an inline query.</summary>
public enum InlineQueryStatus
{
    Pending = 0,
    Processing = 1,
    Answered = 2,
    Failed = 3,
    Cached = 4
}

/// <summary>Content type of a single inline query result.</summary>
public enum InlineQueryResultType
{
    Article = 0,
    Photo = 1,
    Video = 2,
    Audio = 3,
    Document = 4,
    Location = 5,
    Sticker = 6
}