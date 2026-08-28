#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using TelegramBotFramework.Models;

namespace TelegramBotFramework.Keyboard;

/// <summary>
/// Fluent builder for constructing Telegram inline query results.
/// Supports article, photo, document, and other result types with validation
/// enforcing Telegram limits (max 50 results, id length, etc.).
/// </summary>
/// <example>
/// <code>
/// var results = InlineQueryResultBuilder.Create()
///     .AddArticle("article_1", "Article Title", "This is the article content")
///     .AddPhoto("photo_1", "https://example.com/photo.jpg", "Photo caption")
///     .AddDocument("doc_1", "https://example.com/document.pdf", "Document.pdf", "Check out this document")
///     .Validate()
///     .Build();
/// </code>
/// </example>
public sealed class InlineQueryResultBuilder : IInlineQueryResultBuilder
{
    private readonly List<InlineQueryResult> _results = new();
    private readonly HashSet<string> _usedIds = new();
    private int _maxResults = 50;
    private bool _validationEnabled = true;

    /// <summary>
    /// Initialises a new <see cref="InlineQueryResultBuilder"/>
    /// </summary>
    /// <param name="maxResults">Maximum number of results allowed (default: 50, Telegram limit).</param>
    public InlineQueryResultBuilder(int maxResults = 50)
    {
        if (maxResults < 1 || maxResults > 50)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxResults),
                "Must be between 1 and 50 (Telegram inline query result limit).");
        }

        _maxResults = maxResults;
    }

    /// <summary>Creates a new builder instance with the default result limit.</summary>
    public static InlineQueryResultBuilder Create(int maxResults = 50) => new(maxResults);

    /// <summary>
    /// Adds an article result to the builder.
    /// Article results are displayed as text messages in the chat.
    /// </summary>
    /// <param name="id">Unique identifier for this result (max 64 bytes).</param>
    /// <param name="title">Title displayed in the results list.</param>
    /// <param name="content">Message text sent to the chat when this result is selected.</param>
    /// <param name="description">Optional description shown below the title.</param>
    /// <param name="thumbnailUrl">Optional URL of the thumbnail image.</param>
    /// <param name="customPayload">Optional opaque payload for bot-specific routing.</param>
    /// <exception cref="ArgumentException">Thrown when id is already used or exceeds length limits.</exception>
    public InlineQueryResultBuilder AddArticle(
        string id,
        string title,
        string content,
        string? description = null,
        string? thumbnailUrl = null,
        string? customPayload = null)
    {
        ValidateId(id);
        ValidateTitle(title);
        ValidateContent(content);

        _results.Add(new InlineQueryResult
        {
            ResultId = id,
            Type = InlineQueryResultType.Article,
            Title = title,
            Content = content,
            Description = description,
            ThumbnailUrl = thumbnailUrl,
            CustomPayload = customPayload
        });

        return this;
    }

    /// <summary>
    /// Adds a photo result to the builder.
    /// Photo results display an image with optional caption.
    /// </summary>
    /// <param name="id">Unique identifier for this result (max 64 bytes).</param>
    /// <param name="photoUrl">URL of the photo file.</param>
    /// <param name="thumbnailUrl">Optional URL of the thumbnail image.</param>
    /// <param name="caption">Optional caption displayed below the photo.</param>
    /// <param name="customPayload">Optional opaque payload for bot-specific routing.</param>
    /// <exception cref="ArgumentException">Thrown when id is already used or exceeds length limits.</exception>
    public InlineQueryResultBuilder AddPhoto(
        string id,
        string photoUrl,
        string? thumbnailUrl = null,
        string? caption = null,
        string? customPayload = null)
    {
        ValidateId(id);
        ValidatePhotoUrl(photoUrl);

        _results.Add(new InlineQueryResult
        {
            ResultId = id,
            Type = InlineQueryResultType.Photo,
            Title = caption ?? string.Empty,
            Content = photoUrl,
            Description = caption,
            ThumbnailUrl = thumbnailUrl,
            CustomPayload = customPayload
        });

        return this;
    }

    /// <summary>
    /// Adds a document result to the builder.
    /// Document results display a file with title and optional description.
    /// </summary>
    /// <param name="id">Unique identifier for this result (max 64 bytes).</param>
    /// <param name="documentUrl">URL of the document file.</param>
    /// <param name="title">Title of the document.</param>
    /// <param name="fileName">Name of the document file.</param>
    /// <param name="description">Optional description shown below the title.</param>
    /// <param name="thumbnailUrl">Optional URL of the thumbnail image.</param>
    /// <param name="customPayload">Optional opaque payload for bot-specific routing.</param>
    /// <exception cref="ArgumentException">Thrown when id is already used or exceeds length limits.</exception>
    public InlineQueryResultBuilder AddDocument(
        string id,
        string documentUrl,
        string title,
        string fileName,
        string? description = null,
        string? thumbnailUrl = null,
        string? customPayload = null)
    {
        ValidateId(id);
        ValidateTitle(title);
        ValidateDocumentUrl(documentUrl);

        _results.Add(new InlineQueryResult
        {
            ResultId = id,
            Type = InlineQueryResultType.Document,
            Title = title,
            Content = documentUrl,
            Description = description ?? fileName,
            ThumbnailUrl = thumbnailUrl,
            CustomPayload = customPayload
        });

        return this;
    }

    /// <summary>
    /// Adds a video result to the builder.
    /// Video results display a video with optional caption.
    /// </summary>
    /// <param name="id">Unique identifier for this result (max 64 bytes).</param>
    /// <param name="videoUrl">URL of the video file.</param>
    /// <param name="thumbnailUrl">URL of the video thumbnail.</param>
    /// <param name="title">Title displayed in the results list.</param>
    /// <param name="caption">Optional caption displayed below the video.</param>
    /// <param name="customPayload">Optional opaque payload for bot-specific routing.</param>
    /// <exception cref="ArgumentException">Thrown when id is already used or exceeds length limits.</exception>
    public InlineQueryResultBuilder AddVideo(
        string id,
        string videoUrl,
        string thumbnailUrl,
        string title,
        string? caption = null,
        string? customPayload = null)
    {
        ValidateId(id);
        ValidateTitle(title);
        ValidateVideoUrl(videoUrl);
        ValidateThumbnailUrl(thumbnailUrl);

        _results.Add(new InlineQueryResult
        {
            ResultId = id,
            Type = InlineQueryResultType.Video,
            Title = title,
            Content = videoUrl,
            Description = caption,
            ThumbnailUrl = thumbnailUrl,
            CustomPayload = customPayload
        });

        return this;
    }

    /// <summary>
    /// Adds an audio result to the builder.
    /// Audio results display an audio file with optional title.
    /// </summary>
    /// <param name="id">Unique identifier for this result (max 64 bytes).</param>
    /// <param name="audioUrl">URL of the audio file.</param>
    /// <param name="title">Title of the audio file.</param>
    /// <param name="caption">Optional caption displayed below the audio.</param>
    /// <param name="customPayload">Optional opaque payload for bot-specific routing.</param>
    /// <exception cref="ArgumentException">Thrown when id is already used or exceeds length limits.</exception>
    public InlineQueryResultBuilder AddAudio(
        string id,
        string audioUrl,
        string title,
        string? caption = null,
        string? customPayload = null)
    {
        ValidateId(id);
        ValidateTitle(title);
        ValidateAudioUrl(audioUrl);

        _results.Add(new InlineQueryResult
        {
            ResultId = id,
            Type = InlineQueryResultType.Audio,
            Title = title,
            Content = audioUrl,
            Description = caption,
            CustomPayload = customPayload
        });

        return this;
    }

    /// <summary>
    /// Adds a location result to the builder.
    /// Location results display a map location with optional title.
    /// </summary>
    /// <param name="id">Unique identifier for this result (max 64 bytes).</param>
    /// <param name="latitude">Latitude of the location.</param>
    /// <param name="longitude">Longitude of the location.</param>
    /// <param name="title">Title displayed in the results list.</param>
    /// <param name="customPayload">Optional opaque payload for bot-specific routing.</param>
    /// <exception cref="ArgumentException">Thrown when id is already used or exceeds length limits.</exception>
    public InlineQueryResultBuilder AddLocation(
        string id,
        double latitude,
        double longitude,
        string title,
        string? customPayload = null)
    {
        ValidateId(id);
        ValidateTitle(title);

        _results.Add(new InlineQueryResult
        {
            ResultId = id,
            Type = InlineQueryResultType.Location,
            Title = title,
            Content = $"{latitude},{longitude}",
            CustomPayload = customPayload
        });

        return this;
    }

    /// <summary>
    /// Adds a sticker result to the builder.
    /// Sticker results display a sticker with optional title.
    /// </summary>
    /// <param name="id">Unique identifier for this result (max 64 bytes).</param>
    /// <param name="stickerUrl">URL of the sticker file.</param>
    /// <param name="title">Title displayed in the results list.</param>
    /// <param name="customPayload">Optional opaque payload for bot-specific routing.</param>
    /// <exception cref="ArgumentException">Thrown when id is already used or exceeds length limits.</exception>
    public InlineQueryResultBuilder AddSticker(
        string id,
        string stickerUrl,
        string title,
        string? customPayload = null)
    {
        ValidateId(id);
        ValidateTitle(title);
        ValidateStickerUrl(stickerUrl);

        _results.Add(new InlineQueryResult
        {
            ResultId = id,
            Type = InlineQueryResultType.Sticker,
            Title = title,
            Content = stickerUrl,
            CustomPayload = customPayload
        });

        return this;
    }

    /// <summary>
    /// Adds multiple results at once from an existing collection.
    /// </summary>
    /// <param name="results">Collection of inline query results.</param>
    /// <returns>The builder for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="results"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when any id is already used or exceeds length limits.</exception>
    public InlineQueryResultBuilder AddRange(IEnumerable<InlineQueryResult> results)
    {
        ArgumentNullException.ThrowIfNull(results);

        foreach (var result in results)
        {
            AddExisting(result);
        }

        return this;
    }

    /// <summary>
    /// Adds an existing result to the builder.
    /// </summary>
    /// <param name="result">The result to add.</param>
    /// <returns>The builder for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="result"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when id is already used or exceeds length limits.</exception>
    public InlineQueryResultBuilder Add(InlineQueryResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        AddExisting(result);
        return this;
    }

    /// <summary>
    /// Enables or disables automatic validation.
    /// When enabled (default), Validate() must be called before Build().
    /// When disabled, results are built without validation.
    /// </summary>
    public InlineQueryResultBuilder WithValidation(bool enabled = true)
    {
        _validationEnabled = enabled;
        return this;
    }

    /// <summary>
    /// Validates the current state of the builder and returns a list of validation errors.
    /// </summary>
    /// <returns>List of validation error messages, empty if valid.</returns>
    /// <exception cref="InvalidOperationException">Thrown if validation fails with errors.</exception>
    public IReadOnlyList<string> Validate()
    {
        var errors = new List<string>();

        // Check result count
        if (_results.Count == 0)
        {
            errors.Add("Cannot build empty result set - add at least one result.");
        }
        else if (_results.Count > _maxResults)
        {
            errors.Add($"Result count ({_results.Count}) exceeds maximum allowed ({_maxResults}).");
        }

        // Check for duplicate IDs
        var idGroups = _results.GroupBy(r => r.ResultId).Where(g => g.Count() > 1);
        foreach (var group in idGroups)
        {
            errors.Add($"Duplicate result ID '{group.Key}' found {group.Count()} times.");
        }

        // Validate each result
        for (int i = 0; i < _results.Count; i++)
        {
            var result = _results[i];
            var resultErrors = ValidateSingle(result, i);
            errors.AddRange(resultErrors);
        }

        if (errors.Count > 0)
        {
            throw new InvalidOperationException(
                $"InlineQueryResultBuilder validation failed with {errors.Count} error(s):{Environment.NewLine}" +
                string.Join(Environment.NewLine, errors));
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates the current state of the builder.
    /// </summary>
    /// <returns>True if valid, false otherwise.</returns>
    public bool IsValid()
    {
        try
        {
            Validate();
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Builds and returns the collection of inline query results.
    /// When validation is enabled, this will throw if validation fails.
    /// </summary>
    /// <returns>Collection of inline query results.</returns>
    /// <exception cref="InvalidOperationException">Thrown if validation fails.</exception>
    public IList<InlineQueryResult> Build()
    {
        if (_validationEnabled)
        {
            Validate();
        }

        return _results.AsReadOnly();
    }

    // -------------------------------------------------------------------------
    // Private validation helpers
    // -------------------------------------------------------------------------

    private void AddExisting(InlineQueryResult result)
    {
        ValidateId(result.ResultId);
        ValidateTitle(result.Title);
        ValidateContent(result.Content);

        if (_usedIds.Contains(result.ResultId))
        {
            throw new ArgumentException($"Result ID '{result.ResultId}' is already in use.", nameof(result));
        }

        _usedIds.Add(result.ResultId);
        _results.Add(result);
    }

    private void ValidateId(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Result ID cannot be null or whitespace.", nameof(id));
        }

        var byteLength = System.Text.Encoding.UTF8.GetByteCount(id);
        if (byteLength > 64)
        {
            throw new ArgumentException(
                $"Result ID '{id}' is {byteLength} bytes, which exceeds Telegram's 64-byte limit.",
                nameof(id));
        }

        if (_usedIds.Contains(id))
        {
            throw new ArgumentException($"Result ID '{id}' is already in use.", nameof(id));
        }
    }

    private void ValidateTitle(string? title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Title cannot be null or whitespace.", nameof(title));
        }

        if (title.Length > 64)
        {
            throw new ArgumentException(
                $"Title '{title}' is {title.Length} characters, which exceeds Telegram's 64-character limit.",
                nameof(title));
        }
    }

    private void ValidateContent(string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException("Content cannot be null or whitespace.", nameof(content));
        }

        if (content.Length > 1024)
        {
            throw new ArgumentException(
                $"Content is {content.Length} characters, which exceeds Telegram's 1024-character limit.",
                nameof(content));
        }
    }

    private void ValidatePhotoUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ArgumentException("Photo URL cannot be null or whitespace.", nameof(url));
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out _))
        {
            throw new ArgumentException($"Invalid photo URL format: '{url}'", nameof(url));
        }
    }

    private void ValidateDocumentUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ArgumentException("Document URL cannot be null or whitespace.", nameof(url));
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out _))
        {
            throw new ArgumentException($"Invalid document URL format: '{url}'", nameof(url));
        }
    }

    private void ValidateVideoUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ArgumentException("Video URL cannot be null or whitespace.", nameof(url));
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out _))
        {
            throw new ArgumentException($"Invalid video URL format: '{url}'", nameof(url));
        }
    }

    private void ValidateAudioUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ArgumentException("Audio URL cannot be null or whitespace.", nameof(url));
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out _))
        {
            throw new ArgumentException($"Invalid audio URL format: '{url}'", nameof(url));
        }
    }

    private void ValidateThumbnailUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return;
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out _))
        {
            throw new ArgumentException($"Invalid thumbnail URL format: '{url}'", nameof(url));
        }
    }

    private void ValidateStickerUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ArgumentException("Sticker URL cannot be null or whitespace.", nameof(url));
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out _))
        {
            throw new ArgumentException($"Invalid sticker URL format: '{url}'", nameof(url));
        }
    }

    private IEnumerable<string> ValidateSingle(InlineQueryResult result, int index)
    {
        var errors = new List<string>();

        // Validate required fields
        if (string.IsNullOrWhiteSpace(result.ResultId))
        {
            errors.Add($"Result at index {index} has null or empty ResultId.");
        }
        else if (System.Text.Encoding.UTF8.GetByteCount(result.ResultId) > 64)
        {
            errors.Add($"Result at index {index} has ResultId exceeding 64 bytes.");
        }

        if (string.IsNullOrWhiteSpace(result.Title))
        {
            errors.Add($"Result at index {index} has null or empty Title.");
        }
        else if (result.Title.Length > 64)
        {
            errors.Add($"Result at index {index} has Title exceeding 64 characters (length: {result.Title.Length}).");
        }

        if (string.IsNullOrWhiteSpace(result.Content))
        {
            errors.Add($"Result at index {index} has null or empty Content.");
        }
        else if (result.Content.Length > 1024)
        {
            errors.Add($"Result at index {index} has Content exceeding 1024 characters (length: {result.Content.Length}).");
        }

        // Validate type-specific fields
        switch (result.Type)
        {
            case InlineQueryResultType.Article:
                // Already validated above
                break;

            case InlineQueryResultType.Photo:
                if (string.IsNullOrWhiteSpace(result.Content))
                {
                    errors.Add($"Photo result at index {index} has null or empty photo URL in Content.");
                }
                else if (!Uri.TryCreate(result.Content, UriKind.Absolute, out _))
                {
                    errors.Add($"Photo result at index {index} has invalid photo URL in Content: '{result.Content}'");
                }
                break;

            case InlineQueryResultType.Document:
                if (string.IsNullOrWhiteSpace(result.Content))
                {
                    errors.Add($"Document result at index {index} has null or empty document URL in Content.");
                }
                else if (!Uri.TryCreate(result.Content, UriKind.Absolute, out _))
                {
                    errors.Add($"Document result at index {index} has invalid document URL in Content: '{result.Content}'");
                }
                break;

            case InlineQueryResultType.Video:
                if (string.IsNullOrWhiteSpace(result.Content))
                {
                    errors.Add($"Video result at index {index} has null or empty video URL in Content.");
                }
                else if (!Uri.TryCreate(result.Content, UriKind.Absolute, out _))
                {
                    errors.Add($"Video result at index {index} has invalid video URL in Content: '{result.Content}'");
                }
                if (string.IsNullOrWhiteSpace(result.ThumbnailUrl))
                {
                    errors.Add($"Video result at index {index} has null or empty thumbnail URL.");
                }
                else if (!Uri.TryCreate(result.ThumbnailUrl, UriKind.Absolute, out _))
                {
                    errors.Add($"Video result at index {index} has invalid thumbnail URL: '{result.ThumbnailUrl}'");
                }
                break;

            case InlineQueryResultType.Audio:
                if (string.IsNullOrWhiteSpace(result.Content))
                {
                    errors.Add($"Audio result at index {index} has null or empty audio URL in Content.");
                }
                else if (!Uri.TryCreate(result.Content, UriKind.Absolute, out _))
                {
                    errors.Add($"Audio result at index {index} has invalid audio URL in Content: '{result.Content}'");
                }
                break;

            case InlineQueryResultType.Location:
                if (string.IsNullOrWhiteSpace(result.Content) || !result.Content.Contains(','))
                {
                    errors.Add($"Location result at index {index} has invalid coordinate format in Content: '{result.Content}'");
                }
                break;

            case InlineQueryResultType.Sticker:
                if (string.IsNullOrWhiteSpace(result.Content))
                {
                    errors.Add($"Sticker result at index {index} has null or empty sticker URL in Content.");
                }
                else if (!Uri.TryCreate(result.Content, UriKind.Absolute, out _))
                {
                    errors.Add($"Sticker result at index {index} has invalid sticker URL in Content: '{result.Content}'");
                }
                break;
        }

        return errors;
    }
}
