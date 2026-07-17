namespace TelegramBotFramework.Models;

/// <summary>
/// Provides extension methods for <see cref="ExecutionContext"/>.
/// </summary>
public static class ExecutionContextExtensions
{
    /// <summary>
    /// Adds an error to the execution context.
    /// </summary>
    /// <param name="context">The execution context.</param>
    /// <param name="error">The error message to add.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="error"/> is null or whitespace.</exception>
    public static void AddError(this ExecutionContext context, string error)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentException.ThrowIfNullOrEmpty(error);

        context.AddError(error);
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
