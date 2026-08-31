#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;

namespace TelegramBotFramework.Tests;

/// <summary>
/// Constants used in StringExtensionTests to avoid magic values.
/// </summary>
internal static class StringExtensionTestsConstants
{
    // Test strings for Truncate method
    public const string HelloWorld = "Hello World";
    public const string Hi = "Hi";
    public const string Short = "Short";
    public const string TruncatedHelloWorld = "Hell…";

    // Test strings for IsValidEmail method
    public const string ValidEmail = "user@example.com";
    public const string MissingAtSign = "userexample.com";
    public const string MissingDomain = "user@";
    public const string Empty = "";

    // Test strings for Repeat method
    public const string Ab = "ab";
    public const string Abc = "abc";
    public const int RepeatCount = 3;
    public const int ZeroCount = 0;
    public const int NegativeCount = -1;
    public const string RepeatedAb = "ababab";

    // Test strings for ExtractNumbers method
    public const string MixedString = "abc123def456";
    public const string LettersOnly = "abcdef";
    public const string ExtractedNumbers = "123456";

    // Test strings for EnsureStartsWith method
    public const string ExampleCom = "example.com";
    public const string HttpsPrefix = "https://";
    public const string HttpsExampleCom = "https://example.com";

    // Test strings for EnsureEndsWith method
    public const string Hello = "hello";
    public const string Exclamation = "!";
    public const string HelloWithExclamation = "hello!";

    // Test strings for Capitalize method
    public const string HelloWorldLowercase = "hello world";
    public const string HelloCapitalized = "Hello";
    public const string HelloWorldCapitalized = "Hello world";

    // Test strings for IsAlphanumeric method
    public const string Alphanumeric = "abc123";
    public const string WithSpecialChars = "abc!123";
    public const string WithSpaces = "abc 123";

    // Test strings for Reverse method
    public const string Palindrome = "racecar";
    public const string Asymmetric = "hello";
    public const string ReversedAsymmetric = "olleh";

    // Test numbers for Truncate method
    public const int ShortMaxLength = 5;
    public const int NormalMaxLength = 10;
}