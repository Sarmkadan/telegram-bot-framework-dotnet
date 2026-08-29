#nullable enable

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TelegramBotFramework.Integration;
using Xunit;

namespace TelegramBotFramework.Tests.Integration;

public interface IPollingStrategyTests
{
    void Constructor_WithNullApiClient_ThrowsArgumentNullException();
    void Constructor_WithNullLogger_UsesConsoleLogger();
    void Start_WhenAlreadyRunning_DoesNotStartAnotherPollingTask();
    Task StopAsync_WhenNotRunning_DoesNotThrow();
    Task ProcessUpdateAsync_WithNullUpdate_ThrowsArgumentNullException();
    Task ProcessUpdateAsync_AdvancesLastUpdateId();
    Task ProcessUpdateAsync_InvokesOnUpdateReceivedEvent();
    Task ProcessUpdateAsync_WithException_LogsErrorAndContinues();
    void GetStatus_ReturnsCorrectPollingStatus();
    Task Start_WithCustomPollInterval_SetsCorrectInterval();
    Task Start_WithDefaultInterval_UsesOneSecondInterval();
    Task Polling_WithEmptyUpdates_AppliesDelay();
    Task Polling_WithUpdates_ProcessesThemAndAdvancesOffset();
    Task Polling_WithException_AppliesBackoffDelay();
    Task StopAsync_CancelsPollingLoop();
    Task Polling_AdvancesUpdateIdThroughMultiplePolls();
    Task Polling_WithOffsetAdvancement_RequestsUpdatesWithCorrectOffset();
    Task LastPollTime_IsUpdatedOnEachPoll();
    Task Polling_WithCancellation_StopsGracefully();
}