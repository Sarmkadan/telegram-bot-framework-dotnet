using System;
using TelegramBotFramework.Exceptions;

namespace TelegramBotFramework.Exceptions;

/// <summary>
/// Provides extension methods for <see cref="BotFrameworkException"/>.
/// </summary>
public static class BotFrameworkExceptionExtensions
{
    /// <summary>
    /// Checks if the exception has the specified error code.
    /// </summary>
    /// <param name="exception">The exception instance.</param>
    /// <param name="errorCode">The error code to check.</param>
    /// <returns>True if the exception has the specified error code; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null.</exception>
    public static bool HasErrorCode(this BotFrameworkException exception, string errorCode)
    {
        ArgumentNullException.ThrowIfNull(exception);
        
        return string.Equals(exception.ErrorCode, errorCode, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets a descriptive string representation of the exception including the error code.
    /// </summary>
    /// <param name="exception">The exception instance.</param>
    /// <returns>A string containing the error code and message.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null.</exception>
    public static string ToSummary(this BotFrameworkException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return exception.ErrorCode != null 
            ? $"[{exception.ErrorCode}] {exception.Message}"
            : exception.Message;
    }

    /// <summary>
    /// Checks if the exception or its inner exception chain contains an exception of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of exception to look for.</typeparam>
    /// <param name="exception">The exception instance.</param>
    /// <returns>True if found; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null.</exception>
    public static bool ContainsInnerException<T>(this BotFrameworkException exception) where T : Exception
    {
        ArgumentNullException.ThrowIfNull(exception);

        Exception? current = exception.InnerException;
        while (current != null)
        {
            if (current is T)
            {
                return true;
            }
            current = current.InnerException;
        }
        return false;
    }
}
