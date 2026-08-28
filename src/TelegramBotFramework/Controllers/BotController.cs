#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.AspNetCore.Mvc;
using TelegramBotFramework.Models;
using TelegramBotFramework.Services;

namespace TelegramBotFramework.Controllers;

/// <summary>
/// Main bot controller for handling incoming updates and commands.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class BotController : ControllerBase, IBotController
{
    private readonly IUserService _userService;
    private readonly ICommandService _commandService;
    private readonly ISessionService _sessionService;
    private readonly IMessageService _messageService;
    private readonly IMenuService _menuService;
    private readonly ILogger<BotController> _logger;

    public BotController(
        IUserService userService,
        ICommandService commandService,
        ISessionService sessionService,
        IMessageService messageService,
        IMenuService menuService,
        ILogger<BotController> logger)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
        _sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
        _messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
        _menuService = menuService ?? throw new ArgumentNullException(nameof(menuService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Health check endpoint.
    /// </summary>
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// Process incoming message.
    /// </summary>
    [HttpPost("message")]
    public async Task<IActionResult> ProcessMessage([FromBody] ProcessMessageRequest request, CancellationToken cancellationToken = default)
    {
        if (request  is null)
        {
            return BadRequest("Request body is required");
        }

        try
        {
            // Get or create user
            var user = await _userService.GetOrCreateUserAsync(
                request.UserId,
                request.FirstName,
                request.LastName,
                cancellationToken);

            // Get or create session
            var session = await _sessionService.GetActiveSessionAsync(request.UserId, cancellationToken);
            session ??= await _sessionService.CreateSessionAsync(request.UserId, request.ChatId, cancellationToken);

            // Record activity
            await _userService.RecordUserActivityAsync(request.UserId, cancellationToken);
            await _sessionService.RecordSessionActivityAsync(session.SessionId, cancellationToken);

            // Process message
            var message = new Message
            {
                UserId = request.UserId,
                ChatId = request.ChatId,
                Content = request.Content,
                Type = request.MessageType,
                CreatedAt = DateTime.UtcNow
            };

            message.Validate();
            var processedMessage = await _messageService.ProcessIncomingMessageAsync(message, cancellationToken);

            // Create execution context
            var context = new TelegramBotFramework.Models.ExecutionContext
            {
                UserId = request.UserId,
                ChatId = request.ChatId,
                User = user,
                Session = session,
                Message = processedMessage,
                CreatedAt = DateTime.UtcNow
            };

            context.Validate();

            // If message is a command, process it
            if (request.Content.StartsWith(Constants.BotConstants.CommandPrefix))
            {
                var commandName = ExtractCommandName(request.Content);
                var command = await _commandService.GetCommandAsync(commandName, cancellationToken);

                if (command  is not null)
                {
                    context.Command = command;
                    context = await _commandService.ExecuteCommandAsync(context, cancellationToken);
                }
                else
                {
                    context.AddError($"Command '{commandName}' not found");
                }
            }

            await _messageService.MarkAsProcessedAsync(processedMessage.MessageId, cancellationToken);

            _logger.LogInformation("Message processed successfully - UserId: {UserId}, MessageId: {MessageId}",
                request.UserId, processedMessage.MessageId);

            return Ok(new
            {
                success = context.IsValid,
                contextId = context.ContextId,
                sessionId = session.SessionId,
                errors = context.Errors
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    /// Get user information.
    /// </summary>
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUser(long userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userService.GetUserByIdAsync(userId, cancellationToken);
            if (user  is null)
            {
                return NotFound($"User {userId} not found");
            }

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get user session.
    /// </summary>
    [HttpGet("session/{userId}")]
    public async Task<IActionResult> GetSession(long userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var session = await _sessionService.GetActiveSessionAsync(userId, cancellationToken);
            if (session  is null)
            {
                return NotFound($"No active session for user {userId}");
            }

            return Ok(session);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving session");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get all available commands.
    /// </summary>
    [HttpGet("commands")]
    public async Task<IActionResult> GetCommands(CancellationToken cancellationToken = default)
    {
        try
        {
            var commands = await _commandService.GetAvailableCommandsAsync(UserRole.User, cancellationToken);
            return Ok(commands);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving commands");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get menu.
    /// </summary>
    [HttpGet("menu/{menuId}")]
    public async Task<IActionResult> GetMenu(string menuId, CancellationToken cancellationToken = default)
    {
        try
        {
            var menu = await _menuService.GetMenuAsync(menuId, cancellationToken);
            if (menu  is null)
            {
                return NotFound($"Menu {menuId} not found");
            }

            return Ok(menu);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving menu");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Extract command name from message.
    /// </summary>
    private static string ExtractCommandName(string messageContent)
    {
        var parts = messageContent.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length > 0 ? parts[0].TrimStart('/') : string.Empty;
    }
}

/// <summary>
/// Request model for message processing.
/// </summary>
public sealed class ProcessMessageRequest
{
    public long UserId { get; set; }

    public long ChatId { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string? LastName { get; set; }

    public string Content { get; set; } = string.Empty;

    public MessageType MessageType { get; set; } = MessageType.Text;
}