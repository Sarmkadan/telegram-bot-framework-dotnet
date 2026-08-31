#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace TelegramBotFramework.Integration;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;

/// <summary>
/// Represents a media item for sending in a media group.
/// </summary>
public record MediaGroupItem
{
    public MediaGroupItem(MediaType Type, string FileIdOrUrl, string? Caption = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(FileIdOrUrl);

        this.Type = Type;
        this.FileIdOrUrl = FileIdOrUrl;
        this.Caption = Caption;
    }

    public MediaType Type { get; init; }

    public string FileIdOrUrl { get; init; }

    public string? Caption { get; init; }

    public void Deconstruct(out MediaType Type, out string FileIdOrUrl, out string? Caption)
    {
        Type = this.Type;
        FileIdOrUrl = this.FileIdOrUrl;
        Caption = this.Caption;
    }
}

/// <summary>
/// Media type for media group items.
/// </summary>
public enum MediaType
{
    Photo,
    Video,
    Audio,
    Document
}

/// <summary>
/// Simple representation of a bot command (name + description) for the
/// <c>setMyCommands</c> Telegram API method.
/// </summary>
public record BotCommand(string Command, string Description);

/// <summary>
/// Abstraction over the Telegram Bot API surface used by the framework.
/// </summary>
/// <remarks>
/// <see cref="TelegramApiClient"/> is the default HTTP implementation.
/// <see cref="PollingStrategy"/> and <see cref="WebhookService"/> depend on this
/// interface rather than the concrete client, so both can be exercised in tests
/// with a fake that never touches the network.
/// </remarks>
public interface ITelegramApiClient
{
    /// <summary>Sends a plain text message to a chat.</summary>
    Task<bool> SendMessageAsync(long chatId, string text, CancellationToken cancellationToken = default);

    /// <summary>Sends a message with an inline keyboard.</summary>
    Task<bool> SendMessageWithButtonsAsync(long chatId, string text, string[][] buttonLabels, CancellationToken cancellationToken = default);

    /// <summary>Edits a previously sent message.</summary>
    Task<bool> EditMessageAsync(long chatId, int messageId, string newText, CancellationToken cancellationToken = default);

    /// <summary>Deletes a message from a chat.</summary>
    Task<bool> DeleteMessageAsync(long chatId, int messageId, CancellationToken cancellationToken = default);

    /// <summary>Sends a poll to a chat.</summary>
    Task<int?> SendPollAsync(long chatId, string question, string[] options, bool allowsMultipleAnswers = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a media group (album) to a chat.
    /// </summary>
    /// <param name="chatId">Target chat identifier</param>
    /// <param name="items">List of media items (MediaGroupItemConstants.MinimumMediaGroupSize-MediaGroupItemConstants.MaximumMediaGroupSize items)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of message IDs for the sent media items if successful, empty list otherwise</returns>
    Task<IList<int>> SendMediaGroupAsync(long chatId, IList<MediaGroupItem> items, CancellationToken cancellationToken = default);

    /// <summary>Returns the bot username via <c>getMe</c>, or null on failure.</summary>
    Task<string?> GetMeAsync();

    /// <summary>Answers a callback query, optionally showing a notification.</summary>
    Task<bool> AnswerCallbackQueryAsync(string callbackQueryId, string? notificationText = null);

    /// <summary>Registers a webhook URL with Telegram.</summary>
    Task<bool> SetWebhookAsync(string webhookUrl);

    /// <summary>Removes the currently registered webhook.</summary>
    Task<bool> RemoveWebhookAsync();

    /// <summary>Long-polls Telegram for new updates starting at <paramref name="offset"/>.</summary>
    Task<IReadOnlyList<JsonElement>> GetUpdatesAsync(long offset = 0, int timeoutSeconds = TelegramApiClientConstants.DefaultGetUpdatesTimeoutSeconds);

    /// <summary>Sets the list of bot commands shown in the Telegram UI.</summary>
    /// <param name="commands">Collection of command name / description pairs.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>True if the request succeeded, false otherwise.</returns>
    Task<bool> SetMyCommandsAsync(IReadOnlyList<BotCommand> commands, CancellationToken cancellationToken = default);

/// <summary>
/// Gets information about a file stored on Telegram servers.
/// </summary>
/// <param name="fileId">File identifier to get info for</param>
/// <param name="cancellationToken">Cancellation token</param>
/// <returns>File information including file path and size, or null if not found</returns>
Task<FileInfoResult?> GetFileAsync(string fileId, CancellationToken cancellationToken = default);

}

/// <summary>
/// File information returned by Telegram API's getFile method.
/// </summary>
public record FileInfoResult(string FileId, string FileUniqueId, string FilePath, long FileSize, DateTimeOffset CreatedAt);
