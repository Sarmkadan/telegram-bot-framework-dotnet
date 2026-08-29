#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace TelegramBotFramework.Utilities;

/// <summary>
/// Constants used by StringExtensions class.
/// </summary>
internal static class StringExtensionsConstants
{
    /// <summary>
    /// Default suffix used when truncating strings.
    /// </summary>
    public const string DefaultSuffix = "…";

    /// <summary>
    /// Regex pattern for invalid characters in slug generation.
    /// </summary>
    public const string InvalidSlugCharactersPattern = "[^a-z0-9\\s-]";

    /// <summary>
    /// Regex pattern for multiple whitespace characters.
    /// </summary>
    public const string MultipleSpacesPattern = @"\s+";

    /// <summary>
    /// Regex pattern for multiple consecutive dashes.
    /// </summary>
    public const string MultipleDashesPattern = @"-+";

    /// <summary>
    /// Regex pattern for email validation.
    /// </summary>
    public const string EmailValidationPattern = "^[^@\\s]+@[^@\\s]+\\.[^@\\s]+$";
}