#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Integration;

/// <summary>
/// Constants for HttpClientFactory to avoid magic strings and numbers.
/// </summary>
internal static class HttpClientFactoryConstants
{
    // Header names and values
    public const string UserAgentHeaderName = "User-Agent";
    public const string UserAgentHeaderValue = "TelegramBotFramework/1.0";
    public const string AcceptHeaderName = "Accept";
    public const string AcceptHeaderValue = "application/json";

    // Telegram API base URL
    public const string TelegramBaseUrl = "https://api.telegram.org";

    // Cache key parts
    public const string CacheKeyHeaderSeparator = "\n";
    public const string CacheKeyKeyValueSeparator = ":";
    public const string CacheKeyHeaderPart = "|headers|";
    public const string CacheKeyAuthPart = "|auth|";

    // Timeouts and time spans
    public static readonly TimeSpan PooledConnectionLifetime = TimeSpan.FromMinutes(2);
    public static readonly TimeSpan PooledConnectionIdleTimeout = TimeSpan.FromMinutes(1);
    public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);
    public static readonly TimeSpan TelegramTimeout = TimeSpan.FromSeconds(45);
}