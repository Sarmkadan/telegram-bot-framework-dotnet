#nullable disable

namespace TelegramBotFramework.Services
{
    /// <summary>
    /// Validation helpers for <see cref="BroadcastProgress"/>.
    /// </summary>
    public static class BroadcastProgressValidation
    {
        /// <summary>
        /// Validates the given <paramref name="value"/> and returns a list of problems.
        /// </summary>
        /// <param name="value">The progress instance to validate.</param>
        /// <returns>A list of human-readable problems (empty if valid).</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this BroadcastProgress value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = new List<string>();

            if (value.TotalChats < 1)
                problems.Add("TotalChats must be at least 1.");

            if (value.ProcessedCount < 0)
                problems.Add("ProcessedCount cannot be negative.");
            if (value.ProcessedCount > value.TotalChats)
                problems.Add("ProcessedCount cannot exceed TotalChats.");

            if (value.SuccessCount < 0)
                problems.Add("SuccessCount cannot be negative.");
            if (value.SuccessCount > value.ProcessedCount)
                problems.Add("SuccessCount cannot exceed ProcessedCount.");

            if (value.FailedCount < 0)
                problems.Add("FailedCount cannot be negative.");
            if (value.FailedCount > value.ProcessedCount)
                problems.Add("FailedCount cannot exceed ProcessedCount.");

            if (value.Failures != null)
            {
                if (value.Failures.Count > 0 && value.FailedCount == 0)
                    problems.Add("Failures list should be empty when FailedCount is 0.");
                if (value.FailedCount > 0 && value.Failures.Count == 0)
                    problems.Add("Failures list should not be empty when FailedCount > 0.");
            }

            if (value.ElapsedTime.TotalSeconds < 0)
                problems.Add("ElapsedTime cannot be negative.");

            if (value.EstimatedTimeRemaining.HasValue && value.EstimatedTimeRemaining.Value.TotalSeconds < 0)
                problems.Add("EstimatedTimeRemaining cannot be negative.");

            if (value.CurrentMessagesPerSecond < 0)
                problems.Add("CurrentMessagesPerSecond cannot be negative.");

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Checks if the progress instance is valid.
        /// </summary>
        /// <param name="value">The progress instance to check.</param>
        /// <returns>True if valid, false otherwise.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static bool IsValid(this BroadcastProgress value)
        {
            ArgumentNullException.ThrowIfNull(value);
            return Validate(value).Count == 0;
        }

        /// <summary>
        /// Ensures the progress instance is valid, throwing if not.
        /// </summary>
        /// <param name="value">The progress instance to validate.</param>
        /// <exception cref="ArgumentException">Thrown if validation fails.</exception>
        public static void EnsureValid(this BroadcastProgress value)
        {
            var problems = Validate(value);
            if (problems.Count > 0)
            {
                throw new ArgumentException("Validation failed: " + string.Join("; ", problems));
            }
        }
    }
}
