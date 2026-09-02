#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Exceptions;

/// <summary>
/// Base exception for all bot framework errors.
/// </summary>
public class BotFrameworkException : Exception, IBotFrameworkException, IEquatable<BotFrameworkException>
{
    public string? ErrorCode { get; set; }

    public BotFrameworkException()
    {
    }

    public BotFrameworkException(string message) : base(message)
    {
    }

    public BotFrameworkException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public BotFrameworkException(string message, string errorCode)
        : base(message)
    {
        ErrorCode = errorCode;
    }

    public BotFrameworkException(string message, string errorCode, Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }

    public bool Equals(BotFrameworkException? other)
    {
        if (other is null)
            return false;
        if (ReferenceEquals(this, other))
            return true;
        return ErrorCode == other.ErrorCode;
    }

    public override bool Equals(object? obj)
    {
        if (obj is BotFrameworkException other)
            return Equals(other);
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(ErrorCode);
    }

    public override string ToString()
    {
        string? commandName = null;
        long? userId = null;
        string? requiredPermission = null;
        string? sessionId = null;

        switch (this)
        {
            case CommandExecutionException ce:
                commandName = ce.CommandName;
                break;
            case CommandNotFoundException cnf:
                commandName = cnf.CommandName;
                break;
            case InsufficientPermissionException ipe:
                userId = ipe.UserId;
                requiredPermission = ipe.RequiredPermission;
                break;
            case SessionException se:
                sessionId = se.SessionId;
                break;
            case UserException ue:
                userId = ue.UserId;
                break;
        }

        return $"BotFrameworkException {{ ErrorCode = {ErrorCode}, CommandName = {commandName}, UserId = {userId}, RequiredPermission = {requiredPermission}, SessionId = {sessionId} }}";
    }

    public static bool operator ==(BotFrameworkException? left, BotFrameworkException? right)
    {
        if (left is null)
            return right is null;
        return left.Equals(right);
    }

    public static bool operator !=(BotFrameworkException? left, BotFrameworkException? right)
    {
        return !(left == right);
    }
}

/// <summary>
/// Thrown when a command execution fails.
/// </summary>
public sealed class CommandExecutionException : BotFrameworkException
{
    public string? CommandName { get; set; }

    public CommandExecutionException(string message, string? commandName = null)
        : base(message, BotFrameworkExceptionConstants.CommandExecutionError)
    {
        CommandName = commandName;
    }

    public CommandExecutionException(string message, string? commandName, Exception innerException)
        : base(message, BotFrameworkExceptionConstants.CommandExecutionError, innerException)
    {
        CommandName = commandName;
    }
}

/// <summary>
/// Thrown when a command is not found.
/// </summary>
public sealed class CommandNotFoundException : BotFrameworkException
{
    public string? CommandName { get; set; }

    public CommandNotFoundException(string commandName)
        : base(string.Format(BotFrameworkExceptionConstants.CommandNotFoundMessageFormat, commandName),
            BotFrameworkExceptionConstants.CommandNotFound)
    {
        CommandName = commandName;
    }
}

/// <summary>
/// Thrown when user lacks permission to execute a command.
/// </summary>
public sealed class InsufficientPermissionException : BotFrameworkException
{
    public long? UserId { get; set; }

    public string? RequiredPermission { get; set; }

    public InsufficientPermissionException(long userId, string? requiredPermission = null)
        : base(string.Format(BotFrameworkExceptionConstants.InsufficientPermissionMessageFormat, userId),
            BotFrameworkExceptionConstants.InsufficientPermission)
    {
        UserId = userId;
        RequiredPermission = requiredPermission;
    }
}

/// <summary>
/// Thrown when a session operation fails.
/// </summary>
public sealed class SessionException : BotFrameworkException
{
    public string? SessionId { get; set; }

    public SessionException(string message, string? sessionId = null)
        : base(message, BotFrameworkExceptionConstants.SessionError)
    {
        SessionId = sessionId;
    }

    public SessionException(string message, string? sessionId, Exception innerException)
        : base(message, BotFrameworkExceptionConstants.SessionError, innerException)
    {
        SessionId = sessionId;
    }
}

/// <summary>
/// Thrown when a user operation fails.
/// </summary>
public sealed class UserException : BotFrameworkException
{
    public long? UserId { get; set; }

    public UserException(string message, long? userId = null)
        : base(message, BotFrameworkExceptionConstants.UserError)
    {
        UserId = userId;
    }

    public UserException(string message, long? userId, Exception innerException)
        : base(message, BotFrameworkExceptionConstants.UserError, innerException)
    {
        UserId = userId;
    }
}

/// <summary>
/// Thrown when a rate limit is exceeded.
/// </summary>
public sealed class RateLimitExceededException : BotFrameworkException
{
    public long? UserId { get; set; }

    public int? RetryAfterSeconds { get; set; }

    public RateLimitExceededException(long? userId = null, int? retryAfter = null)
        : base(BotFrameworkExceptionConstants.RateLimitExceededMessage,
            BotFrameworkExceptionConstants.RateLimitExceeded)
    {
        UserId = userId;
        RetryAfterSeconds = retryAfter;
    }
}

/// <summary>
/// Thrown when a configuration error occurs.
/// </summary>
public sealed class ConfigurationException : BotFrameworkException
{
    public ConfigurationException(string message)
        : base(message, BotFrameworkExceptionConstants.ConfigurationError)
    {
    }

    public ConfigurationException(string message, Exception innerException)
        : base(message, BotFrameworkExceptionConstants.ConfigurationError, innerException)
    {
    }
}

/// <summary>
/// Thrown when a duplicate update is detected to prevent double-processing.
/// </summary>
public sealed class DuplicateUpdateException : BotFrameworkException
{
    public long? UpdateId { get; set; }

    public DuplicateUpdateException(string message, long? updateId = null)
        : base(message, BotFrameworkExceptionConstants.DuplicateUpdate)
    {
        UpdateId = updateId;
    }

    public DuplicateUpdateException(string message, long? updateId, Exception innerException)
        : base(message, BotFrameworkExceptionConstants.DuplicateUpdate, innerException)
    {
        UpdateId = updateId;
    }
}
