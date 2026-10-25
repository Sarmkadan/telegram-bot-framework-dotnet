# CallbackDataSigner

Static utility that protects Telegram callback data against forgery. `Sign` appends a truncated HMAC-SHA256 signature to a payload, and `TryValidate` recomputes that signature on an incoming callback and returns the original payload only if it matches. The signed string stays within Telegram's 64-byte callback data limit. The class is `public static` and stateless, so it is safe to call from any thread.

Source: `src/TelegramBotFramework/Utilities/CallbackDataSigner.cs`

## Signed format

```
{original_data}|{signature}
```

- `original_data` is the payload exactly as passed to `Sign`.
- `signature` is the first 8 bytes of the HMAC-SHA256 digest of `original_data`, encoded as 16 uppercase hex characters.
- The separator is `|`. Validation splits on the **last** `|`, so the payload itself may contain `|`.

Because the signature adds 17 bytes (`|` plus 16 hex characters), the payload must be at most **47 UTF-8 bytes** for the signed result to fit in Telegram's 64-byte limit.

## API

### `public static string Sign(string data, string secret)`
Signs `data` with `secret` and returns the signed callback data.

- **Parameters:**
  - `data` (string) - The callback payload to sign. Must not be empty or whitespace.
  - `secret` (string) - The HMAC key. Must not be empty or whitespace. It is UTF-8 encoded.
- **Return value:** `data` followed by `|` and the 16-character signature.
- **Throws:**
  - `ArgumentNullException` if `data` or `secret` is `null`.
  - `ArgumentException` if `data` or `secret` is empty or whitespace, or if the signed result exceeds 64 UTF-8 bytes (the message includes the byte count).

### `public static bool TryValidate(string signedData, string secret, out string originalData)`
Checks that `signedData` carries a valid signature for `secret` and extracts the original payload.

- **Parameters:**
  - `signedData` (string) - The callback data received from Telegram.
  - `secret` (string) - The same key used by `Sign`.
  - `originalData` (out string) - Set to the original payload when validation succeeds. Set to `string.Empty` otherwise.
- **Return value:** `true` if the signature matches; otherwise `false`.
- **Behavior:**
  - Returns `false` (without throwing) if `signedData` or `secret` is null or empty, if there is no `|`, if `|` is the last character, or if the signature does not match.
  - The signature comparison runs in constant time to avoid timing attacks.
  - Any exception during validation is caught and reported as `false`.

## Usage example

```csharp
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotFramework.Utilities;

public sealed class ConfirmationKeyboard
{
    private readonly string _secret;

    public ConfirmationKeyboard(string secret)
    {
        _secret = secret;
    }

    // 1. Sign the payload when building the button
    public InlineKeyboardButton BuildConfirmButton()
    {
        var signed = CallbackDataSigner.Sign("user_action:123", _secret);
        // signed == "user_action:123|<16 hex chars>"

        return InlineKeyboardButton.WithCallbackData("Confirm", signed);
    }

    // 2. Validate the callback when Telegram sends it back
    public Task HandleCallbackAsync(string callbackData, CancellationToken cancellationToken)
    {
        if (!CallbackDataSigner.TryValidate(callbackData, _secret, out var originalData))
        {
            // Forged, tampered with, or signed with a different secret: ignore it
            return Task.CompletedTask;
        }

        // originalData == "user_action:123"
        return ProcessActionAsync(originalData, cancellationToken);
    }

    private Task ProcessActionAsync(string action, CancellationToken cancellationToken)
    {
        // Application logic goes here
        return Task.CompletedTask;
    }
}
```

Handling a payload that is too long:

```csharp
try
{
    // 60 bytes of payload + 17 bytes of signature exceeds the 64-byte limit
    var signed = CallbackDataSigner.Sign(new string('x', 60), secret);
}
catch (ArgumentException ex)
{
    // ex.Message reports the original and signed byte counts.
    // Shorten the payload, for example by storing the data server-side and passing an ID.
}
```

## Notes

- Use a long, random secret kept out of source control, for example from an environment variable or secret store.
- A valid signature proves the callback was produced with the secret. It does not prevent replay, so add expiry or one-time IDs if a callback must only be acted on once.
- Validation returns `false` for any problem, so callers should treat `false` as "ignore this callback" rather than try to diagnose why.
