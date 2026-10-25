# TokenRedaction

Static utility that removes Telegram bot tokens from strings before they are logged, returned to callers, or otherwise exposed. Telegram bot API URLs embed the token as a path segment (`https://api.telegram.org/bot<token>/getMe`), so URLs and exception messages built from them can leak credentials. All members are static and stateless, so they are safe to call from any thread.

Matched tokens are replaced with the placeholder `[BOT_TOKEN_REDACTED]`.

## Token pattern

Every method uses the same regular expression, applied case-insensitively:

```
bot\d+:[a-zA-Z0-9_-]+(?::[a-zA-Z0-9_-]+)*
```

A match is `bot`, followed by one or more digits (the bot ID), a colon, and one or more token characters. Additional colon-separated segments are also consumed. The pattern does not require a word boundary before `bot`, so a substring such as `robot123:abc` is also redacted.

## API

### RedactTokenFromUrl
- **Purpose:** Redacts bot tokens from a URL string, such as a Telegram API request URL.
- **Parameters:** `url` (string) – The URL that may contain a bot token.
- **Return value:** The URL with each matched token replaced by `[BOT_TOKEN_REDACTED]`. If no token is found, the original text is returned unchanged.
- **Throws:** `ArgumentNullException` if `url` is `null`.

### RedactTokenFromMessage
- **Purpose:** Redacts bot tokens from a free-form message, such as an exception message or log text.
- **Parameters:** `message` (string) – The text that may contain a bot token.
- **Return value:** The message with each matched token replaced by `[BOT_TOKEN_REDACTED]`. If no token is found, the original text is returned unchanged.
- **Throws:** `ArgumentNullException` if `message` is `null`.
- **Notes:** Uses the same pattern as `RedactTokenFromUrl`. The two methods are separate so that call sites state their intent. Their behavior is currently identical.

### RedactToken
- **Purpose:** Produces a redacted message from an exception, including the messages of its inner exceptions when needed.
- **Parameters:** `exception` (Exception) – The exception to redact.
- **Return value:** The redacted outer message. If the outer message contained a token and there is an inner exception, the redacted inner message is appended as `{outer} (Inner exception: {inner})`. Otherwise only the redacted outer message is returned.
- **Throws:** `ArgumentNullException` if `exception` is `null`.
- **Notes:** The inner exception is included only when the outer message was itself changed by redaction. If the outer message has no token but an inner exception does, the inner text is dropped and the outer message is returned as is. Only `Exception.Message` is inspected; the stack trace and other properties are not.

## Where it is used

- `Integration/TelegramApiRetryHandler.cs` redacts request URLs before they are written to logs.
- `Integration/TelegramApiClient.cs` passes exceptions through `RedactToken` before logging them and before returning them in `ApiResult<T>.Fail(...)`.

## Usage

```csharp
using TelegramBotFramework.Utilities;

// Redact a request URL before logging it
string url = "https://api.telegram.org/bot123456:ABC-DEF1234ghIkl-zyx57W2v1u123ew11/getMe";
string safeUrl = TokenRedaction.RedactTokenFromUrl(url);
// "https://api.telegram.org/[BOT_TOKEN_REDACTED]/getMe"

// Redact a free-form message
string message = "Request failed for bot123456:ABC-DEF1234ghIkl-zyx57W2v1u123ew11";
string safeMessage = TokenRedaction.RedactTokenFromMessage(message);
// "Request failed for [BOT_TOKEN_REDACTED]"

// Redact an exception before returning it to a caller
try
{
    // ... call the Telegram API ...
}
catch (HttpRequestException ex)
{
    string safeError = TokenRedaction.RedactToken(ex);
    // Log or return safeError instead of ex.Message
}
```

## Limitations

- Only tokens in the `bot<id>:<secret>` form are matched. A bare token with no `bot` prefix is not redacted.
- Inner exception text is included only when the outer message contains a token (see `RedactToken`).
- Stack traces and exception data dictionaries are not redacted.
