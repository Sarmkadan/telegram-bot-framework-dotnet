# JsonFormatter

`JsonFormatter` is a utility type responsible for serializing framework objects into their JSON string representations. It provides a consistent formatting surface for individual messages, sequences of messages, error details, and generic payloads, enabling structured logging, debugging output, or external consumption of bot activity data.

## API

### `public JsonFormatter()`

Default parameterless constructor. Creates a new `JsonFormatter` instance ready to produce JSON output. No configuration or dependencies are required.

- **Parameters:** none.
- **Returns:** a new `JsonFormatter` object.
- **Throws:** nothing.

### `public string Format<T>(T value)`

Serializes a single value of type `T` to its JSON representation.

- **Parameters:**
  - `value` (`T`): The object to format. Can be `null`; the formatter will produce a JSON `null` literal.
- **Returns:** `string` — the JSON string corresponding to `value`.
- **Throws:** may throw if the underlying serializer encounters a type that cannot be safely serialized (e.g., self-referencing object graphs, types with non-serializable members). No explicit validation is performed on `T` before serialization begins.

### `public string Format<T>(IEnumerable<T> values)`

Serializes a sequence of values of type `T` to a JSON array string.

- **Parameters:**
  - `values` (`IEnumerable<T>`): The collection of items to format. Can be `null` or empty; a `null` sequence typically produces a JSON `null` literal, while an empty sequence produces an empty JSON array `[]`.
- **Returns:** `string` — the JSON array string containing each element serialized in order.
- **Throws:** may throw for the same serialization reasons as the single-value overload. If `values` is not `null` but contains elements that fail to serialize, the exception surfaces during enumeration.

### `public string FormatError(Exception exception)`

Produces a JSON representation of an exception, typically including its type name, message, and optionally stack trace or inner exception details.

- **Parameters:**
  - `exception` (`Exception`): The error object to format. Must not be `null`; passing `null` results in a `NullReferenceException` or an explicit `ArgumentNullException`.
- **Returns:** `string` — a JSON object string describing the error.
- **Throws:** `ArgumentNullException` if `exception` is `null`. May also throw if the exception object contains non-serializable data in its custom properties or `Data` dictionary.

### `public string FormatMessage(Message message)`

Serializes a single bot message object to its JSON representation. This is the primary entry point for converting a received or outgoing message into structured text.

- **Parameters:**
  - `message` (`Message`): The message instance to format. Must not be `null`; passing `null` results in a `NullReferenceException` or an explicit `ArgumentNullException`.
- **Returns:** `string` — the JSON string for the message, including all relevant fields such as ID, text, sender information, and timestamps.
- **Throws:** `ArgumentNullException` if `message` is `null`. Serialization exceptions may occur if the message graph contains unsupported types.

### `public string FormatMessages(IEnumerable<Message> messages)`

Serializes a sequence of bot messages to a JSON array string.

- **Parameters:**
  - `messages` (`IEnumerable<Message>`): The collection of messages to format. Can be `null` or empty; a `null` sequence produces a JSON `null` literal, while an empty sequence produces `[]`.
- **Returns:** `string` — the JSON array string containing each message serialized in order.
- **Throws:** `ArgumentNullException` if `messages` is `null`. Individual message serialization failures will surface during enumeration.

## Usage

### Example 1: Formatting a single incoming message for logging

```csharp
var formatter = new JsonFormatter();

Message incomingMessage = await bot.GetNextUpdateAsync();
string json = formatter.FormatMessage(incomingMessage);

Console.WriteLine($"Received: {json}");
```

### Example 2: Formatting a batch of messages and an error

```csharp
var formatter = new JsonFormatter();

try
{
    var messages = await bot.FetchPendingMessagesAsync();
    string batchJson = formatter.FormatMessages(messages);
    await File.WriteAllTextAsync("pending.json", batchJson);
}
catch (Exception ex)
{
    string errorJson = formatter.FormatError(ex);
    Console.Error.WriteLine($"Processing failed: {errorJson}");
}
```

## Notes

- **Null handling:** `FormatMessage` and `FormatError` treat `null` arguments as programmer error and throw immediately. The generic `Format<T>` overloads accept `null` values and serialize them as JSON `null`; a `null` sequence argument likewise produces a JSON `null` literal rather than an empty array.
- **Empty sequences:** Passing an empty `IEnumerable<T>` or `IEnumerable<Message>` yields a valid JSON array string (`[]`), not `null`.
- **Thread safety:** `JsonFormatter` holds no mutable instance state and relies on the thread safety guarantees of the underlying serializer. Multiple threads can safely call any formatting method on the same instance concurrently, provided the objects being serialized are not mutated during the call.
- **Serialization depth:** The formatter does not impose explicit depth limits. Circular references or excessively deep object graphs will cause the underlying serializer to throw; callers should ensure that `Message` objects and custom generic types are acyclic.
- **Custom types:** When using `Format<T>` with user-defined types, ensure they are composed of serializable members. Types exposing `IDictionary` with non-string keys, `IntPtr`, or delegate fields may cause runtime serialization failures.
