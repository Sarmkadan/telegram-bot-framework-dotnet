# FlowDefinition

A `FlowDefinition` acts as the primary orchestrator for conversational interactions within the `telegram-bot-framework-dotnet`, defining the complete structure, sequence, and logic of a user-guided process. It encapsulates a collection of `FlowStep` objects, managing the initial entry point, navigation rules, state storage via variable binding, and configuration for timeouts and completion handling.

## API

### FlowDefinition Members

*   **`FlowId`** (required `string`): A unique identifier used to reference this specific flow within the bot's configuration.
*   **`Name`** (required `string`): A human-readable name for the flow.
*   **`Description`** (`string?`): An optional, detailed description of the flow's purpose.
*   **`InitialStepId`** (required `string`): The `StepId` of the first step to be executed when the flow starts. Must exist within the `Steps` collection.
*   **`Steps`** (required `IReadOnlyList<FlowStep>`): The collection of steps that define the flow logic.
*   **`Timeout`** (`TimeSpan?`): An optional duration after which an inactive flow is considered timed out.
*   **`AllowResume`** (`bool`): Determines if a user can resume this flow if it was previously interrupted.
*   **`CompletionMenuId`** (`string?`): The ID of the menu to present to the user once the flow completes.
*   **`Metadata`** (`Dictionary<string, string>`): A collection of custom key-value pairs for additional flow-level configuration.

### FlowStep Members (Contained in `Steps`)

*   **`StepId`** (required `string`): Unique identifier for this step within the flow.
*   **`Prompt`** (required `string`): The message content displayed to the user for this step.
*   **`HelpText`** (`string?`): Optional text displayed if the user requests help during this step.
*   **`IsTerminal`** (`bool`): Indicates whether this step marks the end of the flow.
*   **`InputType`** (required `FlowInputType`): Specifies the expected format of the user's input (e.g., Text, CallbackQuery, File).
*   **`Validation`** (`FlowValidation?`): Defines rules to validate the user's input for this step.
*   **`VariableName`** (`string?`): The name of the variable where the validated input from this step will be stored.
*   **`Transitions`** (`IReadOnlyList<FlowTransition>`): Defines the logic for navigating to subsequent steps based on user input.
*   **`QuickReplies`** (`IReadOnlyList<string>?`): An optional list of predefined buttons or text responses for the user.
*   **`DefaultNextStepId`** (`string?`): The `StepId` to transition to if no specific rules match in `Transitions`.
*   **`Metadata`** (`Dictionary<string, string>`): A collection of custom key-value pairs specific to this step.

## Usage

### Example 1: Defining a User Registration Flow

```csharp
var registrationFlow = new FlowDefinition
{
    FlowId = "user_reg",
    Name = "User Registration",
    InitialStepId = "ask_name",
    Steps = new List<FlowStep>
    {
        new FlowStep
        {
            StepId = "ask_name",
            Prompt = "Please enter your full name:",
            InputType = FlowInputType.Text,
            VariableName = "UserName",
            DefaultNextStepId = "ask_email"
        },
        new FlowStep
        {
            StepId = "ask_email",
            Prompt = "Now, please enter your email address:",
            InputType = FlowInputType.Text,
            VariableName = "UserEmail",
            IsTerminal = true
        }
    }
};
```

### Example 2: Defining a Menu-Based Selection Flow

```csharp
var menuFlow = new FlowDefinition
{
    FlowId = "main_menu",
    Name = "Main Menu",
    InitialStepId = "select_option",
    Steps = new List<FlowStep>
    {
        new FlowStep
        {
            StepId = "select_option",
            Prompt = "Please select an option:",
            InputType = FlowInputType.CallbackQuery,
            QuickReplies = new List<string> { "Support", "Settings" },
            Transitions = new List<FlowTransition>
            {
                new FlowTransition { Value = "Support", NextStepId = "support_step" },
                new FlowTransition { Value = "Settings", NextStepId = "settings_step" }
            }
        }
        // ... subsequent steps
    }
};
```

## Notes

*   **Immutability:** The `Steps` collection is defined as `IReadOnlyList<FlowStep>`, ensuring that the structure of the flow cannot be modified after the `FlowDefinition` object is instantiated.
*   **Thread-Safety:** While the `FlowDefinition` structure itself is immutable after initialization, the `Metadata` dictionaries are mutable. Care should be taken if accessing `Metadata` across multiple threads simultaneously.
*   **Validation:** If `InitialStepId` does not correspond to any `StepId` within the `Steps` collection, the framework will throw an exception during flow initialization.
*   **Execution:** When `IsTerminal` is set to `true`, the framework will finalize the flow and, if defined, display the menu specified by `CompletionMenuId`.
