#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Attributes;

/// <summary>
/// Marks a command handler with a cooldown period to prevent abuse.
/// When applied, the same user cannot invoke the same command more frequently than the specified cooldown.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class CooldownAttribute : Attribute
{
    /// <summary>
    /// The cooldown period in seconds.
    /// </summary>
    public int Seconds { get; }

    /// <summary>
    /// Initializes a new instance of the CooldownAttribute.
    /// </summary>
    /// <param name="seconds">Cooldown period in seconds.</param>
    public CooldownAttribute(int seconds)
    {
        if (seconds <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(seconds), "Cooldown must be greater than 0 seconds.");
        }

        Seconds = seconds;
    }
}