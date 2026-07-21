# InlineQueryResultBuilder

A builder for constructing a collection of `InlineQueryResult` objects to be returned in response to an inline query. It provides fluent methods for adding various result types (article, photo, document, video, audio, location, sticker) and supports optional validation of the built results before retrieval.

## API

### `public InlineQueryResultBuilder()`

Initializes a new instance of the builder with an empty internal list of results.

### `public static InlineQueryResultBuilder Create()`

Creates a new `InlineQueryResultBuilder` instance.  
**Returns:** A fresh builder ready to accept results.

### `public InlineQueryResultBuilder AddArticle(…)`

Adds an article result to the builder.  
**Parameters:** The exact parameters depend on the Telegram Bot API’s `InlineQueryResultArticle` (typically an ID, title, input message content, and optional fields).  
**Returns:** The same builder instance for chaining.  
**Throws:** `ArgumentNullException` if required parameters are null.

### `public InlineQueryResultBuilder AddPhoto(…)`

Adds a photo result to the builder.  
**Parameters:** As defined by `InlineQueryResultPhoto` (e.g., ID, photo URL, thumbnail URL).  
**Returns:** The builder instance.  
**Throws:** `ArgumentNullException` if required parameters are null.

### `public InlineQueryResultBuilder AddDocument(…)`

Adds a document result to the builder.  
**Parameters:** As defined by `InlineQueryResultDocument` (e.g., ID, title, document URL, MIME type).  
**Returns:** The builder instance.  
**Throws:** `ArgumentNullException` if required parameters are null.

### `public InlineQueryResultBuilder AddVideo(…)`

Adds a video result to the builder.  
**Parameters:** As defined by `InlineQueryResultVideo` (e.g., ID, video URL, MIME type, thumbnail URL).  
**Returns:** The builder instance.  
**Throws:** `ArgumentNullException` if required parameters are null.

### `public InlineQueryResultBuilder AddAudio(…)`

Adds an audio result to the builder.  
**Parameters:** As defined by `InlineQueryResultAudio` (e.g., ID, audio URL, title).  
**Returns:** The builder instance.  
**Throws:** `ArgumentNullException` if required parameters are null.

### `public InlineQueryResultBuilder AddLocation(…)`

Adds a location result to the builder.  
**Parameters:** As defined by `InlineQueryResultLocation` (e.g., ID, latitude, longitude, title).  
**Returns:** The builder instance.  
**Throws:** `ArgumentNullException` if required parameters are null.

### `public InlineQueryResultBuilder AddSticker(…)`

Adds a sticker result to the builder.  
**Parameters:** As defined by `InlineQueryResultSticker` (e.g., ID, sticker file ID).  
**Returns:** The builder instance.  
**Throws:** `ArgumentNullException` if required parameters are null.

### `public InlineQueryResultBuilder AddRange(IEnumerable<InlineQueryResult> results)`

Adds multiple pre‑constructed `InlineQueryResult` objects to the builder.  
**Parameters:** `results` – a collection of results to append.  
**Returns:** The builder instance.  
**Throws:** `ArgumentNullException` if `results` is null.

### `public InlineQueryResultBuilder Add(InlineQueryResult result)`

Adds a single `InlineQueryResult` object to the builder. This is a generic method for result types not covered by the dedicated `Add*` methods.  
**Parameters:** `result` – the result to add.  
**Returns:** The builder instance.  
**Throws:** `ArgumentNullException` if `result` is null.

### `public InlineQueryResultBuilder WithValidation()`

Enables validation of the built results. When validation is active, the `Validate` and `IsValid` properties reflect the state of the results.  
**Returns:** The builder instance.

### `public IReadOnlyList<string> Validate`

Gets the list of validation error messages for the current set of results. The list is empty if no errors are found. Validation must be enabled via `WithValidation()` before this property returns meaningful data; otherwise it may return an empty list.  
**Throws:** `InvalidOperationException` if validation has not been enabled.

### `public bool IsValid`

Gets whether the current set of results passes validation. Returns `true` if no validation errors exist, `false` otherwise.  
**Throws:** `InvalidOperationException` if validation has not been enabled.

### `public IList<InlineQueryResult> Build`

Gets the list of `InlineQueryResult` objects that have been added to the builder. The returned list is a snapshot of the current state; subsequent modifications to the builder do not affect the returned list.  
**Returns:** A read‑only list of the accumulated results.

## Usage

### Example 1: Building a simple set of results with validation

```csharp
var builder = InlineQueryResultBuilder.Create()
    .AddArticle("1", "Article Title", new InputTextMessageContent("Hello"))
    .AddPhoto("2", "https://example.com/photo.jpg", "https://example.com/thumb.jpg")
    .WithValidation();

if (builder.IsValid)
{
    IList<InlineQueryResult> results = builder.Build;
    // Send results via Telegram API
}
else
{
    foreach (string error in builder.Validate)
    {
        Console.WriteLine($"Validation error: {error}");
    }
}
```

### Example 2: Adding results from an external source and using generic Add

```csharp
var builder = new InlineQueryResultBuilder();

// Add a pre‑constructed result
var videoResult = new InlineQueryResultVideo
{
    Id = "vid1",
    VideoUrl = "https://example.com/video.mp4",
    MimeType = "video/mp4",
    ThumbUrl = "https://example.com/thumb.jpg"
};
builder.Add(videoResult);

// Add a range of results from a collection
var moreResults = new List<InlineQueryResult>
{
    new InlineQueryResultArticle { Id = "a1", Title = "First", InputMessageContent = new InputTextMessageContent("A") },
    new InlineQueryResultArticle { Id = "a2", Title = "Second", InputMessageContent = new InputTextMessageContent("B") }
};
builder.AddRange(moreResults);

IList<InlineQueryResult> allResults = builder.Build;
```

## Notes

- The builder is **not thread‑safe**. Concurrent access from multiple threads must be synchronized externally.
- Validation is an opt‑in feature. Calling `WithValidation()` enables it; without it, `Validate` and `IsValid` throw `InvalidOperationException`.
- The `Build` property returns a snapshot of the results at the time of access. Adding or removing results after calling `Build` does not affect the previously obtained list.
- All `Add*` methods return the same builder instance, allowing fluent chaining.
- The `Add` method accepts any `InlineQueryResult` subtype, including custom implementations, but the dedicated `Add*` methods provide compile‑time safety for the most common result types.
- If a required parameter is omitted (e.g., null ID), the corresponding `Add*` method throws `ArgumentNullException`. Validation, when enabled, catches additional semantic errors (e.g., duplicate IDs, missing required fields).
