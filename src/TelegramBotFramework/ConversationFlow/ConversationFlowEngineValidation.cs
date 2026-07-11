#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using System.Globalization;
using Microsoft.Extensions.Logging;

namespace TelegramBotFramework.ConversationFlow;

/// <summary>
/// Provides validation helpers for <see cref="ConversationFlowEngine"/> instances.
/// </summary>
public static class ConversationFlowEngineValidation
{
    /// <summary>
    /// Validates the specified <see cref="ConversationFlowEngine"/> instance.
    /// </summary>
    /// <param name="value">The engine instance to validate.</param>
    /// <returns>An empty list if valid; otherwise, a list of human-readable validation errors.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this ConversationFlowEngine value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate public API contract through exception testing
        // This validates that required dependencies are properly initialized

        try
        {
            // Test that basic operations don't throw null reference exceptions
            var _ = value.GetAllFlowsAsync();
            var __ = value.GetActiveFlowStateAsync(12345);
        }
        catch (NullReferenceException)
        {
            errors.Add("ConversationFlowEngine contains null internal references in critical components.");
        }
        catch (ArgumentNullException ex)
        {
            // This indicates a null dependency was passed to constructor
            errors.Add($"ConversationFlowEngine has null dependency: {ex.ParamName}");
        }
        catch (Exception ex) when (ex is not InvalidOperationException and not KeyNotFoundException)
        {
            // Other exceptions may be acceptable depending on context
        }

        // Validate that flows collection is accessible and not corrupted
        try
        {
            var flows = value.GetAllFlowsAsync().Result;
            if (flows == null)
                errors.Add("ConversationFlowEngine.GetAllFlowsAsync() returned null.");
        }
        catch (AggregateException ae) when (ae.InnerException is not null)
        {
            errors.Add($"ConversationFlowEngine flow retrieval failed: {ae.InnerException.Message}");
        }

        // Validate that active states collection is accessible
        try
        {
            var state = value.GetActiveFlowStateAsync(12345).Result;
            // null is acceptable for non-existent states
        }
        catch (AggregateException ae) when (ae.InnerException is not null)
        {
            errors.Add($"ConversationFlowEngine state retrieval failed: {ae.InnerException.Message}");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="ConversationFlowEngine"/> instance is valid.
    /// </summary>
    /// <param name="value">The engine instance to check.</param>
    /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this ConversationFlowEngine value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="ConversationFlowEngine"/> instance is valid.
    /// </summary>
    /// <param name="value">The engine instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the instance is not valid, containing a list of validation errors.</exception>
    public static void EnsureValid(this ConversationFlowEngine value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"ConversationFlowEngine is not valid. Validation errors:\n{string.Join("\n", errors)}",
                nameof(value));
        }
    }
}