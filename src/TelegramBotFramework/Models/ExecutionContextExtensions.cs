namespace TelegramBotFramework.Models;

/// <summary>
/// Provides extension methods for <see cref="ExecutionContext"/>.
/// </summary>
public static class ExecutionContextExtensions
{
    /// <summary>
    /// Gets a parameter of the specified type from the execution context.
    /// </summary>
    /// <typeparam name="T">The type of the parameter.</typeparam>
    /// <param name="context">The execution context.</param>
    /// <param name="key">The key of the parameter.</param>
    /// <returns>The parameter of the specified type, or default(T) if not found.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is null.</exception>
    public static T? GetParameter<T>(this ExecutionContext context, string key)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.GetParameter<T>(key);
    }

    /// <summary>
    /// Adds an error to the execution context.
    /// </summary>
    /// <param name="context">The execution context.</param>
    /// <param name="error">The error to add.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> or <paramref name="error"/> is null.</exception>
    public static void AddError(this ExecutionContext context, string error)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentException.ThrowIfNullOrEmpty(error);

        if (context.Errors == null)
        {
            context.Errors = new List<string>();
        }

        context.Errors.Add(error);
    }

    /// <summary>
    /// Checks if the execution context has any errors.
    /// </summary>
    /// <param name="context">The execution context.</param>
    /// <returns>true if the execution context has any errors; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is null.</exception>
    public static bool HasErrors(this ExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.Errors?.Count > 0;
    }
}
