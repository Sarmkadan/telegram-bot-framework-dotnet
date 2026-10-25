#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Integration;

/// <summary>
/// Outcome of a Telegram Bot API call. Expected failures (a rejected request, a transport error,
/// an open circuit) are reported through this type instead of being thrown.
/// </summary>
/// <typeparam name="T">The type of the value produced by a successful call.</typeparam>
/// <param name="Success"><see langword="true"/> when Telegram accepted the request.</param>
/// <param name="Value">The value produced by the call; <see langword="null"/> or default when the call failed.</param>
/// <param name="ErrorCode">The Telegram <c>error_code</c> (or the HTTP status code when the body carries none); <see langword="null"/> when no response was received.</param>
/// <param name="Description">A human-readable description of the failure; <see langword="null"/> on success.</param>
public sealed record ApiResult<T>(bool Success, T? Value, int? ErrorCode, string? Description)
{
    /// <summary>Creates a successful result carrying <paramref name="value"/>.</summary>
    /// <param name="value">The value produced by the call.</param>
    /// <returns>A successful <see cref="ApiResult{T}"/>.</returns>
    public static ApiResult<T> Ok(T value) => new(true, value, null, null);

    /// <summary>Creates a failed result.</summary>
    /// <param name="errorCode">The Telegram error code or HTTP status code, if one is known.</param>
    /// <param name="description">A human-readable description of the failure.</param>
    /// <returns>A failed <see cref="ApiResult{T}"/> without a value.</returns>
    public static ApiResult<T> Fail(int? errorCode, string? description) => new(false, default, errorCode, description);
}
