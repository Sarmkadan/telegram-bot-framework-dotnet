#nullable enable

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TelegramBotFramework.Events;
using Xunit;

namespace TelegramBotFramework.Tests;

/// <summary>
/// Interface for EventBusExtensionsTests.
/// </summary>
public interface IEventBusExtensionsTests
{
    void IsEventRegistered_WithNullBus_ThrowsArgumentNullException();
    void IsEventRegistered_WhenEventTypeNotRegistered_ReturnsFalse();
    void IsEventRegistered_WhenEventTypeRegistered_ReturnsTrue();
    void GetTotalSubscriberCount_WithNullBus_ThrowsArgumentNullException();
    void GetTotalSubscriberCount_WhenNoHandlers_ReturnsZero();
    void GetTotalSubscriberCount_WithSingleHandler_ReturnsOne();
    void GetTotalSubscriberCount_WithMultipleHandlers_ReturnsCorrectCount();
    void GetTotalSubscriberCount_WithMultipleEventTypes_ReturnsSumOfAllHandlers();
    void UseMiddleware_WithNullBus_ThrowsArgumentNullException();
    void UseMiddleware_WithNullMiddleware_ThrowsArgumentNullException();
    void UseMiddleware_WithSingleMiddleware_ReturnsBusForChaining();
    void UseMiddleware_WithMultipleMiddleware_ReturnsBusForChaining();
    void UseMiddleware_WithDuplicateMiddleware_DoesNotAddDuplicate();
    Task PublishAsync_WithNoHandlers_ShouldNotThrow();
    Task PublishAsync_WithNoHandlers_ShouldLogWarning();
    Task PublishAsync_WithMultipleHandlers_AllHandlersAreInvoked();
    Task PublishAsync_WhenOneHandlerThrows_OtherHandlersStillExecute();
    Task PublishAsync_WithMiddleware_ExecutesMiddlewareInOrder();
}