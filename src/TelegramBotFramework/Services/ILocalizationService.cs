#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Services;

/// <summary>
/// Service interface for localization and internationalization.
/// Provides localized string templates with fallback to default language.
/// </summary>
public interface ILocalizationService
{
    /// <summary>
    /// Gets a localized string template by key and language.
    /// </summary>
    /// <param name="key">The localization key.</param>
    /// <param name="language">The language code (e.g., "en", "uk", "ru").</param>
    /// <returns>The localized template, or null if not found.</returns>
    string? GetTemplate(string key, string language);

    /// <summary>
    /// Gets a localized string by key, language, and format arguments.
    /// Falls back to default language if the requested language is not available.
    /// </summary>
    /// <param name="key">The localization key.</param>
    /// <param name="language">The language code to try first.</param>
    /// <param name="args">Format arguments for the template.</param>
    /// <returns>The formatted localized string.</returns>
    string Get(string key, string language, params object[] args);

    /// <summary>
    /// Gets a localized string by key with default language.
    /// </summary>
    /// <param name="key">The localization key.</param>
    /// <param name="args">Format arguments for the template.</param>
    /// <returns>The formatted localized string.</returns>
    string Get(string key, params object[] args);

    /// <summary>
    /// Registers a localization template for a specific language and key.
    /// </summary>
    /// <param name="key">The localization key.</param>
    /// <param name="language">The language code.</param>
    /// <param name="template">The localized template string.</param>
    void RegisterTemplate(string key, string language, string template);

    /// <summary>
    /// Registers multiple localization templates at once.
    /// </summary>
    /// <param name="templates">Dictionary of (key, language) -> template.</param>
    void RegisterTemplates(IDictionary<(string key, string language), string> templates);
}
