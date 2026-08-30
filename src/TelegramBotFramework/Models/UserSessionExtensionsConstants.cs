namespace TelegramBotFramework.Models;

/// <summary>
/// Constants for UserSessionExtensions.
/// </summary>
internal static class UserSessionExtensionsConstants
{
    /// <summary>
    /// Default timeout for considering a session idle.
    /// </summary>
    public static readonly TimeSpan DefaultIdleTimeout = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Default threshold for warning about session expiration.
    /// </summary>
    public static readonly TimeSpan DefaultWarningThreshold = TimeSpan.FromHours(1);
}