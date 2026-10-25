# FileDownloadHelper

Static helper that downloads a file that a user sent to the bot (photo, document, audio, and so on) from Telegram's file servers and saves it to a local path. It first resolves the `file_id` through `ITelegramApiClient.GetFileAsync` to get the server-side `file_path` and size, then fetches the bytes from `https://api.telegram.org/file/bot{token}/{file_path}`. The class is `public static` and stateless, so it is safe to call from any thread.

Source: `src/TelegramBotFramework/Utilities/FileDownloadHelper.cs`

## Purpose

Telegram does not send file contents in updates; it sends a `file_id`. Turning that identifier into bytes on disk takes two HTTP calls with the bot token, plus a size check and directory setup. This helper wraps those steps so a handler can save an upload in one call.

## API

### `public static Task<bool> DownloadFileAsync(ITelegramApiClient apiClient, string fileId, string destinationPath, long maxSizeBytes = 0, CancellationToken cancellationToken = default)`
Downloads the file identified by `fileId` and writes it to `destinationPath`.

- **Parameters:**
  - `apiClient` (`ITelegramApiClient`) - The API client used to resolve the file. The bot token is read from it, so it must be the framework's `TelegramApiClient` (see [Limitations](#limitations)).
  - `fileId` (string) - The file identifier from the incoming message (for example, `Document.FileId`). Must not be empty or whitespace.
  - `destinationPath` (string) - Full path of the target file. Must not be empty or whitespace. Missing parent directories are created. An existing file at this path is overwritten.
  - `maxSizeBytes` (long) - Upper limit on the file size reported by Telegram, in bytes. `0` (the default) disables the check.
  - `cancellationToken` (CancellationToken) - Cancels the lookup and the download.
- **Return value:** `true` if the file was written; `false` otherwise. Failures are reported through the return value and `Console.Error`, not by exceptions.
- **Throws:**
  - `ArgumentNullException` if `apiClient` is `null`.
  - `ArgumentException` if `fileId` or `destinationPath` is empty or whitespace. These checks run before any network call.

### `private static string GetBotTokenFromClient(ITelegramApiClient apiClient)`
Internal helper, not part of the public API. It reads the private `_botToken` field of `TelegramApiClient` through reflection and throws `InvalidOperationException` for any other `ITelegramApiClient` implementation. The exception is caught inside `DownloadFileAsync` and turned into a `false` result.

## Processing steps

1. **Resolve the file.** Calls `GetFileAsync(fileId)`. If the result has no value (the Telegram call failed), writes `Failed to get file info for file_id: {fileId}` to `Console.Error` and returns `false`. The failure description from the API result is not logged.
2. **Check the size.** When `maxSizeBytes > 0` and the `FileSize` reported by Telegram is greater than the limit, writes the sizes to `Console.Error` and returns `false` before any download starts. The check uses the size Telegram reports; the bytes actually received are not counted.
3. **Build the URL.** `https://api.telegram.org/file/bot{token}/{file_path}`.
4. **Download.** Sends a `GET` with a new `HttpClient`. A non-success HTTP status writes the status code and `file_path` to `Console.Error` and returns `false`.
5. **Save.** Creates the parent directory if it does not exist, reads the whole response body into memory, and writes it with `File.WriteAllBytesAsync`. Returns `true`.

### Error handling

| Situation | Result |
|---|---|
| `apiClient` is `null`, or `fileId`/`destinationPath` is blank | Throws (`ArgumentNullException` / `ArgumentException`) |
| `GetFileAsync` returns no value | `false`, message on `Console.Error` |
| File larger than `maxSizeBytes` | `false`, message on `Console.Error` |
| Non-success HTTP status on download | `false`, message on `Console.Error` |
| Cancellation requested | `false`, `File download operation was cancelled` on `Console.Error`. `OperationCanceledException` is not thrown. |
| Any other exception (network error, disk error, token extraction failure) | `false`, `Error downloading file: {ex.Message}` on `Console.Error` |

Because of the last row, callers only see `false` for most failures. Check the return value and inspect the error output when you need the cause.

## Limitations

- **Requires `TelegramApiClient`.** The token is taken from the private `_botToken` field by reflection. With any other `ITelegramApiClient` (a test double or a custom implementation), `GetFileAsync` may succeed but the download step fails, and the method returns `false`. Renaming `_botToken` in `TelegramApiClient` breaks downloads silently in the same way.
- **Whole file in memory.** The response is read into a `byte[]` before it is written. Use `maxSizeBytes` to bound memory use.
- **Size check depends on Telegram's value.** The limit is compared with the `FileSize` returned by `getFile`. The helper does not verify the length of the downloaded content.
- **New `HttpClient` per call.** The helper does not use `IHttpClientFactory`, so each call creates and disposes its own client.
- **Token in error output.** The download URL contains the bot token. The catch-all branch writes `ex.Message` to `Console.Error` without the `TokenRedaction` helper that `TelegramApiClient` uses, so an exception message that includes the URL would expose the token.
- **No path sanitising.** `destinationPath` is used as given. If the file name comes from the user (for example, `Document.FileName`), strip it to its base name first.
- **Overwrites silently.** An existing file at `destinationPath` is replaced without a warning.
- **Console output.** Diagnostics go to `Console.Error`, not to the framework's logging pipeline.

## Usage example

```csharp
using TelegramBotFramework.Integration;
using TelegramBotFramework.Utilities;

public sealed class UploadedDocumentSaver
{
    // 20 MB upper limit for uploads
    private const long MaxUploadBytes = 20L * 1024 * 1024;

    private readonly ITelegramApiClient _apiClient;
    private readonly string _downloadRoot;

    public UploadedDocumentSaver(ITelegramApiClient apiClient, string downloadRoot)
    {
        _apiClient = apiClient;
        _downloadRoot = downloadRoot;
    }

    /// <summary>
    /// Saves a document sent by a user and returns its local path, or null if the download failed.
    /// </summary>
    public async Task<string?> SaveDocumentAsync(string fileId, string fileName, CancellationToken cancellationToken)
    {
        // Keep only the base name so a crafted file name cannot escape the download folder
        var destination = Path.Combine(_downloadRoot, Path.GetFileName(fileName));

        var saved = await FileDownloadHelper.DownloadFileAsync(
            _apiClient,
            fileId,
            destination,
            MaxUploadBytes,
            cancellationToken).ConfigureAwait(false);

        return saved ? destination : null;
    }
}
```

Call it from a handler that has received a message with a document:

```csharp
var path = await _saver.SaveDocumentAsync(
    message.Document.FileId,
    message.Document.FileName ?? "upload.bin",
    cancellationToken);

if (path is null)
{
    // Download failed or the file was too large; details were written to Console.Error
    return;
}
```

## Related

- `ITelegramApiClient` (`src/TelegramBotFramework/Integration/ITelegramApiClient.cs`) - Source of `GetFileAsync`, which resolves the file path.
- [TokenRedaction](TokenRedaction.md) - Removes bot tokens from exception messages; not applied by this helper.
