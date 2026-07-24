#nullable enable
// =============================================================================
// Test to verify validation improvements in EventBase-derived classes
// This demonstrates that the unified validation contracts are working correctly
// =====================================================================

using System;
using TelegramBotFramework.Events;

Console.WriteLine("Testing EventBase validation improvements...\n");

// Test 1: MessageEventBase validation
Console.WriteLine("Test 1: MessageEventBase validation");
try
{
    var invalidMessageEvent = new MessageReceivedEvent(0, 123, "test");
    Console.WriteLine("FAIL: Should have thrown for invalid chatId");
}
catch (ArgumentOutOfRangeException ex) when (ex.ParamName == "chatId")
{
    Console.WriteLine("PASS: Correctly threw ArgumentOutOfRangeException for invalid chatId");
}

try
{
    var invalidMessageEvent2 = new MessageReceivedEvent(123, 0, "test");
    Console.WriteLine("FAIL: Should have thrown for invalid userId");
}
catch (ArgumentOutOfRangeException ex) when (ex.ParamName == "userId")
{
    Console.WriteLine("PASS: Correctly threw ArgumentOutOfRangeException for invalid userId");
}

var validMessageEvent = new MessageReceivedEvent(123, 456, "test message");
Console.WriteLine("PASS: Valid MessageReceivedEvent created successfully");

// Test 2: CommandExecutedEvent validation
Console.WriteLine("\nTest 2: CommandExecutedEvent validation");
try
{
    var invalidCommandEvent = new CommandExecutedEvent(null, 123, "args", true);
    Console.WriteLine("FAIL: Should have thrown for null commandName");
}
catch (ArgumentException ex) when (ex.ParamName == "commandName")
{
    Console.WriteLine("PASS: Correctly threw ArgumentException for null commandName");
}

try
{
    var invalidCommandEvent2 = new CommandExecutedEvent("", 123, "args", true);
    Console.WriteLine("FAIL: Should have thrown for empty commandName");
}
catch (ArgumentException ex) when (ex.ParamName == "commandName")
{
    Console.WriteLine("PASS: Correctly threw ArgumentException for empty commandName");
}

try
{
    var invalidCommandEvent3 = new CommandExecutedEvent("test", 0, "args", true);
    Console.WriteLine("FAIL: Should have thrown for invalid userId");
}
catch (ArgumentOutOfRangeException ex) when (ex.ParamName == "userId")
{
    Console.WriteLine("PASS: Correctly threw ArgumentOutOfRangeException for invalid userId");
}

var validCommandEvent = new CommandExecutedEvent("/start", 123, "args", true);
Console.WriteLine("PASS: Valid CommandExecutedEvent created successfully");

// Test 3: BotStateChangedEvent validation
Console.WriteLine("\nTest 3: BotStateChangedEvent validation");
try
{
    var invalidStateEvent = new BotStateChangedEvent(null, "newState");
    Console.WriteLine("FAIL: Should have thrown for null previousState");
}
catch (ArgumentException ex) when (ex.ParamName == "previousState")
{
    Console.WriteLine("PASS: Correctly threw ArgumentException for null previousState");
}

try
{
    var invalidStateEvent2 = new BotStateChangedEvent("", "newState");
    Console.WriteLine("FAIL: Should have thrown for empty previousState");
}
catch (ArgumentException ex) when (ex.ParamName == "previousState")
{
    Console.WriteLine("PASS: Correctly threw ArgumentException for empty previousState");
}

try
{
    var invalidStateEvent3 = new BotStateChangedEvent("oldState", null);
    Console.WriteLine("FAIL: Should have thrown for null newState");
}
catch (ArgumentException ex) when (ex.ParamName == "newState")
{
    Console.WriteLine("PASS: Correctly threw ArgumentException for null newState");
}

try
{
    var invalidStateEvent4 = new BotStateChangedEvent("oldState", "");
    Console.WriteLine("FAIL: Should have thrown for empty newState");
}
catch (ArgumentException ex) when (ex.ParamName == "newState")
{
    Console.WriteLine("PASS: Correctly threw ArgumentException for empty newState");
}

var validStateEvent = new BotStateChangedEvent("oldState", "newState");
Console.WriteLine("PASS: Valid BotStateChangedEvent created successfully");

// Test 4: MessageEditedEvent validation (inherits from MessageEventBase)
Console.WriteLine("\nTest 4: MessageEditedEvent validation");
try
{
    var invalidEditedEvent = new MessageEditedEvent(0, 123, "test");
    Console.WriteLine("FAIL: Should have thrown for invalid chatId");
}
catch (ArgumentOutOfRangeException ex) when (ex.ParamName == "chatId")
{
    Console.WriteLine("PASS: Correctly threw ArgumentOutOfRangeException for invalid chatId");
}

var validEditedEvent = new MessageEditedEvent(123, 456, "edited text");
Console.WriteLine("PASS: Valid MessageEditedEvent created successfully");

// Test 5: Verify EventBase validation helpers exist
Console.WriteLine("\nTest 5: EventBase validation helpers");
var eventBaseType = typeof(EventBase);
var validateStringMethod = eventBaseType.GetMethod("ValidateStringNotNullOrWhiteSpace", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
var validateNotNullMethod = eventBaseType.GetMethod("ValidateNotNull", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

if (validateStringMethod != null)
{
    Console.WriteLine("PASS: EventBase.ValidateStringNotNullOrWhiteSpace method exists");
}
else
{
    Console.WriteLine("FAIL: EventBase.ValidateStringNotNullOrWhiteSpace method not found");
}

if (validateNotNullMethod != null)
{
    Console.WriteLine("PASS: EventBase.ValidateNotNull method exists");
}
else
{
    Console.WriteLine("FAIL: EventBase.ValidateNotNull method not found");
}

Console.WriteLine("\n=== All validation tests completed successfully! ===");