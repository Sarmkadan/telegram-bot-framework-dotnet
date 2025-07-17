// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.AspNetCore.Mvc;
using TelegramBotFramework.Models;
using TelegramBotFramework.Services;

namespace TelegramBotFramework.Controllers;

/// <summary>
/// Admin controller for managing bot configuration, users, and commands.
/// </summary>
[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ICommandService _commandService;
    private readonly ISessionService _sessionService;
    private readonly IMenuService _menuService;
    private readonly BotConfiguration _configuration;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        IUserService userService,
        ICommandService commandService,
        ISessionService sessionService,
        IMenuService menuService,
        BotConfiguration configuration,
        ILogger<AdminController> logger)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
        _sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
        _menuService = menuService ?? throw new ArgumentNullException(nameof(menuService));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get bot configuration.
    /// </summary>
    [HttpGet("config")]
    public IActionResult GetConfiguration()
    {
        return Ok(new
        {
            botUsername = _configuration.BotUsername,
            sessionTimeoutMinutes = _configuration.SessionTimeoutMinutes,
            enableLogging = _configuration.EnableLogging,
            enableRateLimiting = _configuration.EnableRateLimiting,
            maxConcurrentRequests = _configuration.MaxConcurrentRequests
        });
    }

    /// <summary>
    /// Get statistics.
    /// </summary>
    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics(CancellationToken cancellationToken = default)
    {
        try
        {
            var totalUsers = await _userService.GetTotalUsersCountAsync(cancellationToken);
            var activeUsers = await _userService.GetActiveUsersCountAsync(cancellationToken);
            var admins = await _userService.GetAdministratorsAsync(cancellationToken);

            return Ok(new
            {
                totalUsers,
                activeUsers,
                adminCount = admins.Count,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting statistics");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get all administrators.
    /// </summary>
    [HttpGet("admins")]
    public async Task<IActionResult> GetAdministrators(CancellationToken cancellationToken = default)
    {
        try
        {
            var admins = await _userService.GetAdministratorsAsync(cancellationToken);
            return Ok(admins);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving administrators");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Promote user to administrator.
    /// </summary>
    [HttpPost("promote-admin/{userId}")]
    public async Task<IActionResult> PromoteToAdmin(long userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _userService.PromoteToAdminAsync(userId, cancellationToken);
            if (!result)
            {
                return NotFound($"User {userId} not found");
            }

            _logger.LogInformation("User promoted to admin: {UserId}", userId);
            return Ok(new { message = $"User {userId} promoted to administrator" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error promoting user");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Demote administrator to user.
    /// </summary>
    [HttpPost("demote-admin/{userId}")]
    public async Task<IActionResult> DemoteAdmin(long userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _userService.DemoteAdminAsync(userId, cancellationToken);
            if (!result)
            {
                return NotFound($"Administrator {userId} not found");
            }

            _logger.LogInformation("Admin demoted to user: {UserId}", userId);
            return Ok(new { message = $"Administrator {userId} demoted to user" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error demoting admin");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Ban user.
    /// </summary>
    [HttpPost("ban-user/{userId}")]
    public async Task<IActionResult> BanUser(long userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _userService.BanUserAsync(userId, cancellationToken);
            if (!result)
            {
                return NotFound($"User {userId} not found");
            }

            _logger.LogWarning("User banned: {UserId}", userId);
            return Ok(new { message = $"User {userId} has been banned" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error banning user");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Unban user.
    /// </summary>
    [HttpPost("unban-user/{userId}")]
    public async Task<IActionResult> UnbanUser(long userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _userService.UnbanUserAsync(userId, cancellationToken);
            if (!result)
            {
                return NotFound($"User {userId} not found");
            }

            _logger.LogInformation("User unbanned: {UserId}", userId);
            return Ok(new { message = $"User {userId} has been unbanned" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unbanning user");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Register new command.
    /// </summary>
    [HttpPost("commands")]
    public async Task<IActionResult> RegisterCommand([FromBody] Command command, CancellationToken cancellationToken = default)
    {
        try
        {
            var registered = await _commandService.RegisterCommandAsync(command, cancellationToken);
            _logger.LogInformation("Command registered: {CommandName}", command.Name);
            return Created($"/api/admin/commands/{command.Name}", registered);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering command");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get command by name.
    /// </summary>
    [HttpGet("commands/{commandName}")]
    public async Task<IActionResult> GetCommand(string commandName, CancellationToken cancellationToken = default)
    {
        try
        {
            var command = await _commandService.GetCommandAsync(commandName, cancellationToken);
            if (command == null)
            {
                return NotFound($"Command {commandName} not found");
            }

            return Ok(command);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving command");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Delete command.
    /// </summary>
    [HttpDelete("commands/{commandName}")]
    public async Task<IActionResult> DeleteCommand(string commandName, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _commandService.UnregisterCommandAsync(commandName, cancellationToken);
            if (!result)
            {
                return NotFound($"Command {commandName} not found");
            }

            _logger.LogInformation("Command deleted: {CommandName}", commandName);
            return Ok(new { message = $"Command {commandName} has been deleted" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting command");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get all menus.
    /// </summary>
    [HttpGet("menus")]
    public async Task<IActionResult> GetMenus(CancellationToken cancellationToken = default)
    {
        try
        {
            var menus = await _menuService.GetActiveMenusAsync(cancellationToken);
            return Ok(menus);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving menus");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Close expired sessions.
    /// </summary>
    [HttpPost("sessions/close-expired")]
    public async Task<IActionResult> CloseExpiredSessions(CancellationToken cancellationToken = default)
    {
        try
        {
            var count = await _sessionService.CloseExpiredSessionsAsync(cancellationToken);
            _logger.LogInformation("Closed {Count} expired sessions", count);
            return Ok(new { message = $"Closed {count} expired sessions" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error closing expired sessions");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal server error" });
        }
    }
}
