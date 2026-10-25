# InlineQueryResultBuilderExtensions

`InlineQueryResultBuilderExtensions` is a static class of extension methods on `InlineQueryResultBuilder` (namespace `TelegramBotFramework.Keyboard`). Each method adds several inline query results in one call, so callers do not need to write a loop around `AddArticle`, `AddPhoto`, or `AddDocument`. The source is in `src/TelegramBotFramework/Keyboard/InlineQueryResultBuilderExtensions.cs`.

All methods return the same `InlineQueryResultBuilder` instance, so they chain with the builder's other methods (`AddArticle`, `AddPhoto`, `AddVideo`, `Build`, and so on). The extensions do not add any state of their own. Every result is created by calling the corresponding single-item method on the builder, so the builder's validation applies to each item.

## Purpose

Inline query answers often contain many results built from the same template: a list of articles with one message body, a set of photos with shared thumbnails, a set of documents named by pattern, or a grid of images. These methods handle the repetitive part:

- They pair each id with its URL, title, or caption.
- They generate per-item values (a file name from an index, or a grid id from row and column).
- They delegate every addition to the builder, so id, title, content, and URL checks run as usual.

## Public methods

### AddArticles

```csharp
public static InlineQueryResultBuilder AddArticles(
    this InlineQueryResultBuilder builder,
    IEnumerable<string> ids,
    string title,
    string content,
    string? description = null,
    string? thumbnailUrl = null)
```

Adds one article result per id. Every article uses the same `title`, `content`, `description`, and `thumbnailUrl`. The ids are the only per-item value.

| Parameter | Description |
|-----------|-------------|
| `builder` | The builder to add to. |
| `ids` | Unique result id for each article. |
| `title` | Title shown in the results list. |
| `content` | Message text sent to the chat when the result is chosen. |
| `description` | Optional text shown below the title. |
| `thumbnailUrl` | Optional thumbnail URL, applied to every article. |

Throws `ArgumentNullException` if `builder`, `ids`, `title`, or `content` is `null`. Throws `ArgumentException` (from the builder) if an id is already used or exceeds the length limits.

### AddPhotos

```csharp
public static InlineQueryResultBuilder AddPhotos(
    this InlineQueryResultBuilder builder,
    IEnumerable<string> ids,
    IEnumerable<string> photoUrls,
    IEnumerable<string?>? captions = null,
    string? thumbnailUrl = null)
```

Adds one photo result per pair of `ids` and `photoUrls`. Captions are matched by position. If `captions` is shorter than the other lists, the remaining items get no caption. Extra captions are ignored.

| Parameter | Description |
|-----------|-------------|
| `ids` | Unique result id for each photo. |
| `photoUrls` | Photo file URL for each id. |
| `captions` | Optional caption for each photo, by position. |
| `thumbnailUrl` | Optional thumbnail URL, applied to every photo. |

Iteration stops at the shorter of `ids` and `photoUrls`. Throws `ArgumentNullException` if `builder`, `ids`, or `photoUrls` is `null`. Throws `ArgumentException` if an id is already used, or if a URL is not valid.

### AddDocuments

```csharp
public static InlineQueryResultBuilder AddDocuments(
    this InlineQueryResultBuilder builder,
    IEnumerable<string> ids,
    IEnumerable<string> documentUrls,
    IEnumerable<string> titles,
    string fileNamePattern,
    IEnumerable<string?>? descriptions = null,
    string? thumbnailUrl = null)
```

Adds one document result per id, with a title from `titles` and a file name built from `fileNamePattern`. The file name is `string.Format(CultureInfo.InvariantCulture, fileNamePattern, index)`, where `index` is the zero-based position of the id in `ids`. For example, the pattern `"document_{0}.pdf"` gives `document_0.pdf` for the first id.

| Parameter | Description |
|-----------|-------------|
| `ids` | Unique result id for each document. |
| `documentUrls` | Document file URL for each id. |
| `titles` | Title for each document. |
| `fileNamePattern` | Format string with a `{0}` placeholder for the index. |
| `descriptions` | Optional description for each document, by position. |
| `thumbnailUrl` | Optional thumbnail URL, applied to every document. |

Iteration stops at the shortest of `ids`, `documentUrls`, and `titles`. Throws `ArgumentNullException` if `builder`, `ids`, `documentUrls`, `titles`, or `fileNamePattern` is `null`. Throws `ArgumentException` for duplicate ids, invalid URLs, or length limits.

### AddPhotoGrid

```csharp
public static InlineQueryResultBuilder AddPhotoGrid(
    this InlineQueryResultBuilder builder,
    string baseId,
    IEnumerable<string> photoUrls,
    int rows,
    int columns,
    Func<int, int, string?>? captionProvider = null,
    string? thumbnailUrl = null)
```

Adds up to `rows * columns` photo results, laid out in row-major order. The photo at index `i` goes to row `i / columns` and column `i % columns`. Each result id is `{baseId}_{row}_{col}`, with zero-based indices. If there are fewer photos than cells, the grid stops at the last photo.

| Parameter | Description |
|-----------|-------------|
| `baseId` | Prefix for every generated id, such as `"photo_grid"`. |
| `photoUrls` | Photo file URLs, filled into the grid in order. Extra URLs are not used. |
| `rows` | Number of rows. Must be at least 1. |
| `columns` | Number of columns. Must be at least 1. |
| `captionProvider` | Optional function `(row, col) => caption`, called for each filled cell. |
| `thumbnailUrl` | Optional thumbnail URL, applied to every photo. |

Throws `ArgumentNullException` if `builder`, `baseId`, or `photoUrls` is `null`. Throws `ArgumentOutOfRangeException` if `rows` or `columns` is less than 1. Throws `ArgumentException` if a generated id is too long or a URL is not valid.

## Limits

The builder enforces these limits (from `InlineQueryResultBuilderConstants`) when results are added or built:

| Limit | Value | Enforced by |
|-------|-------|-------------|
| Results per query | 50 (`MaxResultsLimit`) | `Validate()` and `Build()` when validation is enabled. The builder constructor also rejects a `maxResults` outside 1 to 50. |
| Id length | 64 bytes (`MaxIdLengthBytes`) | Each single-item add method. |
| Title length | 64 characters (`MaxTitleLength`) | Each single-item add method. |
| Content length | 1024 characters (`MaxContentLength`) | `AddArticle`. |

Photo, document, and other URLs are checked by the builder's URL validation for each item.

## Behavior notes

- **Partial results on error.** The methods add items one at a time. If an item fails validation, the exception is thrown and the items added before it stay in the builder. Callers that need an all-or-nothing result should discard the builder on exception.
- **Duplicate ids.** Ids must be unique across the whole builder, not only within one call. The builder throws on the first duplicate.
- **Repeated enumeration.** `AddDocuments` enumerates `ids` more than once (it uses `ids.ToArray()` to find each index). Pass a materialized collection such as an array or `List<T>`. A lazy sequence that is expensive or non-repeatable will be evaluated repeatedly.
- **Caption as title.** `AddPhotos` sends the caption to the builder's `AddPhoto` method, which uses it as both the title and the description.
- **Document description fallback.** `AddDocuments` sets each result's description to the built file name when `descriptions` has no value for that item. The builder's `AddDocument` method applies this fallback.
- **Grid ids.** Ids from `AddPhotoGrid` use the `{baseId}_{row}_{col}` pattern, so two grids with different `baseId` values do not collide.

## Usage

```csharp
using TelegramBotFramework.Keyboard;

var galleryUrls = new[]
{
    "https://example.com/images/1.jpg",
    "https://example.com/images/2.jpg",
    "https://example.com/images/3.jpg",
    "https://example.com/images/4.jpg",
    "https://example.com/images/5.jpg",
    "https://example.com/images/6.jpg"
};

var results = InlineQueryResultBuilder.Create()
    .AddArticles(
        ids: new[] { "digest-today", "digest-week" },
        title: "Digest",
        content: "The latest digest is on the project page.",
        description: "Send a link to the digest")
    .AddPhotos(
        ids: new[] { "logo-light", "logo-dark" },
        photoUrls: new[]
        {
            "https://example.com/logo/light.png",
            "https://example.com/logo/dark.png"
        },
        captions: new[] { "Light theme", "Dark theme" })
    .AddDocuments(
        ids: new[] { "spec-1", "spec-2" },
        documentUrls: new[]
        {
            "https://example.com/files/spec-1.pdf",
            "https://example.com/files/spec-2.pdf"
        },
        titles: new[] { "Spec part 1", "Spec part 2" },
        fileNamePattern: "spec_{0}.pdf")
    .AddPhotoGrid(
        baseId: "gallery",
        photoUrls: galleryUrls,
        rows: 2,
        columns: 3,
        captionProvider: (row, col) => $"Row {row + 1}, column {col + 1}")
    .Build();

// results is an IList<InlineQueryResult> with 2 + 2 + 2 + 6 = 12 items.
// Pass it to your inline query answer call.
```

Ids in the example (`digest-today`, `logo-light`, `spec-1`, `gallery_0_0`, and so on) are all distinct, as the builder requires. `Build()` returns the results in the order they were added.

## Related types

- `InlineQueryResultBuilder` (`Keyboard/InlineQueryResultBuilder.cs`): the single-item methods (`AddArticle`, `AddPhoto`, `AddDocument`, `AddVideo`, `AddAudio`, `AddLocation`, `AddSticker`, `AddRange`, `Add`), plus `WithValidation`, `Validate`, `IsValid`, and `Build`.
- `InlineQueryResultBuilderConstants` (`Keyboard/InlineQueryResultBuilderConstants.cs`): the limits listed above.
