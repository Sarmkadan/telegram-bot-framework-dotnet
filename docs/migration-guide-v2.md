# Migration Guide: v1.x to v2.0

This guide helps you migrate your existing Telegram bot from v1.x to v2.0 of the Telegram Bot Framework for .NET. The v2.0 release introduces a powerful conversation flow engine with branching dialogs and context management, while maintaining backward compatibility where possible.

---

## Table of Contents

- [Breaking Changes](#breaking-changes)
- [New Features in v2.0](#new-features-in-v20)
- [Migration Steps](#migration-steps)
- [Code Examples: Old vs New](#code-examples-old-vs-new)
- [Configuration Changes](#configuration-changes)
- [Testing Your Migration](#testing-your-migration)

---

## Breaking Changes

### 1. Session Management API Changes

**v1.x:**
```csharp
// Session storage was manual
var session = await sessionService.CreateSessionAsync(userId, chatId);
session.SetContextData("key", "value");
await sessionService.UpdateSessionAsync(session);
```

**v2.0:**
```csharp
// Sessions are now managed automatically by the framework
// Use UserFlowState for conversation context instead
```

### 2. Command Registration Simplified

**v1.x:**
```csharp
var command = new Command {
    Name = "/start",
    HandlerType = "StartCommandHandler",  // Required in v1
    // ...
};
await commandService.RegisterCommandAsync(command);
```

**v2.0:**
```csharp
// HandlerType is no longer required
var command = new Command {
    Name = "/start",
    Description = "Start the bot",
    // HandlerType removed in v2.0
};
await commandService.RegisterCommandAsync(command);
```

### 3. Message Processing Changes

**v1.x:**
```csharp
var message = new Message {
    UserId = userId,
    ChatId = chatId,
    Content = "Hello",
    Type = MessageType.Text
};
var result = await messageService.ProcessIncomingMessageAsync(message);
```

**v2.0:**
```csharp
// Message processing now goes through the orchestrator
var orchestrator = serviceProvider.GetRequiredService<IBotOrchestrator>();
await orchestrator.HandleUpdateAsync(update); // Telegram Update object
```

### 4. Repository Interface Changes

**v1.x:**
```csharp
public interface IRepository<T> where T : class
```

**v2.0:**
```csharp
// More specific interfaces for different entity types
public interface IRepository<T> where T : class
public interface IUserRepository : IRepository<User>
public interface ISessionRepository : IRepository<UserSession>
```

### 5. Cache Provider Changes

**v1.x:**
```csharp
// Cache configuration was simpler
"CacheConfiguration": {
    "Provider": "LocalCache"
}
```

**v2.0:**
```csharp
// More configuration options available
"CacheConfiguration": {
    "Provider": "LocalCache",
    "DefaultExpirationMinutes": 60,
    "RedisConnection": "..."
}
```

---

## New Features in v2.0

### 1. Conversation Flow Engine 🎯

The most significant addition is the conversation flow engine that enables:

- **Branching dialogs**: Create multi-step conversations with conditional logic
- **Context management**: Store and retrieve conversation state automatically
- **State machines**: Define complex user flows with transitions
- **Validation**: Built-in input validation for different data types
- **Event system**: Flow lifecycle events for integration

### 2. Enhanced Type System

- **FlowInputType**: Text, Number, Boolean, Choice, DateTime, PhoneNumber, Email, Confirmation, Any
- **FlowConditionOperator**: Equals, NotEquals, Contains, StartsWith, EndsWith, GreaterThan, LessThan, IsEmpty, IsNotEmpty
- **FlowStateStatus**: Active, WaitingForInput, Suspended, Completed, Aborted, TimedOut

### 3. Flow Definition Structure

```csharp
var flowDefinition = new FlowDefinition {
    FlowId = "user_registration",
    Name = "User Registration Flow",
    Description = "Guides new users through registration",
    InitialStepId = "ask_name",
    Steps = new List<FlowStep> {
        new FlowStep {
            StepId = "ask_name",
            Prompt = "What is your name?",
            InputType = FlowInputType.Text,
            VariableName = "user_name",
            Transitions = new List<FlowTransition> {
                new FlowTransition {
                    TargetStepId = "ask_email",
                    Condition = new FlowCondition {
                        VariableName = "user_name",
                        Operator = FlowConditionOperator.IsNotEmpty,
                        Value = "true"
                    }
                }
            }
        },
        new FlowStep {
            StepId = "ask_email",
            Prompt = "What is your email?",
            InputType = FlowInputType.Email,
            VariableName = "user_email"
        }
    },
    Timeout = TimeSpan.FromMinutes(10),
    AllowResume = true,
    CompletionMenuId = "main_menu"
};
```

### 4. Flow Engine Services

- **IConversationFlowEngine**: Register flows and process user input
- **FlowStateRepository**: Persist flow state (in-memory or distributed)
- **FlowEventHandlers**: Listen to flow lifecycle events

### 5. Event System Enhancements

New flow-related events:
- `FlowStartedEvent`: Published when a flow begins
- `FlowStepCompletedEvent`: Published after each step
- `FlowCompletedEvent`: Published when flow finishes successfully
- `FlowAbortedEvent`: Published when flow is cancelled

### 6. Improved Configuration

More granular configuration options:
- **ConversationFlowOptions**: Default flow timeout, max active flows
- **FlowStateConfiguration**: Session timeout for flows, cleanup interval
- **EventBusConfiguration**: Enable/disable specific event types

---

## Migration Steps

### Step 1: Update NuGet Package

Update your project file to use v2.0:

```xml
<PackageReference Include="TelegramBotFramework" Version="2.0.0" />
```

### Step 2: Review Breaking Changes

Check your code against the breaking changes listed above. Focus on:
1. Session management code
2. Command registration (HandlerType removal)
3. Message processing pipeline
4. Repository usage patterns

### Step 3: Migrate Session Management

**Before:**
```csharp
var session = await sessionService.CreateSessionAsync(userId, chatId);
session.SetContextData("current_step", "input_name");
session.SetContextData("form_data", JsonConvert.SerializeObject(data));
await sessionService.UpdateSessionAsync(session);
```

**After:**
```csharp
// Use conversation flows instead
var flowEngine = serviceProvider.GetRequiredService<IConversationFlowEngine>();
var flowDefinition = new FlowDefinition {
    FlowId = "user_form",
    Name = "User Form",
    InitialStepId = "step1",
    Steps = new List<FlowStep> { ... }
};

// Start flow
await flowEngine.StartFlowAsync("user_form", userId, chatId);

// Process input - handled automatically by middleware
```

### Step 4: Update Command Registration

Remove `HandlerType` from your command definitions:

```csharp
// Before
var command = new Command {
    Name = "/start",
    HandlerType = "StartCommandHandler",  // ❌ Remove this
    Description = "Start the bot"
};

// After
var command = new Command {
    Name = "/start",
    Description = "Start the bot"
    // HandlerType removed ✅
};
```

### Step 5: Update Message Processing

**Before:**
```csharp
var message = new Message {
    UserId = userId,
    ChatId = chatId,
    Content = "/start",
    Type = MessageType.Text
};
var result = await messageService.ProcessIncomingMessageAsync(message);
```

**After:**
```csharp
// Use the orchestrator with Telegram Update objects
var update = new Update {
    Message = new Message {
        From = new User { Id = userId },
        Chat = new Chat { Id = chatId },
        Text = "/start"
    }
};

var orchestrator = serviceProvider.GetRequiredService<IBotOrchestrator>();
await orchestrator.HandleUpdateAsync(update);
```

### Step 6: Migrate to Conversation Flows

Identify repetitive conversation patterns and convert them to flows:

**Example: Simple Q&A Flow**

```csharp
// Define the flow
var faqFlow = new FlowDefinition {
    FlowId = "faq_flow",
    Name = "FAQ Flow",
    Description = "Answers frequently asked questions",
    InitialStepId = "welcome",
    Steps = new List<FlowStep> {
        new FlowStep {
            StepId = "welcome",
            Prompt = "📚 FAQ Categories:\n\n1. General\n2. Payments\n3. Support\n\nReply with a number or ask a question:",
            InputType = FlowInputType.Choice,
            VariableName = "faq_category",
            QuickReplies = new List<string> { "1", "2", "3", "General", "Payments", "Support" },
            Transitions = new List<FlowTransition> {
                new FlowTransition { TargetStepId = "general_info", Condition = new FlowCondition { VariableName = "faq_category", Operator = FlowConditionOperator.Equals, Value = "1" } },
                new FlowTransition { TargetStepId = "general_info", Condition = new FlowCondition { VariableName = "faq_category", Operator = FlowConditionOperator.Equals, Value = "General" } },
                new FlowTransition { TargetStepId = "payments_info", Condition = new FlowCondition { VariableName = "faq_category", Operator = FlowConditionOperator.Equals, Value = "2" } },
                new FlowTransition { TargetStepId = "payments_info", Condition = new FlowCondition { VariableName = "faq_category", Operator = FlowConditionOperator.Equals, Value = "Payments" } },
                new FlowTransition { TargetStepId = "support_info", Condition = new FlowCondition { VariableName = "faq_category", Operator = FlowConditionOperator.Equals, Value = "3" } },
                new FlowTransition { TargetStepId = "support_info", Condition = new FlowCondition { VariableName = "faq_category", Operator = FlowConditionOperator.Equals, Value = "Support" } }
            }
        },
        new FlowStep {
            StepId = "general_info",
            Prompt = "📖 General Information:\n\nThe bot helps you with various tasks...\n\nType 'back' to return or 'home' to go to main menu:",
            InputType = FlowInputType.Text,
            VariableName = "general_info_response",
            QuickReplies = new List<string> { "back", "home" }
        },
        new FlowStep {
            StepId = "payments_info",
            Prompt = "💳 Payment Information:\n\nPayments are processed via Stripe...\n\nType 'back' to return or 'home' to go to main menu:",
            InputType = FlowInputType.Text,
            VariableName = "payments_info_response",
            QuickReplies = new List<string> { "back", "home" }
        },
        new FlowStep {
            StepId = "support_info",
            Prompt = "🎧 Support:\n\nContact support@company.com or visit our help center...\n\nType 'back' to return or 'home' to go to main menu:",
            InputType = FlowInputType.Text,
            VariableName = "support_info_response",
            QuickReplies = new List<string> { "back", "home" },
            IsTerminal = true
        }
    },
    CompletionMenuId = "main_menu"
};

// Register the flow
var flowEngine = serviceProvider.GetRequiredService<IConversationFlowEngine>();
await flowEngine.RegisterFlowAsync(faqFlow);
```

### Step 7: Update Configuration

Review your `appsettings.json` and update with new options:

```json
{
  "ConversationFlowOptions": {
    "DefaultFlowTimeout": "00:10:00",
    "MaxActiveFlowsPerUser": 5,
    "EnableFlowTimeout": true
  },
  "FlowStateConfiguration": {
    "SessionTimeoutMinutes": 30,
    "StateCleanupIntervalMinutes": 5,
    "UseDistributedState": false
  }
}
```

### Step 8: Test Thoroughly

1. Run existing tests
2. Test all commands
3. Test conversation flows
4. Verify session management
5. Check event publishing

---

## Code Examples: Old vs New

### Example 1: Simple Two-Step Form

**v1.x (Manual State Management):**
```csharp
// Complex state tracking
var session = await sessionService.GetOrCreateSessionAsync(userId, chatId);

if (session.GetContextData("step") == "name") {
    // Process name
    session.SetContextData("name", input);
    session.SetContextData("step", "email");
    await sessionService.UpdateSessionAsync(session);
}
else if (session.GetContextData("step") == "email") {
    // Process email
    session.SetContextData("email", input);
    session.SetContextData("step", "complete");
    await sessionService.UpdateSessionAsync(session);
}
```

**v2.0 (Conversation Flow):**
```csharp
var flowDefinition = new FlowDefinition {
    FlowId = "registration",
    Name = "User Registration",
    InitialStepId = "ask_name",
    Steps = new List<FlowStep> {
        new FlowStep {
            StepId = "ask_name",
            Prompt = "What is your name?",
            InputType = FlowInputType.Text,
            VariableName = "user_name",
            Transitions = new List<FlowTransition> {
                new FlowTransition { TargetStepId = "ask_email" }
            }
        },
        new FlowStep {
            StepId = "ask_email",
            Prompt = "What is your email?",
            InputType = FlowInputType.Email,
            VariableName = "user_email",
            IsTerminal = true
        }
    }
};

await flowEngine.RegisterFlowAsync(flowDefinition);
await flowEngine.StartFlowAsync("registration", userId, chatId);

// User input is automatically processed by the flow engine
// Variables are stored in UserFlowState
```

### Example 2: Conditional Logic

**v1.x (Manual Conditions):**
```csharp
var session = await sessionService.GetSessionAsync(userId);
var age = int.Parse(session.GetContextData("age"));

if (age >= 18) {
    await SendAdultMenuAsync(userId, chatId);
} else {
    await SendMinorMenuAsync(userId, chatId);
}
```

**v2.0 (Flow Conditions):**
```csharp
var flowDefinition = new FlowDefinition {
    FlowId = "age_verification",
    InitialStepId = "ask_age",
    Steps = new List<FlowStep> {
        new FlowStep {
            StepId = "ask_age",
            Prompt = "How old are you?",
            InputType = FlowInputType.Number,
            VariableName = "user_age",
            Validation = new FlowValidation {
                MinValue = 1,
                MaxValue = 120,
                ErrorMessage = "Please enter a valid age between 1 and 120"
            },
            Transitions = new List<FlowTransition> {
                new FlowTransition {
                    TargetStepId = "adult_path",
                    Condition = new FlowCondition {
                        VariableName = "user_age",
                        Operator = FlowConditionOperator.GreaterThan,
                        Value = "17"
                    }
                },
                new FlowTransition {
                    TargetStepId = "minor_path",
                    Condition = new FlowCondition {
                        VariableName = "user_age",
                        Operator = FlowConditionOperator.LessThan,
                        Value = "18"
                    }
                }
            }
        },
        new FlowStep {
            StepId = "adult_path",
            Prompt = "Welcome! Here are adult options...",
            IsTerminal = true
        },
        new FlowStep {
            StepId = "minor_path",
            Prompt = "Welcome! Here are options for minors...",
            IsTerminal = true
        }
    }
};
```

### Example 3: Input Validation

**v1.x (Manual Validation):**
```csharp
var input = message.Content.Trim();
if (string.IsNullOrEmpty(input)) {
    await SendMessageAsync(userId, "Please enter a valid email");
    return;
}

if (!Regex.IsMatch(input, @"^[^@\s]+@[^@\s]+\.[^@\s]+$")) {
    await SendMessageAsync(userId, "Invalid email format");
    return;
}
```

**v2.0 (Built-in Validation):**
```csharp
var flowDefinition = new FlowDefinition {
    FlowId = "email_collection",
    InitialStepId = "ask_email",
    Steps = new List<FlowStep> {
        new FlowStep {
            StepId = "ask_email",
            Prompt = "Please enter your email address:",
            InputType = FlowInputType.Email,
            VariableName = "user_email",
            Validation = new FlowValidation {
                ErrorMessage = "Please enter a valid email address (e.g., user@example.com)"
            },
            IsTerminal = true
        }
    }
};

// Engine automatically validates input against FlowInputType.Email
// Returns error message if validation fails
```

### Example 4: Event Handling

**v1.x (Manual Event Tracking):**
```csharp
// Manual event tracking
await analyticsService.TrackEventAsync("message_received", new {
    userId,
    content = message.Content,
    timestamp = DateTime.UtcNow
});
```

**v2.0 (Automatic Events):**
```csharp
// Subscribe to flow events
var eventBus = serviceProvider.GetRequiredService<IEventBus>();

eventBus.Subscribe<FlowStartedEvent>(async evt => {
    await analyticsService.TrackEventAsync("flow_started", new {
        flowId = evt.FlowId,
        userId = evt.UserId
    });
});

eventBus.Subscribe<FlowCompletedEvent>(async evt => {
    await analyticsService.TrackEventAsync("flow_completed", new {
        flowId = evt.FlowId,
        userId = evt.UserId,
        duration = DateTime.UtcNow - evt.StartedAt
    });
});
```

---

## Configuration Changes

### appsettings.json Changes

**v1.x:**
```json
{
  "SessionConfiguration": {
    "SessionTimeoutMinutes": 30
  }
}
```

**v2.0:**
```json
{
  "ConversationFlowOptions": {
    "DefaultFlowTimeout": "00:10:00",
    "MaxActiveFlowsPerUser": 5
  },
  "FlowStateConfiguration": {
    "SessionTimeoutMinutes": 30,
    "StateCleanupIntervalMinutes": 5,
    "UseDistributedState": false
  },
  "SessionConfiguration": {
    "SessionTimeoutMinutes": 30
  }
}
```

### Environment Variables

All configuration can be overridden via environment variables:

```bash
# Flow configuration
export CONVERSATION_FLOW__DEFAULT_FLOW_TIMEOUT=00:15:00
export CONVERSATION_FLOW__MAX_ACTIVE_FLOWS_PER_USER=10

# Flow state
export FLOW_STATE__SESSION_TIMEOUT_MINUTES=45
export FLOW_STATE__USE_DISTRIBUTED_STATE=true
```

---

## Testing Your Migration

### 1. Unit Tests

Update your tests to match the new APIs:

```csharp
[Fact]
public async Task TestCommandRegistration()
{
    // Arrange
    var command = new Command {
        Name = "/test",
        Description = "Test command"
    };
    
    // Act
    await _commandService.RegisterCommandAsync(command);
    
    // Assert
    var registered = await _commandService.GetCommandAsync("/test");
    Assert.NotNull(registered);
    Assert.Equal("Test command", registered.Description);
}
```

### 2. Integration Tests

Test the complete flow:

```csharp
[Fact]
public async Task TestConversationFlow()
{
    // Arrange
    var flowEngine = serviceProvider.GetRequiredService<IConversationFlowEngine>();
    var flowDefinition = CreateTestFlow();
    
    // Act
    await flowEngine.RegisterFlowAsync(flowDefinition);
    await flowEngine.StartFlowAsync("test_flow", 123L, 456L);
    
    // Simulate user input
    var result = await flowEngine.ProcessInputAsync(123L, "test input");
    
    // Assert
    Assert.True(result.IsValid);
    Assert.NotNull(result.FlowState);
}
```

### 3. End-to-End Tests

Test the complete bot behavior with real Telegram updates.

### 4. Performance Tests

Verify that:
- Flow registration is fast (< 10ms per flow)
- Input processing is efficient (< 5ms per input)
- State persistence doesn't bottleneck the system

---

## Common Migration Patterns

### Pattern 1: Replacing Manual State Machines

**Old:**
```csharp
while (true) {
    var step = session.GetContextData("current_step");
    
    if (step == "step1") {
        // Handle step 1
        session.SetContextData("current_step", "step2");
    }
    else if (step == "step2") {
        // Handle step 2
        break; // Complete
    }
    
    await sessionService.UpdateSessionAsync(session);
    await Task.Delay(100);
}
```

**New:**
```csharp
var flowDefinition = new FlowDefinition {
    FlowId = "multi_step",
    InitialStepId = "step1",
    Steps = new List<FlowStep> {
        new FlowStep { StepId = "step1", Prompt = "Step 1", InputType = FlowInputType.Text },
        new FlowStep { StepId = "step2", Prompt = "Step 2", InputType = FlowInputType.Text, IsTerminal = true }
    }
};

await flowEngine.RegisterFlowAsync(flowDefinition);
await flowEngine.StartFlowAsync("multi_step", userId, chatId);
// Flow engine handles state transitions automatically
```

### Pattern 2: Replacing Conditional Menus

**Old:**
```csharp
var menu = new Menu {
    Id = "main",
    Title = "Main Menu"
};

var user = await userService.GetUserAsync(userId);
if (user.Age >= 18) {
    menu.AddButton("Adult Section", "adult");
} else {
    menu.AddButton("Minor Section", "minor");
}
```

**New:**
```csharp
var flowDefinition = new FlowDefinition {
    FlowId = "age_based_menu",
    InitialStepId = "ask_age",
    Steps = new List<FlowStep> {
        new FlowStep {
            StepId = "ask_age",
            Prompt = "How old are you?",
            InputType = FlowInputType.Number,
            VariableName = "user_age"
        },
        new FlowStep {
            StepId = "adult_menu",
            Prompt = "Adult Section",
            IsTerminal = true,
            Condition = new FlowCondition {
                VariableName = "user_age",
                Operator = FlowConditionOperator.GreaterThan,
                Value = "17"
            }
        },
        new FlowStep {
            StepId = "minor_menu",
            Prompt = "Minor Section",
            IsTerminal = true,
            Condition = new FlowCondition {
                VariableName = "user_age",
                Operator = FlowConditionOperator.LessThan,
                Value = "18"
            }
        }
    }
};
```

---

## Troubleshooting Migration Issues

### Issue: HandlerType is required in v1.x but removed in v2.0

**Solution:** Remove the `HandlerType` property from your command definitions. The framework now uses a simpler command routing system.

### Issue: Sessions not persisting

**Solution:** Check if you're using `IConversationFlowEngine` for flows. Flows have their own state management separate from sessions.

### Issue: Commands not responding

**Solution:** Update your message processing to use `IBotOrchestrator.HandleUpdateAsync()` instead of `IMessageService.ProcessIncomingMessageAsync()`.

### Issue: Flow not advancing

**Solution:** Verify that:
1. Flow is registered with the engine
2. Input matches the expected `FlowInputType`
3. Conditions are properly defined
4. No validation errors are occurring

### Issue: Performance degradation

**Solution:** Check if you're:
1. Creating too many flows
2. Using complex conditions
3. Not cleaning up completed flows
4. Using distributed state without Redis

---

## Rollback Plan

If migration issues arise:

1. **Revert to v1.x**:
   ```bash
   git checkout v1.x-branch
   dotnet add package TelegramBotFramework --version 1.*.*
   ```

2. **Isolate changes**: Create a feature branch for v2 migration

3. **Gradual rollout**: Deploy to staging first, monitor, then production

4. **Feature flags**: Use configuration to enable/disable v2 features


---

## Additional Resources

- [Conversation Flow Engine Documentation](./conversation-flow-engine.md)
- [API Reference](../api-reference.md)
- [Examples Directory](../examples/)
- [GitHub Issues](https://github.com/sarmkadan/telegram-bot-framework-dotnet/issues)

---

## Support

For migration assistance:
- 📮 [Open an issue](https://github.com/sarmkadan/telegram-bot-framework-dotnet/issues)
- 📧 Email: rutova2@gmail.com
- 🌐 Website: https://sarmkadan.com

---

**Last Updated:** May 2026
**Version:** 2.0.0

*Copyright (c) 2026 Vladyslav Zaiets*
