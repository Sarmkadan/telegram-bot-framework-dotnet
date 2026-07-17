using System;
using System.Threading;
using System.Threading.Tasks;

namespace TelegramBotFramework.ConversationFlow
{
    /// <summary>
    /// Extension methods that simplify configuring <see cref="ConversationFlowOptions"/>.
    /// </summary>
    public static class ConversationFlowOptionsExtensions
    {
        /// <summary>
        /// Sets the default timeout for a flow.
        /// </summary>
        /// <param name="options">The options instance to configure.</param>
        /// <param name="timeout">The timeout to apply.</param>
        /// <returns>The same <see cref="ConversationFlowOptions"/> instance, enabling fluent configuration.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="timeout"/> is less than or equal to <see cref="TimeSpan.Zero"/>.</exception>
        public static ConversationFlowOptions WithDefaultFlowTimeout(this ConversationFlowOptions options, TimeSpan timeout)
        {
            ArgumentNullException.ThrowIfNull(options);
            if (timeout <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(timeout), "Timeout must be greater than zero.");
            }

            options.DefaultFlowTimeout = timeout;
            return options;
        }

        /// <summary>
        /// Sets the maximum number of active flows a single user may have simultaneously.
        /// </summary>
        /// <param name="options">The options instance to configure.</param>
        /// <param name="maxActiveFlows">The maximum number of active flows per user.</param>
        /// <returns>The same <see cref="ConversationFlowOptions"/> instance, enabling fluent configuration.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxActiveFlows"/> is less than 1.</exception>
        public static ConversationFlowOptions WithMaxActiveFlowsPerUser(this ConversationFlowOptions options, int maxActiveFlows)
        {
            ArgumentNullException.ThrowIfNull(options);
            if (maxActiveFlows < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(maxActiveFlows), "There must be at least one active flow per user.");
            }

            options.MaxActiveFlowsPerUser = maxActiveFlows;
            return options;
        }

        /// <summary>
        /// Enables an abort keyword and its acknowledgement message.
        /// </summary>
        /// <param name="options">The options instance to configure.</param>
        /// <param name="abortKeyword">The keyword that a user can send to abort a flow.</param>
        /// <param name="acknowledgementMessage">The message sent back when the flow is aborted.</param>
        /// <returns>The same <see cref="ConversationFlowOptions"/> instance, enabling fluent configuration.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="abortKeyword"/> or <paramref name="acknowledgementMessage"/> is <c>null</c> or empty.</exception>
        public static ConversationFlowOptions EnableAbortKeyword(this ConversationFlowOptions options, string abortKeyword, string acknowledgementMessage)
        {
            ArgumentNullException.ThrowIfNull(options);
            ArgumentException.ThrowIfNullOrEmpty(abortKeyword);
            ArgumentException.ThrowIfNullOrEmpty(acknowledgementMessage);

            options.AbortKeyword = abortKeyword;
            options.AbortAcknowledgementMessage = acknowledgementMessage;
            return options;
        }

        /// <summary>
        /// Validates the configuration and throws if any required setting is invalid.
        /// </summary>
        /// <param name="options">The options instance to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <see cref="ConversationFlowOptions.DefaultFlowTimeout"/> is not positive,
        /// or when <see cref="ConversationFlowOptions.MaxActiveFlowsPerUser"/> is less than 1,
        /// or when <see cref="ConversationFlowOptions.MaxHistoryPerUser"/> is negative,
        /// or when <see cref="ConversationFlowOptions.CleanupIntervalMinutes"/> is less than 1.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <see cref="ConversationFlowOptions.AbortKeyword"/> is set but empty,
        /// or when <see cref="ConversationFlowOptions.AbortAcknowledgementMessage"/> is set but empty.
        /// </exception>
        public static void Validate(this ConversationFlowOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);

            if (options.DefaultFlowTimeout <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(options.DefaultFlowTimeout), "DefaultFlowTimeout must be greater than zero.");
            }

            if (options.MaxActiveFlowsPerUser < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(options.MaxActiveFlowsPerUser), "MaxActiveFlowsPerUser must be at least 1.");
            }

            if (options.MaxHistoryPerUser < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(options.MaxHistoryPerUser), "MaxHistoryPerUser cannot be negative.");
            }

            if (options.CleanupIntervalMinutes < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(options.CleanupIntervalMinutes), "CleanupIntervalMinutes must be at least 1.");
            }

            if (options.AbortKeyword is { Length: > 0 } keyword && string.IsNullOrWhiteSpace(keyword))
            {
                throw new ArgumentException("AbortKeyword cannot be empty when provided.", nameof(options.AbortKeyword));
            }

            if (options.AbortAcknowledgementMessage is { Length: > 0 } message && string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException("AbortAcknowledgementMessage cannot be empty when provided.", nameof(options.AbortAcknowledgementMessage));
            }
        }
    }
}