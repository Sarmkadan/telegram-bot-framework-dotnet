#nullable enable

namespace TelegramBotFramework.Services;

/// <summary>
/// Constants for LocalizationService.
/// </summary>
internal static class LocalizationServiceConstants
{
    /// <summary>
    /// Default language code used when no language is specified.
    /// </summary>
    public const string DefaultLanguage = "en";

    /// <summary>
    /// Error message for null or empty localization key.
    /// </summary>
    public const string LocalizationKeyCannotBeNullOrEmpty = "Localization key cannot be null or empty";

    /// <summary>
    /// Error message for null or empty language.
    /// </summary>
    public const string LanguageCannotBeNullOrEmpty = "Language cannot be null or empty";

    /// <summary>
    /// Error message for null or empty template.
    /// </summary>
    public const string TemplateCannotBeNullOrEmpty = "Template cannot be null or empty";

    /// <summary>
    /// Exception message format for localization key not found.
    /// </summary>
    public const string LocalizationKeyNotFoundFormat = "Localization key '{key}' not found for language '{language}' or default language '{_defaultLanguage}'";

    /// <summary>
    /// Exception message format for localization formatting failure.
    /// </summary>
    public const string LocalizationFormatFailedFormat = "Failed to format localization key '{key}' with {args.Length} arguments. Template: '{template}'";
}