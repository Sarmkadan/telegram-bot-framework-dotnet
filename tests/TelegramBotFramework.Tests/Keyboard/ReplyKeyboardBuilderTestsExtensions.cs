#nullable enable

using TelegramBotFramework.Tests.Keyboard;
using Xunit;

namespace TelegramBotFramework.Tests.Keyboard;

/// <summary>
/// Provides extension methods for <see cref="ReplyKeyboardBuilderTests"/>.
/// </summary>
public static class ReplyKeyboardBuilderTestsExtensions
{
    /// <summary>
    /// Asserts that a sequence is not empty.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="collection">The collection to check.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="collection"/> is null.</exception>
    public static void AssertCollectionNotEmpty<T>(this ReplyKeyboardBuilderTests tests, IEnumerable<T> collection)
    {
        ArgumentNullException.ThrowIfNull(collection);
        Assert.NotEmpty(collection);
    }

    /// <summary>
    /// Asserts that two objects are the same instance.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="expected">The expected object.</param>
    /// <param name="actual">The actual object.</param>
    public static void AssertSameInstance(this ReplyKeyboardBuilderTests tests, object expected, object actual)
    {
        Assert.Same(expected, actual);
    }
}
