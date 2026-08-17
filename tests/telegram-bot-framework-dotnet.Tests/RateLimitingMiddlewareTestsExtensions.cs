using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace TelegramBotFramework.Middleware.Tests
{
    /// <summary>
    /// Extension methods that aid in executing and analysing <see cref="RateLimitingMiddlewareTests"/>.
    /// </summary>
    public static class RateLimitingMiddlewareTestsExtensions
    {
        /// <summary>
        /// Retrieves the names of all public test methods that return <see cref="Task"/>
        /// and whose name starts with <c>ProcessAsync_</c>.
        /// </summary>
        /// <param name="tests">The test instance.</param>
        /// <returns>An <see cref="IReadOnlyList{T}"/> of method names.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tests"/> is <c>null</c>.</exception>
        public static IReadOnlyList<string> GetTestMethodNames(this RateLimitingMiddlewareTests tests)
        {
            ArgumentNullException.ThrowIfNull(tests);

            return typeof(RateLimitingMiddlewareTests)
                .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Where(m => m.ReturnType == typeof(Task) && m.Name.StartsWith("ProcessAsync_", StringComparison.Ordinal))
                .Select(m => m.Name)
                .ToArray();
        }

        /// <summary>
        /// Executes all <c>ProcessAsync_*</c> test methods on the supplied <paramref name="tests"/>
        /// instance and returns a collection describing the outcome of each test.
        /// </summary>
        /// <param name="tests">The test instance.</param>
        /// <returns>
        /// A read‑only list of tuples containing the test name, a boolean indicating success,
        /// and the exception that caused a failure (if any).
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="tests"/> is <c>null</c>.</exception>
        public static async Task<IReadOnlyList<(string TestName, bool Passed, Exception? Exception)>> RunAllAsync(
            this RateLimitingMiddlewareTests tests)
        {
            ArgumentNullException.ThrowIfNull(tests);

            var results = new List<(string TestName, bool Passed, Exception? Exception)>();
            var methods = typeof(RateLimitingMiddlewareTests)
                .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Where(m => m.ReturnType == typeof(Task) && m.Name.StartsWith("ProcessAsync_", StringComparison.Ordinal));

            foreach (var method in methods)
            {
                try
                {
                    var task = (Task)method.Invoke(tests, null)!;
                    await task.ConfigureAwait(false);
                    results.Add((method.Name, true, null));
                }
                catch (TargetInvocationException tie) when (tie.InnerException is not null)
                {
                    results.Add((method.Name, false, tie.InnerException));
                }
                catch (Exception ex)
                {
                    results.Add((method.Name, false, ex));
                }
            }

            return results;
        }

        /// <summary>
        /// Executes all <c>ProcessAsync_*</c> test methods and throws an <see cref="AggregateException"/>
        /// if any of them fail.
        /// </summary>
        /// <param name="tests">The test instance.</param>
        /// <exception cref="ArgumentNullException"><paramref name="tests"/> is <c>null</c>.</exception>
        /// <exception cref="AggregateException">One or more tests threw an exception.</exception>
        public static async Task VerifyAllPassAsync(this RateLimitingMiddlewareTests tests)
        {
            ArgumentNullException.ThrowIfNull(tests);

            var results = await tests.RunAllAsync().ConfigureAwait(false);
            var failures = results
                .Where(r => !r.Passed)
                .Select(r => r.Exception!)
                .ToArray();

            if (failures.Length > 0)
                throw new AggregateException("One or more RateLimitingMiddlewareTests failed.", failures);
        }
    }
}
