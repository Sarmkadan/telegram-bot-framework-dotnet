#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Formatters;

using System.Globalization;

/// <summary>
/// Extension methods for <see cref="CsvFormatter"/> to provide additional formatting capabilities.
/// </summary>
public static class CsvFormatterExtensions
{
    /// <summary>
    /// Formats a collection of objects with custom property selection.
    /// </summary>
    /// <typeparam name="T">The type of objects to format.</typeparam>
    /// <param name="formatter">The CSV formatter instance.</param>
    /// <param name="data">The collection of objects to format.</param>
    /// <param name="propertyNames">Names of properties to include in the output.</param>
    /// <returns>CSV formatted string with only the specified properties.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="formatter"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="propertyNames"/> is <see langword="null"/>.</exception>
    public static string FormatWithProperties<T>(this CsvFormatter formatter, IEnumerable<T> data, params string[] propertyNames)
    {
        ArgumentNullException.ThrowIfNull(formatter);
        ArgumentNullException.ThrowIfNull(propertyNames);

        var list = data?.ToList() ?? new List<T>();
        if (list.Count == 0)
            return CsvFormatterExtensionsConstants.EmptyString;

        var type = typeof(T);
        var properties = propertyNames
            .Select(name => type.GetProperty(name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
            .Where(p => p != null)
            .ToList();

        if (properties.Count == 0)
            return CsvFormatterExtensionsConstants.EmptyString;

        var sb = new System.Text.StringBuilder();

        // Write headers
        var headers = properties.Select(p => EscapeField(p!.Name));
        sb.AppendLine(string.Join(CsvFormatterExtensionsConstants.FieldSeparator, headers));

        // Write data rows
        foreach (var item in list)
        {
            var values = properties.Select(p =>
            {
                var value = p!.GetValue(item);
                var stringValue = value?.ToString() ?? CsvFormatterExtensionsConstants.EmptyString;
                return EscapeField(stringValue);
            });

            sb.AppendLine(string.Join(CsvFormatterExtensionsConstants.FieldSeparator, values));
        }

        return sb.ToString();
    }

    /// <summary>
    /// Formats a collection of objects with custom property selection (single object).
    /// </summary>
    /// <typeparam name="T">The type of object to format.</typeparam>
    /// <param name="formatter">The CSV formatter instance.</param>
    /// <param name="data">The object to format.</param>
    /// <param name="propertyNames">Names of properties to include in the output.</param>
    /// <returns>CSV formatted string with only the specified properties.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="formatter"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="data"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="propertyNames"/> is <see langword="null"/>.</exception>
    public static string FormatWithProperties<T>(this CsvFormatter formatter, T data, params string[] propertyNames)
    {
        ArgumentNullException.ThrowIfNull(formatter);
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(propertyNames);

        var items = new[] { data };
        return formatter.FormatWithProperties((IEnumerable<T>)items, propertyNames);
    }

    /// <summary>
    /// Formats a collection of objects with custom header names.
    /// </summary>
    /// <typeparam name="T">The type of objects to format.</typeparam>
    /// <param name="formatter">The CSV formatter instance.</param>
    /// <param name="data">The collection of objects to format.</param>
    /// <param name="headers">Custom header names to use instead of property names.</param>
    /// <returns>CSV formatted string with custom headers.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="formatter"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="headers"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="headers"/> is empty.</exception>
    public static string FormatWithHeaders<T>(this CsvFormatter formatter, IEnumerable<T> data, params string[] headers)
    {
        ArgumentNullException.ThrowIfNull(formatter);
        ArgumentNullException.ThrowIfNull(headers);

        var list = data?.ToList() ?? new List<T>();
        if (list.Count == 0)
            return CsvFormatterExtensionsConstants.EmptyString;

        if (headers.Length == 0)
            throw new ArgumentException(CsvFormatterExtensionsConstants.AtLeastOneHeaderMessage, nameof(headers));

        var type = typeof(T);
        var properties = type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
            .Where(p => p.CanRead)
            .ToList();

        if (properties.Count == 0)
            return CsvFormatterExtensionsConstants.EmptyString;

        var sb = new System.Text.StringBuilder();

        // Write custom headers
        var escapedHeaders = headers.Select(h => EscapeField(h));
        sb.AppendLine(string.Join(CsvFormatterExtensionsConstants.FieldSeparator, escapedHeaders));

        // Write data rows using all properties in order
        foreach (var item in list)
        {
            var values = properties.Select(p =>
            {
                var value = p.GetValue(item);
                var stringValue = value?.ToString() ?? CsvFormatterExtensionsConstants.EmptyString;
                return EscapeField(stringValue);
            });

            sb.AppendLine(string.Join(CsvFormatterExtensionsConstants.FieldSeparator, values));
        }

        return sb.ToString();
    }

    /// <summary>
    /// Formats a collection of objects with custom delimiter.
    /// </summary>
    /// <typeparam name="T">The type of objects to format.</typeparam>
    /// <param name="formatter">The CSV formatter instance.</param>
    /// <param name="data">The collection of objects to format.</param>
    /// <param name="delimiter">The delimiter character to use instead of comma.</param>
    /// <returns>CSV formatted string with custom delimiter.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="formatter"/> is <see langword="null"/>.</exception>
    public static string FormatWithDelimiter<T>(this CsvFormatter formatter, IEnumerable<T> data, char delimiter = CsvFormatterExtensionsConstants.DefaultDelimiter)
    {
        ArgumentNullException.ThrowIfNull(formatter);

        var list = data?.ToList() ?? new List<T>();
        if (list.Count == 0)
            return CsvFormatterExtensionsConstants.EmptyString;

        var type = typeof(T);
        var properties = type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
            .Where(p => p.CanRead)
            .ToList();

        if (properties.Count == 0)
            return CsvFormatterExtensionsConstants.EmptyString;

        var sb = new System.Text.StringBuilder();

        // Write headers
        var headers = properties.Select(p => EscapeField(p.Name, delimiter));
        sb.AppendLine(string.Join(delimiter.ToString(), headers));

        // Write data rows
        foreach (var item in list)
        {
            var values = properties.Select(p =>
            {
                var value = p.GetValue(item);
                var stringValue = value?.ToString() ?? CsvFormatterExtensionsConstants.EmptyString;
                return EscapeField(stringValue, delimiter);
            });

            sb.AppendLine(string.Join(delimiter.ToString(), values));
        }

        return sb.ToString();
    }

    /// <summary>
    /// Escapes a field value for CSV format.
    /// </summary>
    private static string EscapeField(string? field)
    {
        if (string.IsNullOrEmpty(field))
            return CsvFormatterExtensionsConstants.EmptyString;

        // If field contains special characters, wrap in quotes and escape inner quotes
        if (field.Contains(CsvFormatterExtensionsConstants.FieldSeparator) || field.Contains(CsvFormatterExtensionsConstants.LineEnding) || field.Contains(CsvFormatterExtensionsConstants.QuoteChar.ToString()))
        {
            return CsvFormatterExtensionsConstants.QuoteChar + field.Replace(CsvFormatterExtensionsConstants.QuoteChar.ToString(), CsvFormatterExtensionsConstants.QuoteChar.ToString() + CsvFormatterExtensionsConstants.QuoteChar) + CsvFormatterExtensionsConstants.QuoteChar;
        }

        return field;
    }

    /// <summary>
    /// Escapes a field value for CSV format with custom delimiter support.
    /// </summary>
    private static string EscapeField(string? field, char delimiter)
    {
        if (string.IsNullOrEmpty(field))
            return CsvFormatterExtensionsConstants.EmptyString;

        var escaped = EscapeField(field);

        // If the escaped field contains the custom delimiter, we need to re-escape it
        if (escaped.Contains(delimiter.ToString()))
        {
            // Re-escape with quotes
            return CsvFormatterExtensionsConstants.QuoteChar + escaped.Replace(CsvFormatterExtensionsConstants.QuoteChar.ToString(), CsvFormatterExtensionsConstants.QuoteChar.ToString() + CsvFormatterExtensionsConstants.QuoteChar) + CsvFormatterExtensionsConstants.QuoteChar;
        }

        return escaped;
    }
}
