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
    IDisposable? ILogger.BeginScope<TState>(TState state) => new NullDisposable();

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        Console.WriteLine($"[{logLevel}] {formatter(state, exception)}");
    }
}

internal sealed class NullDisposable : IDisposable
{
    public void Dispose() { }
}