#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Utilities;

using System.Security.Cryptography;
using System.Text;

/// <summary>
/// Utility class for cryptographic operations including hashing and encoding.
/// Provides secure methods for password hashing and data encryption.
/// </summary>
public static class CryptoUtility
{
    /// <summary>
    /// Generates a SHA256 hash of the input string.
    /// </summary>
    public static string HashSHA256(string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(hashedBytes);
    }

    /// <summary>
    /// Generates an MD5 hash of the input string (use only for non-security purposes).
    /// </summary>
    [System.Obsolete("MD5 is cryptographically broken. Use HashSHA256 instead.")]
    public static string HashMD5(string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        using var md5 = MD5.Create();
        var hashedBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
        return BitConverter.ToString(hashedBytes).Replace("-", "").ToLowerInvariant();
    }

    /// <summary>
    /// Generates a secure password hash using PBKDF2.
    /// Includes salt and iteration count for security.
    /// </summary>
    public static string HashPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("Password cannot be empty", nameof(password));

        using var rng = new RNGCryptoServiceProvider();
        var saltBytes = new byte[16];
        rng.GetBytes(saltBytes);

        using var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, 10000, HashAlgorithmName.SHA256);
        var hash = pbkdf2.GetBytes(20);

        var hashBytes = new byte[36];
        Array.Copy(saltBytes, 0, hashBytes, 0, 16);
        Array.Copy(hash, 0, hashBytes, 16, 20);

        return Convert.ToBase64String(hashBytes);
    }

    /// <summary>
    /// Verifies a password against its PBKDF2 hash.
    /// </summary>
    public static bool VerifyPassword(string password, string hash)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hash))
            return false;

        try
        {
            var hashBytes = Convert.FromBase64String(hash);
            var saltBytes = new byte[16];
            Array.Copy(hashBytes, 0, saltBytes, 0, 16);

            using var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, 10000, HashAlgorithmName.SHA256);
            var hash2 = pbkdf2.GetBytes(20);

            for (int i = 0; i < 20; i++)
            {
                if (hashBytes[i + 16] != hash2[i])
                    return false;
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Generates a cryptographically secure random string of specified length.
    /// </summary>
    public static string GenerateRandomString(int length, string allowedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789")
    {
        if (length <= 0)
            throw new ArgumentException("Length must be greater than 0", nameof(length));

        using var rng = new RNGCryptoServiceProvider();
        var bytes = new byte[length];
        rng.GetBytes(bytes);

        var sb = new StringBuilder(length);
        foreach (var b in bytes)
            sb.Append(allowedChars[b % allowedChars.Length]);

        return sb.ToString();
    }

    /// <summary>
    /// Generates a cryptographically secure random token (hex format).
    /// </summary>
    public static string GenerateRandomToken(int lengthInBytes = 32)
    {
        using var rng = new RNGCryptoServiceProvider();
        var bytes = new byte[lengthInBytes];
        rng.GetBytes(bytes);
        return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
    }

    /// <summary>
    /// Computes HMAC-SHA256 for message authentication.
    /// </summary>
    public static string ComputeHmacSHA256(string message, string key)
    {
        if (string.IsNullOrEmpty(message) || string.IsNullOrEmpty(key))
            return string.Empty;

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
        return Convert.ToBase64String(hash);
    }

    /// <summary>
    /// Encodes a string to Base64.
    /// </summary>
    public static string EncodeBase64(string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        return Convert.ToBase64String(Encoding.UTF8.GetBytes(input));
    }

    /// <summary>
    /// Decodes a Base64 string.
    /// </summary>
    public static string? DecodeBase64(string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        try
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(input));
        }
        catch
        {
            return null;
        }
    }
}