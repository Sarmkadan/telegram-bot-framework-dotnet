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
    Task<ApiResult<bool>> SendMessageAsync(long chatId, string text, CancellationToken cancellationToken = default);

    /// <summary>Sends a message with an inline keyboard.</summary>
    Task<ApiResult<bool>> SendMessageWithButtonsAsync(long chatId, string text, string[][] buttonLabels, CancellationToken cancellationToken = default);

    /// <summary>Edits a previously sent message.</summary>
    Task<ApiResult<bool>> EditMessageAsync(long chatId, int messageId, string newText, CancellationToken cancellationToken = default);

    /// <summary>Deletes a message from a chat.</summary>
    Task<ApiResult<bool>> DeleteMessageAsync(long chatId, int messageId, CancellationToken cancellationToken = default);

    /// <summary>Sends a poll to a chat. The value is the poll message ID.</summary>
    Task<ApiResult<int>> SendPollAsync(long chatId, string question, string[] options, bool allowsMultipleAnswers = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a media group (album) to a chat.
    /// </summary>
    /// <param name="chatId">Target chat identifier</param>
    /// <param name="items">List of media items (MediaGroupItemConstants.MinimumMediaGroupSize-MediaGroupItemConstants.MaximumMediaGroupSize items)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The message IDs of the sent media items on success; a failed result otherwise</returns>
    Task<ApiResult<IList<int>>> SendMediaGroupAsync(long chatId, IList<MediaGroupItem> items, CancellationToken cancellationToken = default);

    /// <summary>Returns the raw JSON describing the bot via <c>getMe</c>.</summary>
    Task<ApiResult<string>> GetMeAsync();

    /// <summary>Answers a callback query, optionally showing a notification.</summary>
    Task<ApiResult<bool>> AnswerCallbackQueryAsync(string callbackQueryId, string? notificationText = null);

    /// <summary>
    /// Registers a webhook URL with Telegram. When a secret token is supplied, Telegram includes it in the
    /// <c>X-Telegram-Bot-Api-Secret-Token</c> header of every update it delivers.
    /// </summary>
    /// <param name="webhookUrl">The absolute URL to which Telegram sends updates.</param>
    /// <param name="secretToken">Optional secret token (see <see cref="WebhookSecretToken"/>); <see langword="null"/> registers without one.</param>
    Task<ApiResult<bool>> SetWebhookAsync(string webhookUrl, string? secretToken = null);

    /// <summary>Removes the currently registered webhook.</summary>
    Task<ApiResult<bool>> RemoveWebhookAsync();

    /// <summary>Long-polls Telegram for new updates starting at <paramref name="offset"/>.</summary>
    Task<ApiResult<IReadOnlyList<JsonElement>>> GetUpdatesAsync(long offset = 0, int timeoutSeconds = TelegramApiClientConstants.DefaultGetUpdatesTimeoutSeconds);

    /// <summary>Sets the list of bot commands shown in the Telegram UI.</summary>
    /// <param name="commands">Collection of command name / description pairs.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A result whose <see cref="ApiResult{T}.Success"/> is true when Telegram accepted the commands.</returns>
    Task<ApiResult<bool>> SetMyCommandsAsync(IReadOnlyList<BotCommand> commands, CancellationToken cancellationToken = default);

/// <summary>
/// Gets information about a file stored on Telegram servers.
/// </summary>
/// <param name="fileId">File identifier to get info for</param>
/// <param name="cancellationToken">Cancellation token</param>
/// <returns>File information including file path and size on success; a failed result otherwise</returns>
Task<ApiResult<FileInfoResult>> GetFileAsync(string fileId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the circuit breaker guarding this client's Telegram API calls, or <see langword="null"/> when none is configured.
    /// </summary>
    /// <remarks>
    /// <see cref="PollingStrategy"/> consults this breaker and pauses polling while it is open.
    /// </remarks>
    TelegramCircuitBreaker? CircuitBreaker => null;
}

/// <summary>
/// File information returned by Telegram API's getFile method.
/// </summary>
public record FileInfoResult(string FileId, string FileUniqueId, string FilePath, long FileSize, DateTimeOffset CreatedAt);
