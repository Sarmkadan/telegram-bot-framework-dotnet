#nullable enable
namespace TelegramBotFramework.Middleware
{
    using TelegramBotFramework.Models;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IBotLoggingMiddleware
    {
        int Priority { get; }
        Task<TelegramBotFramework.Models.ExecutionContext> ProcessAsync(
            TelegramBotFramework.Models.ExecutionContext context,
            Func<TelegramBotFramework.Models.ExecutionContext, Task<TelegramBotFramework.Models.ExecutionContext>> next,
            CancellationToken cancellationToken = default);
    }
}