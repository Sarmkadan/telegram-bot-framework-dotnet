#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Formatters;

/// <summary>
/// Constants for <see cref="CsvFormatterExtensions"/> to avoid magic values.
/// </summary>
internal static class CsvFormatterExtensionsConstants
{
    /// <summary>
    /// Represents an empty string.
    /// </summary>
    public const string EmptyString = "";

    /// <summary>
    /// Default field separator for CSV (comma).
    /// </summary>
    public const char FieldSeparator = ',';

    /// <summary>
    /// Line ending used when constructing CSV rows.
    /// </summary>
    public const string LineEnding = "\r\n";

    /// <summary>
    /// Character used to quote fields that contain special characters.
    /// </summary>
    public const char QuoteChar = '"';

    /// <summary>
    /// Default delimiter used when a custom delimiter is not supplied.
    /// </summary>
    public const char DefaultDelimiter = ',';

    /// <summary>
    /// Error message when no headers are supplied to <c>FormatWithHeaders</c>.
    /// </summary>
    public const string AtLeastOneHeaderMessage = "At least one header must be provided.";
}
