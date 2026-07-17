# XmlFormatterJsonExtensions

Provides JSON serialization and deserialization support for `XmlFormatter` instances, along with configurable formatting options. The class combines static conversion methods with instance properties that control output behavior, enabling consistent JSON representation of XML formatter settings across an application.

## API

### Static Methods

#### `public static string ToJson(this XmlFormatter formatter)`

Converts the specified `XmlFormatter` to its JSON string representation.

- **Parameters**  
  `formatter` – The `XmlFormatter` instance to serialize. Must not be `null`.

- **Returns**  
  A JSON string that represents the state of the `formatter`.

- **Throws**  
  `ArgumentNullException` if `formatter` is `null`.

#### `public static XmlFormatter? FromJson(string json)`

Deserializes a JSON string into an `XmlFormatter` instance.

- **Parameters**  
  `json` – A JSON string produced by `ToJson`. Must not be `null` or empty.

- **Returns**  
  A new `XmlFormatter` instance if deserialization succeeds; otherwise `null`.

- **Throws**  
  `ArgumentNullException` if `json` is `null`.  
  `JsonException` if the JSON is malformed or cannot be mapped to an `XmlFormatter`.

#### `public static bool TryFromJson(string json, out XmlFormatter? result)`

Attempts to deserialize a JSON string into an `XmlFormatter` instance without throwing exceptions.

- **Parameters**  
  `json` – A JSON string to deserialize. Must not be `null`.  
  `result` – When this method returns, contains the deserialized `XmlFormatter` if successful, or `null` if deserialization failed.

- **Returns**  
  `true` if the JSON was successfully deserialized; otherwise `false`.

- **Throws**  
  `ArgumentNullException` if `json` is `null`.

### Instance Properties

#### `public bool Pretty { get; set; }`

Gets or sets a value indicating whether the JSON output should be indented and formatted for readability.

- **Value**  
  `true` to produce pretty-printed JSON; `false` for compact output. Default is `false`.

#### `public XmlFormatterConfiguration Configuration { get; set; }`

Gets or sets the configuration object that controls serialization behavior (e.g., naming policies, handling of null values).

- **Value**  
  An `XmlFormatterConfiguration` instance. Setting this property to `null` restores the default configuration.

## Usage

### Basic serialization and deserialization

```csharp
using Telegram.Bot.Framework.Utilities;

// Create an XmlFormatter with custom settings
var formatter = new XmlFormatter
{
    Indent = true,
    OmitXmlDeclaration = false
};

// Serialize to JSON using the static extension method
string json = formatter.ToJson();
Console.WriteLine(json);

// Deserialize back
XmlFormatter? restored = XmlFormatterJsonExtensions.FromJson(json);
if (restored != null)
{
    Console.WriteLine(restored.Indent); // True
}
```

### Configuring output with instance properties

```csharp
using Telegram.Bot.Framework.Utilities;

// Create an instance of the extensions class to hold configuration
var config = new XmlFormatterJsonExtensions
{
    Pretty = true,
    Configuration = new XmlFormatterConfiguration
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    }
};

// Use the static methods – they respect the global/default configuration
// (In practice, the static methods may use a shared instance or require passing the config)
// For this example, assume the static methods use the last set configuration:
var formatter = new XmlFormatter();
string json = formatter.ToJson(); // Uses Pretty = true and camelCase naming
Console.WriteLine(json);

// Try deserialization with error handling
if (XmlFormatterJsonExtensions.TryFromJson(json, out var result))
{
    // Use result
}
else
{
    Console.WriteLine("Deserialization failed.");
}
```

## Notes

- **Thread safety**  
  The static methods are thread-safe. Instance properties (`Pretty`, `Configuration`) are not synchronized; if the same `XmlFormatterJsonExtensions` instance is accessed concurrently from multiple threads, external synchronization is required.

- **Null handling**  
  All static methods throw `ArgumentNullException` when required string parameters are `null`. The `TryFromJson` method does not throw on deserialization failure, but still throws on a `null` JSON argument.

- **Configuration defaults**  
  When `Configuration` is set to `null`, the default `XmlFormatterConfiguration` is used. The default configuration typically uses `CamelCase` property naming and includes null values.

- **Edge cases**  
  - An empty JSON string (`""`) causes `FromJson` to return `null` and `TryFromJson` to return `false`.
  - Serializing an `XmlFormatter` with default settings produces a JSON string that, when deserialized, yields an equivalent (but not necessarily identical) instance.
  - The `Pretty` property only affects output when the static methods are called; it has no effect on deserialization.
