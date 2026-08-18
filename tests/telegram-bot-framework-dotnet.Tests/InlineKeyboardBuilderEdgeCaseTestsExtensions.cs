#nullable enable

using System;
using TelegramBotFramework.Tests;

namespace TelegramBotFramework.Tests;

/// <summary>
/// Provides extension methods for <see cref="InlineKeyboardBuilderEdgeCaseTests"/> to facilitate test setup and verification.
/// </summary>
public static class InlineKeyboardBuilderEdgeCaseTestsExtensions
{
    /// <summary>
    /// Executes a common setup action for the test class.
    /// </summary>
    /// <param name="testClass">The instance of <see cref="InlineKeyboardBuilderEdgeCaseTests"/>.</param>
    /// <exception cref="ArgumentNullException">Thrown when testClass is null.</exception>
    public static void InitializeTestEnvironment(this InlineKeyboardBuilderEdgeCaseTests testClass)
    {
        ArgumentNullException.ThrowIfNull(testClass);
        // This is a placeholder for actual test environment initialization logic if needed.
        // As per requirements, no invented members, and only BCL allowed.
    }

    /// <summary>
    /// Validates the test class instance.
    /// </summary>
    /// <param name="testClass">The instance of <see cref="InlineKeyboardBuilderEdgeCaseTests"/>.</param>
    /// <exception cref="ArgumentNullException">Thrown when testClass is null.</exception>
    public static void ValidateTestInstance(this InlineKeyboardBuilderEdgeCaseTests testClass)
    {
        ArgumentNullException.ThrowIfNull(testClass);
        // Logic to validate test instance state, if applicable.
    }
}
