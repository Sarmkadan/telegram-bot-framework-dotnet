#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================
using System;

namespace TelegramBotFramework.Tests;

/// <summary>
/// Constants for CommandUsageTrackerTests.
/// </summary>
internal static class CommandUsageTrackerTestsConstants
{
    public const string TestCommand = "/test";
    public const string OtherCommand = "/other";
    public const string Test2Command = "/test2";
    public const string LeastCommand = "/least";
    public const string MostCommand = "/most";
    public const string MiddleCommand = "/middle";
    public const string NonexistentCommand = "/nonexistent";
    public const string Test1Command = "/test1";
    public const string TestInputWithoutSlash = "test";
    public const int ShortSleepMilliseconds = 10;
    public const int TopCommandsCount = 10;
    public const int LimitedTopCommandsCount = 2;
    public const int ZeroCount = 0;
    public const int NegativeOneCount = -1;
    public const int TimestampDeltaMilliseconds = 10;
}