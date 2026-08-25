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
        string directory = value.GetDirectory();
        if (string.IsNullOrWhiteSpace(directory))
        {
            problems.Add("FileConversationStateStoreValidationConstants.DirectoryPathCannotBeNullOrWhitespace");
        }
        else
        {
            try
            {
                if (!Directory.Exists(directory) && !string.IsNullOrEmpty(directory))
                {
                    problems.Add("FileConversationStateStoreValidationConstants.ConfiguredDirectoryDoesNotExist");
                }
            }
            catch (Exception ex) when (ex is not ArgumentNullException and not ArgumentException)
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
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static bool IsValid(this FileConversationStateStore value)
    {
        return value.Validate().Count is 0;
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

        var problems = value.Validate();
        if (problems.Count is 0)
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
            problems.Add("FileConversationStateStoreValidationConstants.StateIdCannotBeNullOrWhitespace");
        }

        if (string.IsNullOrWhiteSpace(state.FlowId))
        {
            problems.Add("FileConversationStateStoreValidationConstants.FlowIdCannotBeNullOrWhitespace");
        }

        if (state.UserId == default)
        {
            problems.Add("FileConversationStateStoreValidationConstants.UserIdMustBeNonDefault");
        }

        if (state.ChatId == default)
        {
            problems.Add("FileConversationStateStoreValidationConstants.ChatIdMustBeNonDefault");
        }

        if (string.IsNullOrWhiteSpace(state.CurrentStepId))
        {
            problems.Add("FileConversationStateStoreValidationConstants.CurrentStepIdCannotBeNullOrWhitespace");
        }

        // Validate dates
        if (state.StartedAt == default)
        {
            problems.Add("FileConversationStateStoreValidationConstants.StartedAtCannotBeDefault");
        }
        else if (state.StartedAt.Kind is not DateTimeKind.Utc)
        {
            problems.Add("FileConversationStateStoreValidationConstants.StartedAtMustBeUtc");
        }

        if (state.LastActivityAt == default)
        {
            problems.Add("FileConversationStateStoreValidationConstants.LastActivityAtCannotBeDefault");
        }
        else if (state.LastActivityAt.Kind is not DateTimeKind.Utc)
        {
            problems.Add("FileConversationStateStoreValidationConstants.LastActivityAtMustBeUtc");
        }

        if (state.CompletedAt.HasValue)
        {
            if (state.CompletedAt.Value == default)
            {
                problems.Add("FileConversationStateStoreValidationConstants.CompletedAtCannotBeDefaultWhenSet");
            }
            else if (state.CompletedAt.Value.Kind is not DateTimeKind.Utc)
            {
                problems.Add("FileConversationStateStoreValidationConstants.CompletedAtMustBeUtcWhenSet");
            }

            if (state.CompletedAt.Value < state.StartedAt)
            {
                problems.Add("FileConversationStateStoreValidationConstants.CompletedAtCannotBeEarlierThanStartedAt");
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
            problems.Add("FileConversationStateStoreValidationConstants.VariablesDictionaryCannotBeNull");
        }

        if (state.History == null)
        {
            problems.Add("FileConversationStateStoreValidationConstants.HistoryListCannotBeNull");
        }

        // Validate history entries
        if (state.History != null)
        {
            foreach (var entry in state.History)
            {
                if (entry == null)
                {
                    problems.Add("FileConversationStateStoreValidationConstants.HistoryContainsNullEntry");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(entry.StepId))
                {
                    problems.Add("FileConversationStateStoreValidationConstants.HistoryEntryStepIdCannotBeNullOrWhitespace");
                }

                if (entry.EnteredAt == default)
                {
                    problems.Add("FileConversationStateStoreValidationConstants.HistoryEntryEnteredAtCannotBeDefault");
                }
                else if (entry.EnteredAt.Kind is not DateTimeKind.Utc)
                {
                    problems.Add("FileConversationStateStoreValidationConstants.HistoryEntryEnteredAtMustBeUtc");
                }

                if (entry.CompletedAt.HasValue && entry.CompletedAt.Value.Kind is not DateTimeKind.Utc)
                {
                    problems.Add("History entry FileConversationStateStoreValidationConstants.CompletedAtMustBeUtcWhenSet");
                }

                if (entry.CompletedAt.HasValue && entry.CompletedAt.Value < entry.EnteredAt)
                {
                    problems.Add("FileConversationStateStoreValidationConstants.HistoryEntryCompletedAtCannotBeEarlierThanEnteredAt");
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
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="state"/> is <see langword="null"/>.</exception>
    public static bool IsValid(this UserFlowState state)
    {
        return state.Validate().Count is 0;
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

        var problems = state.Validate();
        if (problems.Count is 0)
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
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="store"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when reflection fails to access the private field.</exception>
    private static string GetDirectory(this FileConversationStateStore store)
    {
        ArgumentNullException.ThrowIfNull(store);

        // Use reflection to access the private _directory field
        var field = typeof(FileConversationStateStore).GetField(
            "_directory",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        return field?.GetValue(store) as string
            ?? throw new InvalidOperationException("Failed to access the private _directory field via reflection.");
    }
}