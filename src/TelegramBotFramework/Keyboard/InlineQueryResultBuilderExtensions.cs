#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Globalization;
using TelegramBotFramework.Models;

namespace TelegramBotFramework.Keyboard;

/// <summary>
/// Provides extension methods for <see cref="InlineQueryResultBuilder"/> to simplify common inline query result construction patterns.
/// </summary>
public static class InlineQueryResultBuilderExtensions
{
    /// <summary>
    /// Adds multiple article results with the same content template.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <param name="ids">Collection of unique identifiers for each article.</param>
    /// <param name="title">Title displayed in the results list.</param>
    /// <param name="content">Message text sent to the chat when this result is selected.</param>
    /// <param name="description">Optional description shown below the title.</param>
    /// <param name="thumbnailUrl">Optional URL of the thumbnail image.</param>
    /// <returns>The builder for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    /// <exception cref="ArgumentException">Thrown when id is already used or exceeds length limits.</exception>
    public static InlineQueryResultBuilder AddArticles(
        this InlineQueryResultBuilder builder,
        IEnumerable<string> ids,
        string title,
        string content,
        string? description = null,
        string? thumbnailUrl = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(ids);
        ArgumentNullException.ThrowIfNull(title);
        ArgumentNullException.ThrowIfNull(content);

        foreach (var id in ids)
        {
            builder.AddArticle(id, title, content, description, thumbnailUrl);
        }

        return builder;
    }

    /// <summary>
    /// Adds multiple photo results with the same thumbnail URL.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <param name="ids">Collection of unique identifiers for each photo.</param>
    /// <param name="photoUrls">Collection of photo file URLs.</param>
    /// <param name="captions">Optional captions displayed below each photo.</param>
    /// <param name="thumbnailUrl">Optional URL of the thumbnail image (applied to all photos).</param>
    /// <returns>The builder for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when builder, ids, or photoUrls is null.</exception>
    /// <exception cref="ArgumentException">Thrown when any id is already used or exceeds length limits.</exception>
    public static InlineQueryResultBuilder AddPhotos(
        this InlineQueryResultBuilder builder,
        IEnumerable<string> ids,
        IEnumerable<string> photoUrls,
        IEnumerable<string?>? captions = null,
        string? thumbnailUrl = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(ids);
        ArgumentNullException.ThrowIfNull(photoUrls);

        var captionList = captions?.ToList() ?? new List<string?>();

        foreach (var (id, photoUrl, caption) in ids.Zip(photoUrls, captionList.Concat(Enumerable.Repeat<string?>(null, int.MaxValue))))
        {
            builder.AddPhoto(id, photoUrl, thumbnailUrl, caption);
        }

        return builder;
    }

    /// <summary>
    /// Adds multiple document results with the same file name pattern.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <param name="ids">Collection of unique identifiers for each document.</param>
    /// <param name="documentUrls">Collection of document file URLs.</param>
    /// <param name="titles">Titles of the documents.</param>
    /// <param name="fileNamePattern">File name pattern with {0} placeholder for index (e.g., "document_{0}.pdf").</param>
    /// <param name="descriptions">Optional descriptions shown below each title.</param>
    /// <param name="thumbnailUrl">Optional URL of the thumbnail image (applied to all documents).</param>
    /// <returns>The builder for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when builder, ids, documentUrls, or titles is null.</exception>
    /// <exception cref="ArgumentException">Thrown when any id is already used or exceeds length limits.</exception>
    public static InlineQueryResultBuilder AddDocuments(
        this InlineQueryResultBuilder builder,
        IEnumerable<string> ids,
        IEnumerable<string> documentUrls,
        IEnumerable<string> titles,
        string fileNamePattern,
        IEnumerable<string?>? descriptions = null,
        string? thumbnailUrl = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(ids);
        ArgumentNullException.ThrowIfNull(documentUrls);
        ArgumentNullException.ThrowIfNull(titles);
        ArgumentNullException.ThrowIfNull(fileNamePattern);

        var descriptionList = descriptions?.ToList() ?? new List<string?>();

        foreach (var (id, documentUrl, title, description) in ids.Zip(documentUrls, titles).Zip(descriptionList.Concat(Enumerable.Repeat<string?>(null, int.MaxValue)))
            .Select(x => (x.First.First, x.First.Second, x.First.Third, x.Second)))
        {
            var fileName = string.Format(CultureInfo.InvariantCulture, fileNamePattern, Array.IndexOf(ids.ToArray(), id));
            builder.AddDocument(id, documentUrl, title, fileName, description, thumbnailUrl);
        }

        return builder;
    }

    /// <summary>
    /// Adds a grid of photo results arranged in rows and columns.
    /// Useful for creating image galleries or icon grids.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <param name="baseId">Base identifier for the grid (e.g., "photo_grid").</param>
    /// <param name="photoUrls">Collection of photo file URLs.</param>
    /// <param name="rows">Number of rows in the grid.</param>
    /// <param name="columns">Number of columns in the grid.</param>
    /// <param name="captionProvider">Optional function to generate captions based on row and column indices.</param>
    /// <param name="thumbnailUrl">Optional URL of the thumbnail image (applied to all photos).</param>
    /// <returns>The builder for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when builder or photoUrls is null.</exception>
    /// <exception cref="ArgumentException">Thrown when any generated id exceeds length limits.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when rows or columns is less than 1.</exception>
    public static InlineQueryResultBuilder AddPhotoGrid(
        this InlineQueryResultBuilder builder,
        string baseId,
        IEnumerable<string> photoUrls,
        int rows,
        int columns,
        Func<int, int, string?>? captionProvider = null,
        string? thumbnailUrl = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(baseId);
        ArgumentNullException.ThrowIfNull(photoUrls);

        if (rows < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(rows), "Rows must be at least 1.");
        }

        if (columns < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(columns), "Columns must be at least 1.");
        }

        var photoList = photoUrls.ToList();
        var totalCells = rows * columns;

        for (int i = 0; i < totalCells; i++)
        {
            if (i >= photoList.Count)
            {
                break;
            }

            var row = i / columns;
            var col = i % columns;
            var caption = captionProvider?.Invoke(row, col);
            var id = $"{baseId}_{row}_{col}";

            builder.AddPhoto(id, photoList[i], thumbnailUrl, caption);
        }

        return builder;
    }
}