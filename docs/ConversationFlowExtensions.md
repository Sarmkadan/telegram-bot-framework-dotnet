# ConversationFlowExtensions

The `ConversationFlowExtensions` class provides a set of static extension methods designed to simplify the configuration and definition of conversational flows within a `telegram-bot-framework-dotnet` application. These methods facilitate the integration of flow management services into the dependency injection container and provide a fluent, builder-based syntax for constructing complex conversation scenarios.

## API

### AddConversationFlows
Registers the core services required for conversation flow management in the `IServiceCollection`.
*   **Parameters:** `IServiceCollection services`
*   **Returns:** The `IServiceCollection` instance for chaining.
*   **Throws:** `ArgumentNullException` if the service collection is null.

### AddConversationFlowsWithFileStore
Registers conversation flow management services, including an implementation of a file-based storage provider for state persistence.
*   **Parameters:** `IServiceCollection services`
*   **Returns:** The `IServiceCollection` instance for chaining.
*   **Throws:** `ArgumentNullException` if the service collection is null.

### CreateFlow
Initializes a new `IFlowDefinitionBuilder` to start defining a conversation flow.
*   **Parameters:** `string flowName`
*   **Returns:** An `IFlowDefinitionBuilder` instance.
*   **Throws:** `ArgumentNullException` if the flow name is null or whitespace.

### WithDescription
Adds a human-readable description to the conversation flow definition.
*   **Parameters:** `IFlowDefinitionBuilder builder`, `string description`
*   **Returns:** The `IFlowDefinitionBuilder` instance for chaining.
*   **Throws:** `ArgumentNullException` if the builder or description is null.

### WithTimeout
Configures the maximum duration allowed for the conversation flow before it is considered timed out.
*   **Parameters:** `IFlowDefinitionBuilder builder`, `TimeSpan timeout`
*   **Returns:** The `IFlowDefinitionBuilder` instance for chaining.
*   **Throws:** `ArgumentNullException` if the builder is null.

### OnCompletionNavigateTo
Specifies the target identifier or flow to navigate to upon the successful completion of the current flow.
*   **Parameters:** `IFlowDefinitionBuilder builder`, `string targetFlowId`
*   **Returns:** The `IFlowDefinitionBuilder` instance for chaining.
*   **Throws:** `ArgumentNullException` if the builder or target flow ID is null.

### AllowResume
Configures the flow definition to permit resumption after an interruption or pause.
*   **Parameters:** `IFlowDefinitionBuilder builder`, `bool allow`
*   **Returns:** The `IFlowDefinitionBuilder` instance for chaining.
*   **Throws:** `ArgumentNullException` if the builder is null.

### AddStep
Registers a new step within the conversation flow definition.
*   **Parameters:** `IFlowDefinitionBuilder builder`, `IFlowStep step`
*   **Returns:** The `IFlowDefinitionBuilder` instance for chaining.
*   **Throws:** `ArgumentNullException` if the builder or step is null.

### WithMetadata
Associates custom metadata with the flow definition for organizational or processing purposes.
*   **Parameters:** `IFlowDefinitionBuilder builder`, `string key`, `object value`
*   **Returns:** The `IFlowDefinitionBuilder` instance for chaining.
*   **Throws:** `ArgumentNullException` if the builder or key is null.

### Build
Compiles the configured `IFlowDefinitionBuilder` into a finalized `FlowDefinition` object.
*   **Parameters:** `IFlowDefinitionBuilder builder`
*   **Returns:** A fully configured `FlowDefinition`.
*   **Throws:** `InvalidOperationException` if the builder state is invalid or incomplete.

## Usage

### Configuring Services
```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Register conversation flow services with file-based storage
    services.AddConversationFlowsWithFileStore();
}
```

### Building a Conversation Flow
```csharp
public FlowDefinition DefineOrderFlow()
{
    return ConversationFlowExtensions.CreateFlow("OrderProcessing")
        .WithDescription("Handles the user order intake process")
        .WithTimeout(TimeSpan.FromMinutes(15))
        .AddStep(new RequestItemStep())
        .AddStep(new ConfirmOrderStep())
        .OnCompletionNavigateTo("OrderConfirmation")
        .AllowResume(true)
        .Build();
}
```

## Notes

*   **Builder Pattern:** The fluent methods modifying the `IFlowDefinitionBuilder` generally return the same builder instance to allow for method chaining. It is critical to call `Build()` as the final step to finalize the definition.
*   **Thread Safety:** While the `ConversationFlowExtensions` methods themselves are static and stateless, the resulting `FlowDefinition` objects produced by the builder should be treated as immutable once built.
*   **Validation:** The `Build()` method performs internal validation of the flow definition state. If required steps are missing or configuration parameters are contradictory, it may throw an `InvalidOperationException`.
*   **Dependency Injection:** Ensure that `AddConversationFlows` (or its file-store variant) is called during the application startup phase to ensure all necessary infrastructure is available for flow execution.
