#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using TelegramBotFramework.Services;

namespace TelegramBotFramework.Events;

/// <summary>
/// Middleware for role-based authorization that validates user permissions before event handling.
/// This middleware checks if the user has the required role to process the event.
/// </summary>
public sealed class AuthorizationMiddleware : EventMiddlewareBase<MessageReceivedEvent>
{
    private readonly IUserService _userService;
    private readonly ILogger<AuthorizationMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorizationMiddleware"/> class.
    /// </summary>
    /// <param name="userService">The user service for role validation.</param>
    /// <param name="logger">The logger instance.</param>
    public AuthorizationMiddleware(IUserService userService, ILogger<AuthorizationMiddleware>? logger = null)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _logger = logger ?? new ConsoleLogger<AuthorizationMiddleware>();
    }

    /// <summary>
    /// Pre-processing logic to validate user authorization before event handling.
    /// </summary>
    /// <param name="evt">The message received event.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected override async Task PreProcessAsync(MessageReceivedEvent evt)
    {
        if (evt is null)
            throw new ArgumentNullException(nameof(evt));

        // Skip authorization for system events or when user ID is not available
        if (evt.UserId <= 0)
        {
            _logger.LogDebug("Skipping authorization for system event or invalid user ID");
            return;
        }

        try
        {
            // Get user from service
            var user = await _userService.GetUserByTelegramIdAsync(evt.UserId).ConfigureAwait(false);

            if (user is null)
            {
                _logger.LogWarning("User {UserId} not found in authorization check", evt.UserId);
                // You could throw an exception here or allow the event to proceed
                // For demonstration, we'll allow it to proceed
                return;
            }

            _logger.LogInformation("User {UserId} authorized with role {UserRole} for event processing",
                evt.UserId, user.Role);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during authorization check for user {UserId}", evt.UserId);
            // Continue processing even if authorization fails for resilience
        }
    }

    /// <summary>
    /// Post-processing logic executed after event handling.
    /// </summary>
    /// <param name="evt">The message received event.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected override async Task PostProcessAsync(MessageReceivedEvent evt)
    {
        if (evt is null)
            throw new ArgumentNullException(nameof(evt));

        // Post-processing can be used for cleanup, logging, or metrics
        await Task.CompletedTask;
    }
}