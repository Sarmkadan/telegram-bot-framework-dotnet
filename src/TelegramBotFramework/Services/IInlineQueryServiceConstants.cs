namespace TelegramBotFramework.Services;

/// <summary>
/// Constants for IInlineQueryService.
/// </summary>
internal static class IInlineQueryServiceConstants
{
    /// <summary>
    /// Prefix for inline query cache keys.
    /// </summary>
    public const string CacheKeyPrefix = "inline_query_";

    /// <summary>
    /// Default number of results per page.
    /// </summary>
    public const int DefaultPageSize = 10;

    /// <summary>
    /// Default cache expiry in minutes.
    /// </summary>
    public const int DefaultCacheExpiryInMinutes = 5;
}