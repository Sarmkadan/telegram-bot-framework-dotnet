#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Utilities;

using System.Security.Cryptography;
using System.Text;

/// <summary>
/// Provides HMAC-based signing and validation for Telegram callback data.
/// Protects against forged callback queries by appending a truncated HMAC signature.
/// Designed to work within Telegram's 64-byte callback data limit.
/// </summary>
/// <remarks>
/// Telegram callback data has a strict 64-byte UTF-8 limit.
/// This signer uses HMAC-SHA256 to create a signature and truncates it to fit.
///
/// Signature format: {original_data}|{truncated_signature}
/// Where truncated_signature is the first N bytes of the HMAC-SHA256 hash.
///
/// Recommended usage with InlineKeyboardBuilder:
/// <code>
/// var secret = "my-secret-key";
/// var data = "user_action:123";
/// var signed = CallbackDataSigner.Sign(data, secret);
///
/// // Use in keyboard builder
/// builder.AddButton("Confirm", signed);
///
/// // Later validate incoming callback
/// if (CallbackDataSigner.TryValidate(signed, secret, out var originalData))
/// {
///     // originalData contains "user_action:123"
///     // Process the callback safely
/// }
/// </code>
/// </remarks>
public static class CallbackDataSigner
{
    private const char Separator = '|';
    private const int MaxTotalBytes = 64;
    private const int MaxDataBytes = 56; // Leave room for separator + signature
    private const int SignatureBytes = 8; // 8 bytes = 16 hex chars

    /// <summary>
    /// Signs the data with HMAC-SHA256 and returns signed callback data.
    /// </summary>
    /// <param name="data">The original callback data to sign.</param>
    /// <param name="secret">The secret key used for signing.</param>
    /// <returns>Signed callback data with HMAC signature appended.</returns>
    /// <exception cref="ArgumentNullException">Thrown if data or secret is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the resulting signed data exceeds Telegram's 64-byte limit.</exception>
    public static string Sign(string data, string secret)
    {
        if (data is null)
            throw new ArgumentNullException(nameof(data));

        if (secret is null)
            throw new ArgumentNullException(nameof(secret));

        if (string.IsNullOrWhiteSpace(data))
            throw new ArgumentException("Data cannot be empty or whitespace.", nameof(data));

        if (string.IsNullOrWhiteSpace(secret))
            throw new ArgumentException("Secret cannot be empty or whitespace.", nameof(secret));

        // Calculate signature
        var signature = ComputeHmacSha256(data, secret);
        var truncatedSignature = TruncateSignature(signature);

        // Build signed data: {data}|{signature}
        var signedData = $"{data}{Separator}{truncatedSignature}";

        // Validate Telegram's 64-byte limit
        var byteLength = Encoding.UTF8.GetByteCount(signedData);
        if (byteLength > MaxTotalBytes)
        {
            throw new ArgumentException(
                $"Signed callback data exceeds Telegram's {MaxTotalBytes}-byte limit. " +
                $"Original data: {data.Length} chars, Signed data: {byteLength} bytes. " +
                "Reduce the data payload or use a shorter separator.",
                nameof(data));
        }

        return signedData;
    }

    /// <summary>
    /// Attempts to validate signed callback data and extract the original data.
    /// </summary>
    /// <param name="signedData">The signed callback data received from Telegram.</param>
    /// <param name="secret">The secret key used for validation.</param>
    /// <param name="originalData">Outputs the original data if validation succeeds.</param>
    /// <returns>True if validation succeeds and data is extracted; false otherwise.</returns>
    public static bool TryValidate(string signedData, string secret, out string originalData)
    {
        originalData = string.Empty;

        if (string.IsNullOrEmpty(signedData) || string.IsNullOrEmpty(secret))
        {
            return false;
        }

        try
        {
            // Split signed data into original + signature parts
            var separatorIndex = signedData.LastIndexOf(Separator);
            if (separatorIndex < 0 || separatorIndex == signedData.Length - 1)
            {
                return false; // No separator or separator at end
            }

            var dataPart = signedData.Substring(0, separatorIndex);
            var receivedSignature = signedData.Substring(separatorIndex + 1);

            // Recompute expected signature
            var expectedSignature = ComputeHmacSha256(dataPart, secret);
            var expectedTruncated = TruncateSignature(expectedSignature);

            // Constant-time comparison to prevent timing attacks
            if (!FixedTimeEquals(expectedTruncated, receivedSignature))
            {
                return false;
            }

            originalData = dataPart;
            return true;
        }
        catch
        {
            // Any error during validation
            return false;
        }
    }

    /// <summary>
    /// Computes HMAC-SHA256 signature for the given data.
    /// </summary>
    private static string ComputeHmacSha256(string data, string key)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return Convert.ToHexString(hash); // Hex format for readability
    }

    /// <summary>
    /// Truncates the HMAC signature to fit within Telegram's callback data limit.
    /// Uses first 8 bytes (16 hex characters) of the SHA256 hash.
    /// </summary>
    private static string TruncateSignature(string hexSignature)
    {
        // Signature is already in hex format (2 chars per byte)
        // We want first 8 bytes = 16 hex characters
        if (hexSignature.Length <= SignatureBytes * 2)
        {
            return hexSignature;
        }

        return hexSignature.Substring(0, SignatureBytes * 2);
    }

    /// <summary>
    /// Constant-time comparison to prevent timing attacks.
    /// </summary>
    private static bool FixedTimeEquals(string a, string b)
    {
        if (a.Length != b.Length)
        {
            return false;
        }

        var result = 0;
        for (var i = 0; i < a.Length; i++)
        {
            result |= a[i] ^ b[i];
        }

        return result == 0;
    }
}
