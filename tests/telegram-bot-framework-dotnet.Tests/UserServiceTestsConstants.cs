#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;

namespace TelegramBotFramework.Tests;

/// <summary>
/// Constants used in UserServiceTests to avoid magic values.
/// </summary>
internal static class UserServiceTestsConstants
{
    // Test user IDs
    public const long ExistingUserId = 123;
    public const long NonExistingUserId = 999;

    // Test user IDs for lists
    public const long UserIdOne = 1;
    public const long UserIdTwo = 2;
    public const long UserIdThree = 3;

    // Test names
    public const string FirstNameJohn = "John";
    public const string LastNameDoe = "Doe";
    public const string UsernameJohnDoe = "johndoe";
    public const string FirstNameJane = "Jane";
    public const string LastNameSmith = "Smith";
    public const string FirstNameJohnny = "Johnny";
    public const string LastNameAppleseed = "Appleseed";
    public const string LastNameOld = "OldLast";
    public const string FirstNameOld = "OldName";
    public const string UsernameOldUser = "olduser";
    public const string LastNameSmithUpdated = "Smith";
    public const string UsernameJohnSmith = "johnsmith";

    // Test values for updates
    public const string FirstNameJohnUnchanged = "John";

    // Test time values
    public const int ActivityUpdateToleranceSeconds = 1;

    // Test strings for search
    public const string SearchQueryJohn = "John";
    public const string EmptySearchQuery = "";

    // Test counts
    public const int InitialMessageCount = 0;
    public const int IncrementedMessageCount = 6;
    public const int ExpectedFilteredUsersCount = 2;
    public const int ExpectedAllUsersCount = 2;
    public const int ExpectedActiveUsersCount = 2;

    // Test message counts
    public const int InitialMessageCountForRecordActivity = 5;
    public const int IncrementedMessageCountForRecordActivity = 6;
}