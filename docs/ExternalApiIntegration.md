# ExternalApiIntegration

`ExternalApiIntegration` provides a structured client for interacting with third-party REST APIs within the telegram-bot-framework-dotnet ecosystem, abstracting HTTP request execution, serialization, and response handling to simplify integration workflows.

## API

### Constructors
- `ExternalApiIntegration()`
  Initializes a new instance of the `ExternalApiIntegration` class.

### Methods

- `async Task<T?> GetAsync<T>(string url)`
  Performs an asynchronous HTTP GET request to the specified URL and deserializes the JSON response into an object of type `T`. Returns `null` if the request fails, the response status code is not successful, or if deserialization results in an empty object.

- `async Task<bool> PostAsync<TRequest>(string url, TRequest payload)`
  Performs an asynchronous HTTP POST request to the specified URL, serializing the provided `payload` object as JSON in the request body. Returns `true` if the server returns a successful status code, otherwise returns `false`.

- `async Task<string?> GetWithHeadersAsync(string url, IDictionary<string, string> headers)`
  Performs an asynchronous HTTP GET request to the specified URL with the provided custom headers, returning the full response body as a string. Returns `null` if the request fails or the response cannot be read.

- `static T? ParseResponse<T>(string json)`
  A static utility method that deserializes a provided JSON string into an object of type `T`. Returns `null` if the JSON string is empty, malformed, or does not map to type `T`.

## Usage

### Fetching data from an API
```csharp
var client = new ExternalApiIntegration();
var userProfile = await client.GetAsync<UserProfile>("https://api.example.com/users/123");

if (userProfile != null)
{
    Console.WriteLine($"User: {userProfile.Name}");
}
```

### Posting data to an API
```csharp
var client = new ExternalApiIntegration();
var newLog = new { Level = "Info", Message = "System initialized" };

bool success = await client.PostAsync("https://api.example.com/logs", newLog);

if (success)
{
    Console.WriteLine("Log entry created successfully.");
}
```

## Notes

- **Error Handling:** All asynchronous methods are designed to handle common HTTP exceptions internally. If a network request fails or the remote server returns an error status code, the methods will return a default value (e.g., `null` or `false`) rather than throwing exceptions, unless otherwise specified.
- **Deserialization:** `GetAsync<T>` and `ParseResponse<T>` rely on standard JSON serialization conventions. Ensure that the target types have appropriate parameterless constructors or property mapping configurations compatible with the internal serializer.
- **Thread Safety:** Instances of `ExternalApiIntegration` are thread-safe and intended to be used as long-lived clients. It is recommended to register the class as a singleton or scoped service within a dependency injection container to efficiently manage underlying HTTP connection pools.
