#nullable enable

using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TelegramBotFramework.Events;
using Xunit;

namespace TelegramBotFramework.Tests;

public interface IEventPublisherTests
{
    void Constructor_WithNullEventBus_ThrowsArgumentNullException();
    void Constructor_WithNullLogger_CreatesConsoleLogger();
    void WithCorrelationId_SetsCorrelationIdAndReturnsPublisher();
    void WithCorrelationId_MultipleCalls_OverwritesPreviousValue();
    Task PublishMessageReceivedAsync_CallsEventBusWithCorrectEvent();
    Task PublishMessageReceivedAsync_WithNullMessageText_SetsMessageTextToNull();
    Task PublishMessageReceivedAsync_WithEmptyMessageText_SetsMessageTextToEmpty();
    Task PublishMessageReceivedAsync_WithCorrelationId_SetsCorrelationIdOnEvent();
    Task PublishCommandExecutedAsync_CallsEventBusWithCorrectEvent();
    Task PublishCommandExecutedAsync_WithErrorMessage_SetsErrorMessage();
    Task PublishCommandExecutedAsync_WithNullArguments_SetsArgumentsToNull();
    Task PublishCommandExecutedAsync_WithEmptyArguments_SetsArgumentsToEmpty();
    Task PublishCommandExecutedAsync_WithCorrelationId_SetsCorrelationIdOnEvent();
    Task PublishBotStateChangedAsync_CallsEventBusWithCorrectEvent();
    Task PublishBotStateChangedAsync_WithNullReason_SetsReasonToNull();
    Task PublishBotStateChangedAsync_WithEmptyReason_SetsReasonToEmpty();
    Task PublishBotStateChangedAsync_WithCorrelationId_SetsCorrelationIdOnEvent();
    Task PublishAsync_GenericMethod_CallsEventBusWithCorrectEvent();
    void LoggingMessageEventHandler_CanBeCreated();
}