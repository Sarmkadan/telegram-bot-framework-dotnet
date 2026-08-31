namespace TelegramBotFramework.Formatters;

/// <summary>
/// Constants for XmlFormatter to avoid magic strings.
/// </summary>
internal static class XmlFormatterConstants
{
    public static readonly string ItemsRoot = "items";
    public static readonly string ItemElement = "item";
    public static readonly string ErrorRoot = "error";
    public static readonly string ErrorCode = "code";
    public static readonly string Message = "message";
    public static readonly string Details = "details";
    public static readonly string Timestamp = "timestamp";
    public static readonly string DateFormatRoundtrip = "O";
    public static readonly string Id = "id";
    public static readonly string Content = "content";
    public static readonly string UserId = "userId";
    public static readonly string ChatId = "chatId";
    public static readonly string CreatedAt = "createdAt";
    public static readonly string Type = "type";
    public static readonly string MessagesRoot = "messages";
    public static readonly string CountAttribute = "count";
}