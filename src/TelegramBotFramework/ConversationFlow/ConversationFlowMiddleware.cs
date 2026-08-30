#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;
using TelegramBotFramework.Middleware;
using TelegramBotFramework.Services;

namespace TelegramBotFramework.ConversationFlow.Middleware;

/// <summary>
/// Pipeline middleware that intercepts incoming messages for users who have an active
/// conversation flow. When a flow is in progress, the raw message text is forwarded
/// directly to <see cref="IConversationFlowEngine.ProcessInputAsync"/> instead of the
/// normal command dispatcher, and the resulting prompt is attached to the execution context.
/// </summary>
/// <remarks>
/// This middleware runs at priority 85, which places it between
/// <c>AuthorizationMiddleware</c> (90) and <c>RateLimitMiddleware</c> (95).
/// Messages that begin with the abort keyword (e.g. <c>/cancel</c>) are also short-circuited
/// here; the engine aborts the flow and control does not reach the command layer.
/// </remarks>
public sealed class ConversationFlowMiddleware : IBotMiddleware
{
    /// <summary>
    /// Execution priority within the middleware pipeline.
    /// Higher values run earlier. 85 places this after authorization and before rate-limiting.
    /// </summary>
    public int Priority => ConversationFlowMiddlewareConstants.Priority;

    private readonly IConversationFlowEngine _flowEngine;
    private readonly ILogger<ConversationFlowMiddleware> _logger;

    /// <summary>
    /// Execution context state-bag key under which the <see cref="FlowStepResult"/> is stored
    /// after processing. Downstream middleware and handlers can read this to obtain the
    /// next prompt text and quick-reply suggestions.
    /// </summary>
    public const string FlowResultContextKey = "flow_step_result";

    /// <summary>
    /// Execution context state-bag key that signals the rest of the pipeline to skip
    /// normal command resolution because the message was consumed by the flow engine.
    /// </summary>
    public const string FlowHandledContextKey = "flow_handled";

    /// <summary>
    /// Initialises a new instance of <see cref="ConversationFlowMiddleware"/>.
    /// </summary>
    public ConversationFlowMiddleware(
        IConversationFlowEngine flowEngine,
        ILogger<ConversationFlowMiddleware> logger)
    {
        _flowEngine = flowEngine ?? throw new ArgumentNullException(nameof(flowEngine));
        _logger     = logger     ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<Models.ExecutionContext> ProcessAsync(
        Models.ExecutionContext context,
        Func<Models.ExecutionContext, Task<Models.ExecutionContext>> next,
        CancellationToken cancellationToken = default)
    {
        // Only intercept text messages; pass everything else through.
        var messageContent = context.Message?.Content;
        if (string.IsNullOrEmpty(messageContent))
            return await next(context);

        var isInFlow = await _flowEngine.IsUserInFlowAsync(context.UserId, cancellationToken);
        if (!isInFlow)
            return await next(context);

        _logger.LogDebug(
            "ConversationFlowMiddleware intercepting message — UserId: {UserId}, ContextId: {ContextId}",
            context.UserId, context.ContextId);

        FlowStepResult result;
        try
        {
            result = await _flowEngine.ProcessInputAsync(context.UserId, messageContent, cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            // Engine lost its state (e.g. process restart). Fall through to normal pipeline.
            _logger.LogWarning(ex,
                "Flow engine returned an error for UserId {UserId} — falling through to normal pipeline",
                context.UserId);
            return await next(context);
        }

        // Attach the result so downstream components (e.g. a response sender) can act on it.
        context.SetState(FlowResultContextKey, result);
        context.SetState(FlowHandledContextKey, true);

        if (result.IsCompleted)
        {
            _logger.LogInformation(
                "Flow reached terminal state for UserId {UserId} — IsCompleted: true, FlowId: {FlowId}",
                context.UserId, result.FlowState.FlowId);
        }
        else if (!result.IsValid)
        {
            _logger.LogDebug(
                "Flow validation failed for UserId {UserId} — Error: {Error}",
                context.UserId, result.ValidationError);
        }

        // Do NOT call next() — message has been fully consumed by the flow engine.
        return context;
    }
}