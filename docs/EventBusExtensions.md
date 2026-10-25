# EventBusExtensions

Extension methods that add query and registration helpers to the event bus. The class lives in the `TelegramBotFramework.Events` namespace (`src/TelegramBotFramework/Events/EventBusExtensions.cs`).

Two of the methods extend the concrete `EventBus` class (inspection helpers). The two `UseMiddleware` overloads extend `IEventBus` and return the bus so calls can be chained.

## API

### `IsEventRegistered<TEvent>`

Determines whether the specified event type is registered in the bus.

- **Type parameters**
  - `TEvent`: The event type to check. No constraint is declared on this method.
- **Parameters**
  - `bus` (`EventBus`): The bus instance to inspect. Extends `EventBus`, not `IEventBus`.
- **Return value**
  - `true` if the event type is registered; otherwise, `false`.
- **Exceptions**
  - Throws `ArgumentNullException` if `bus` is `null`.

### `GetTotalSubscriberCount`

Calculates the total number of subscribers across all registered event types.

- **Parameters**
  - `bus` (`EventBus`): The bus instance to inspect. Extends `EventBus`, not `IEventBus`.
- **Return value**
  - The sum of subscriber counts for every registered event type. Returns `0` when no event types are registered.
- **Exceptions**
  - Throws `ArgumentNullException` if `bus` is `null`.

### `UseMiddleware` (single middleware)

Registers one middleware to run around event handling. Middleware executes in the order it is registered.

- **Parameters**
  - `bus` (`IEventBus`): The event bus instance. Extends `IEventBus`.
  - `middleware` (`IEventMiddleware`): The middleware to register.
- **Return value**
  - The same `IEventBus` instance, for method chaining.
- **Exceptions**
  - Throws `ArgumentNullException` if `bus` or `middleware` is `null`.

### `UseMiddleware` (multiple middleware)

Registers several middleware components in one call. Middleware executes in the order it is provided.

- **Parameters**
  - `bus` (`IEventBus`): The event bus instance. Extends `IEventBus`.
  - `middleware` (`params IEventMiddleware[]`): The middleware components to register. Can be passed as a comma-separated list or as an array.
- **Return value**
  - The same `IEventBus` instance, for method chaining.
- **Exceptions**
  - Throws `ArgumentNullException` if `bus` or `middleware` (the array) is `null`.
  - Null elements inside the array are not checked by this method. Their handling depends on `IEventBus.RegisterMiddleware`.

## Usage

```csharp
using System;
using System.Threading.Tasks;
using TelegramBotFramework.Events;

// A custom middleware: logs around event processing.
public sealed class TimingMiddleware : IEventMiddleware
{
    public string MiddlewareName => nameof(TimingMiddleware);

    public async Task InvokeAsync(IEvent evt, Func<Task> next)
    {
        Console.WriteLine($"Before: {evt.EventType}");
        await next();
        Console.WriteLine($"After: {evt.EventType}");
    }
}

// Register middleware in a chain. Both overloads return the bus.
IEventBus bus = new EventBus();
bus.UseMiddleware(new TimingMiddleware())
   .UseMiddleware(new AnotherMiddleware(), new YetAnotherMiddleware());

// Inspect the concrete EventBus instance.
var concreteBus = new EventBus();
bool registered = concreteBus.IsEventRegistered<SomeEvent>(); // SomeEvent: any type implementing IEvent
int subscribers = concreteBus.GetTotalSubscriberCount();
```

`AnotherMiddleware` and `YetAnotherMiddleware` stand for any `IEventMiddleware` implementations. `SomeEvent` stands for any event type that implements `IEvent`.
