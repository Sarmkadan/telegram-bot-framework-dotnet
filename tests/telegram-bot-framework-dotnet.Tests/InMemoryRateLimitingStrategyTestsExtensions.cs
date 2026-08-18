using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TelegramBotFramework.Strategies.Tests
{
    /// <summary>
    /// Extension methods that simplify executing the individual test cases of <see cref="InMemoryRateLimitingStrategyTests"/>.
    /// </summary>
    public static class InMemoryRateLimitingStrategyTestsExtensions
    {
        /// <summary>
        /// Executes the three boundary‑condition tests that verify request expiration behaviour.
        /// </summary>
        /// <param name="tests">The test instance on which to invoke the methods.</param>
        /// <returns>
        /// An <see cref="IReadOnlyList{T}"/> of tuples containing the test name, a flag indicating whether the test passed,
        /// and any exception that was thrown (or <c>null</c> if the test succeeded).
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="tests"/> is <c>null</c>.</exception>
        public static IReadOnlyList<(string TestName, bool Passed, Exception? Exception)> RunBoundaryExpirationTests(this InMemoryRateLimitingStrategyTests tests)
        {
            ArgumentNullException.ThrowIfNull(tests);

            var results = new List<(string TestName, bool Passed, Exception? Exception)>();

            void Execute(string name, Action action)
            {
                try
                {
                    action();
                    results.Add((name, true, null));
                }
                catch (Exception ex)
                {
                    results.Add((name, false, ex));
                }
            }

            Execute(nameof(tests.IsRequestAllowed_RequestsAtWindowBoundaryAreExpired), tests.IsRequestAllowed_RequestsAtWindowBoundaryAreExpired);
            Execute(nameof(tests.IsRequestAllowed_RequestsInsideWindowAreNotExpired), tests.IsRequestAllowed_RequestsInsideWindowAreNotExpired);
            Execute(nameof(tests.IsRequestAllowed_RequestsOutsideWindowAreExpired), tests.IsRequestAllowed_RequestsOutsideWindowAreExpired);

            return results;
        }

        /// <summary>
        /// Executes the test that validates the handling of remaining‑request calculations at boundary conditions.
        /// </summary>
        /// <param name="tests">The test instance on which to invoke the method.</param>
        /// <returns>
        /// A tuple containing the test name, a flag indicating success, and any exception that was thrown.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="tests"/> is <c>null</c>.</exception>
        public static (string TestName, bool Passed, Exception? Exception) RunRemainingRequestsBoundaryTest(this InMemoryRateLimitingStrategyTests tests)
        {
            ArgumentNullException.ThrowIfNull(tests);

            try
            {
                tests.GetRemainingRequests_HandlesBoundaryConditionsCorrectly();
                return (nameof(tests.GetRemainingRequests_HandlesBoundaryConditionsCorrectly), true, null);
            }
            catch (Exception ex)
            {
                return (nameof(tests.GetRemainingRequests_HandlesBoundaryConditionsCorrectly), false, ex);
            }
        }

        /// <summary>
        /// Executes the asynchronous boundary‑condition test for <c>IsActionAllowedAsync</c>.
        /// </summary>
        /// <param name="tests">The test instance on which to invoke the method.</param>
        /// <returns>
        /// A task that resolves to a tuple containing the test name, a flag indicating success, and any exception that was thrown.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="tests"/> is <c>null</c>.</exception>
        public static async Task<(string TestName, bool Passed, Exception? Exception)> RunAsyncBoundaryTest(this InMemoryRateLimitingStrategyTests tests)
        {
            ArgumentNullException.ThrowIfNull(tests);

            try
            {
                await tests.IsActionAllowedAsync_HandlesBoundaryConditionsCorrectly().ConfigureAwait(false);
                return (nameof(tests.IsActionAllowedAsync_HandlesBoundaryConditionsCorrectly), true, null);
            }
            catch (Exception ex)
            {
                return (nameof(tests.IsActionAllowedAsync_HandlesBoundaryConditionsCorrectly), false, ex);
            }
        }

        /// <summary>
        /// Executes the test that ensures different identifiers are limited independently.
        /// </summary>
        /// <param name="tests">The test instance on which to invoke the method.</param>
        /// <returns>
        /// A tuple containing the test name, a flag indicating success, and any exception that was thrown.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="tests"/> is <c>null</c>.</exception>
        public static (string TestName, bool Passed, Exception? Exception) RunIdentifierIsolationTest(this InMemoryRateLimitingStrategyTests tests)
        {
            ArgumentNullException.ThrowIfNull(tests);

            try
            {
                tests.IsRequestAllowed_DifferentIdentifiersLimitedIndependently();
                return (nameof(tests.IsRequestAllowed_DifferentIdentifiersLimitedIndependently), true, null);
            }
            catch (Exception ex)
            {
                return (nameof(tests.IsRequestAllowed_DifferentIdentifiersLimitedIndependently), false, ex);
            }
        }
    }
}
