#nullable enable
namespace TelegramBotFramework.Models;

/// <summary>
/// Builder for creating <see cref="Command"/> instances with fluent configuration.
/// </summary>
public sealed class CommandBuilder
{
    private string _name = string.Empty;
    private string _description = string.Empty;
    private string _handlerType = string.Empty;
    private CommandType _type = CommandType.Standard;
    private bool _requiresAdmin = false;
    private bool _isEnabled = true;
    private int _executionCount = 0;
    private List<CommandParameter>? _parameters = null;
    private List<string> _aliases = new();
    private DateTime _createdAt = DateTime.UtcNow;

    /// <summary>
    /// Sets the command name.
    /// </summary>
    /// <param name="name">The command name (must start with '/' for standard commands).</param>
    /// <returns>This builder instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is null, empty, or whitespace.</exception>
    public CommandBuilder WithName(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        _name = name;
        return this;
    }

    /// <summary>
    /// Sets the command description.
    /// </summary>
    /// <param name="description">The command description.</param>
    /// <returns>This builder instance.</returns>
    public CommandBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    /// <summary>
    /// Sets the handler type.
    /// </summary>
    /// <param name="handlerType">The handler type.</param>
    /// <returns>This builder instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="handlerType"/> is null, empty, or whitespace.</exception>
    public CommandBuilder WithHandlerType(string handlerType)
    {
        ArgumentException.ThrowIfNullOrEmpty(handlerType);
        _handlerType = handlerType;
        return this;
    }

    /// <summary>
    /// Sets the command type.
    /// </summary>
    /// <param name="type">The command type.</param>
    /// <returns>This builder instance.</returns>
    public CommandBuilder WithType(CommandType type)
    {
        _type = type;
        return this;
    }

    /// <summary>
    /// Sets whether the command requires administrator privileges.
    /// </summary>
    /// <param name="requiresAdmin">True if administrator privileges are required.</param>
    /// <returns>This builder instance.</returns>
    public CommandBuilder WithRequiresAdmin(bool requiresAdmin)
    {
        _requiresAdmin = requiresAdmin;
        return this;
    }

    /// <summary>
    /// Sets whether the command is enabled.
    /// </summary>
    /// <param name="isEnabled">True if the command is enabled.</param>
    /// <returns>This builder instance.</returns>
    public CommandBuilder WithIsEnabled(bool isEnabled)
    {
        _isEnabled = isEnabled;
        return this;
    }

    /// <summary>
    /// Sets the execution count.
    /// </summary>
    /// <param name="executionCount">The execution count.</param>
    /// <returns>This builder instance.</returns>
    public CommandBuilder WithExecutionCount(int executionCount)
    {
        _executionCount = executionCount;
        return this;
    }

    /// <summary>
    /// Sets the command parameters.
    /// </summary>
    /// <param name="parameters">The command parameters.</param>
    /// <returns>This builder instance.</returns>
    public CommandBuilder WithParameters(List<CommandParameter>? parameters)
    {
        _parameters = parameters;
        return this;
    }

    /// <summary>
    /// Sets the command aliases.
    /// </summary>
    /// <param name="aliases">The command aliases.</param>
    /// <returns>This builder instance.</returns>
    public CommandBuilder WithAliases(List<string> aliases)
    {
        _aliases = aliases;
        return this;
    }

    /// <summary>
    /// Sets the creation timestamp.
    /// </summary>
    /// <param name="createdAt">The creation timestamp.</param>
    /// <returns>This builder instance.</returns>
    public CommandBuilder WithCreatedAt(DateTime createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    /// <summary>
    /// Creates a new <see cref="CommandBuilder"/> pre-filled with values from an existing command.
    /// </summary>
    /// <param name="template">The command to copy values from.</param>
    /// <returns>A new builder instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="template"/> is null.</exception>
    public static CommandBuilder From(Command template)
    {
        ArgumentNullException.ThrowIfNull(template);
        return new CommandBuilder
        {
            _name = template.Name,
            _description = template.Description,
            _handlerType = template.HandlerType,
            _type = template.Type,
            _requiresAdmin = template.RequiresAdmin,
            _isEnabled = template.IsEnabled,
            _executionCount = template.ExecutionCount,
            _parameters = template.Parameters,
            _aliases = template.Aliases,
            _createdAt = template.CreatedAt
        };
    }

    /// <summary>
    /// Builds the <see cref="Command"/> instance with the current configuration.
    /// </summary>
    /// <returns>A configured <see cref="Command"/> instance.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <see cref="Name"/> is null, empty, or whitespace;
    /// or when <see cref="HandlerType"/> is null, empty, or whitespace;
    /// or when <see cref="Type"/> is <see cref="CommandType.Standard"/> and <see cref="Name"/> does not start with '/'.
    /// </exception>
    public Command Build()
    {
        if (string.IsNullOrWhiteSpace(_name))
            throw new ArgumentException("Command name is required", nameof(Command.Name));

        if (string.IsNullOrWhiteSpace(_handlerType))
            throw new ArgumentException("HandlerType is required", nameof(Command.HandlerType));

        if (_type == CommandType.Standard && !_name.StartsWith("/"))
            throw new ArgumentException("Standard commands must start with /", nameof(Command.Name));

        return new Command
        {
            Name = _name,
            Description = _description,
            HandlerType = _handlerType,
            Type = _type,
            RequiresAdmin = _requiresAdmin,
            IsEnabled = _isEnabled,
            ExecutionCount = _executionCount,
            Parameters = _parameters,
            Aliases = _aliases,
            CreatedAt = _createdAt
        };
    }
}