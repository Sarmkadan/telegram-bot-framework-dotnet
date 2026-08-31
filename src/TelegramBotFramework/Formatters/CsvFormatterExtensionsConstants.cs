namespace TelegramBotFramework.Formatters;

/// <summary>
/// Constants for CsvFormatterExtensions.
/// </summary>
internal static class CsvFormatterExtensionsConstants
{
    /// <summary>
    /// Represents an empty string.
    /// </summary>
    public const string EmptyString = "";

    /// <summary>
    /// The default field separator for CSV (comma).
    /// </summary>
    public const string FieldSeparator = ",";

    /// <summary>
    /// The default delimiter character for CSV (comma).
    /// </summary>
    public const char DefaultDelimiter = ',';

    /// <summary>
    /// The message when at least one header must be specified.
    /// </summary>
    public const string AtLeastOneHeaderMessage = "At least one header must be specified.";

    /// <summary>
    /// The line ending string for the current environment.
    /// </summary>
    public static readonly string LineEnding = System.Environment.NewLine;

    /// <summary>
    /// The quote character used for escaping fields in CSV (double quote).
    /// </summary>
    public const char QuoteChar = '\"';
}
