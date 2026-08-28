#nullable enable
namespace TelegramBotFramework.Keyboard;

using System.Collections.Generic;
using TelegramBotFramework.Models;

/// <summary>
/// Interface for building Telegram inline query results fluently.
/// </summary>
public interface IInlineQueryResultBuilder
{
    /// <summary>
    /// Adds an article result to the builder.
    /// </summary>
    InlineQueryResultBuilder AddArticle(
        string id,
        string title,
        string content,
        string? description = null,
        string? thumbnailUrl = null,
        string? customPayload = null);

    /// <summary>
    /// Adds a photo result to the builder.
    /// </summary>
    InlineQueryResultBuilder AddPhoto(
        string id,
        string photoUrl,
        string? thumbnailUrl = null,
        string? caption = null,
        string? customPayload = null);

    /// <summary>
    /// Adds a document result to the builder.
    /// </summary>
    InlineQueryResultBuilder AddDocument(
        string id,
        string documentUrl,
        string title,
        string fileName,
        string? description = null,
        string? thumbnailUrl = null,
        string? customPayload = null);

    /// <summary>
    /// Adds a video result to the builder.
    /// </summary>
    InlineQueryResultBuilder AddVideo(
        string id,
        string videoUrl,
        string thumbnailUrl,
        string title,
        string? caption = null,
        string? customPayload = null);

    /// <summary>
    /// Adds an audio result to the builder.
    /// </summary>
    InlineQueryResultBuilder AddAudio(
        string id,
        string audioUrl,
        string title,
        string? caption = null,
        string? customPayload = null);

    /// <summary>
    /// Adds a location result to the builder.
    /// </summary>
    InlineQueryResultBuilder AddLocation(
        string id,
        double latitude,
        double longitude,
        string title,
        string? customPayload = null);

    /// <summary>
    /// Adds a sticker result to the builder.
    /// </summary>
    InlineQueryResultBuilder AddSticker(
        string id,
        string stickerUrl,
        string title,
        string? customPayload = null);

    /// <summary>
    /// Adds multiple results at once from an existing collection.
    /// </summary>
    InlineQueryResultBuilder AddRange(IEnumerable<InlineQueryResult> results);

    /// <summary>
    /// Adds an existing result to the builder.
    /// </summary>
    InlineQueryResultBuilder Add(InlineQueryResult result);

    /// <summary>
    /// Enables or disables automatic validation.
    /// </summary>
    InlineQueryResultBuilder WithValidation(bool enabled = true);

    /// <summary>
    /// Validates the current state of the builder and returns a list of validation errors.
    /// </summary>
    IReadOnlyList<string> Validate();

    /// <summary>
    /// Validates the current state of the builder.
    /// </summary>
    bool IsValid();

    /// <summary>
    /// Builds and returns the collection of inline query results.
    /// </summary>
    IList<InlineQueryResult> Build();
}