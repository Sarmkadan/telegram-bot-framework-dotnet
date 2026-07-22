# QuizFlowHelper

Helper class for managing and executing quiz flows within a Telegram bot conversation. It encapsulates the state and lifecycle of a quiz, including question management, scoring, and event publishing.

## API

### `Name`
Gets the name of the quiz flow. This is a required identifier for the quiz.

- **Type:** `string`
- **Access:** Read-only

### `Description`
Gets the optional description of the quiz flow, providing context or instructions for participants.

- **Type:** `string?`
- **Access:** Read-only

### `CompletionMenuId`
Gets the optional menu ID to display after quiz completion. If set, this menu will be shown to the user once the quiz ends.

- **Type:** `string?`
- **Access:** Read-only

### `QuizFlowHelper(long userId, long chatId, string flowId)`
Initializes a new quiz flow for the specified user and chat with a unique flow identifier.

- **Parameters:**
  - `userId` – The Telegram user ID of the participant.
  - `chatId` – The Telegram chat ID where the quiz is being conducted.
  - `flowId` – A unique identifier for this quiz flow instance.
- **Throws:** `ArgumentException` if `flowId` is null or whitespace.

### `AddQuestion(QuizQuestion question)`
Adds a single question to the quiz flow.

- **Parameters:**
  - `question` – The `QuizQuestion` instance to add.
- **Returns:** The current `QuizFlowHelper` instance to support method chaining.
- **Throws:** `ArgumentNullException` if `question` is null.

### `AddQuestions(IEnumerable<QuizQuestion> questions)`
Adds multiple questions to the quiz flow in bulk.

- **Parameters:**
  - `questions` – An enumerable collection of `QuizQuestion` instances to add.
- **Returns:** The current `QuizFlowHelper` instance to support method chaining.
- **Throws:** `ArgumentNullException` if `questions` is null.

### `GetQuestionCount()`
Returns the total number of questions currently in the quiz flow.

- **Returns:** The count of questions as an `int`.

### `GetQuestions()`
Returns an immutable list of all questions in the quiz flow, in the order they were added.

- **Returns:** An `IReadOnlyList<QuizQuestion>` containing the questions.
- **Throws:** `ObjectDisposedException` if the helper has been disposed.

### `Dispose()`
Releases all resources used by the `QuizFlowHelper`. After disposal, the instance cannot be used further.

- **Throws:** `ObjectDisposedException` if methods are called after disposal.

### `UserId`
Gets the Telegram user ID associated with this quiz flow.

- **Type:** `long`
- **Access:** Read-only

### `ChatId`
Gets the Telegram chat ID where this quiz flow is active.

- **Type:** `long`
- **Access:** Read-only

### `FlowId`
Gets the unique identifier for this quiz flow instance.

- **Type:** `string`
- **Access:** Read-only

### `TotalQuestions`
Gets the total number of questions in the quiz flow.

- **Type:** `int`
- **Access:** Read-only

### `QuizStartedEvent`
Event raised when the quiz flow is started. Subscribers receive the user ID, chat ID, and flow ID.

- **Type:** `event Action<QuizStartedEvent>`
- **Access:** Read-only

### `Score`
Gets the current score of the participant in the quiz.

- **Type:** `int`
- **Access:** Read-only

### `MaxScore`
Gets the maximum possible score for the quiz, based on the total number of questions.

- **Type:** `int`
- **Access:** Read-only

### `QuizCompletedEvent`
Event raised when the quiz flow is completed. Subscribers receive the user ID, chat ID, flow ID, score, and maximum score.

- **Type:** `event Action<QuizCompletedEvent>`
- **Access:** Read-only

## Usage

### Example 1: Creating and Running a Simple Quiz
```csharp
var quiz = new QuizFlowHelper(12345L, 67890L, "math-quiz-001")
    .AddQuestion(new QuizQuestion("What is 2+2?", "4"))
    .AddQuestion(new QuizQuestion("What is 5×3?", "15"));

quiz.QuizStartedEvent += (e) => Console.WriteLine($"Quiz started for user {e.UserId}");
quiz.QuizCompletedEvent += (e) => Console.WriteLine($"Quiz completed! Score: {e.Score}/{e.MaxScore}");

quiz.Start(); // Assumes a Start() method exists in the actual implementation
```

### Example 2: Bulk-Adding Questions and Handling Completion
```csharp
var questions = new List<QuizQuestion>
{
    new QuizQuestion("Capital of France?", "Paris"),
    new QuizQuestion("Capital of Germany?", "Berlin"),
    new QuizQuestion("Capital of Italy?", "Rome")
};

var quiz = new QuizFlowHelper(98765L, 54321L, "geo-quiz-002")
    .AddQuestions(questions)
    .AddQuestion(new QuizQuestion("Capital of Spain?", "Madrid"));

quiz.QuizCompletedEvent += OnQuizCompleted;

void OnQuizCompleted(QuizCompletedEvent e)
{
    Console.WriteLine($"Flow {e.FlowId} finished. Result: {e.Score}/{e.MaxScore}");
}
```

## Notes

- **Thread Safety:** This class is not thread-safe. All operations should be performed on the same thread or with appropriate synchronization. Concurrent access may lead to race conditions, especially when modifying the question list or raising events.
- **Disposal:** Always call `Dispose()` when the quiz flow is no longer needed to release resources and prevent memory leaks. After disposal, any attempt to access methods or properties will throw an `ObjectDisposedException`.
- **Event Subscription:** Events are raised synchronously. Long-running handlers may delay quiz progression. Avoid blocking operations in event handlers.
- **Immutability:** The list returned by `GetQuestions()` is read-only but reflects the current state. If questions are added or removed after calling `GetQuestions()`, the returned list will not update.
