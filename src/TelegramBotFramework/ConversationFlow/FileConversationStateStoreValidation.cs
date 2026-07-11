#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;

namespace TelegramBotFramework.ConversationFlow;

/// <summary>
/// Provides validation helpers for <see cref="FileConversationStateStore"/> instances.
/// Validates the configuration and runtime state of file-based conversation state storage.
/// </summary>
public static class FileConversationStateStoreValidation
{
    /// <summary>
    /// Validates the configuration and runtime state of a <see cref="FileConversationStateStore"/> instance.
    /// </summary>
    /// <param name="value">The store instance to validate.</param>
    /// <returns>An empty list if valid; otherwise, a list of human-readable problem descriptions.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this FileConversationStateStore value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate directory path
        if (string.IsNullOrWhiteSpace(value.GetDirectory()))
        {
            problems.Add("Directory path cannot be null or whitespace.");
        }
        else
        {
            try
            {
                var directory = value.GetDirectory();
                if (!Directory.Exists(directory) && !string.IsNullOrEmpty(directory))
                {
                    problems.Add("Configured directory does not exist and cannot be created automatically.");
                }
            }
            catch (Exception ex)
            {
                problems.Add($"Directory validation failed: {ex.Message}");
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="FileConversationStateStore"/> instance is valid.
    /// </summary>
    /// <param name="value">The store instance to check.</param>
    /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this FileConversationStateStore value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="FileConversationStateStore"/> instance is valid.
    /// </summary>
    /// <param name="value">The store instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when the instance is invalid; the message lists all problems.</exception>
    public static void EnsureValid(this FileConversationStateStore value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count == 0)
            return;

        throw new ArgumentException(
            $"FileConversationStateStore is invalid:{Environment.NewLine}- {
                string.Join(Environment.NewLine + "- ", problems)
            }");
    }

    /// <summary>
    /// Validates a <see cref="UserFlowState"/> instance for persistence.
    /// </summary>
    /// <param name="state">The state to validate.</param>
    /// <returns>An empty list if valid; otherwise, a list of human-readable problem descriptions.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="state"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this UserFlowState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        var problems = new List<string>();

        // Validate required string fields
        if (string.IsNullOrWhiteSpace(state.StateId))
        {
            problems.Add("StateId cannot be null or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(state.FlowId))
        {
            problems.Add("FlowId cannot be null or whitespace.");
        }

        if (state.UserId == default)
        {
            problems.Add("UserId must be a non-default value (cannot be 0).");
        }

        if (state.ChatId == default)
        {
            problems.Add("ChatId must be a non-default value (cannot be 0).");
        }

        if (string.IsNullOrWhiteSpace(state.CurrentStepId))
        {
            problems.Add("CurrentStepId cannot be null or whitespace.");
        }

        // Validate dates
        if (state.StartedAt == default)
        {
            problems.Add("StartedAt cannot be the default DateTime value.");
        }
        else if (state.StartedAt.Kind != DateTimeKind.Utc)
        {
            problems.Add("StartedAt must be in UTC timezone.");
        }

        if (state.LastActivityAt == default)
        {
            problems.Add("LastActivityAt cannot be the default DateTime value.");
        }
        else if (state.LastActivityAt.Kind != DateTimeKind.Utc)
        {
            problems.Add("LastActivityAt must be in UTC timezone.");
        }

        if (state.CompletedAt.HasValue)
        {
            if (state.CompletedAt.Value == default)
            {
                problems.Add("CompletedAt cannot be the default DateTime value when set.");
            }
            else if (state.CompletedAt.Value.Kind != DateTimeKind.Utc)
            {
                problems.Add("CompletedAt must be in UTC timezone when set.");
            }

            if (state.CompletedAt.Value < state.StartedAt)
            {
                problems.Add("CompletedAt cannot be earlier than StartedAt.");
            }
        }

        // Validate status
        if (!Enum.IsDefined(typeof(FlowStateStatus), state.Status))
        {
            problems.Add($"Status '{state.Status}' is not a valid FlowStateStatus value.");
        }

        // Validate collections
        if (state.Variables == null)
        {
            problems.Add("Variables dictionary cannot be null.");
        }

        if (state.History == null)
        {
            problems.Add("History list cannot be null.");
        }

        // Validate history entries
        if (state.History != null)
        {
            foreach (var entry in state.History)
            {
                if (entry == null)
                {
                    problems.Add("History contains a null entry.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(entry.StepId))
                {
                    problems.Add("History entry StepId cannot be null or whitespace.");
                }

                if (entry.EnteredAt == default)
                {
                    problems.Add("History entry EnteredAt cannot be the default DateTime value.");
                }
                else if (entry.EnteredAt.Kind != DateTimeKind.Utc)
                {
                    problems.Add("History entry EnteredAt must be in UTC timezone.");
                }

                if (entry.CompletedAt.HasValue && entry.CompletedAt.Value.Kind != DateTimeKind.Utc)
                {
                    problems.Add("History entry CompletedAt must be in UTC timezone when set.");
                }

                if (entry.CompletedAt.HasValue && entry.CompletedAt.Value < entry.EnteredAt)
                {
                    problems.Add("History entry CompletedAt cannot be earlier than EnteredAt.");
                }
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="UserFlowState"/> instance is valid for persistence.
    /// </summary>
    /// <param name="state">The state to check.</param>
    /// <returns><see langword="true"/> if the state is valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this UserFlowState state)
    {
        return Validate(state).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="UserFlowState"/> instance is valid for persistence.
    /// </summary>
    /// <param name="state">The state to check.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="state"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when the state is invalid; the message lists all problems.</exception>
    public static void EnsureValid(this UserFlowState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        var problems = Validate(state);
        if (problems.Count == 0)
            return;

        throw new ArgumentException(
            $"UserFlowState is invalid:{Environment.NewLine}- {
                string.Join(Environment.NewLine + "- ", problems)
            }");
    }

    /// <summary>
    /// Gets the directory path from a <see cref="FileConversationStateStore"/> instance.
    /// </summary>
    /// <param name="store">The store instance.</param>
    /// <returns>The configured directory path.</returns>
    private static string GetDirectory(this FileConversationStateStore store)
    {
        // Use reflection to access the private _directory field
        var field = typeof(FileConversationStateStore).GetField(
            "_directory",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (string)field!.GetValue(store)!;
    }
}