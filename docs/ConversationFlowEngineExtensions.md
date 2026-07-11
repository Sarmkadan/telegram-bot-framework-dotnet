# ConversationFlowEngineExtensions

Provides extension methods for querying the state of a conversation flow engine instance. These methods allow callers to determine whether a flow is active, retrieve the current step identifier, access stored flow variables, and obtain the active flow definition without directly manipulating the underlying engine state.

## API

### HasActiveFlowAsync

```csharp
public static async Task<bool> HasActiveFlowAsync(this IConversationFlowEngine engine, long userId)
```

Determines whether the specified user currently has an active conversation flow.

**Parameters:**
- `engine` — the conversation flow engine instance being extended.
- `userId` — the unique identifier of the user to check.

**Returns:** `true` if an active flow exists for the user; otherwise `false`.

**Exceptions:** Throws `ArgumentNullException` if `engine` is null.

---

### GetCurrentStepIdAsync

```csharp
public static async Task<string?> GetCurrentStepIdAsync(this IConversationFlowEngine engine, long userId)
```

Retrieves the identifier of the current step within the active flow for the given user.

**Parameters:**
- `engine` — the conversation flow engine instance being extended.
- `userId` — the unique identifier of the user.

**Returns:** The step identifier string if a flow is active and a current step is set; `null` if no flow is active or the step cannot be resolved.

**Exceptions:** Throws `ArgumentNullException` if `engine` is null.

---

### GetVariableAsync

```csharp
public static async Task<string?> GetVariableAsync(this IConversationFlowEngine engine, long userId, string variableName)
```

Reads the value of a named variable stored within the active flow context for the specified user.

**Parameters:**
- `engine` — the conversation flow engine instance being extended.
- `userId` — the unique identifier of the user.
- `variableName` — the case-sensitive name of the variable to retrieve.

**Returns:** The variable value as a string if the variable exists; `null` if no flow is active or the variable has not been set.

**Exceptions:** Throws `ArgumentNullException` if `engine` is null. Throws `ArgumentException` if `variableName` is null or empty.

---

### GetActiveFlowAsync

```csharp
public static async Task<FlowDefinition?> GetActiveFlowAsync(this IConversationFlowEngine engine, long userId)
```

Obtains the full definition of the active flow for the specified user.

**Parameters:**
- `engine` — the conversation flow engine instance being extended.
- `userId` — the unique identifier of the user.

**Returns:** The `FlowDefinition` object representing the active flow if one exists; `null` otherwise.

**Exceptions:** Throws `ArgumentNullException` if `engine` is null.

## Usage

### Example 1: Checking Flow State Before Processing a Message

```csharp
public async Task HandleUserMessage(IConversationFlowEngine engine, long userId, string messageText)
{
    bool hasFlow = await engine.HasActiveFlowAsync(userId);

    if (!hasFlow)
    {
        // No flow in progress — handle as a free-form command
        await ProcessFreeCommand(userId, messageText);
        return;
    }

    string? currentStep = await engine.GetCurrentStepIdAsync(userId);
    Console.WriteLine($"User {userId} is at step '{currentStep}'");
    await AdvanceFlow(engine, userId, currentStep, messageText);
}
```

### Example 2: Reading Flow Variables for Conditional Logic

```csharp
public async Task<string> BuildPersonalizedPrompt(IConversationFlowEngine engine, long userId)
{
    FlowDefinition? flow = await engine.GetActiveFlowAsync(userId);
    if (flow == null)
        return "How can I help you?";

    string? preferredLanguage = await engine.GetVariableAsync(userId, "language");
    string? userName = await engine.GetVariableAsync(userId, "user_name");

    if (preferredLanguage == "de")
        return $"Hallo {userName ?? "Benutzer"}, wie kann ich helfen?";
    
    return $"Hello {userName ?? "user"}, how can I assist?";
}
```

## Notes

- All methods are asynchronous and should be awaited to ensure the underlying engine state is fully resolved before the result is consumed.
- Returning `null` from `GetCurrentStepIdAsync`, `GetVariableAsync`, or `GetActiveFlowAsync` is the standard way these methods signal absence of data; callers should always perform null checks before dereferencing results.
- These methods are designed as read-only queries against the engine. They do not mutate flow state, transition steps, or modify variables.
- Thread safety depends entirely on the implementation of `IConversationFlowEngine` passed to these extensions. The extension methods themselves hold no mutable state and delegate all work to the engine instance.
- `GetVariableAsync` performs a case-sensitive lookup. Passing a variable name with incorrect casing will return `null` even if a similarly named variable exists.
- If a user has an active flow but no step has been set (e.g., the flow was just initialized), `GetCurrentStepIdAsync` may return `null`. This is distinct from having no active flow at all.
