using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace TelegramBotFramework.Tests
{
    /// <summary>
    /// Extension methods for <see cref="CallbackDataSignerTests"/>.
    /// </summary>
    public static class CallbackDataSignerTestsExtensions
    {
        /// <summary>
        /// Gets the names of all public test methods that start with <c>Sign_</c>.
        /// </summary>
        /// <param name="tests">The test instance.</param>
        /// <returns>An immutable list of method names.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tests"/> is <c>null</c>.</exception>
        public static IReadOnlyList<string> GetSignTestNames(this CallbackDataSignerTests tests)
        {
            ArgumentNullException.ThrowIfNull(tests);
            return typeof(CallbackDataSignerTests)
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .Where(m => m.Name.StartsWith("Sign_", StringComparison.Ordinal))
                .Select(m => m.Name)
                .ToArray();
        }

        /// <summary>
        /// Gets the names of all public test methods that start with <c>TryValidate_</c>.
        /// </summary>
        /// <param name="tests">The test instance.</param>
        /// <returns>An immutable list of method names.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tests"/> is <c>null</c>.</exception>
        public static IReadOnlyList<string> GetValidateTestNames(this CallbackDataSignerTests tests)
        {
            ArgumentNullException.ThrowIfNull(tests);
            return typeof(CallbackDataSignerTests)
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .Where(m => m.Name.StartsWith("TryValidate_", StringComparison.Ordinal))
                .Select(m => m.Name)
                .ToArray();
        }

        /// <summary>
        /// Executes all <c>Sign_</c> test methods and returns a map indicating whether each method completed without throwing.
        /// </summary>
        /// <param name="tests">The test instance.</param>
        /// <returns>
        /// A read‑only dictionary where the key is the method name and the value is <c>true</c> if the method succeeded,
        /// otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="tests"/> is <c>null</c>.</exception>
        public static IReadOnlyDictionary<string, bool> RunAllSignTests(this CallbackDataSignerTests tests)
        {
            ArgumentNullException.ThrowIfNull(tests);
            var result = new Dictionary<string, bool>(StringComparer.Ordinal);
            foreach (var method in typeof(CallbackDataSignerTests)
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .Where(m => m.Name.StartsWith("Sign_", StringComparison.Ordinal)))
            {
                try
                {
                    method.Invoke(tests, null);
                    result[method.Name] = true;
                }
                catch
                {
                    result[method.Name] = false;
                }
            }
            return result;
        }

        /// <summary>
        /// Executes all <c>TryValidate_</c> test methods and returns a map indicating whether each method completed without throwing.
        /// </summary>
        /// <param name="tests">The test instance.</param>
        /// <returns>
        /// A read‑only dictionary where the key is the method name and the value is <c>true</c> if the method succeeded,
        /// otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="tests"/> is <c>null</c>.</exception>
        public static IReadOnlyDictionary<string, bool> RunAllValidateTests(this CallbackDataSignerTests tests)
        {
            ArgumentNullException.ThrowIfNull(tests);
            var result = new Dictionary<string, bool>(StringComparer.Ordinal);
            foreach (var method in typeof(CallbackDataSignerTests)
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .Where(m => m.Name.StartsWith("TryValidate_", StringComparison.Ordinal)))
            {
                try
                {
                    method.Invoke(tests, null);
                    result[method.Name] = true;
                }
                catch
                {
                    result[method.Name] = false;
                }
            }
            return result;
        }
    }
}
