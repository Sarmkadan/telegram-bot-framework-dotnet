# InlineQuery
The `InlineQuery` type represents an inline query received from a user, providing information about the query itself, the user who sent it, and the status of the query. It allows for the storage and retrieval of metadata associated with the query, as well as the validation of the query and calculation of its processing duration.

## API
* `QueryId`: A unique identifier for the inline query.
* `UserId`: The identifier of the user who sent the inline query.
* `Query`: The text of the inline query.
* `Offset`: The offset of the results for the inline query.
* `Status`: The current status of the inline query.
* `ReceivedAt`: The date and time when the inline query was received.
* `AnsweredAt`: The date and time when the inline query was answered, or `null` if it has not been answered yet.
* `Metadata`: A dictionary of metadata associated with the inline query, or `null` if no metadata is available.
* `SetMetadata(Dictionary<string, object> metadata)`: Sets the metadata associated with the inline query.
* `GetMetadata(string key)`: Retrieves the metadata value associated with the specified key, or `null` if no such metadata exists.
* `Validate()`: Validates the inline query and returns a boolean indicating whether the validation was successful.
* `GetProcessingDurationMs()`: Calculates the processing duration of the inline query in milliseconds.
* `ResultId`: A unique identifier for the result of the inline query.
* `Type`: The type of the result of the inline query.
* `Title`: The title of the result of the inline query.
* `Description`: The description of the result of the inline query, or `null` if no description is available.
* `Content`: The content of the result of the inline query.
* `ThumbnailUrl`: The URL of the thumbnail image for the result of the inline query, or `null` if no thumbnail is available.
* `CustomPayload`: The custom payload associated with the result of the inline query, or `null` if no custom payload is available.
* `GeneratedAt`: The date and time when the result of the inline query was generated.

## Usage
```csharp
// Example 1: Creating and validating an inline query
var inlineQuery = new InlineQuery
{
    QueryId = "12345",
    UserId = 67890,
    Query = "example query",
    Offset = "0",
    Status = InlineQueryStatus.Pending,
    ReceivedAt = DateTime.Now,
    AnsweredAt = null,
    Metadata = new Dictionary<string, object>
    {
        { "key1", "value1" },
        { "key2", "value2" }
    }
};

if (inlineQuery.Validate())
{
    Console.WriteLine("Inline query is valid");
}
else
{
    Console.WriteLine("Inline query is invalid");
}

// Example 2: Retrieving and updating metadata
var metadataValue = inlineQuery.GetMetadata("key1");
Console.WriteLine($"Metadata value: {metadataValue}");

inlineQuery.SetMetadata(new Dictionary<string, object>
{
    { "key3", "value3" }
});

metadataValue = inlineQuery.GetMetadata("key3");
Console.WriteLine($"Updated metadata value: {metadataValue}");
```

## Notes
The `InlineQuery` type is not thread-safe, and its members should not be accessed concurrently from multiple threads. The `Metadata` dictionary is not guaranteed to be persisted across different instances of the `InlineQuery` type, and its contents should be treated as volatile. The `Validate` method may throw exceptions if the inline query is invalid, and its return value should be checked carefully to avoid unexpected behavior. The `GetProcessingDurationMs` method may return inaccurate results if the inline query has not been processed yet, and its return value should be used with caution.
