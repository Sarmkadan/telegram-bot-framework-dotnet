#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ====================================================================

namespace TelegramBotFramework.Integration;

using System;
using System.Collections.Generic;

/// <summary>
/// Centralizes magic values used by <see cref="TelegramApiClient"/>:
/// URL templates, API method names, JSON media type, validation limits,
/// defaults and idempotency classifications.
/// </summary>
internal static class TelegramApiClientConstants
{
    // ------------------------------------------------------------------
    // URL templates
    // ------------------------------------------------------------------

    /// <summary>
    /// Template for a Bot API endpoint URL: bot token + method name.
    /// </summary>
    public const string BotApiUrlFormat = "bot{0}/{1}";

    /// <summary>
    /// Template for getting file information: bot token + escaped file id.
    /// </summary>
    public const string GetFileUrlFormat = "bot{0}/getFile?file_id={1}";

    /// <summary>
    /// Template for the getUpdates long-polling query string.
    /// </summary>
    public const string GetUpdatesQueryStringFormat = "getUpdates?offset={0}&timeout={1}";

    // ------------------------------------------------------------------
    // Media type
    // ------------------------------------------------------------------

    /// <summary>
    /// Content type used for JSON request bodies.
    /// </summary>
    public const string JsonContentType = "application/json";

    // ------------------------------------------------------------------
    // API method names
    // ------------------------------------------------------------------

    public const string SendMessageMethod = "sendMessage";
    public const string EditMessageTextMethod = "editMessageText";
    public const string DeleteMessageMethod = "deleteMessage";
    public const string SendPollMethod = "sendPoll";
    public const string SendMediaGroupMethod = "sendMediaGroup";
    public const string GetMeMethod = "getMe";
    public const string GetUpdatesMethod = "getUpdates";
    public const string GetFileMethod = "getFile";
    public const string AnswerCallbackQueryMethod = "answerCallbackQuery";
    public const string SetWebhookMethod = "setWebhook";
    public const string SetMyCommandsMethod = "setMyCommands";

    // ------------------------------------------------------------------
    // Validation limits
    // ------------------------------------------------------------------

    /// <summary>Maximum allowed length of a poll question.</summary>
    public const int MaxPollQuestionLength = 256;

    /// <summary>Minimum number of poll answer options.</summary>
    public const int MinPollOptions = 2;

    /// <summary>Maximum number of poll answer options.</summary>
    public const int MaxPollOptions = 10;

    /// <summary>Maximum allowed length of a single poll option.</summary>
    public const int MaxPollOptionLength = 100;

    /// <summary>Minimum number of items in a media group (album).</summary>
    public const int MinMediaGroupItems = 2;

    /// <summary>Maximum number of items in a media group (album).</summary>
    public const int MaxMediaGroupItems = 10;

    // ------------------------------------------------------------------
    // Defaults
    // ------------------------------------------------------------------

    /// <summary>Default long-polling timeout (in seconds) for getUpdates.</summary>
    public const int DefaultGetUpdatesTimeoutSeconds = 30;

    // ------------------------------------------------------------------
    // Idempotency classification
    // ------------------------------------------------------------------

    /// <summary>
    /// Methods that are generally safe to retry (idempotent).
    /// </summary>
    public static readonly HashSet<string> IdempotentMethods = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        GetMeMethod,
        GetUpdatesMethod,
        GetFileMethod,
        "getChat",
        "getChatAdministrators",
        "getChatMemberCount",
        "getChatMembersCount",
        "getUserProfilePhotos",
        "getStickerSet",
        "getStickers",
        "answerInlineQuery",
        "getMyCommands",
        "getMyDescription",
        "getMyShortDescription",
        "getMyName",
        "getUserChatBoosts",
        "getChatMenuButton",
        "getMyDefaultAdministratorRights"
    };

    /// <summary>
    /// Methods that modify state and should not be retried blindly.
    /// </summary>
    public static readonly HashSet<string> NonIdempotentMethods = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        SendMessageMethod,
        "forwardMessage",
        "copyMessage",
        "sendPhoto",
        "sendAudio",
        "sendDocument",
        "sendVideo",
        "sendAnimation",
        "sendVoice",
        "sendVideoNote",
        SendMediaGroupMethod,
        "sendLocation",
        "sendVenue",
        "sendContact",
        SendPollMethod,
        "sendDice",
        "sendChatAction",
        EditMessageTextMethod,
        "editMessageCaption",
        "editMessageMedia",
        "editMessageReplyMarkup",
        "editMessageLiveLocation",
        "stopMessageLiveLocation",
        DeleteMessageMethod,
        "sendSticker",
        AnswerCallbackQueryMethod,
        "setChatTitle",
        "setChatDescription",
        "setChatPermissions",
        "pinChatMessage",
        "unpinChatMessage",
        "leaveChat",
        "promoteChatMember",
        "setChatAdministratorCustomTitle",
        "banChatMember",
        "unbanChatMember",
        "restrictChatMember",
        SetMyCommandsMethod,
        "deleteMyCommands",
        "setMyDescription",
        "setMyShortDescription",
        "setMyName",
        "setChatMenuButton",
        SetWebhookMethod,
        "deleteWebhook"
    };
}
