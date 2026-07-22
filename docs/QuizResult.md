# QuizResult

The `QuizResult` class represents the outcome of a user's interaction with a quiz within the Telegram Bot Framework. It encapsulates both aggregate statistics for the entire quiz session, such as total scores and completion timestamps, and detailed information regarding specific question attempts, including the user's answer, the correct answer, and immediate feedback. This type serves as the primary data transfer object for persisting quiz outcomes, generating summary reports, or triggering post-quiz logic based on performance metrics.

## API

### `TotalScore`
```csharp
public required int TotalScore { get; init; }
```
Gets the cumulative score achieved by the user across all questions in the quiz. This property is mandatory and must be initialized during object creation. It represents the sum of `ScoreAwarded` values from individual question results.

### `MaxScore`
```csharp
public required int MaxScore { get; init; }
```
Gets the maximum possible score achievable in the quiz. This property is mandatory and serves as the denominator for calculating performance percentages. It is typically derived from the sum of maximum points available for each question.

### `QuestionResults`
```csharp
public required IReadOnlyList<QuestionResult> QuestionResults { get; init; }
```
Gets a read-only list containing the detailed results for each question answered during the quiz session. Each element in the list corresponds to a specific question attempt, providing granular data such as the question text, user input, and correctness status. This property is mandatory.

### `CompletedAt`
```csharp
public required DateTime CompletedAt { get; init; }
```
Gets the precise date and time when the quiz session was finalized. This property is mandatory and is typically set to `DateTime.UtcNow` at the moment the user submits their final answer or the quiz times out.

### `UserId`
```csharp
public long? UserId { get; init; }
```
Gets the unique identifier of the Telegram user who completed the quiz. This property is optional; it may be null if the quiz was conducted in a context where the user identity is not captured or if the result represents an aggregated or anonymous entry.

### `ChatId`
```csharp
public long? ChatId { get; init; }
```
Gets the unique identifier of the Telegram chat (private or group) where the quiz took place. This property is optional and allows correlating quiz results with specific conversation contexts.

### `FormatSummary`
```csharp
public string FormatSummary { get; init; }
```
Gets a pre-formatted string summary of the quiz results. This property is optional and intended for direct display to the user or logging purposes, often containing a human-readable breakdown of the score (e.g., "You scored 8/10").

### `QuestionNumber`
```csharp
public required int QuestionNumber { get; init; }
```
Gets the sequential index or number of the specific question within the quiz flow. This property is mandatory and is used to order results or reference specific questions in feedback loops. Note: In the context of a collection, this identifies the position of the current item.

### `QuestionText`
```csharp
public required string QuestionText { get; init; }
```
Gets the text content of the question presented to the user. This property is mandatory and ensures the result record is self-descriptive without requiring a lookup to the original quiz definition.

### `UserAnswerText`
```csharp
public required string UserAnswerText { get; init; }
```
Gets the exact text response provided by the user. This property is mandatory and captures the user's input for validation, auditing, or display in a review screen.

### `CorrectAnswerText`
```csharp
public required string CorrectAnswerText { get; init; }
```
Gets the canonical correct answer for the question. This property is mandatory and is used to verify correctness and provide educational feedback if the user's answer was incorrect.

### `IsCorrect`
```csharp
public required bool IsCorrect { get; init; }
```
Gets a boolean value indicating whether the user's answer matches the correct answer. This property is mandatory and drives conditional logic for scoring and feedback generation.

### `ScoreAwarded`
```csharp
public required int ScoreAwarded { get; init; }
```
Gets the number of points awarded for this specific question attempt. This property is mandatory. The value is typically equal to the question's maximum weight if `IsCorrect` is true, and zero otherwise, though partial credit logic may vary.

### `Feedback`
```csharp
public string? Feedback { get; init; }
```
Gets optional explanatory text provided to the user after answering. This may include hints, explanations of why an answer was incorrect, or additional context. It is null if no specific feedback is configured for the question.

## Usage

### Example 1: Constructing and Serializing a Quiz Result
This example demonstrates how to instantiate a `QuizResult` with aggregate data and a list of individual question outcomes, then generate a summary for logging.

```csharp
var questionResults = new List<QuestionResult>
{
    new QuestionResult
    {
        QuestionNumber = 1,
        QuestionText = "What is the capital of France?",
        UserAnswerText = "Paris",
        CorrectAnswerText = "Paris",
        IsCorrect = true,
        ScoreAwarded = 10
    },
    new QuestionResult
    {
        QuestionNumber = 2,
        QuestionText = "Which planet is known as the Red Planet?",
        UserAnswerText = "Venus",
        CorrectAnswerText = "Mars",
        IsCorrect = false,
        ScoreAwarded = 0,
        Feedback = "Mars is known for its iron oxide surface."
    }
};

var result = new QuizResult
{
    TotalScore = 10,
    MaxScore = 20,
    QuestionResults = questionResults.AsReadOnly(),
    CompletedAt = DateTime.UtcNow,
    UserId = 123456789,
    ChatId = 987654321,
    FormatSummary = "Score: 10/20 (50%)"
};

// Accessing specific details
Console.WriteLine($"User {result.UserId} completed quiz at {result.CompletedAt}");
foreach (var qr in result.QuestionResults)
{
    if (!qr.IsCorrect)
    {
        Console.WriteLine($"Q{qr.QuestionNumber}: {qr.Feedback}");
    }
}
```

### Example 2: Evaluating Performance Thresholds
This example shows how to consume a `QuizResult` to determine if a user has passed a quiz based on a percentage threshold and handle the logic accordingly.

```csharp
public void ProcessQuizCompletion(QuizResult result)
{
    if (result.MaxScore == 0)
    {
        throw new InvalidOperationException("Cannot calculate percentage with MaxScore of 0.");
    }

    double percentage = (double)result.TotalScore / result.MaxScore;
    
    if (percentage >= 0.8)
    {
        // Logic for passing
        SendNotification(result.ChatId, $"Congratulations! You passed with {percentage:P1}.");
    }
    else
    {
        // Logic for failing
        var incorrectQuestions = result.QuestionResults
            .Where(q => !q.IsCorrect)
            .Select(q => q.QuestionText);
            
        SendReviewMaterial(result.ChatId, incorrectQuestions);
    }
}
```

## Notes

*   **Immutability**: All properties marked as `required` utilize `init` accessors, ensuring that once a `QuizResult` instance is constructed, its core data cannot be modified. This makes the type inherently thread-safe for read operations after initialization.
*   **Data Consistency**: The `TotalScore` property is not automatically calculated from the `QuestionResults` collection by the class itself. The consumer is responsible for ensuring that `TotalScore` matches the sum of `ScoreAwarded` across all items in `QuestionResults` to maintain data integrity.
*   **Nullability**: While `UserId` and `ChatId` are nullable, `QuestionResults` must always be a valid list instance (though it may be empty). Accessing `QuestionResults` without a null check is safe, but enumerating an empty list will yield no items.
*   **DateTime Kind**: The `CompletedAt` property stores a `DateTime`. It is recommended to consistently use `DateTime.UtcNow` when populating this field to avoid timezone ambiguity in distributed bot environments.
*   **Memory Usage**: Since `QuestionResults` holds an `IReadOnlyList`, care should be taken when constructing results for extremely long quizzes to avoid excessive memory allocation in a single object graph.
