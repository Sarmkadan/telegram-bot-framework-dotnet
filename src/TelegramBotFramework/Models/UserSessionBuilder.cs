#nullable enable
namespace TelegramBotFramework.Models;

/// <summary>
/// Builder for <see cref="UserSession"/> objects.
/// </summary>
public sealed class UserSessionBuilder
{
    // Fields to store property values, initialized to UserSession defaults
    private string sessionId = string.Empty;
    private long userId = 0;
    private long chatId = 0;
    private SessionState state = SessionState.Active;
    private string currentContext = "menu";
    private string? currentMenuId = null;
    private DateTime createdAt = DateTime.UtcNow;
    private DateTime? lastActivityAt = DateTime.UtcNow;
    private DateTime? expiresAt = null;
    private Dictionary<string, string>? contextData = null;

    /// <summary>
    /// Sets the SessionId.
    /// </summary>
    /// <param name="sessionId">The session ID.</param>
    /// <returns>This builder instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="sessionId"/> is null, empty, or whitespace.</exception>
    public UserSessionBuilder WithSessionId(string sessionId)
    {
        ArgumentException.ThrowIfNullOrEmpty(sessionId);
        this.sessionId = sessionId;
        return this;
    }

    /// <summary>
    /// Sets the UserId.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <returns>This builder instance.</returns>
    public UserSessionBuilder WithUserId(long userId)
    {
        this.userId = userId;
        return this;
    }

    /// <summary>
    /// Sets the ChatId.
    /// </summary>
    /// <param name="chatId">The chat ID.</param>
    /// <returns>This builder instance.</returns>
    public UserSessionBuilder WithChatId(long chatId)
    {
        this.chatId = chatId;
        return this;
    }

    /// <summary>
    /// Sets the State.
    /// </summary>
    /// <param name="state">The session state.</param>
    /// <returns>This builder instance.</returns>
    public UserSessionBuilder WithState(SessionState state)
    {
        this.state = state;
        return this;
    }

    /// <summary>
    /// Sets the CurrentContext.
    /// </summary>
    /// <param name="currentContext">The current context.</param>
    /// <returns>This builder instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="currentContext"/> is null, empty, or whitespace.</exception>
    public UserSessionBuilder WithCurrentContext(string currentContext)
    {
        ArgumentException.ThrowIfNullOrEmpty(currentContext);
        this.currentContext = currentContext;
        return this;
    }

    /// <summary>
    /// Sets the CurrentMenuId.
    /// </summary>
    /// <param name="currentMenuId">The current menu ID (can be null).</param>
    /// <returns>This builder instance.</returns>
    public UserSessionBuilder WithCurrentMenuId(string? currentMenuId)
    {
        this.currentMenuId = currentMenuId;
        return this;
    }

    /// <summary>
    /// Sets the CreatedAt.
    /// </summary>
    /// <param name="createdAt">The creation timestamp.</param>
    /// <returns>This builder instance.</returns>
    public UserSessionBuilder WithCreatedAt(DateTime createdAt)
    {
        this.createdAt = createdAt;
        return this;
    }

    /// <summary>
    /// Sets the LastActivityAt.
    /// </summary>
    /// <param name="lastActivityAt">The last activity timestamp (can be null).</param>
    /// <returns>This builder instance.</returns>
    public UserSessionBuilder WithLastActivityAt(DateTime? lastActivityAt)
    {
        this.lastActivityAt = lastActivityAt;
        return this;
    }

    /// <summary>
    /// Sets the ExpiresAt.
    /// </summary>
    /// <param name="expiresAt">The expiration timestamp (can be null).</param>
    /// <returns>This builder instance.</returns>
    public UserSessionBuilder WithExpiresAt(DateTime? expiresAt)
    {
        this.expiresAt = expiresAt;
        return this;
    }

    /// <summary>
    /// Sets the ContextData.
    /// </summary>
    /// <param name="contextData">The context data dictionary (can be null).</param>
    /// <returns>This builder instance.</returns>
    public UserSessionBuilder WithContextData(Dictionary<string, string>? contextData)
    {
        this.contextData = contextData;
        return this;
    }

    /// <summary>
    /// Creates a new <see cref="UserSession"/> instance with the configured values.
    /// </summary>
    /// <returns>A configured <see cref="UserSession"/> instance.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <see cref="SessionId"/> is null, empty, or whitespace;
    /// or when <see cref="UserId"/> is not positive;
    /// or when <see cref="ChatId"/> is not positive.
    /// </exception>
    public UserSession Build()
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            throw new ArgumentException("SessionId is required", nameof(sessionId));

        if (userId <= 0)
            throw new ArgumentException("UserId must be positive", nameof(userId));

        if (chatId <= 0)
            throw new ArgumentException("ChatId must be positive", nameof(chatId));

        return new UserSession
        {
            SessionId = sessionId,
            UserId = userId,
            ChatId = chatId,
            State = state,
            CurrentContext = currentContext,
            CurrentMenuId = currentMenuId,
            CreatedAt = createdAt,
            LastActivityAt = lastActivityAt,
            ExpiresAt = expiresAt,
            ContextData = contextData
        };
    }

    /// <summary>
    /// Creates a builder initialized with values from an existing <see cref="UserSession"/> instance.
    /// </summary>
    /// <param name="template">The session to copy values from.</param>
    /// <returns>A builder pre-filled with the template's values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="template"/> is null.</exception>
    public static UserSessionBuilder From(UserSession template)
    {
        ArgumentNullException.ThrowIfNull(template);
        return new UserSessionBuilder
        {
            sessionId = template.SessionId,
            userId = template.UserId,
            chatId = template.ChatId,
            state = template.State,
            currentContext = template.CurrentContext,
            currentMenuId = template.CurrentMenuId,
            createdAt = template.CreatedAt,
            lastActivityAt = template.LastActivityAt,
            expiresAt = template.ExpiresAt,
            contextData = template.ContextData
        };
    }
}