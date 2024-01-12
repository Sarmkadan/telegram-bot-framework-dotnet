# CryptoUtility

Utility class providing common cryptographic helpers such as hashing, password verification, random string/token generation, HMAC computation, and Base64 encoding/decoding. All members are static and stateless, making them safe to call from any thread without external synchronization.

## API

### HashSHA256
- **Purpose:** Computes the SHA‑256 hash of the supplied data.
- **Parameters:** `input` (string) – The text to be hashed. UTF‑8 encoding is used to convert the string to bytes.
- **Return value:** A lowercase hexadecimal string representing the 32‑byte hash.
- **Throws:** `ArgumentNullException` if `input` is `null`.

### HashMD5
- **Purpose:** Computes the MD5 hash of the supplied data.
- **Parameters:** `input` (string) – The text to be hashed. UTF‑8 encoding is used.
- **Return value:** A lowercase hexadecimal string representing the 16‑byte hash.
- **Throws:** `ArgumentNullException` if `input` is `null`.

### HashPassword
- **Purpose:** Hashes a password using a salted, adaptive algorithm (PBKDF2 with SHA‑256, 100 000 iterations) suitable for storage.
- **Parameters:** `password` (string) – The plain‑text password to hash.
- **Return value:** A string containing the algorithm identifier, salt, and hash, delimited for later verification.
- **Throws:** `ArgumentNullException` if `password` is `null`.

### VerifyPassword
- **Purpose:** Checks whether a plain‑text password matches a previously hashed password.
- **Parameters:** 
  - `password` (string) – The password to verify.
  - `hashedPassword` (string) – The output from `HashPassword`.
- **Return value:** `true` if the password matches the hash; otherwise `false`.
- **Throws:** 
  - `ArgumentNullException` if either parameter is `null`.
  - `FormatException` if `hashedPassword` does not conform to the expected format.

### GenerateRandomString
- **Purpose:** Produces a cryptographically strong random string of the requested length using the default alphanumeric character set (A‑Z, a‑z, 0‑9).
- **Parameters:** `length` (int) – Desired number of characters. Must be greater than zero.
- **Return value:** A random string of the specified length.
- **Throws:** `ArgumentOutOfRangeException` if `length` is less than or equal to zero.

### GenerateRandomToken
- **Purpose:** Produces a cryptographically strong random token suitable for use as an authentication token or nonce.
- **Parameters:** `length` (int) – Desired number of bytes in the token; the returned string is Base64‑url encoded, so the character count will be approximately `4 * ceil(length / 3)`.
- **Return value:** A Base64‑url encoded string containing the random bytes.
- **Throws:** `ArgumentOutOfRangeException` if `length` is less than or equal to zero.

### ComputeHmacSHA256
- **Purpose:** Computes an HMAC‑SHA256 signature for the supplied data using the given key.
- **Parameters:** 
  - `data` (string) – The message to authenticate. UTF‑8 encoding is used.
  - `key` (string) – The secret key. UTF‑8 encoding is used.
- **Return value:** A lowercase hexadecimal string representing the 32‑byte HMAC.
- **Throws:** 
  - `ArgumentNullException` if either `data` or `key` is `null`.
  - `ArgumentException` if either string is empty.

### EncodeBase64
- **Purpose:** Encodes a byte array to a Base64 string.
- **Parameters:** `input` (byte[]) – The data to encode.
- **Return value:** A Base64‑encoded string.
- **Throws:** `ArgumentNullException` if `input` is `null`.

### DecodeBase64
- **Purpose:** Decodes a Base64 string to a byte array and returns the result as a UTF‑8 string. Returns `null` if the input is `null` or not valid Base64.
- **Parameters:** `input` (string) – The Base64‑encoded text.
- **Return value:** The decoded string, or `null` when decoding fails.
- **Throws:** None; invalid input results in a `null` return value rather than an exception.

## Usage

```csharp
using TelegramBotFrameworkDotnet.Security;

// Hash a password and later verify it
string password = "CorrectHorseBatteryStaple";
string hashed = CryptoUtility.HashPassword(password);

// ... store hashed ...

bool isValid = CryptoUtility.VerifyPassword("WrongGuess", hashed); // false
bool isValid2 = CryptoUtility.VerifyPassword(password, hashed);   // true
```

```csharp
using TelegramBotFrameworkDotnet.Security;

// Generate a random token for a CSRF guard
string token = CryptoUtility.GenerateRandomToken(32); // 32 random bytes → Base64‑url string

// Compute an HMAC for request authentication
string requestBody = "{ \"action\": \"sendMessage\", \"chat_id\": 12345 }";
string secret = "my‑shared‑secret";
string signature = CryptoUtility.ComputeHmacSHA256(requestBody, secret);

// Encode/decode arbitrary binary data
byte[] raw = System.Text.Encoding.UTF8.GetBytes("example");
string b64 = CryptoUtility.EncodeBase64(raw);
string? decoded = CryptoUtility.DecodeBase64(b64); // "example"
```

## Notes

- All methods are pure functions; they rely only on their arguments and have no hidden state. Consequently, they are thread‑safe and can be invoked concurrently from multiple threads without additional synchronization.
- Input validation is strict: `null` arguments raise `ArgumentNullException`. For hashing and HMAC methods, empty strings are considered invalid and will raise an `ArgumentException`.
- `HashPassword` uses a fixed iteration count (100 000) and a random salt generated via `RandomNumberGenerator`. The output format is intended to be opaque; applications should treat it as a single blob for storage.
- `DecodeBase64` returns `null` instead of throwing to allow callers to treat malformed Base64 as a missing value; if an exception is preferred, callers can check for `null` and throw accordingly.
- The random string and token generators use `RandomNumberGenerator.Fill` under the hood, providing cryptographic suitability for security‑sensitive contexts such as nonces, passwords, or API keys. 
- Encoding and decoding operations assume UTF‑8 for string‑to‑byte conversions unless otherwise noted; if a different encoding is required, perform the conversion manually before calling these methods.
