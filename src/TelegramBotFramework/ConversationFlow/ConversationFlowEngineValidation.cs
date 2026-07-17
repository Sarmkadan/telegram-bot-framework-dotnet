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
    /// Validates the specified <see cref="ConversationFlowEngine"/> instance asynchronously.
    /// </summary>
    /// <param name="value">The engine instance to validate.</param>
    /// <returns>An empty list if valid; otherwise, a list of human-readable validation errors.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static async Task<IReadOnlyList<string>> ValidateAsync(this ConversationFlowEngine value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate public API contract through exception testing
        // This validates that required dependencies are properly initialized
        try
        {
            // Test that basic operations don't throw null reference exceptions
            _ = await value.GetAllFlowsAsync().ConfigureAwait(false);
            _ = await value.GetActiveFlowStateAsync(12345).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException and not ObjectDisposedException)
        {
            errors.Add($"ConversationFlowEngine failed basic operation test: {ex.Message}");
        }

        // Validate that flows collection is accessible and not corrupted
        try
        {
            var flows = await value.GetAllFlowsAsync().ConfigureAwait(false);
            if (flows == null)
            {
                errors.Add("ConversationFlowEngine.GetAllFlowsAsync() returned null.");
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException and not ObjectDisposedException)
        {
            errors.Add($"ConversationFlowEngine flow retrieval failed: {ex.Message}");
        }

        // Validate that active states collection is accessible
        try
        {
            var state = await value.GetActiveFlowStateAsync(12345).ConfigureAwait(false);
            // null is acceptable for non-existent states
        }
        catch (Exception ex) when (ex is not OperationCanceledException and not ObjectDisposedException)
        {
            errors.Add($"ConversationFlowEngine state retrieval failed: {ex.Message}");
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
        return ValidateAsync(value).GetAwaiter().GetResult().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="ConversationFlowEngine"/> instance is valid.
    /// </summary>
    /// <param name="value">The engine instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the instance is not valid, containing a list of validation errors in the exception message.</exception>
    public static void EnsureValid(this ConversationFlowEngine value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = ValidateAsync(value).GetAwaiter().GetResult();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"ConversationFlowEngine is not valid. Validation errors:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}",
                nameof(value));
        }
    }
}