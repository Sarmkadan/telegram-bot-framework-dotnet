#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Globalization;
using TelegramBotFramework.Events;

namespace TelegramBotFramework.ConversationFlow.QuizFlow;

/// <summary>
/// Extension methods for registering and using QuizFlow helpers.
/// </summary>
public static class QuizFlowExtensions
{
    /// <summary>
    /// Creates and registers a quiz flow helper with the conversation flow engine.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="quizId">The unique identifier for the quiz flow.</param>
    /// <param name="name">The human-readable name of the quiz.</param>
    /// <param name="configureQuestions">Action to configure quiz questions.</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="quizId"/> is null or white space.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is null or white space.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configureQuestions"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when required services (<see cref="ILogger{QuizFlowHelper}"/> or <see cref="IEventBus"/>) are not registered.</exception>
    public static IServiceCollection AddQuizFlow(
        this IServiceCollection services,
        string quizId,
        string name,
        Action<QuizFlowHelper> configureQuestions)
    {
        ArgumentNullException.ThrowIfNull(services);

        if (string.IsNullOrWhiteSpace(quizId))
            throw new ArgumentException(QuizFlowExtensionsConstants.QuizIdEmptyExceptionMessage, nameof(quizId));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(QuizFlowExtensionsConstants.QuizNameEmptyExceptionMessage, nameof(name));

        ArgumentNullException.ThrowIfNull(configureQuestions);

        // Build service provider once to get required services
        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetService<ILogger<QuizFlowHelper>>();
        var eventBus = serviceProvider.GetService<IEventBus>();

        if (logger == null)
            throw new InvalidOperationException($"Required service {typeof(ILogger<QuizFlowHelper>).Name} is not registered.");

        if (eventBus == null)
            throw new InvalidOperationException($"Required service {typeof(IEventBus).Name} is not registered.");

        // Create the quiz flow helper
        var quizHelper = new QuizFlowHelper(quizId, name, logger, eventBus);
        configureQuestions(quizHelper);

        services.AddSingleton(quizHelper);

        return services;
    }

    /// <summary>
    /// Creates a FlowDefinition from a QuizFlowHelper.
    /// </summary>
    /// <param name="quizHelper">The quiz flow helper.</param>
    /// <returns>A flow definition representing the quiz.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="quizHelper"/> is null.</exception>
    internal static FlowDefinition CreateQuizFlowDefinition(QuizFlowHelper quizHelper)
    {
        ArgumentNullException.ThrowIfNull(quizHelper);

        var questions = quizHelper.GetFieldValue("_questions") as List<QuizQuestion> ?? new List<QuizQuestion>();
        var completionMenuId = quizHelper.GetType().GetProperty("CompletionMenuId")?.GetValue(quizHelper) as string;

        var steps = new List<FlowStep>();
        var initialStepId = $"{quizHelper.FlowId}{QuizFlowExtensionsConstants.StepIdStartSuffix}";
        var questionStepIdPrefix = $"{quizHelper.FlowId}{QuizFlowExtensionsConstants.StepIdQuestionPrefixSuffix}";
        var resultStepId = $"{quizHelper.FlowId}{QuizFlowExtensionsConstants.StepIdResultSuffix}";

        // Welcome step
        steps.Add(new FlowStep
        {
            StepId = initialStepId,
            Prompt = string.Format(CultureInfo.InvariantCulture, QuizFlowExtensionsConstants.WelcomeMessageFormat, quizHelper.Name, questions.Count),
            InputType = FlowInputType.Confirmation,
            IsTerminal = false,
            QuickReplies = new[] { "/start" }
        });

        // Question steps
        for (int i = 0; i < questions.Count; i++)
        {
            var question = questions[i];
            var stepId = $"{questionStepIdPrefix}{i}";

            var validation = new FlowValidation
            {
                MinLength = QuizFlowExtensionsConstants.ValidationMinLength,
                MaxLength = QuizFlowExtensionsConstants.ValidationMaxLength,
                AllowedValues = Enumerable.Range(1, question.Options.Count).Select(x => x.ToString(CultureInfo.InvariantCulture)).ToList(),
                ErrorMessage = string.Format(CultureInfo.InvariantCulture, QuizFlowExtensionsConstants.ValidationErrorMessageFormat, question.Options.Count)
            };

            var quickReplies = question.Options.Select((opt, idx) => $"{idx + 1}").ToList();

            steps.Add(new FlowStep
            {
                StepId = stepId,
                Prompt = question.FormatQuestion(),
                InputType = FlowInputType.Number,
                VariableName = $"answer_{i}",
                Validation = validation,
                IsTerminal = false,
                QuickReplies = quickReplies
            });
        }

        // Result step (terminal)
        var resultPromptBuilder = new System.Text.StringBuilder();
        resultPromptBuilder.AppendLine(QuizFlowExtensionsConstants.QuizCompletedMessage);
        resultPromptBuilder.AppendLine();
        resultPromptBuilder.AppendLine(QuizFlowExtensionsConstants.QuizResultsCalculatingMessage);

        steps.Add(new FlowStep
        {
            StepId = resultStepId,
            Prompt = resultPromptBuilder.ToString(),
            InputType = FlowInputType.Any,
            IsTerminal = true
        });

        // Transitions
        for (int i = 0; i < questions.Count; i++)
        {
            var nextStepId = i < questions.Count - 1
                ? $"{questionStepIdPrefix}{i + 1}"
                : resultStepId;

            var transitions = new List<FlowTransition>();
            transitions.Add(new FlowTransition
            {
                TargetStepId = nextStepId,
                Condition = new FlowCondition
                {
                    VariableName = $"answer_{i}",
                    Operator = FlowConditionOperator.IsNotEmpty,
                    Value = "true"
                }
            });

            // Use reflection to set Transitions since it's init-only
            var step = steps[i + 1];
            var stepType = step.GetType();
            var transitionsProperty = stepType.GetProperty("Transitions");
            transitionsProperty?.SetValue(step, transitions);
        }

        // Initial step transitions
        var initialTransitions = new List<FlowTransition>();
        initialTransitions.Add(new FlowTransition
        {
            TargetStepId = $"{questionStepIdPrefix}0",
            Condition = new FlowCondition
            {
                VariableName = "start",
                Operator = FlowConditionOperator.Equals,
                Value = "true"
            }
        });

        // Use reflection to set Transitions for initial step
        var initialStep = steps[0];
        var initialStepType = initialStep.GetType();
        var initialTransitionsProperty = initialStepType.GetProperty("Transitions");
        initialTransitionsProperty?.SetValue(initialStep, initialTransitions);

        return new FlowDefinition
        {
            FlowId = quizHelper.FlowId,
            Name = quizHelper.Name,
            Description = quizHelper.Description,
            InitialStepId = initialStepId,
            Steps = steps,
            CompletionMenuId = completionMenuId,
            Metadata = new Dictionary<string, string>
            {
                [QuizFlowExtensionsConstants.MetadataQuizTypeKey] = QuizFlowExtensionsConstants.QuizTypeValue,
                [QuizFlowExtensionsConstants.MetadataQuestionCountKey] = questions.Count.ToString(CultureInfo.InvariantCulture)
            }
        };
    }

    /// <summary>
    /// Uses reflection to get a private field value (for internal use only).
    /// </summary>
    /// <param name="obj">The object to get the field value from.</param>
    /// <param name="fieldName">The name of the field.</param>
    /// <returns>The value of the field, or null if the field does not exist.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="obj"/> is null.</exception>
    internal static object? GetFieldValue(this object obj, string fieldName)
    {
        ArgumentNullException.ThrowIfNull(obj);
        var field = obj.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return field?.GetValue(obj);
    }
}