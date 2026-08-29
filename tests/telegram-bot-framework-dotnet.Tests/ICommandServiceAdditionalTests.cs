#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Threading;
using System.Threading.Tasks;
using TelegramBotFramework.Models;

namespace TelegramBotFramework.Tests;

/// <summary>
/// Interface for additional test suite for <see cref="CommandService"/> functionality covering advanced scenarios
/// such as role-based command filtering, command execution tracking, and rate limiting.
/// </summary>
public interface ICommandServiceAdditionalTests
{
    Task GetAvailableCommandsAsync_WithAdminRole_ReturnsAdminCommands();
    Task GetAvailableCommandsAsync_WithUserRole_ReturnsOnlyNonAdminCommands();
    Task GetAvailableCommandsAsync_WithModeratorRole_ReturnsCommandsForModeratorAndAbove();
    Task ExecuteCommandAsync_WithValidContext_ExecutesSuccessfully();
    Task ExecuteCommandAsync_WithDisabledCommand_AddsErrorToContext();
    Task ExecuteCommandAsync_WithInsufficientPermissions_AddsErrorToContext();
    Task CanUserExecuteCommandAsync_WithInactiveUser_ReturnsFalse();
    Task CanUserExecuteCommandAsync_WithNonExistentCommand_ReturnsFalse();
    Task CanUserExecuteCommandAsync_WithDisabledCommand_ReturnsFalse();
    Task RecordCommandExecutionAsync_WithValidCommand_IncrementsExecutionCount();
    Task RecordCommandExecutionAsync_WithNonExistentCommand_DoesNotThrow();
    Task GetCommandExecutionCountAsync_WithExistingCommand_ReturnsCount();
    Task GetCommandExecutionCountAsync_WithNonExistentCommand_ReturnsZero();
    Task IsCommandRateLimitedAsync_WithNoRateLimitConfigured_ReturnsFalse();
    Task IsCommandRateLimitedAsync_WithMultipleUsers_ResetsRateLimitPerUser();
}