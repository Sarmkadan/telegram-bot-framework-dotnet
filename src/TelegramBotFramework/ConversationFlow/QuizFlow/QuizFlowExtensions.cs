#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
    public static IServiceCollection AddQuizFlow(
        this IServiceCollection services,
        string quizId,
        string name,
        Action<QuizFlowHelper> configureQuestions)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        if (string.IsNullOrWhiteSpace(quizId))
            throw new ArgumentException(QuizFlowExtensionsConstants.QuizIdEmptyExceptionMessage, nameof(quizId));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(QuizFlowExtensionsConstants.QuizNameEmptyExceptionMessage, nameof(name));

        if (configureQuestions == null)
            throw new ArgumentNullException(nameof(configureQuestions));

        // Create the quiz flow helper
        var logger = services.BuildServiceProvider()?.GetService<ILogger<QuizFlowHelper>>();
        var eventBus = services.BuildServiceProvider()?.GetService<IEventBus>();

        var quizHelper = new QuizFlowHelper(quizId, name, logger, eventBus);
        configureQuestions(quizHelper);

        services.AddSingleton(quizHelper);

        return services;
    }

    /// <summary>
    /// Creates a FlowDefinition from a QuizFlowHelper.
    /// </summary>
    internal static FlowDefinition CreateQuizFlowDefinition(QuizFlowHelper quizHelper)
    {
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
            Prompt = string.Format(QuizFlowExtensionsConstants.WelcomeMessageFormat, quizHelper.Name, questions.Count),
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
                AllowedValues = Enumerable.Range(1, question.Options.Count).Select(x => x.ToString()).ToList(),
                ErrorMessage = string.Format(QuizFlowExtensionsConstants.ValidationErrorMessageFormat, question.Options.Count)
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
                [QuizFlowExtensionsConstants.MetadataQuestionCountKey] = questions.Count.ToString()
            }
        };
    }

    /// <summary>
    /// Uses reflection to get a private field value (for internal use only).
    /// </summary>
    internal static object? GetFieldValue(this object obj, string fieldName)
    {
        var field = obj.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return field?.GetValue(obj);
    }
}
