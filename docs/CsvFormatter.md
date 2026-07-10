# CsvFormatter
The `CsvFormatter` type provides utilities for converting objects into CSV‑formatted strings. It is intended for use within the Telegram bot framework to serialize messages, updates, or error information into a simple comma‑separated representation that can be logged, transmitted, or stored.

## API
### Format<T>(T item)
Formats a single instance of type `T` as a CSV row.  
**Parameters**  
- `item`: The object to format.  
**Return value**  
A string containing the CSV representation of `item`’s public properties, with values escaped as needed.  
**Exceptions**  
- `ArgumentNullException` if `item` is `null`.  
- `InvalidOperationException` if `T` has no accessible public properties to serialize.

### Format<T>(IEnumerable<T> items)
Formats a sequence of `T` instances as CSV, including a header line.  
**Parameters**  
- `items`: The collection of objects to format.  
**Return value**  
A string with a header row (property names) followed by one CSV row per item in `items`.  
**Exceptions**  
- `ArgumentNullException` if `items` is `null`.  
- `InvalidOperationException` if the element type `T` lacks accessible public properties.

### FormatError
Gets or sets the format string used when rendering error messages as CSV.  
**Return value**  
The current error‑message format string.  
**Exceptions**  
None.

### FormatMessage
Gets or sets the format string used when rendering regular messages as CSV.  
**Return value**  
The current message format string.  
**Exceptions**  
None.

### FormatMessages
Gets or sets the format string used when rendering a collection of messages as CSV.  
**Return value**  
The current messages format string.  
**Exceptions**  
None.

## Usage
```csharp
using TelegramBotFrameworkDotnet.Formatting;

// Example 1: format a single update
var update = new Update { Id = 123, Type = UpdateType.Message, Timestamp = DateTime.UtcNow };
string csv = CsvFormatter.Format<Update>(update);
// csv => "123,Message,2025-09-24T12:34:56Z"

// Example 2: format a list of updates with a header
IEnumerable<Update> updates = GetUpdatesFromSource();
string csvBatch = CsvFormatter.Format<Update>(updates);
// csvBatch =>
// "Id,Type,Timestamp\r\n123,Message,2025-09-24T12:34:56Z\r\n456,CallbackQuery,2025-09-24T12:35:01Z"
```

## Notes
- If `item` or any element in `items` is `null`, the formatter treats the corresponding property value as an empty field.  
- Types with complex properties (e.g., nested objects or collections) are serialized using their `ToString()` result; overriding `ToString()` is recommended for meaningful output.  
- The format string properties (`FormatError`, `FormatMessage`, `FormatMessages`) are intended for advanced scenarios where a custom CSV layout (such as quoting or alternative delimiters) is required. Changing these properties affects all subsequent formatting calls.  
- The formatter does not maintain internal state beyond the format strings; therefore, multiple threads can safely invoke the `Format<T>` overloads concurrently. However, concurrent modification of the format string properties requires external synchronization to avoid race conditions.
