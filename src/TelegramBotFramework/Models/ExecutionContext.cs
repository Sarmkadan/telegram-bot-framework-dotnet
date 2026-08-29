#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Models;

/// <summary>
/// Represents the execution context for a command or operation.
/// </summary>
public sealed class ExecutionContext : IExecutionContext, IEquatable<ExecutionContext>
{
    public string ContextId { get; set; } = Guid.NewGuid().ToString();

    public long UserId { get; set; }

    public long ChatId { get; set; }

    public BotUser? User { get; set; }

    public UserSession? Session { get; set; }

    public Command? Command { get; set; }

    public Message? Message { get; set; }

    public Dictionary<string, object>? Parameters { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Dictionary<string, object> States { get; set; } = new();

    public List<string>? Errors { get; set; } = new();

    public bool IsValid { get; set; } = true;

    /// <summary>
    /// Gets a parameter value.
    /// </summary>
    public T? GetParameter<T>(string key)
    {
        if (Parameters?.TryGetValue(key, out var value) == true)
        {
            return value is T tValue ? tValue : default;
        }
        return default;
    }

    /// <summary>
    /// Sets a parameter value.
    /// </summary>
    public void SetParameter(string key, object value)
    {
        Parameters ??= new Dictionary<string, object>();
        Parameters[key] = value;
    }

    /// <summary>
    /// Gets state value.
    /// </summary>
    public T? GetState<T>(string key)
    {
        if (States.TryGetValue(key, out var value))
        {
            return value is T tValue ? tValue : default;
        }
        return default;
    }

    /// <summary>
    /// Sets state value.
    /// </summary>
    public void SetState(string? key, object value)
    {
        if (string.IsNullOrEmpty(key))
        {
            return;
        }

        States[key] = value;
    }

    /// <summary>
    /// Gets the response message injected by a middleware that called <see cref="RespondAndStop"/>.
    /// <c>null</c> when no middleware requested a short-circuit response.
    /// </summary>
    public string? PendingResponse { get; private set; }

    /// <summary>
    /// Gets a value indicating whether a middleware has short-circuited the pipeline via
    /// <see cref="RespondAndStop"/>. When <c>true</c>, the pipeline executor skips all
    /// remaining middleware and returns the context immediately.
    /// </summary>
    public bool IsStopped { get; private set; }

    /// <summary>
    /// Short-circuits the middleware pipeline and injects a response message that the
    /// presentation layer should send to the user. Call this inside a middleware to both
    /// provide feedback (e.g., "Too many requests, please wait") and halt further processing
    /// without needing direct access to <c>ITelegramBotClient</c>.
    /// </summary>
    /// <param name="responseMessage">The message text to deliver to the user.</param>
    public void RespondAndStop(string responseMessage)
    {
        PendingResponse = responseMessage;
        IsStopped = true;
        IsValid = false;
    }

    /// <summary>
    /// Short-circuits the middleware pipeline without injecting a response message.
    /// </summary>
    public void StopProcessing()
    {
        IsStopped = true;
    }

    /// <summary>
    /// Adds an error message.
    /// </summary>
    public void AddError(string? errorMessage)
    {
        if (string.IsNullOrEmpty(errorMessage))
            return;

        Errors ??= new List<string>();
        Errors.Add(errorMessage);
        IsValid = false;
    }

    /// <summary>
    /// Validates the context has required data.
    /// </summary>
    public bool Validate()
    {
        var errors = new List<string>();

        if (UserId <= 0)
            errors.Add("UserId must be positive");

        if (ChatId <= 0)
            errors.Add("ChatId must be positive");

        if (errors.Count > 0)
        {
            Errors = errors;
            IsValid = false;
            return false;
        }

        IsValid = true;
        return true;
    }

    /// <summary>
    /// Gets execution duration.
    /// </summary>
    public TimeSpan GetDuration() =>
        DateTime.UtcNow - CreatedAt;

    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>true if the current object is equal to the <paramref name="other">parameter</paramref>; otherwise, false.</returns>
    public bool Equals(ExecutionContext? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return ContextId == other.ContextId
               && UserId == other.UserId
               && ChatId == other.ChatId
               && Equals(User, other.User)
               && Equals(Session, other.Session)
               && Equals(Command, other.Command)
               && Equals(Message, other.Message)
               && Equals(Parameters, other.Parameters);
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((ExecutionContext)obj);
    }

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current object.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(ContextId, UserId, ChatId, User, Session, Command, Message, Parameters);
    }

    /// <summary>
    /// Returns a value that indicates whether the values of two <see cref="ExecutionContext"/> objects are equal.
    /// </summary>
    /// <param name="left">The first value to compare.</param>
    /// <param name="right">The second value to compare.</param>
    /// <returns>true if the <paramref name="left"/> and <paramref name="right"/> parameters have the same value; otherwise, false.</returns>
    public static bool operator ==(ExecutionContext? left, ExecutionContext? right)
    {
        if (ReferenceEquals(left, right)) return true;
        if (ReferenceEquals(null, left)) return false;
        if (ReferenceEquals(null, right)) return false;
        return left.Equals(right);
    }

    /// <summary>
    /// Returns a value that indicates whether the values of two <see cref="ExecutionContext"/> objects are not equal.
    /// </summary>
    /// <param name="left">The first value to compare.</param>
    /// <param name="right">The second value to compare.</param>
    /// <returns>true if <paramref name="left"/> and <paramref name="right"/> are not equal; otherwise, false.</returns>
    public static bool operator !=(ExecutionContext? left, ExecutionContext? right)
    {
        return !(left == right);
    }
}