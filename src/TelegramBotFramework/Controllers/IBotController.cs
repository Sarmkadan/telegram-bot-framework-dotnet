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
/// Interface for bot controller handling incoming updates and commands.
/// </summary>
public interface IBotController
{
    IActionResult Health();
    Task<IActionResult> ProcessMessage(ProcessMessageRequest request, CancellationToken cancellationToken = default);
    Task<IActionResult> GetUser(long userId, CancellationToken cancellationToken = default);
    Task<IActionResult> GetSession(long userId, CancellationToken cancellationToken = default);
    Task<IActionResult> GetCommands(CancellationToken cancellationToken = default);
    Task<IActionResult> GetMenu(string menuId, CancellationToken cancellationToken = default);
}