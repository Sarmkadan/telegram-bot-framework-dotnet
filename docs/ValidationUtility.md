# ValidationUtility

Utility class providing a collection of static validation helpers for common Telegram‑bot related values and general data formats. All members are pure functions that return `true` when the supplied value conforms to the expected format and `false` otherwise; they do not modify state.

## API

### `public static bool IsValidTelegramUserId(long userId)`
- **Purpose:** Determines whether `userId` is a valid Telegram user identifier.  
- **Parameters:** `userId` – the numeric user ID to test.  
- **Return value:** `true` if `userId` is a positive 64‑bit integer; otherwise `false`.  
- **Throws:** None (the method does not validate for null as the parameter is a value type).

### `public static bool IsValidTelegramChatId(long chatId)`
- **Purpose:** Determines whether `chatId` is a valid Telegram chat identifier.  
- **Parameters:** `chatId` – the numeric chat ID to test (can be positive for private chats, negative for groups, supergroups, or channels).  
- **Return value:** `true` if `chatId` is a non‑zero 64‑bit integer; otherwise `false`.  
- **Throws:** None.

### `public static bool IsValidTelegramToken(string token)`
- **Purpose:** Checks that `token` matches the format of a Telegram bot token (`<numbers>:<letters_and_underscores>`).  
- **Parameters:** `token` – the bot token string to validate.  
- **Return value:** `true` if `token` consists of one or more digits, a colon, then one or more alphanumeric characters or underscores; otherwise `false`.  
- **Throws:** `ArgumentNullException` if `token` is `null`.

### `public static bool IsValidUrl(string url)`
- **Purpose:** Validates that `url` is a well‑formed absolute URI using the HTTP or HTTPS scheme.  
- **Parameters:** `url` – the URL string to test.  
- **Return value:** `true` if `url` parses to a `Uri` with scheme `http` or `https` and a non‑empty host; otherwise `false`.  
- **Throws:** `ArgumentNullException` if `url` is `null`.

### `public static bool IsValidIPv4(string ip)`
- **Purpose:** Determines whether `ip` represents a valid IPv4 address in dotted‑decimal notation.  
- **Parameters:** `ip` – the IPv4 address string to test.  
- **Return value:** `true` if `ip` consists of four decimal numbers ranging from 0 to 255 separated by periods, with no leading zeros unless the number is exactly zero; otherwise `false`.  
- **Throws:** `ArgumentNullException` if `ip` is `null`.

### `public static bool IsValidPhoneNumber(string phoneNumber)`
- **Purpose:** Checks that `phoneNumber` conforms to the E.164 international format (optional leading `+` followed by digits).  
- **Parameters:** `phoneNumber` – the phone number string to validate.  
- **Return value:** `true` if `phoneNumber` matches the pattern `\\+?[1-9]\\d{1,14}`; otherwise `false`.  
- **Throws:** `ArgumentNullException` if `phoneNumber` is `null`.

### `public static bool IsValidCommandName(string command)`
- **Purpose:** Validates a Telegram bot command name (the text after the leading `/`).  
- **Parameters:** `command` – the command string to test, **without** the leading slash.  
- **Return value:** `true` if `command` consists of one or more alphanumeric characters or underscores; otherwise `false`.  
- **Throws:** `ArgumentNullException` if `command` is `null`.

### `public static bool IsValidFilename(string fileName)`
- **Purpose:** Ensures that `fileName` does not contain characters illegal for file names on Windows (`<>:"/\\|?*`) and is not empty or whitespace only.  
- **Parameters:** `fileName` – the file name string to validate.  
- **Return value:** `true` if `fileName` is non‑empty, contains no invalid characters, and is not reserved (e.g., `CON`, `PRN`); otherwise `false`.  
- **Throws:** `ArgumentNullException` if `fileName` is `null`.

### `public static bool IsStrongPassword(string password)`
- **Purpose:** Evaluates password strength based on length, character variety, and common patterns.  
- **Parameters:** `password` – the password string to assess.  
- **Return value:** `true` if the password is at least 8 characters long, contains at least one uppercase letter, one lowercase letter, one digit, and one special character from the set `!@#$%^&*()-_=+[]{}|;:,.<>?/`; otherwise `false`.  
- **Throws:** `ArgumentNullException` if `password` is `null`.

### `public static bool IsValidLength(string input, int minLength, int maxLength)`
- **Purpose:** Checks that the length of `input` falls within the inclusive range `[minLength, maxLength]`.  
- **Parameters:**  
  - `input` – the string whose length is to be checked.  
  - `minLength` – the minimum allowed length (must be ≥ 0).  
  - `maxLength` – the maximum allowed length (must be ≥ `minLength`).  
- **Return value:** `true` if `input` is not `null` and its length is between `minLength` and `maxLength`; otherwise `false`.  
- **Throws:**  
  - `ArgumentNullException` if `input` is `null`.  
  - `ArgumentOutOfRangeException` if `minLength` < 0 or `maxLength` < `minLength`.

### `public static bool IsNumeric(string input)`
- **Purpose:** Determines whether `input` consists solely of decimal digits.  
- **Parameters:** `input` – the string to test.  
- **Return value:** `true` if `input` is not empty and every character is a Unicode decimal digit (`0`‑`9`); otherwise `false`.  
- **Throws:** `ArgumentNullException` if `input` is `null`.

### `public static bool IsValidGuid(string guid)`
- **Purpose:** Validates that `guid` is a correctly formatted GUID string.  
- **Parameters:** `guid` – the GUID representation to test (can be with or without braces, hyphens optional).  
- **Return value:** `true` if `guid` parses to a `System.Guid`; otherwise `false`.  
- **Throws:** `ArgumentNullException` if `guid` is `null`.

## Usage

```csharp
using TelegramBotFrameworkDotnet.Utilities;

// Validate a bot token before creating the client
string token = "123456789:AAABBBCCCDDD EEFFGGHHIIJJKKLLMMNNOOPP";
if (ValidationUtility.IsValidTelegramToken(token))
{
    var bot = new TelegramBotClient(token);
    // proceed with bot initialization
}
else
{
    throw new ArgumentException("Invalid bot token supplied.");
}
```

```csharp
// Validate a user ID received from an Update
long userId = update.Message.From.Id;
if (ValidationUtility.IsValidTelegramUserId(userId))
{
    // safe to store or use the ID
    userRepository.RecordActivity(userId);
}
else
{
    logger.Warning("Received update with invalid user ID: {UserId}", userId);
}
```

## Notes

- All validation methods are **pure** and stateless; they rely only on their input parameters. Consequently, they are thread‑safe and can be invoked concurrently from any number of threads without external synchronization.
- Methods that accept `string` arguments treat a `null` reference as an invalid input and throw `ArgumentNullException`. Empty strings (`""`) are considered invalid unless the specific validation logic explicitly permits them (e.g., `IsValidLength` with a `minLength` of `0`).
- Culture‑specific considerations:  
  - `IsNumeric` uses the invariant definition of decimal digits (`0`‑`9`) and does not depend on the current culture.  
  - `IsValidPhoneNumber` follows the E.164 specification, which is culture‑neutral.  
  - `IsValidGuid` accepts the standard formats returned by `Guid.TryParse`, which are also culture‑independent.
- Edge cases to be aware of:  
  - Leading `+` signs are allowed for phone numbers but are stripped before digit‑only validation.  
  - For `IsValidIPv4`, addresses such as `0.0.0.0` and `255.255.255.255` are considered valid, while `01.2.3.4` (leading zero) is rejected.  
  - `IsValidFilename` rejects Windows‑reserved names regardless of case and ignores trailing spaces or periods, which would be stripped by the file system.  
  - `IsStrongPassword` does not perform dictionary checks; it only enforces length and character class requirements.  
- The `IsValidLength` helper is intended for scenarios where a range check is needed alongside other validations (e.g., validating a username length after confirming it contains only allowed characters). It will throw if the supplied `minLength`/`maxLength` arguments are themselves invalid, preventing silent logic errors.
