#nullable enable

namespace TelegramBotFramework.Examples;

/// <summary>
/// Validation helpers for <see cref="HmacCallbackExample"/>.
/// </summary>
public static class HmacCallbackExampleValidation
{
    /// <summary>
    /// Validates the <see cref="HmacCallbackExample"/> instance and returns a list of problems.
    /// Since <see cref="HmacCallbackExample"/> is a static class and cannot be instantiated, there is no valid instance.
    /// </summary>
    /// <param name="value">The <see cref="HmacCallbackExample"/> instance to validate.</param>
    /// <returns>A read-only list of human-readable problems. Never empty because there is no valid instance.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this HmacCallbackExample value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return new List<string> { $"Unexpected non-null instance of {nameof(HmacCallbackExample)}." };
    }

    /// <summary>
    /// Determines whether the <see cref="HmacCallbackExample"/> instance is valid.
    /// Since <see cref="HmacCallbackExample"/> is a static class and cannot be instantiated, there is no valid instance.
    /// </summary>
    /// <param name="value">The <see cref="HmacCallbackExample"/> instance to validate.</param>
    /// <returns><see langword="false"/> because there is no valid instance.</returns>
    public static bool IsValid(this HmacCallbackExample value) => false;

    /// <summary>
    /// Ensures the <see cref="HmacCallbackExample"/> instance is valid. Throws an <see cref="ArgumentException"/> if the instance is invalid.
    /// Since <see cref="HmacCallbackExample"/> is a static class and cannot be instantiated, this method always throws for non-null inputs (and throws <see cref="ArgumentNullException"/> for null).
    /// </summary>
    /// <param name="value">The <see cref="HmacCallbackExample"/> instance to validate.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Always thrown for non-null inputs because there is no valid instance.</exception>
    public static void EnsureValid(this HmacCallbackExample value)
    {
        ArgumentNullException.ThrowIfNull(value);
        var problems = Validate(value);
        throw new ArgumentException($"The {nameof(HmacCallbackExample)} instance is invalid. Problems: {string.Join("; ", problems)}", nameof(value));
    }
}