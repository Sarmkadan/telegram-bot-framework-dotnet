#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Services;

/// <summary>
/// Implementation of user management service.
/// </summary>
public sealed class UserService : IUserService
{
    private readonly Repositories.IUserRepository _userRepository;
    private readonly Microsoft.Extensions.Logging.ILogger<UserService> _logger;

    public UserService(
        Repositories.IUserRepository userRepository,
        Microsoft.Extensions.Logging.ILogger<UserService> logger)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Models.BotUser> GetOrCreateUserAsync(
        long telegramId,
        string firstName,
        string? lastName = null,
        CancellationToken cancellationToken = default)
    {
        var existingUser = await _userRepository.GetByTelegramIdAsync(telegramId, cancellationToken).ConfigureAwait(false);
        if (existingUser  is not null)
        {
            return existingUser;
        }

        var newUser = new Models.BotUser
        {
            TelegramId = telegramId,
            FirstName = firstName,
            LastName = lastName,
            Status = Models.UserStatus.Active,
            Role = Models.UserRole.User,
            CreatedAt = DateTime.UtcNow
        };

        newUser.Validate();
        var created = await _userRepository.CreateAsync(newUser, cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("New user created: {UserId} ({UserName})", telegramId, firstName);
        return created;
    }

    public async Task<Models.BotUser?> GetUserByIdAsync(long userId, CancellationToken cancellationToken = default)
    {
        return await _userRepository.GetByIdAsync(userId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<Models.BotUser?> GetUserByTelegramIdAsync(long telegramId, CancellationToken cancellationToken = default)
    {
        return await _userRepository.GetByTelegramIdAsync(telegramId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<Models.BotUser> UpdateUserAsync(Models.BotUser user, CancellationToken cancellationToken = default)
    {
        user.Validate();
        user.UpdatedAt = DateTime.UtcNow;
        var updated = await _userRepository.UpdateAsync(user, cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("User updated: {UserId}", user.TelegramId);
        return updated;
    }

    public async Task<bool> BanUserAsync(long userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken).ConfigureAwait(false);
        if (user  is null)
        {
            return false;
        }

        user.Status = Models.UserStatus.Banned;
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user, cancellationToken).ConfigureAwait(false);
        _logger.LogWarning("User banned: {UserId}", userId);
        return true;
    }

    public async Task<bool> UnbanUserAsync(long userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken).ConfigureAwait(false);
        if (user  is null)
        {
            return false;
        }

        user.Status = Models.UserStatus.Active;
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user, cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("User unbanned: {UserId}", userId);
        return true;
    }

    public async Task<IList<Models.BotUser>> GetAdministratorsAsync(CancellationToken cancellationToken = default)
    {
        return await _userRepository.GetByRoleAsync(Models.UserRole.Administrator, cancellationToken).ConfigureAwait(false);
    }

    public async Task<bool> PromoteToAdminAsync(long userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken).ConfigureAwait(false);
        if (user  is null)
        {
            return false;
        }

        user.Role = Models.UserRole.Administrator;
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user, cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("User promoted to admin: {UserId}", userId);
        return true;
    }

    public async Task<bool> DemoteAdminAsync(long userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken).ConfigureAwait(false);
        if (user  is null || user.Role != Models.UserRole.Administrator)
        {
            return false;
        }

        user.Role = Models.UserRole.User;
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user, cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("User demoted from admin: {UserId}", userId);
        return true;
    }

    public async Task<int> GetTotalUsersCountAsync(CancellationToken cancellationToken = default)
    {
        return await _userRepository.CountAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<int> GetActiveUsersCountAsync(CancellationToken cancellationToken = default)
    {
        var activeUsers = await _userRepository.GetByStatusAsync(Models.UserStatus.Active, cancellationToken).ConfigureAwait(false);
        return activeUsers.Count;
    }

    public async Task RecordUserActivityAsync(long userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken).ConfigureAwait(false);
        if (user  is not null)
        {
            user.UpdateActivity();
            await _userRepository.UpdateAsync(user, cancellationToken).ConfigureAwait(false);
        }
    }
}