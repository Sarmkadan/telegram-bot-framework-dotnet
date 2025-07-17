// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Models;

/// <summary>
/// Represents the execution context for a command or operation.
/// </summary>
public class ExecutionContext
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

    public Dictionary<string, object>? State { get; set; }

    public List<string>? Errors { get; set; }

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
        if (State?.TryGetValue(key, out var value) == true)
        {
            return value is T tValue ? tValue : default;
        }
        return default;
    }

    /// <summary>
    /// Sets state value.
    /// </summary>
    public void SetState(string key, object value)
    {
        State ??= new Dictionary<string, object>();
        State[key] = value;
    }

    /// <summary>
    /// Adds an error message.
    /// </summary>
    public void AddError(string errorMessage)
    {
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
}
