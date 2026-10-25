# EventPublisherExtensions

Extension methods for `EventPublisher` that publish events with a correlation ID, either one event or a collection. The class lives in the `TelegramBotFramework.Events` namespace (`src/TelegramBotFramework/Events/EventPublisherExtensions.cs`).

Both methods extend the concrete `EventPublisher` class, not `IEventPublisher`. Both are `async` and return a `Task`.

## How the correlation ID is applied

Each method calls `EventPublisher.WithCorrelationId(...)` and then `PublishAsync(...)`. Two effects follow from that:

- **The publisher keeps the correlation ID after the call.** `WithCorrelationId` stores the value on the shared `EventPublisher` instance and returns the same instance. The ID stays set until another call replaces it. Later calls to the publisher's built-in methods (`PublishMessageReceivedAsync`, `PublishCommandExecutedAsync`, `PublishBotStateChangedAsync`) reuse it.
- **The ID is not written to the event passed in.** `PublishAsync<TEvent>` forwards the event to the bus unchanged. For a custom event, `CorrelationId` is whatever the event already carried. If you need the ID on a custom event, set `CorrelationId` on the event yourself before publishing.

Because the ID is stored on a shared instance, avoid calling these methods concurrently on the same `EventPublisher` with different IDs.

## API

### `PublishWithCorrelationAsync<TEvent>`

Publishes a single event with a correlation ID.

- **Type parameters**
  - `TEvent`: The event type. Must be a reference type that implements `IEvent`.
- **Parameters**
  - `publisher` (`EventPublisher`): The publisher instance. Extended by this method.
  - `event` (`TEvent`): The event to publish.
  - `correlationId` (`string`): The correlation ID to set on the publisher before publishing.
- **Return value**
  - A `Task` that completes when the bus has published the event.
- **Exceptions**
  - Throws `ArgumentNullException` if `publisher`, `event`, or `correlationId` is `null`. Empty and whitespace strings are not rejected.
  - Exceptions from `IEventBus.PublishAsync` propagate to the caller.

### `PublishCollectionAsync<TEvent>`

Publishes a sequence of events one after another, using the same correlation ID for each.

- **Type parameters**
  - `TEvent`: The event type. Must be a reference type that implements `IEvent`.
- **Parameters**
  - `publisher` (`EventPublisher`): The publisher instance. Extended by this method.
  - `events` (`IEnumerable<TEvent>`): The events to publish, in enumeration order.
  - `correlationId` (`string`): The correlation ID to set on the publisher before each publish.
- **Return value**
  - A `Task` that completes after every event has been published.
- **Exceptions**
  - Throws `ArgumentNullException` if `publisher`, `events`, or `correlationId` is `null`. An empty collection is valid and publishes nothing.
  - Events are published sequentially and are not rolled back. If publishing one event throws, the exception propagates and the remaining events are not published.

## Usage

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TelegramBotFramework.Events;

// A custom event. EventBase supplies EventType, OccurredAt and CorrelationId.
public sealed class OrderPlacedEvent : EventBase
{
    public long UserId { get; init; }
    public string Sku { get; init; } = string.Empty;
}

public sealed class CheckoutService
{
    private readonly EventPublisher _publisher;

    public CheckoutService(EventPublisher publisher)
    {
        _publisher = publisher;
    }

    // Publish one event. The correlation ID groups it with the request that caused it.
    public async Task PlaceOrderAsync(long userId, string sku, string requestId)
    {
        var orderEvent = new OrderPlacedEvent { UserId = userId, Sku = sku, CorrelationId = requestId };

        await _publisher.PublishWithCorrelationAsync(orderEvent, requestId);
    }

    // Publish several events under one correlation ID.
    public async Task PlaceOrdersAsync(long userId, IReadOnlyList<string> skus, string batchId)
    {
        var events = new List<OrderPlacedEvent>(skus.Count);

        foreach (var sku in skus)
        {
            events.Add(new OrderPlacedEvent { UserId = userId, Sku = sku, CorrelationId = batchId });
        }

        await _publisher.PublishCollectionAsync(events, batchId);
    }
}

// Wiring, using an in-memory bus. Dispatch options are left at their defaults.
var bus = new EventBus();
var publisher = new EventPublisher(bus);
var checkout = new CheckoutService(publisher);

await checkout.PlaceOrderAsync(userId: 42, sku: "SKU-1", requestId: "req-2026-10-25-001");
await checkout.PlaceOrdersAsync(userId: 42, skus: new[] { "SKU-2", "SKU-3" }, batchId: "batch-7");
```

In this example each event sets `CorrelationId` itself, because the extension methods do not write the ID onto custom events (see [How the correlation ID is applied](#how-the-correlation-id-is-applied)). The extension calls still pass the same value, so the publisher's stored ID matches for any built-in events published afterward.
