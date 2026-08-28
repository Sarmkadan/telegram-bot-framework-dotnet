#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;

namespace TelegramBotFramework.Services;

/// <summary>
/// In-memory implementation of localization service.
/// </summary>
public class LocalizationService : ILocalizationService
{
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, string>> _templates = new();
    private readonly string _defaultLanguage;

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalizationService"/> class.
    /// </summary>
    /// <param name="defaultLanguage">The default language code to use as fallback (e.g., "en").</param>
    public LocalizationService(string defaultLanguage = LocalizationServiceConstants.DefaultLanguage)
    {
        _defaultLanguage = defaultLanguage ?? throw new ArgumentNullException(nameof(defaultLanguage));
    }

    /// <summary>
    /// Gets a localized string template by key and language.
    /// </summary>
    /// <param name="key">The localization key.</param>
    /// <param name="language">The language code (e.g., "en", "uk", "ru").</param>
    /// <returns>The localized template, or null if not found.</returns>
    public string? GetTemplate(string key, string language)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException(LocalizationServiceConstants.LocalizationKeyCannotBeNullOrEmpty, nameof(key));

        if (string.IsNullOrWhiteSpace(language))
            throw new ArgumentException(LocalizationServiceConstants.LanguageCannotBeNullOrEmpty, nameof(language));

        // Try to get the requested language
        if (_templates.TryGetValue(key, out var languageDict) &&
            languageDict.TryGetValue(language, out var template))
        {
            return template;
        }

        // Fallback to default language
        if (_templates.TryGetValue(key, out var defaultDict) &&
            defaultDict.TryGetValue(_defaultLanguage, out var defaultTemplate))
        {
            return defaultTemplate;
        }

        return null;
    }

    /// <summary>
    /// Gets a localized string by key, language, and format arguments.
    /// Falls back to default language if the requested language is not available.
    /// </summary>
    /// <param name="key">The localization key.</param>
    /// <param name="language">The language code to try first.</param>
    /// <param name="args">Format arguments for the template.</param>
    /// <returns>The formatted localized string.</returns>
    public string Get(string key, string language, params object[] args)
    {
        var template = GetTemplate(key, language);
        if (template is null)
        {
            throw new KeyNotFoundException(string.Format(LocalizationServiceConstants.LocalizationKeyNotFoundFormat, key, language, _defaultLanguage));
        }

        try
        {
            return string.Format(template, args);
        }
        catch (FormatException ex)
        {
            throw new FormatException(string.Format(LocalizationServiceConstants.LocalizationFormatFailedFormat, key, args.Length, template), ex);
        }
    }

    /// <summary>
    /// Gets a localized string by key with default language.
    /// </summary>
    /// <param name="key">The localization key.</param>
    /// <param name="args">Format arguments for the template.</param>
    /// <returns>The formatted localized string.</returns>
    public string Get(string key, params object[] args)
    {
        return Get(key, _defaultLanguage, args);
    }

    /// <summary>
    /// Registers a localization template for a specific language and key.
    /// </summary>
    /// <param name="key">The localization key.</param>
    /// <param name="language">The language code.</param>
    /// <param name="template">The localized template string.</param>
    public void RegisterTemplate(string key, string language, string template)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Localization key cannot be null or empty", nameof(key));

        if (string.IsNullOrWhiteSpace(language))
            throw new ArgumentException("Language cannot be null or empty", nameof(language));

        if (string.IsNullOrWhiteSpace(template))
            throw new ArgumentException(LocalizationServiceConstants.TemplateCannotBeNullOrEmpty, nameof(template));

        var languageDict = _templates.GetOrAdd(key, _ => new ConcurrentDictionary<string, string>());
        languageDict.AddOrUpdate(language, template, (_, _) => template);
    }

    /// <summary>
    /// Registers multiple localization templates at once.
    /// </summary>
    /// <param name="templates">Dictionary of (key, language) -> template.</param>
    public void RegisterTemplates(IDictionary<(string key, string language), string> templates)
    {
        if (templates is null)
            throw new ArgumentNullException(nameof(templates));

        foreach (var kvp in templates)
        {
            RegisterTemplate(kvp.Key.key, kvp.Key.language, kvp.Value);
        }
    }
}
