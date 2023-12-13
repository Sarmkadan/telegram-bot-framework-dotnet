# EventBus

The `EventBus` class provides a lightweight in-memory publish-subscribe mechanism for decoupling event producers from consumers within the application. It allows components to subscribe to specific event types, publish events asynchronously, and manage subscriptions dynamically.

## API

### `public EventBus`
The constructor initializes a new instance of the `EventBus` class. No parameters are required, and it does not throw exceptions.

---

### `public void Subscribe<TEvent>()`
Subscribes the caller to receive notifications for events of type `TEvent`.

**Type Parameters:**
- `TEvent`: The type of event to subscribe to. Must be a reference type.

**Throws:**
- `ArgumentException`: If `TEvent` is a value type (e.g., `struct`).

---

### `public void Unsubscribe<TEvent>()`
Unsubscribes the caller from receiving notifications for events of type `TEvent`.

**Type Parameters:**
- `TEvent`: The type of event to unsubscribe from. Must be a reference type.

**Throws:**
- `ArgumentException`: If `TEvent` is a value type.

---

### `public async Task PublishAsync<TEvent>(TEvent @event)`
Publishes an event of type `TEvent` asynchronously to all subscribers. Subscribers are notified in the order they subscribed.

**Type Parameters:**
- `TEvent`: The type of event being published.

**Parameters:**
- `@event`: The event instance to publish. Must not be `null`.

**Returns:**
- A `Task` representing the asynchronous operation.

**Throws:**
- `ArgumentNullException`: If `@event` is `null`.
- `InvalidOperationException`: If no subscribers exist for `TEvent`.

---

### `public void Clear()`
Removes all subscriptions for all event types. This method does not throw exceptions.

---

### `public int GetSubscriberCount<TEvent>()`
Returns the number of subscribers currently registered for events of type `TEvent`.

**Type Parameters:**
- `TEvent`: The type of event to query.

**Returns:**
- The number of subscribers. Returns `0` if no subscribers exist.

**Throws:**
- `ArgumentException`: If `TEvent` is a value type.

---

### `public IEnumerable<Type> GetRegisteredEventTypes()`
Returns a collection of all event types that have at least one subscriber.

**Returns:**
- An `IEnumerable<Type>` containing the registered event types. Never returns `null`; returns an empty collection if no types are registered.
