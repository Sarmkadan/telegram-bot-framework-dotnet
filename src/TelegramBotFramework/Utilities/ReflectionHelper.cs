// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Utilities;

using System.Reflection;

/// <summary>
/// Utility class for reflection operations.
/// Provides methods for type inspection and dynamic instantiation.
/// </summary>
public static class ReflectionHelper
{
    /// <summary>
    /// Gets all types from an assembly that implement a specific interface.
    /// </summary>
    public static IEnumerable<Type> GetTypesImplementing<TInterface>(Assembly? assembly = null)
    {
        assembly ??= typeof(TInterface).Assembly;
        var interfaceType = typeof(TInterface);

        return assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && interfaceType.IsAssignableFrom(t));
    }

    /// <summary>
    /// Gets all types from an assembly that are decorated with a specific attribute.
    /// </summary>
    public static IEnumerable<Type> GetTypesWithAttribute<TAttribute>(Assembly? assembly = null) where TAttribute : Attribute
    {
        assembly ??= Assembly.GetCallingAssembly();

        return assembly.GetTypes()
            .Where(t => t.GetCustomAttribute<TAttribute>() != null);
    }

    /// <summary>
    /// Creates an instance of a type using its default constructor.
    /// </summary>
    public static T? CreateInstance<T>(Type type) where T : class
    {
        if (type == null)
            return null;

        try
        {
            return Activator.CreateInstance(type) as T;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Creates an instance with constructor arguments.
    /// </summary>
    public static T? CreateInstance<T>(Type type, params object[] args) where T : class
    {
        if (type == null)
            return null;

        try
        {
            return Activator.CreateInstance(type, args) as T;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Gets all properties of a type with optional filtering by attribute.
    /// </summary>
    public static IEnumerable<PropertyInfo> GetProperties<TAttribute>(Type type) where TAttribute : Attribute
    {
        return type.GetProperties()
            .Where(p => p.GetCustomAttribute<TAttribute>() != null);
    }

    /// <summary>
    /// Gets all public methods of a type.
    /// </summary>
    public static IEnumerable<MethodInfo> GetPublicMethods(Type type)
    {
        return type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => !m.IsSpecialName);
    }

    /// <summary>
    /// Gets the value of a property from an object using reflection.
    /// </summary>
    public static object? GetPropertyValue(object obj, string propertyName)
    {
        if (obj == null)
            return null;

        var property = obj.GetType().GetProperty(propertyName);
        return property?.GetValue(obj);
    }

    /// <summary>
    /// Sets the value of a property on an object using reflection.
    /// </summary>
    public static bool SetPropertyValue(object obj, string propertyName, object? value)
    {
        if (obj == null)
            return false;

        var property = obj.GetType().GetProperty(propertyName);
        if (property == null || !property.CanWrite)
            return false;

        try
        {
            property.SetValue(obj, value);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Determines if a type is a subclass of a generic type.
    /// </summary>
    public static bool IsSubclassOfGeneric(Type toCheck, Type generic)
    {
        while (toCheck != typeof(object))
        {
            var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
            if (generic == cur)
                return true;

            toCheck = toCheck.BaseType!;
        }

        return false;
    }

    /// <summary>
    /// Gets the display name of a type (handles nullable and generic types).
    /// </summary>
    public static string GetDisplayName(Type type)
    {
        if (type.IsGenericType)
        {
            var genericArgs = string.Join(", ", type.GetGenericArguments().Select(GetDisplayName));
            var baseName = type.Name.Split('`')[0];
            return $"{baseName}<{genericArgs}>";
        }

        if (Nullable.GetUnderlyingType(type) is Type underlyingType)
            return GetDisplayName(underlyingType) + "?";

        return type.Name;
    }

    /// <summary>
    /// Gets all constants defined in a type.
    /// </summary>
    public static IEnumerable<FieldInfo> GetConstants(Type type)
    {
        return type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(f => f.IsLiteral);
    }
}
