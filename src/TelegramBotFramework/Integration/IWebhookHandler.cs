#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ====================================================================

namespace TelegramBotFramework.Integration;

using System.Threading.Tasks;

/// <summary>
/// Handles incoming webhook updates from Telegram and processes them.
/// Validates update authenticity and dispatches to appropriate handlers.
/// </summary>
public interface IWebhookHandler
{
    /// <summary>
    /// Processes incoming webhook JSON data from Telegram.
    /// </summary>
    Task<TelegramUpdate?> ProcessUpdateAsync(string jsonData);

    /// <summary>
    /// Validates the webhook request authenticity by comparing the X-Telegram-Bot-Api-Secret-Token header
    /// against the configured secret using constant-time comparison to prevent timing attacks.
    /// </summary>
    /// <param name="secretTokenHeader">The value of the X-Telegram-Bot-Api-Secret-Token header from the request.</param>
    /// <param name="configuredSecret">The configured secret token from WebhookOptions.</param>
    /// <returns>True if the tokens match or no secret is configured; false otherwise.</returns>
    bool ValidateSecretToken(string? secretTokenHeader, string? configuredSecret);
}