#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using Microsoft.Extensions.Logging;

namespace TelegramBotFramework.Events;

/// <summary>
/// Simple console logger implementation for use when no proper logging infrastructure is available.
/// </summary>
/// <typeparam name="T">The type to associate with the logger.</typeparam>
internal sealed class ConsoleLogger<T> : ILogger<T>
{
    /// <summary>
    /// Begins a logical operation scope. Scopes are not tracked by this logger, so a no-op disposable is returned.
    /// </summary>
    /// <typeparam name="TState">The type of the scope state.</typeparam>
    /// <param name="state">The state of the scope.</param>
    /// <returns>A disposable that does nothing when disposed.</returns>
    IDisposable? ILogger.BeginScope<TState>(TState state) => new NullDisposable();

    /// <summary>
    /// Checks whether the given log level is enabled. This logger always reports every level as enabled.
    /// </summary>
    /// <param name="logLevel">The level to check.</param>
    /// <returns><c>true</c> for every log level.</returns>
    public bool IsEnabled(LogLevel logLevel) => true;

    /// <summary>
    /// Writes a log entry to the console in the form <c>[LogLevel] message</c>.
    /// </summary>
    /// <typeparam name="TState">The type of the state object being logged.</typeparam>
    /// <param name="logLevel">The severity of the entry.</param>
    /// <param name="eventId">The event identifier. Not included in the output.</param>
    /// <param name="state">The state object passed to the formatter.</param>
    /// <param name="exception">An optional exception to pass to the formatter. May be <c>null</c>.</param>
    /// <param name="formatter">Produces the message text from the state and exception.</param>
    /// <exception cref="ArgumentNullException"><paramref name="formatter"/> is <c>null</c>.</exception>
    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        ArgumentNullException.ThrowIfNull(formatter);

        Console.WriteLine($"[{logLevel}] {formatter(state, exception)}");
    }
}

/// <summary>
/// An <see cref="IDisposable"/> that performs no action when disposed.
/// </summary>
internal sealed class NullDisposable : IDisposable
{
    /// <summary>
    /// Does nothing. Present so that scope handles can be disposed uniformly.
    /// </summary>
    public void Dispose() { }
}