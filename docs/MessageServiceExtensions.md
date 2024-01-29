# MessageServiceExtensions

The `MessageServiceExtensions` class provides a set of static asynchronous extension methods designed to streamline common message handling operations within the Telegram Bot Framework. These utilities abstract complex database queries and state transitions, offering a simplified interface for creating, retrieving, filtering, and updating the status of `Models.Message` entities without requiring direct interaction with the underlying repository layer.

## API

### CreateAndProcessMessageAsync
Creates a new message entity in the system and immediately transitions its status to "processed" in a single atomic operation.
*   **Parameters**: Accepts the necessary data transfer object or entity parameters required to construct the message (specific signature details depend on the overloaded variant used).
*   **Return Value**: Returns a `Task<Models.Message>` containing the newly created and persisted message object.
*   **Exceptions**: Throws an exception if the underlying database transaction fails, if unique constraints are violated, or if the input data is invalid.

### TryGetMessageAsync
Attempts to retrieve a specific message by its identifier without throwing an exception if the entity is not found.
*   **Parameters**: Typically accepts a unique identifier (e.g., `long id` or `string id`) corresponding to the target message.
*   **Return Value**: Returns a `Task<Models.Message?>`. If the message exists, the task result contains the `Models.Message` instance; if not found, the result is `null`.
*   **Exceptions**: Generally does not throw for missing records. May throw if the data provider encounters a connectivity issue or serialization error.

### GetUserMessagesByContentAsync
Retrieves a list of messages associated with a specific user that match a defined content criteria.
*   **Parameters**: Accepts a user identifier and a content filter (e.g., string pattern or enum type) to narrow the search scope.
*   **Return Value**: Returns a `Task<IList<Models.Message>>` containing all matching messages. Returns an empty list if no matches are found.
*   **Exceptions**: Throws if the query execution fails due to database errors or invalid filter parameters.

### GetMessageCountByStatusAsync
Calculates the total number of messages currently holding a specific processing status.
*   **Parameters**: Accepts a status enumerator or string representing the target state (e.g., `Pending`, `Processed`, `Failed`).
*   **Return Value**: Returns a `Task<int>` representing the count of messages matching the specified status.
*   **Exceptions**: Throws if the status value provided is unrecognized or if the counting operation fails at the data layer.

### MarkMessagesAsProcessedAsync
Updates the status of a specified collection or range of messages to "processed".
*   **Parameters**: Accepts a list of message IDs or a filter criteria defining the target messages.
*   **Return Value**: Returns a `Task<bool>` indicating success (`true`) if at least one record was updated, or `false` if no records matched the criteria.
*   **Exceptions**: Throws if the update transaction fails or if concurrency conflicts occur during the batch update.

## Usage

### Example 1: Safe Message Retrieval and Processing
This example demonstrates how to safely attempt to retrieve a message and conditionally process it only if it exists, utilizing `TryGetMessageAsync` to avoid control-flow exceptions.

```csharp
using TelegramBotFramework.Extensions;
using TelegramBotFramework.Models;

public async Task HandleIncomingMessageAsync(long messageId)
{
    // Attempt to fetch the message without throwing if missing
    var message = await MessageServiceExtensions.TryGetMessageAsync(messageId);

    if (message == null)
    {
        Console.WriteLine($"Message {messageId} not found.");
        return;
    }

    // Perform business logic
    if (message.Content.Contains("urgent"))
    {
        await PriorityQueue.EnqueueAsync(message);
    }
}
```

### Example 2: Batch Status Update and Verification
This example illustrates retrieving messages by content for a specific user, performing an action, and then bulk-marking them as processed while verifying the count.

```csharp
using TelegramBotFramework.Extensions;
using TelegramBotFramework.Models;

public async Task ArchiveUserContentAsync(long userId, string keyword)
{
    // Fetch all messages for the user containing the keyword
    var messages = await MessageServiceExtensions.GetUserMessagesByContentAsync(userId, keyword);
    
    if (messages.Count == 0)
    {
        return;
    }

    // Extract IDs for batch processing
    var ids = messages.Select(m => m.Id).ToList();

    // Mark them as processed
    bool success = await MessageServiceExtensions.MarkMessagesAsProcessedAsync(ids);

    if (success)
    {
        // Verify the count of processed messages has updated
        int totalProcessed = await MessageServiceExtensions.GetMessageCountByStatusAsync(MessageStatus.Processed);
        Console.WriteLine($"Archived {messages.Count} messages. Total processed in system: {totalProcessed}");
    }
}
```

## Notes

*   **Null Handling**: Consumers of `TryGetMessageAsync` must explicitly check for `null` return values before accessing properties of the returned `Models.Message` object to prevent `NullReferenceException`.
*   **Empty Collections**: `GetUserMessagesByContentAsync` returns an empty `IList` rather than `null` when no matches are found. Callers should check `.Count` rather than performing null checks.
*   **Thread Safety**: As this class consists entirely of static methods delegating to asynchronous underlying services, it is stateless and thread-safe for concurrent invocation. However, logical race conditions may occur if multiple threads attempt to modify the same message entities simultaneously (e.g., calling `MarkMessagesAsProcessedAsync` on overlapping ID sets).
*   **Transaction Scope**: Methods like `CreateAndProcessMessageAsync` and `MarkMessagesAsProcessedAsync` imply database write operations. These should be awaited properly to ensure the transaction completes before subsequent dependent logic executes.
*   **Return Value Semantics**: For `MarkMessagesAsProcessedAsync`, a return value of `false` indicates that the operation completed successfully but no database rows were affected (i.e., the provided IDs did not exist or were already in the target state), which is distinct from a thrown exception indicating a system failure.
