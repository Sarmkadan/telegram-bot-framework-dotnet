#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// Example: QuizFlow helper usage demonstration
// =============================================================================

using Microsoft.Extensions.DependencyInjection;
using TelegramBotFramework.ConversationFlow.QuizFlow;
using TelegramBotFramework.Services;

namespace TelegramBotFramework.Examples;

/// <summary>
/// Demonstrates how to create and use a quiz flow with the conversation flow engine.
/// This example shows:
/// - Creating a quiz with multiple choice questions
/// - Tracking scores
/// - Generating quiz results
/// - Handling user interactions
/// </summary>
public class QuizFlowExample
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConversationFlowEngine _flowEngine;

    /// <summary>
    /// Initializes a new instance of the <see cref="QuizFlowExample"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    public QuizFlowExample(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _flowEngine = serviceProvider.GetRequiredService<IConversationFlowEngine>();
    }

    /// <summary>
    /// Registers a sample quiz flow with the conversation flow engine.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task RegisterSampleQuizAsync()
    {
        // Get the conversation flow builder to register the quiz
        var flowBuilder = _serviceProvider.GetRequiredService<ConversationFlowBuilder>();

        // Create a quiz with multiple choice questions
        await flowBuilder.AddQuizFlow(
            quizId: "sample_quiz",
            name: "General Knowledge Quiz",
            configureQuestions: quiz =>
            {
                quiz.Description = "Test your general knowledge with 5 multiple choice questions.";
                quiz.CompletionMenuId = "main_menu";

                // Add questions to the quiz
                quiz.AddQuestion(new QuizQuestion
                {
                    QuestionId = "q1",
                    Text = "What is the capital of France?",
                    Options = new[] { "London", "Paris", "Berlin", "Madrid" },
                    CorrectAnswerIndex = 1, // Paris
                    Score = 2,
                    Feedback = "Correct! Paris is the capital of France."
                });

                quiz.AddQuestion(new QuizQuestion
                {
                    QuestionId = "q2",
                    Text = "Which planet is known as the Red Planet?",
                    Options = new[] { "Venus", "Mars", "Jupiter", "Saturn" },
                    CorrectAnswerIndex = 1, // Mars
                    Score = 2,
                    Feedback = "Good job! Mars appears red due to iron oxide on its surface."
                });

                quiz.AddQuestion(new QuizQuestion
                {
                    QuestionId = "q3",
                    Text = "What is the largest mammal in the world?",
                    Options = new[] { "Elephant", "Blue Whale", "Giraffe", "Polar Bear" },
                    CorrectAnswerIndex = 1, // Blue Whale
                    Score = 3,
                    Feedback = "Excellent! The blue whale can weigh up to 200 tons."
                });

                quiz.AddQuestion(new QuizQuestion
                {
                    QuestionId = "q4",
                    Text = "Which language has the most native speakers?",
                    Options = new[] { "English", "Spanish", "Mandarin Chinese", "Hindi" },
                    CorrectAnswerIndex = 2, // Mandarin Chinese
                    Score = 3,
                    Feedback = "Correct! Mandarin Chinese has over 1 billion native speakers."
                });

                quiz.AddQuestion(new QuizQuestion
                {
                    QuestionId = "q5",
                    Text = "Who painted the Mona Lisa?",
                    Options = new[] { "Vincent van Gogh", "Pablo Picasso", "Leonardo da Vinci", "Michelangelo" },
                    CorrectAnswerIndex = 2, // Leonardo da Vinci
                    Score = 2,
                    Feedback = "Well done! Leonardo da Vinci painted the Mona Lisa between 1503-1519."
                });
            });

        Console.WriteLine("✅ Sample quiz 'General Knowledge Quiz' registered successfully!");
    }

    /// <summary>
    /// Example of starting a quiz for a user and processing their answers.
    /// </summary>
    /// <param name="userId">The Telegram user ID.</param>
    /// <param name="chatId">The Telegram chat ID.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task RunQuizExampleAsync(long userId, long chatId)
    {
        // Get the quiz helper from DI
        var quizHelper = _serviceProvider.GetRequiredService<QuizFlowHelper>();

        Console.WriteLine($"Starting quiz for User {userId} in Chat {chatId}");

        // Start the quiz
        var state = await quizHelper.StartQuizAsync(userId, chatId);
        Console.WriteLine($"Quiz started! State ID: {state.StateId}");

        // Simulate user answering questions (in a real bot, this would be actual user input)
        // Question 1: What is the capital of France?
        var result1 = await quizHelper.ProcessQuizInputAsync(userId, "answer:2"); // Select Paris (index 1, user enters 2)
        Console.WriteLine($"Question 1 result - Valid: {result1.IsValid}, Completed: {result1.IsCompleted}");

        // Question 2: Which planet is known as the Red Planet?
        var result2 = await quizHelper.ProcessQuizInputAsync(userId, "answer:2"); // Select Mars (index 1, user enters 2)
        Console.WriteLine($"Question 2 result - Valid: {result2.IsValid}, Completed: {result2.IsCompleted}");

        // Question 3: What is the largest mammal in the world?
        var result3 = await quizHelper.ProcessQuizInputAsync(userId, "answer:2"); // Select Blue Whale (index 1, user enters 2)
        Console.WriteLine($"Question 3 result - Valid: {result3.IsValid}, Completed: {result3.IsCompleted}");

        // Question 4: Which language has the most native speakers?
        var result4 = await quizHelper.ProcessQuizInputAsync(userId, "answer:3"); // Select Mandarin Chinese (index 2, user enters 3)
        Console.WriteLine($"Question 4 result - Valid: {result4.IsValid}, Completed: {result4.IsCompleted}");

        // Question 5: Who painted the Mona Lisa?
        var result5 = await quizHelper.ProcessQuizInputAsync(userId, "answer:3"); // Select Leonardo da Vinci (index 2, user enters 3)
        Console.WriteLine($"Question 5 result - Valid: {result5.IsValid}, Completed: {result5.IsCompleted}");

        // The quiz should now be completed
        if (result5.IsCompleted && result5.QuizResult != null)
        {
            var finalResult = result5.QuizResult;
            Console.WriteLine("\n📊 **QUIZ COMPLETED!**");
            Console.WriteLine($"Score: {finalResult.TotalScore}/{finalResult.MaxScore} ({finalResult.Percentage:F1}%)");
            Console.WriteLine($"Grade: {finalResult.Grade}");
            Console.WriteLine("\n" + finalResult.FormatSummary());
        }
    }

    /// <summary>
    /// Example of getting the active quiz state and result.
    /// </summary>
    /// <param name="userId">The Telegram user ID.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task CheckQuizStatusAsync(long userId)
    {
        var quizHelper = _serviceProvider.GetRequiredService<QuizFlowHelper>();
        var state = await quizHelper.GetActiveQuizStateAsync(userId);

        if (state == null)
        {
            Console.WriteLine("No active quiz for this user.");
            return;
        }

        Console.WriteLine($"User {userId} has an active quiz:");
        Console.WriteLine($"- Flow ID: {state.FlowId}");
        Console.WriteLine($"- Current Step: {state.CurrentStepId}");
        Console.WriteLine($"- Status: {state.Status}");
        Console.WriteLine($"- Started: {state.StartedAt}");
        Console.WriteLine($"- Score: {state.Variables.GetValueOrDefault("_quiz_score") ?? "0"}");
    }

    /// <summary>
    /// Example of aborting an active quiz.
    /// </summary>
    /// <param name="userId">The Telegram user ID.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task AbortQuizExampleAsync(long userId)
    {
        var quizHelper = _serviceProvider.GetRequiredService<QuizFlowHelper>();
        await quizHelper.AbortQuizAsync(userId, "Example cleanup");
        Console.WriteLine($"Quiz for user {userId} aborted.");
    }
}
