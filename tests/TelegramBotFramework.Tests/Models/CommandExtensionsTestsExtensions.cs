using System;

namespace TelegramBotFramework.Tests.Models;

/// <summary>
/// Provides extension methods for <see cref="CommandExtensionsTests"/> that allow
/// programmatic execution of its test methods and reporting of their success.
/// </summary>
public static class CommandExtensionsTestsExtensions
{
    /// <summary>
    /// Executes the <c>HasParameters</c> test and returns <c>true</c> if the test completes without throwing an exception.
    /// </summary>
    /// <param name="test">The <see cref="CommandExtensionsTests"/> instance on which to run the test.</param>
    /// <param name="hasParameters"><c>true</c> to run the test that expects parameters; <c>false</c> to run the test that expects no parameters.</param>
    /// <returns><c>true</c> if the selected test method finishes without throwing; otherwise <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="test"/> is <c>null</c>.</exception>
    public static bool RunHasParametersTest(this CommandExtensionsTests test, bool hasParameters)
    {
        ArgumentNullException.ThrowIfNull(test);

        try
        {
            if (hasParameters)
            {
                test.HasParameters_CommandHasParameters_ReturnsTrue();
            }
            else
            {
                test.HasParameters_CommandHasNoParameters_ReturnsFalse();
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Executes the <c>GetPrimaryPattern</c> test and returns <c>true</c> if the test completes without throwing an exception.
    /// </summary>
    /// <param name="test">The <see cref="CommandExtensionsTests"/> instance on which to run the test.</param>
    /// <returns><c>true</c> if the test method finishes without throwing; otherwise <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="test"/> is <c>null</c>.</exception>
    public static bool RunGetPrimaryPatternTest(this CommandExtensionsTests test)
    {
        ArgumentNullException.ThrowIfNull(test);

        try
        {
            test.GetPrimaryPattern_CommandHasName_ReturnsName();
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Executes the <c>IsStandardCommand</c> test and returns <c>true</c> if the test completes without throwing an exception.
    /// </summary>
    /// <param name="test">The <see cref="CommandExtensionsTests"/> instance on which to run the test.</param>
    /// <returns><c>true</c> if the test method finishes without throwing; otherwise <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="test"/> is <c>null</c>.</exception>
    public static bool RunIsStandardCommandTest(this CommandExtensionsTests test)
    {
        ArgumentNullException.ThrowIfNull(test);

        try
        {
            test.IsStandardCommand_CommandIsStandard_ReturnsTrue();
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Executes the <c>GetFormattedString</c> test and returns <c>true</c> if the test completes without throwing an exception.
    /// </summary>
    /// <param name="test">The <see cref="CommandExtensionsTests"/> instance on which to run the test.</param>
    /// <returns><c>true</c> if the test method finishes without throwing; otherwise <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="test"/> is <c>null</c>.</exception>
    public static bool RunGetFormattedStringTest(this CommandExtensionsTests test)
    {
        ArgumentNullException.ThrowIfNull(test);

        try
        {
            test.GetFormattedString_CommandHasDetails_ReturnsFormattedString();
            return true;
        }
        catch
        {
            return false;
        }
    }
}
