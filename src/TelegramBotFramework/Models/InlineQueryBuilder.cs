#nullable enable
using System;
using System.Collections.Generic;

namespace TelegramBotFramework.Models;

/// <summary>
/// A builder class for creating <see cref="InlineQuery"/> instances.
/// </summary>
public sealed class InlineQueryBuilder
{
    private string _queryId = string.Empty;
    private long _userId;
    private string _query = string.Empty;
    private string _offset = string.Empty;
    private InlineQueryStatus _status = InlineQueryStatus.Pending;
    private DateTime _receivedAt = DateTime.UtcNow;
    private DateTime? _answeredAt;
    private Dictionary<string, object>? _metadata;

    /// <summary>
    /// Initializes a new instance of the <see cref="InlineQueryBuilder"/> class.
    /// </summary>
    public InlineQueryBuilder() { }

    /// <summary>
    /// Sets the unique identifier supplied by Telegram for this query.
    /// </summary>
    /// <param name="value">The query identifier.</param>
    /// <returns>The builder instance.</returns>
    /// <exception cref="ArgumentException">Thrown when value is null or whitespace.</exception>
    public InlineQueryBuilder WithQueryId(string value)
    {
        ArgumentException.ThrowIfNullOrEmpty(value);
        _queryId = value;
        return this;
    }

    /// <summary>
    /// Sets the Telegram user ID that submitted the query.
    /// </summary>
    /// <param name="value">The user identifier.</param>
    /// <returns>The builder instance.</returns>
    public InlineQueryBuilder WithUserId(long value)
    {
        _userId = value;
        return this;
    }

    /// <summary>
    /// Sets the raw text of the query (may be empty for open-ended queries).
    /// </summary>
    /// <param name="value">The query text.</param>
    /// <returns>The builder instance.</returns>
    public InlineQueryBuilder WithQuery(string value)
    {
        _query = value;
        return this;
    }

    /// <summary>
    /// Sets the pagination offset supplied by Telegram; empty on the initial request.
    /// </summary>
    /// <param name="value">The pagination offset.</param>
    /// <returns>The builder instance.</returns>
    public InlineQueryBuilder WithOffset(string value)
    {
        _offset = value;
        return this;
    }

    /// <summary>
    /// Sets the current processing status of this query.
    /// </summary>
    /// <param name="value">The processing status.</param>
    /// <returns>The builder instance.</returns>
    public InlineQueryBuilder WithStatus(InlineQueryStatus value)
    {
        _status = value;
        return this;
    }

    /// <summary>
    /// Sets the UTC timestamp when the query was received.
    /// </summary>
    /// <param name="value">The received timestamp.</param>
    /// <returns>The builder instance.</returns>
    public InlineQueryBuilder WithReceivedAt(DateTime value)
    {
        _receivedAt = value;
        return this;
    }

    /// <summary>
    /// Sets the UTC timestamp when the query was answered; null if not yet answered.
    /// </summary>
    /// <param name="value">The answered timestamp.</param>
    /// <returns>The builder instance.</returns>
    public InlineQueryBuilder WithAnsweredAt(DateTime? value)
    {
        _answeredAt = value;
        return this;
    }

    /// <summary>
    /// Sets the arbitrary metadata attached to this query instance.
    /// </summary>
    /// <param name="value">The metadata dictionary.</param>
    /// <returns>The builder instance.</returns>
    public InlineQueryBuilder WithMetadata(Dictionary<string, object>? value)
    {
        _metadata = value;
        return this;
    }

    /// <summary>
    /// Creates a new <see cref="InlineQueryBuilder"/> instance pre-filled from a template.
    /// </summary>
    /// <param name="template">The template <see cref="InlineQuery"/>.</param>
    /// <returns>A new <see cref="InlineQueryBuilder"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown if template is null.</exception>
    public static InlineQueryBuilder From(InlineQuery template)
    {
        ArgumentNullException.ThrowIfNull(template);
        return new InlineQueryBuilder()
            .WithQueryId(template.QueryId)
            .WithUserId(template.UserId)
            .WithQuery(template.Query)
            .WithOffset(template.Offset)
            .WithStatus(template.Status)
            .WithReceivedAt(template.ReceivedAt)
            .WithAnsweredAt(template.AnsweredAt)
            .WithMetadata(template.Metadata);
    }

    /// <summary>
    /// Builds the <see cref="InlineQuery"/> instance.
    /// </summary>
    /// <returns>A configured <see cref="InlineQuery"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when required properties are missing.</exception>
    public InlineQuery Build()
    {
        if (string.IsNullOrWhiteSpace(_queryId))
        {
            throw new ArgumentException("QueryId is required", nameof(_queryId));
        }

        if (_userId <= 0)
        {
            throw new ArgumentException("UserId must be positive", nameof(_userId));
        }

        return new InlineQuery
        {
            QueryId = _queryId,
            UserId = _userId,
            Query = _query,
            Offset = _offset,
            Status = _status,
            ReceivedAt = _receivedAt,
            AnsweredAt = _answeredAt,
            Metadata = _metadata
        };
    }
}