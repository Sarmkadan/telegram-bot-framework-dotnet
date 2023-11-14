# Migrating from direct Telegram.Bot usage

This guide helps developers who currently use the `Telegram.Bot` library directly — with manual polling loops or raw webhook dispatching — migrate to this framework's command, middleware, and state-machine abstractions.

---

## Table of Contents

- [Why migrate?](#why-migrate)
- [Core concept mapping](#core-concept-mapping)
- [Step 1 — Replace manual update routing](#step-1--replace-manual-update-routing)
- [Step 2 — Wrap existing business logic in command handlers](#step-2--wrap-existing-business-logic-in-command-handlers)
- [Step 3 — Migrate inline keyboards and menus](#step-3--migrate-inline-keyboards-and-menus)
- [Step 4 — Replace session state management](#step-4--replace-session-state-management)
- [Step 5 — Migrate multi-step wizards to conversation flows](#step-5--migrate-multi-step-wizards-to-conversation-flows)
- [Step 6 — Wire up dependency injection](#step-6--wire-up-dependency-injection)
- [Frequently asked questions](#frequently-asked-questions)

---

## Why migrate?

With direct `Telegram.Bot` usage you typically end up with:

- A large `if`/`switch` block dispatching on `update.Type` or `message.Text`
- Ad-hoc state stored in static dictionaries or database tables
- No common error-handling path
- Rate-limiting and logging scattered throughout handlers

This framework replaces that plumbing with:

- Typed **command registration** and automatic dispatch
- A **middleware pipeline** for cross-cutting concerns (logging, auth, rate limiting)
- A **conversation flow engine** for multi-step wizards with input validation and branching
- Structured **session management** backed by a swappable repository

---

## Core concept mapping

| Telegram.Bot (raw)                         | This framework                                  |
|--------------------------------------------|-------------------------------------------------|
| `UpdateType.Message` + `if` on text        | `Command` registered with `ICommandService`     |
| `UpdateType.CallbackQuery`                 | `MenuButton.CallbackData` + `IMenuService`      |
| Manual state dictionary per user           | `UserSession.ContextData` / `UserFlowState`     |
| `botClient.SendTextMessageAsync(...)`      | `context.PendingResponse` / orchestrator result |
| Polling loop / webhook controller          | `IBotOrchestrator.ProcessUserMessageAsync`      |
| Manual try/catch around each handler       | `ErrorHandlingMiddleware` in the pipeline       |
| Custom rate-limit guard in every handler   | `RateLimitMiddleware` (one place)               |

---

## Step 1 — Replace manual update routing

**Before (raw Telegram.Bot):**

```csharp
bot.OnMessage += async (sender, e) =>
{
    var msg = e.Message;
    if (msg.Text == "/start")
        await bot.SendTextMessageAsync(msg.Chat.Id, "Welcome!");
    else if (msg.Text == "/help")
        await bot.SendTextMessageAsync(msg.Chat.Id, "Here is help...");
    // ... dozens more else-if blocks
};

bot.StartReceiving();
```

**After (framework):**

```csharp
// Startup — register commands once
await commandService.RegisterCommandAsync(new Command
{
    Name = "start",
    Description = "Welcome message"
});
await commandService.RegisterCommandAsync(new Command
{
    Name = "help",
    Description = "Help text"
});

// On each incoming update — single call replaces the entire dispatch block
var context = await orchestrator.ProcessUserMessageAsync(
    userId:    update.Message.From.Id,
    chatId:    update.Message.Chat.Id,
    content:   update.Message.Text,
    firstName: update.Message.From.FirstName);

// Check for a short-circuit response injected by middleware (e.g., rate limit)
if (context.IsStopped && context.PendingResponse is not null)
    await bot.SendTextMessageAsync(context.ChatId, context.PendingResponse);
```

---

## Step 2 — Wrap existing business logic in command handlers

Each `/command` handler becomes a registered `Command` object. The handler logic you previously inlined now lives in a separate class or service that you call after receiving the `ExecutionContext`.

```csharp
// Register
await commandService.RegisterCommandAsync(new Command
{
    Name        = "order",
    Description = "Place a new order",
    Parameters  = new List<CommandParameter>
    {
        new() { Name = "item", IsRequired = true }
    }
});

// Handle (your existing business logic — no changes required there)
var context = await orchestrator.ExecuteUserCommandAsync(
    userId:      userId,
    chatId:      chatId,
    commandName: "order",
    parameters:  new Dictionary<string, object> { ["item"] = "pizza" });

if (context.IsValid)
    await orderService.PlaceOrderAsync(context.UserId, (string)context.Parameters!["item"]);
```

You do **not** need to rewrite existing service classes. Wrap the call site and pass parameters through `context.Parameters`.

---

## Step 3 — Migrate inline keyboards and menus

**Before:**

```csharp
var keyboard = new InlineKeyboardMarkup(new[]
{
    new[] { InlineKeyboardButton.WithCallbackData("Option A", "opt_a") },
    new[] { InlineKeyboardButton.WithCallbackData("Option B", "opt_b") }
});
await bot.SendTextMessageAsync(chatId, "Choose:", replyMarkup: keyboard);

// … elsewhere …
bot.OnCallbackQuery += async (_, e) =>
{
    if (e.CallbackQuery.Data == "opt_a") { /* … */ }
    if (e.CallbackQuery.Data == "opt_b") { /* … */ }
};
```

**After:**

```csharp
// Build and register the menu once (e.g., at startup)
var menu = new Menu { Id = "main_choice", Title = "Choose:" };
menu.AddButton(new MenuButton { Label = "Option A", CallbackData = "opt_a", Action = ButtonAction.Callback });
menu.AddButton(new MenuButton { Label = "Option B", CallbackData = "opt_b", Action = ButtonAction.Callback });
await menuService.CreateMenuAsync(menu);

// Display it
await orchestrator.DisplayMenuAsync(userId, "main_choice");

// Handle button press (one call, no if-chain)
await orchestrator.HandleMenuButtonAsync(userId, "main_choice", callbackQuery.Data);
```

> **Important:** Telegram limits `callback_data` to **64 bytes** (UTF-8). The framework enforces this limit in `Menu.AddButton()` and will throw an `InvalidOperationException` with a descriptive message if a value is too long. Shorten your prefixes or use opaque short keys mapped to a lookup table.

---

## Step 4 — Replace session state management

**Before (ad-hoc dictionary):**

```csharp
private static readonly Dictionary<long, string> _userStep = new();

// Inside handler:
_userStep[userId] = "awaiting_name";
// Later:
var step = _userStep.TryGetValue(userId, out var s) ? s : null;
```

**After (session context):**

```csharp
// Write
await sessionService.UpdateSessionContextAsync(sessionId, "step", "awaiting_name");

// Read
var step = await sessionService.GetSessionContextAsync(sessionId, "step");
```

Sessions are created automatically by the orchestrator. Retrieve the active session for a user with:

```csharp
var session = await sessionService.GetActiveSessionAsync(userId);
```

Sessions expire automatically according to `BotConfiguration.SessionTimeoutMinutes`.

---

## Step 5 — Migrate multi-step wizards to conversation flows

If you have a multi-step form (e.g., collect name → email → phone), replace the manual state machine with a `FlowDefinition`:

**Before:**

```csharp
// Manual step tracking
if (_userStep[userId] == "ask_name")
{
    _collected[userId]["name"] = message.Text;
    _userStep[userId] = "ask_email";
    await bot.SendTextMessageAsync(chatId, "Now enter your email:");
}
else if (_userStep[userId] == "ask_email")
{
    if (!IsValidEmail(message.Text)) { await bot.SendTextMessageAsync(chatId, "Invalid email."); return; }
    _collected[userId]["email"] = message.Text;
    _userStep[userId] = "ask_phone";
    await bot.SendTextMessageAsync(chatId, "Now enter your phone:");
}
// etc.
```

**After:**

```csharp
var flow = ConversationFlowExtensions.CreateFlow("user_onboarding", "User Onboarding", "ask_name")
    .AddStep(new FlowStep
    {
        StepId       = "ask_name",
        Prompt       = "What is your name?",
        InputType    = FlowInputType.Text,
        VariableName = "name",
        Validation   = new FlowValidation { MinLength = 2 }
    })
    .AddStep(new FlowStep
    {
        StepId            = "ask_email",
        Prompt            = "Enter your email:",
        InputType         = FlowInputType.Email,
        VariableName      = "email",
        DefaultNextStepId = "ask_phone"
    })
    .AddStep(new FlowStep
    {
        StepId       = "ask_phone",
        Prompt       = "Enter your phone number:",
        InputType    = FlowInputType.PhoneNumber,
        VariableName = "phone",
        IsTerminal   = true
    })
    .WithTimeout(TimeSpan.FromMinutes(10))
    .Build();

await flowEngine.RegisterFlowAsync(flow);

// Start the flow for a user
await flowEngine.StartFlowAsync(userId, chatId, "user_onboarding");

// For every subsequent message from this user:
var result = await flowEngine.ProcessInputAsync(userId, message.Text);
await bot.SendTextMessageAsync(chatId, result.Prompt);

if (result.IsCompleted)
{
    var name  = result.FlowState.Variables["name"];
    var email = result.FlowState.Variables["email"];
    var phone = result.FlowState.Variables["phone"];
    // pass to your existing service
    await registrationService.RegisterAsync(name, email, phone);
}
```

Input validation (type checks, length, regex, allowed values) is handled by the engine — no manual guard clauses needed.

---

## Step 6 — Wire up dependency injection

Add the framework's services in your `Program.cs` / `Startup.cs`:

```csharp
builder.Services
    .AddSingleton<BotConfiguration>(cfg => builder.Configuration
        .GetSection("BotConfiguration").Get<BotConfiguration>()!)
    .AddSingleton<ICommandService, CommandService>()
    .AddSingleton<ISessionService, SessionService>()
    .AddSingleton<IMenuService, MenuService>()
    .AddSingleton<IUserService, UserService>()
    .AddSingleton<IMessageService, MessageService>()
    .AddSingleton<IBotOrchestrator, BotOrchestrator>()
    .AddConversationFlows(options =>
    {
        options.DefaultFlowTimeout       = TimeSpan.FromMinutes(30);
        options.TimeoutEvictionPolicy    = FlowEvictionPolicy.NotifyUser;
        options.OnEviction               = async (state, ct) =>
        {
            // send "your session timed out" via your bot client
            await bot.SendTextMessageAsync(state.ChatId,
                "Your session timed out. Type /start to begin again.", cancellationToken: ct);
        };
    });
```

---

## Frequently asked questions

**Q: Can I keep calling `ITelegramBotClient` directly for Telegram-specific features (e.g., sending photos)?**

Yes. The framework does not wrap every Telegram API method. Use `ITelegramBotClient` for anything the framework does not cover (media, stickers, polls, etc.) and use the framework for routing, state, and middleware.

**Q: Do I have to migrate everything at once?**

No. Start with one command. Register it with `ICommandService`, call `orchestrator.ExecuteUserCommandAsync` from your existing webhook controller for that command only, and keep the rest of your `if`/`switch` block intact. Migrate incrementally.

**Q: The framework's `ExecutionContext` has `IsValid = false` after a middleware runs. What should I do?**

Inspect `context.Errors` for details and `context.PendingResponse` for a user-facing message set by a middleware that called `context.RespondAndStop(...)`. Send `PendingResponse` to the user (if set) and skip your handler logic when `context.IsValid` is `false`.

**Q: How do I test handlers in isolation?**

Construct an `ExecutionContext` directly in your test and pass it to your service. The framework classes have no hidden static state.
