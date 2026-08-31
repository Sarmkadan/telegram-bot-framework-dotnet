#nullable enable

namespace TelegramBotFramework.Tests;

internal static class EventPublisherTestsConstants
{
    // Event types
    public const string MessageReceivedEventType = "MessageReceivedEvent";
    public const string CommandExecutedEventType = "CommandExecutedEvent";
    public const string BotStateChangedEventType = "BotStateChangedEvent";

    // Test correlation IDs
    public const string TestCorrelationId = "test-correlation-123";
    public const string FirstCorrelationId = "first-id";
    public const string SecondCorrelationId = "second-id";
    public const string TestCorrelation = "test-correlation";

    // Test chat and user IDs
    public const long TestChatId = 12345;
    public const long TestUserId = 67890;
    public const long AnotherUserId = 456;
    public const long TestUserId2 = 12345;
    public const long TestChatId2 = 123;
    public const long TestChatId3 = 123;
    public const long TestUserId3 = 456;
    public const string TestMessageShort = "test";

    // Test message texts
    public const string TestMessage = "Hello, world!";
    public const string TestMessageText = "test message";
    public const string TestShortMessage = "test";
    public const string EmptyMessage = "";

    // Test command data
    public const string TestCommandName = "start";
    public const string TestArguments = "arg1 arg2";
    public const string TestErrorMessage = "Command failed";

    // Test bot state data
    public const string TestPreviousState = "Idle";
    public const string TestNewState = "Active";
    public const string TestReason = "User triggered action";
    public const string EmptyReason = "";
}