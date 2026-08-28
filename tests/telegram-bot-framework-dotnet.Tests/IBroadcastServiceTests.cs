#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
using TelegramBotFramework.Integration;
using TelegramBotFramework.Models;
using TelegramBotFramework.Services;
using Xunit;

namespace TelegramBotFramework.Tests;

public interface IBroadcastServiceTests
{
    Task BroadcastAsync_WithEmptyChatIds_ReturnsSuccessWithNoChats();
    Task BroadcastAsync_WithValidChats_SendsMessagesToAllChats();
    Task BroadcastAsync_WithFailedMessages_CollectsFailures();
    Task BroadcastAsync_WithContinueOnErrorFalse_ThrowsOnFirstError();
    Task BroadcastAsync_WithRateLimit_RespectsMessagesPerSecond();
    Task BroadcastAsync_WithProgressCallback_CallsCallback();
    Task BroadcastAsync_WithCancellation_CancelsOperation();
    Task BroadcastToUsersAsync_ConvertsUsersToChatIds();
    void GetRateLimitStats_ReturnsStatistics();
    Task BroadcastAsync_WithMessageFormatter_AppliesFormatter();
    void Dispose_DisposesResources();
}