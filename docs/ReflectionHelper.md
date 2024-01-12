# ReflectionHelper

The `ReflectionHelper` class provides a collection of static utility methods that simplify common reflection tasks in .NET. It is designed to reduce boilerplate when working with types, attributes, properties, methods, and instance creation, making it easier to implement plugin systems, dependency injection, or convention-based configurations.

## API

### `GetTypesImplementing<TInterface>`

```csharp
public static IEnumerable<Type> GetTypesImplementing<TInterface>()
```

Returns all types that implement the specified interface `TInterface`. The search is performed across all loaded assemblies.

- **Type parameters**: `TInterface` – the interface type to search for.
- **Returns**: An `IEnumerable<Type>` containing types that implement `TInterface`. If no types are found, an empty sequence is returned.
- **Throws**: `ArgumentNullException` if `TInterface` is not a valid interface type (e.g., if it is a class or struct).

### `GetTypesWithAttribute<TAttribute>`

```csharp
public static IEnumerable<Type> GetTypesWithAttribute<TAttribute>()
```

Returns all types that are decorated with the specified attribute `TAttribute`. The search is performed across all loaded assemblies.

- **Type parameters**: `TAttribute` – the attribute type to search for (must derive from `Attribute`).
- **Returns**: An `IEnumerable<Type>` containing types that have the attribute applied. If no types are found, an empty sequence is returned.
- **Throws**: `ArgumentNullException` if `TAttribute` is not a valid attribute type.

### `CreateInstance<T>`

```csharp
public static T? CreateInstance<T>()
```

Creates an instance of the type specified by `T` using its parameterless constructor.

- **Type parameters**: `T` – the type to instantiate.
- **Returns**: A new instance of `T`, or `null` if the type does not have a public parameterless constructor or if instantiation fails.
- **Throws**: `InvalidOperationException` if the type cannot be instantiated (e.g., it is abstract, an interface, or a static class).

### `GetProperties<TAttribute>`

```csharp
public static IEnumerable<PropertyInfo> GetProperties<TAttribute>()
```

Returns all properties that are decorated with the specified attribute `TAttribute`. The search is performed across all loaded types.

- **Type parameters**: `TAttribute` – the attribute type to search for (must derive from `Attribute`).
- **Returns**: An `IEnumerable<PropertyInfo>` containing properties that have the attribute applied. If no properties are found, an empty sequence is returned.
- **Throws**: `ArgumentNullException` if `TAttribute` is not a valid attribute type.

### `GetPublicMethods`

```csharp
public static IEnumerable<MethodInfo> GetPublicMethods()
```

Returns all public methods of a given type. The type is provided as an implicit parameter (e.g., via a generic type argument or an explicit `Type` parameter – refer to the specific overload in use).

- **Returns**: An `IEnumerable<MethodInfo>` containing all public methods of the target type. If the type has no public methods, an empty sequence is returned.
- **Throws**: `ArgumentNullException` if the target type is `null`.

### `GetPropertyValue`

```csharp
public static object? GetPropertyValue()
```

Retrieves the value of a property on a given object. The property is identified by name.

- **Parameters** (implied): `object instance`, `string propertyName`.
- **Returns**: The current value of the property, or `null` if the property does not exist or its value is `null`.
- **Throws**: `ArgumentNullException` if the instance or property name is `null`. `InvalidOperationException` if the property cannot be read (e.g., it is write-only).

### `SetPropertyValue`

```csharp
public static bool SetPropertyValue()
```

Sets the value of a property on a given object. The property is identified by name.

- **Parameters** (implied): `object instance`, `string propertyName`, `object? value`.
- **Returns**: `true` if the value was successfully set; `false` if the property does not exist, is read-only, or the value cannot be assigned.
- **Throws**: `ArgumentNullException` if the instance or property name is `null`.

### `IsSubclassOfGeneric`

```csharp
public static bool IsSubclassOfGeneric()
```

Determines whether a type is a subclass of a generic type definition (e.g., `List<>`).

- **Parameters** (implied): `Type type`, `Type genericTypeDefinition`.
- **Returns**: `true` if `type` is a subclass of the open generic type; otherwise `false`.
- **Throws**: `ArgumentNullException` if either parameter is `null`. `ArgumentException` if `genericTypeDefinition` is not a generic type definition.

### `GetDisplayName`

```csharp
public static string GetDisplayName()
```

Retrieves a human-readable display name for a type, member, or other reflection element. The exact behavior depends on the overload used (e.g., may use `DisplayNameAttribute` or fall back to the name).

- **Parameters** (implied): A reflection object such as `Type`, `MemberInfo`, or `PropertyInfo`.
- **Returns**: A string representing the display name. If no display name attribute is found, the simple name of the element is returned.
- **Throws**: `ArgumentNullException` if the input reflection object is `null`.

### `GetConstants`

```csharp
public static IEnumerable<FieldInfo> GetConstants()
```

Returns all constant fields (i.e., `public static readonly` or `public const` fields) of a given type.

- **Parameters** (implied): `Type type`.
- **Returns**: An `IEnumerable<FieldInfo>` containing the constant fields. If no constants are found, an empty sequence is returned.
- **Throws**: `ArgumentNullException` if the type is `null`.

## Usage

### Example 1: Discovering and instantiating handlers

```csharp
using System;
using System.Linq;
using Telegram.Bot.Framework;

public interface IMessageHandler
{
    Task HandleAsync(Update update);
}

// Register all types that implement IMessageHandler
var handlerTypes = ReflectionHelper.GetTypesImplementing<IMessageHandler>();
foreach (var handlerType in handlerTypes)
{
    var handler = ReflectionHelper.CreateInstance<IMessageHandler>();
    if (handler != null)
    {
        // Register handler in DI or invoke directly
        Console.WriteLine($"Registered handler: {handlerType.Name}");
    }
}
```

### Example 2: Inspecting properties with a custom attribute

```csharp
using System;
using System.Reflection;

[AttributeUsage(AttributeTargets.Property)]
public class ConfigurableAttribute : Attribute { }

public class AppSettings
{
    [Configurable]
    public string ConnectionString { get; set; }

    [Configurable]
    public int MaxRetries { get; set; }

    public string InternalKey { get; set; }
}

// Get all configurable properties
var configurableProps = ReflectionHelper.GetProperties<ConfigurableAttribute>();
foreach (var prop in configurableProps)
{
    Console.WriteLine($"Configurable property: {prop.Name} ({prop.PropertyType.Name})");
}
```

## Notes

- **Thread safety**: All methods in `ReflectionHelper` are static and do not modify any shared state. They are inherently thread-safe and can be called concurrently from multiple threads without synchronization.
- **Null handling**: Methods that accept reflection objects (e.g., `Type`, `PropertyInfo`) will throw `ArgumentNullException` if a required argument is `null`. Always validate inputs before calling these methods.
- **Performance**: Reflection operations are relatively expensive. Avoid calling these methods repeatedly in performance-critical loops. Cache results where possible.
- **Generic type definitions**: `IsSubclassOfGeneric` works only with open generic type definitions (e.g., `typeof(List<>)`). Passing a closed generic type (e.g., `typeof(List<int>)`) will return `false` even if the type is a subclass.
- **Assembly scanning**: `GetTypesImplementing<T>` and `GetTypesWithAttribute<T>` scan all currently loaded assemblies. Types in assemblies that have not been loaded will not be discovered. Use `Assembly.Load` or similar if necessary.
- **Nullable return types**: Methods returning `T?` (e.g., `CreateInstance<T>`) may return `null` if instantiation fails. Always check for `null` before using the result.
