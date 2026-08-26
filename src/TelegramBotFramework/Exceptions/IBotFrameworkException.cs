namespace TelegramBotFramework.Exceptions;

/// <summary>
/// Interface for bot framework exceptions.
/// </summary>
public interface IBotFrameworkException
{
    string? ErrorCode { get; set; }
}