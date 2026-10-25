#nullable enable
namespace TelegramBotFramework.Middleware
{
    using TelegramBotFramework.Models;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Middleware contract for logging bot update processing within the pipeline.
    /// </summary>
    public interface IBotLoggingMiddleware
    {
        /// <summary>
        /// Gets the execution order of this middleware; lower values run earlier in the pipeline.
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// Logs the incoming execution context, invokes the next middleware, and returns the resulting context.
        /// </summary>
        /// <param name="context">The execution context for the current bot update.</param>
        /// <param name="next">Delegate that invokes the next middleware in the pipeline.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>A task that resolves to the execution context produced by the pipeline.</returns>
        Task<TelegramBotFramework.Models.ExecutionContext> ProcessAsync(
            TelegramBotFramework.Models.ExecutionContext context,
            Func<TelegramBotFramework.Models.ExecutionContext, Task<TelegramBotFramework.Models.ExecutionContext>> next,
            CancellationToken cancellationToken = default);
    }
}