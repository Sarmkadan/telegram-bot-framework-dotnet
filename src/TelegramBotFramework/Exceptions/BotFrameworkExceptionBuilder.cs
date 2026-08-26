#nullable enable
namespace TelegramBotFramework.Exceptions;

/// <summary>
/// Builder for creating <see cref="BotFrameworkException"/> instances with fluent syntax.
/// </summary>
public class BotFrameworkExceptionBuilder
{
    private string? _errorCode;

    /// <summary>
    /// Sets the error code for the exception.
    /// </summary>
    /// <param name="errorCode">The error code to set.</param>
    /// <returns>The builder instance for chaining.</returns>
    public BotFrameworkExceptionBuilder WithErrorCode(string? errorCode)
    {
        _errorCode = errorCode;
        return this;
    }

    /// <summary>
    /// Creates a builder pre-filled with values from an existing <see cref="BotFrameworkException"/>.
    /// </summary>
    /// <param name="template">The exception to copy values from.</param>
    /// <returns>A new builder instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="template"/> is null.</exception>
    public static BotFrameworkExceptionBuilder From(BotFrameworkException template)
    {
        ArgumentNullException.ThrowIfNull(template);
        return new BotFrameworkExceptionBuilder()
            .WithErrorCode(template.ErrorCode);
    }

    /// <summary>
    /// Builds the <see cref="BotFrameworkException"/> instance with the configured values.
    /// </summary>
    /// <returns>A new <see cref="BotFrameworkException"/> instance.</returns>
    public BotFrameworkException Build()
    {
        // BotFrameworkException has no required properties - ErrorCode can be null
        return new BotFrameworkException()
        {
            ErrorCode = _errorCode
        };
    }
}