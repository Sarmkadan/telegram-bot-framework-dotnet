# XmlFormatter

The `XmlFormatter` is a utility class that serializes .NET objects and messages into XML strings. It is designed to provide consistent XML formatting for structured data, error messages, and collections of messages, making it suitable for logging, telemetry, or inter-process communication scenarios where XML is the required payload format.

## API

### `public XmlFormatter()`

Initializes a new instance of the `XmlFormatter` class with default XML serialization settings.

### `public string Format<T>(T value)`

Serializes the specified object of type `T` into an XML string.

- **Parameters**
  - `value` – The object to serialize.
- **Return value**
  - A string containing the XML representation of the object.
- **Exceptions**
  - Throws `ArgumentNullException` if `value` is `null`.
  - Throws `InvalidOperationException` if XML serialization fails.

### `public string Format<T>(IEnumerable<T> values)`

Serializes a sequence of objects of type `T` into an XML string containing a root element with multiple child elements.

- **Parameters**
  - `values` – The sequence of objects to serialize.
- **Return value**
  - A string containing the XML representation of the sequence.
- **Exceptions**
  - Throws `ArgumentNullException` if `values` is `null`.
  - Throws `InvalidOperationException` if XML serialization fails.

### `public string FormatError(Exception error)`

Serializes an exception into a structured XML string containing error details such as message, stack trace, and inner exceptions.

- **Parameters**
  - `error` – The exception to serialize.
- **Return value**
  - A string containing the XML representation of the exception.
- **Exceptions**
  - Throws `ArgumentNullException` if `error` is `null`.

### `public string FormatMessage(object message)`

Serializes a message object into an XML string. The root element name is inferred from the object’s type name.

- **Parameters**
  - `message` – The message object to serialize.
- **Return value**
  - A string containing the XML representation of the message.
- **Exceptions**
  - Throws `ArgumentNullException` if `message` is `null`.

### `public string FormatMessages(IEnumerable<object> messages)`

Serializes a sequence of message objects into an XML string with a root element containing multiple child elements, each named after the respective message type.

- **Parameters**
  - `messages` – The sequence of message objects to serialize.
- **Return value**
  - A string containing the XML representation of the sequence.
- **Exceptions**
  - Throws `ArgumentNullException` if `messages` is `null`.

## Usage

```csharp
// Example 1: Formatting a single object
var user = new User { Id = 42, Name = "Alice" };
var formatter = new XmlFormatter();
string xml = formatter.Format(user);
Console.WriteLine(xml);
// Output:
// <User xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
//   <Id>42</Id>
//   <Name>Alice</Name>
// </User>
```

```csharp
// Example 2: Formatting a collection of messages
var messages = new object[]
{
    new TextMessage { Text = "Hello" },
    new ImageMessage { Url = "https://example.com/image.png" }
};
var formatter = new XmlFormatter();
string xml = formatter.FormatMessages(messages);
Console.WriteLine(xml);
// Output:
// <ArrayOfObject>
//   <TextMessage>
//     <Text>Hello</Text>
//   </TextMessage>
//   <ImageMessage>
//     <Url>https://example.com/image.png</Url>
//   </ImageMessage>
// </ArrayOfObject>
```

## Notes

- The XML output uses the default `XmlSerializer` settings, which may affect formatting and namespace inclusion.
- Serialization is not thread-safe; each thread should use its own instance of `XmlFormatter` or ensure external synchronization.
- Circular references in objects will cause `InvalidOperationException` during serialization.
- For large object graphs, consider streaming or buffering strategies to avoid high memory usage.
- The root element names are derived from the type names of the objects being serialized.
