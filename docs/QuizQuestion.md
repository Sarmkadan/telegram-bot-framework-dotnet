# QuizQuestion

Represents a single quiz question within the Telegram bot framework. It encapsulates the question text, multiple-choice options, the correct answer index, scoring, optional feedback, and an optional timeout. The class provides methods to validate the question’s consistency, format it for display, and check whether a given answer is correct.

## API

### `public required string QuestionId`

A unique identifier for the question. This property is required and must be set during object initialization. It is typically used to reference the question in storage or during quiz sessions.

### `public required string Text`

The question text displayed to the user. This property is required and must be provided. It can contain formatting placeholders if used with `FormatQuestion`.

### `public required IReadOnlyList<string> Options`

A read-only list of answer options. Each element is a string representing one possible answer. The list must contain at least one element; otherwise, validation will fail.

### `public required int CorrectAnswerIndex`

The zero-based index of the correct answer within the `Options` list. This property is required and must be a valid index (i.e., between 0 and `Options.Count - 1`). Validation ensures this constraint.

### `public int Score`

The number of points awarded for answering this question correctly. Defaults to 0 if not explicitly set. Can be negative if the quiz design requires penalty for wrong answers.

### `public string? Feedback`

Optional feedback text shown to the user after answering. When `null`, no feedback is provided.

### `public int? TimeoutSeconds`

Optional timeout in seconds for answering the question. When `null`, no timeout is enforced. When set, the value must be greater than 0; validation will reject non-positive values.

### `public void Validate()`

Validates the internal consistency of the question. Throws an `InvalidOperationException` if any of the following conditions are met:

- `QuestionId` is `null` or empty.
- `Text` is `null` or empty.
- `Options` is `null` or contains fewer than one element.
- `CorrectAnswerIndex` is less than 0 or greater than or equal to `Options.Count`.
- `TimeoutSeconds` is not `null` and is less than or equal to 0.

This method is typically called before using the question in a quiz to ensure it is well-formed.

### `public string FormatQuestion()`

Returns a formatted string representation of the question, including the question text and all options, each prefixed with a letter or number. The exact formatting is implementation-defined but is intended for display in a Telegram message. Does not throw.

### `public bool IsCorrect(int answerIndex)`

Determines whether the provided `answerIndex` matches the correct answer.

- **Parameters**:  
  `answerIndex` – The zero-based index of the answer chosen by the user.
- **Returns**: `true` if `answerIndex` equals `CorrectAnswerIndex`; otherwise, `false`.
- **Throws**: `ArgumentOutOfRangeException` if `answerIndex` is less than 0 or greater than or equal to `Options.Count`.

## Usage

### Example 1: Creating and validating a quiz question

```csharp
var question = new QuizQuestion
{
    QuestionId = "q001",
    Text = "What is the capital of France?",
    Options = new List<string> { "Berlin", "Madrid", "Paris", "Rome" },
    CorrectAnswerIndex = 2,
    Score = 10,
    Feedback = "Paris is the capital of France.",
    TimeoutSeconds = 30
};

try
{
    question.Validate();
    Console.WriteLine("Question is valid.");
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Validation failed: {ex.Message}");
}
```

### Example 2: Checking an answer and formatting the question

```csharp
var question = new QuizQuestion
{
    QuestionId = "q002",
    Text = "Which planet is known as the Red Planet?",
    Options = new List<string> { "Venus", "Mars", "Jupiter", "Saturn" },
    CorrectAnswerIndex = 1,
    Score = 5
};

// Format the question for display
string formatted = question.FormatQuestion();
Console.WriteLine(formatted);

// Simulate user selecting option index 1 (Mars)
int userAnswer = 1;
bool isCorrect = question.IsCorrect(userAnswer);
Console.WriteLine($"User answer is correct: {isCorrect}"); // True

// Simulate wrong answer
userAnswer = 0;
isCorrect = question.IsCorrect(userAnswer);
Console.WriteLine($"User answer is correct: {isCorrect}"); // False
```

## Notes

- **Edge cases**:  
  - An empty `Options` list or a `CorrectAnswerIndex` outside the valid range will cause `Validate()` to throw.  
  - `Score` can be negative; no validation is performed on its value.  
  - `TimeoutSeconds` must be `null` or a positive integer; zero or negative values are rejected by `Validate()`.  
  - `QuestionId` and `Text` must not be `null` or empty; otherwise validation fails.

- **Thread safety**:  
  Instances of `QuizQuestion` are not thread-safe for concurrent writes. If multiple threads modify properties (`Score`, `Feedback`, `TimeoutSeconds`) simultaneously, data corruption may occur. Reading properties and calling `IsCorrect` or `FormatQuestion` concurrently with writes is unsafe. For thread-safe usage, either synchronize access or treat the object as immutable after initialization (e.g., by not modifying properties after creation). The `Validate` method is safe to call concurrently with reads, but not with writes.
