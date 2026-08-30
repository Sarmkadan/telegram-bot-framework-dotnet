#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;
using TelegramBotFramework.Events;

namespace TelegramBotFramework.ConversationFlow.QuizFlow;

/// <summary>
/// A helper class that simplifies creating quiz flows using the conversation flow engine.
/// Provides methods to define quiz questions, track scores, and generate results.
/// </summary>
public sealed class QuizFlowHelper : IDisposable
{
    private readonly ILogger<QuizFlowHelper>? _logger;
    private readonly IEventBus _eventBus;
    private readonly List<QuizQuestion> _questions = [];
    private readonly string _flowId;
    private bool _isDisposed;

    /// <summary>
    /// Gets the flow identifier for this quiz.
    /// </summary>
    public string FlowId => _flowId;

    /// <summary>
    /// Gets the human-readable name of this quiz.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the description of this quiz.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets the completion menu identifier to navigate to after quiz completion.
    /// </summary>
    public string? CompletionMenuId { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="QuizFlowHelper"/> class.
    /// </summary>
    /// <param name="flowId">The unique identifier for this quiz flow.</param>
    /// <param name="name">The human-readable name of the quiz.</param>
    /// <param name="logger">Logger instance.</param>
    /// <param name="eventBus">Event bus for publishing quiz events.</param>
    public QuizFlowHelper(
        string flowId,
        string name,
        ILogger<QuizFlowHelper>? logger = null,
        IEventBus? eventBus = null)
    {
        _flowId = flowId ?? throw new ArgumentNullException(nameof(flowId));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        _logger = logger;
        _eventBus = eventBus ?? NullEventBus.Instance;
    }

    /// <summary>
    /// Adds a question to the quiz.
    /// </summary>
    /// <param name="question">The quiz question to add.</param>
    /// <returns>The QuizFlowHelper instance for method chaining.</returns>
    public QuizFlowHelper AddQuestion(QuizQuestion question)
    {
        if (_isDisposed)
            throw new ObjectDisposedException(nameof(QuizFlowHelper));

        question.Validate();
        _questions.Add(question);
        _logger?.LogDebug(QuizFlowHelperConstants.AddedQuestionLog, question.QuestionId, _flowId);
        return this;
    }

    /// <summary>
    /// Adds multiple questions to the quiz.
    /// </summary>
    /// <param name="questions">The questions to add.</param>
    /// <returns>The QuizFlowHelper instance for method chaining.</returns>
    public QuizFlowHelper AddQuestions(IEnumerable<QuizQuestion> questions)
    {
        if (_isDisposed)
            throw new ObjectDisposedException(nameof(QuizFlowHelper));

        foreach (var question in questions)
        {
            AddQuestion(question);
        }
        return this;
    }

    /// <summary>
    /// Gets the number of questions in the quiz.
    /// </summary>
    /// <returns>The question count.</returns>
    public int GetQuestionCount()
    {
        if (_isDisposed)
            throw new ObjectDisposedException(nameof(QuizFlowHelper));

        return _questions.Count;
    }

    /// <summary>
    /// Gets the list of questions in the quiz.
    /// </summary>
    /// <returns>The list of questions.</returns>
    public IReadOnlyList<QuizQuestion> GetQuestions()
    {
        if (_isDisposed)
            throw new ObjectDisposedException(nameof(QuizFlowHelper));

        return _questions.AsReadOnly();
    }

    /// <summary>
    /// Disposes the QuizFlowHelper and cleans up resources.
    /// </summary>
    public void Dispose()
    {
        if (_isDisposed) return;

        _isDisposed = true;
        _questions.Clear();
        _logger?.LogDebug(QuizFlowHelperConstants.DisposedLog, _flowId);
    }
}

/// <summary>
/// Event published when a quiz starts.
/// </summary>
public sealed class QuizStartedEvent : Events.EventBase
{
    /// <summary>
    /// Gets the Telegram user identifier.
    /// </summary>
    public long UserId { get; }

    /// <summary>
    /// Gets the Telegram chat identifier.
    /// </summary>
    public long ChatId { get; }

    /// <summary>
    /// Gets the quiz flow identifier.
    /// </summary>
    public string FlowId { get; }

    /// <summary>
    /// Gets the total number of questions in the quiz.
    /// </summary>
    public int TotalQuestions { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="QuizStartedEvent"/> class.
    /// </summary>
    public QuizStartedEvent(long userId, long chatId, string flowId, int totalQuestions)
    {
        UserId = userId;
        ChatId = chatId;
        FlowId = flowId;
        TotalQuestions = totalQuestions;
    }
}

/// <summary>
/// Event published when a quiz is completed.
/// </summary>
public sealed class QuizCompletedEvent : Events.EventBase
{
    /// <summary>
    /// Gets the Telegram user identifier.
    /// </summary>
    public long UserId { get; }

    /// <summary>
    /// Gets the quiz flow identifier.
    /// </summary>
    public string FlowId { get; }

    /// <summary>
    /// Gets the final score.
    /// </summary>
    public int Score { get; }

    /// <summary>
    /// Gets the maximum possible score.
    /// </summary>
    public int MaxScore { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="QuizCompletedEvent"/> class.
    /// </summary>
    public QuizCompletedEvent(long userId, string flowId, int score, int maxScore)
    {
        UserId = userId;
        FlowId = flowId;
        Score = score;
        MaxScore = maxScore;
    }
}

/// <summary>
/// Event published when a quiz is aborted.
/// </summary>
public sealed class QuizAbortedEvent : Events.EventBase
{
    /// <summary>
    /// Gets the Telegram user identifier.
    /// </summary>
    public long UserId { get; }

    /// <summary>
    /// Gets the quiz flow identifier.
    /// </summary>
    public string FlowId { get; }

    /// <summary>
    /// Gets the reason for aborting.
    /// </summary>
    public string Reason { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="QuizAbortedEvent"/> class.
    /// </summary>
    public QuizAbortedEvent(long userId, string flowId, string reason)
    {
        UserId = userId;
        FlowId = flowId;
        Reason = reason;
    }
}

/// <summary>
/// Result of processing a quiz step.
/// </summary>
public sealed record QuizStepResult
{
    /// <summary>
    /// Gets a value indicating whether the input was valid.
    /// </summary>
    public required bool IsValid { get; init; }

    /// <summary>
    /// Gets a value indicating whether the quiz is completed.
    /// </summary>
    public required bool IsCompleted { get; init; }

    /// <summary>
    /// Gets the underlying flow step result.
    /// </summary>
    public required FlowStepResult FlowStepResult { get; init; }

    /// <summary>
    /// Gets the quiz result if the quiz is completed; otherwise, null.
    /// </summary>
    public QuizResult? QuizResult { get; init; }
}

/// <summary>
/// Static class for null event bus implementation.
/// </summary>
internal sealed class NullEventBus : IEventBus
{
    public static readonly IEventBus Instance = new NullEventBus();

    public void Subscribe<TEvent>(IEventHandler<TEvent> handler) where TEvent : class, IEvent
    {
        // Do nothing
    }

    public void Unsubscribe<TEvent>(IEventHandler<TEvent> handler) where TEvent : class, IEvent
    {
        // Do nothing
    }

    public Task PublishAsync<TEvent>(TEvent @event) where TEvent : class, IEvent
    {
        return Task.CompletedTask;
    }

    public void Clear()
    {
        // Do nothing
    }

    public int GetSubscriberCount<TEvent>() where TEvent : class, IEvent
    {
        return 0;
    }

    public void RegisterMiddleware(IEventMiddleware middleware)
    {
        // Do nothing - null implementation
    }

    public IEnumerable<IEventMiddleware> GetMiddleware()
    {
        return Enumerable.Empty<IEventMiddleware>();
    }
}
