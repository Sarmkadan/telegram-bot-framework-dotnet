#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TelegramBotFramework.ConversationFlow.Middleware;

namespace TelegramBotFramework.ConversationFlow;

/// <summary>
/// Extension methods for registering conversation flow services in the dependency-injection
/// container and for building <see cref="FlowDefinition"/> instances using a fluent API.
/// </summary>
public static class ConversationFlowExtensions
{
    // -------------------------------------------------------------------------
    // DI Registration
    // -------------------------------------------------------------------------

    /// <summary>
    /// Adds the conversation flow engine and its dependencies to the service collection.
    /// Call this after <c>AddTelegramBotFramework</c> in your startup configuration.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to configure.</param>
    /// <param name="configure">
    /// An optional delegate used to configure <see cref="ConversationFlowOptions"/>.
    /// When omitted, default option values are used.
    /// </param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="services"/> is <see langword="null"/>.</exception>
    /// <example>
    /// <code>
    /// services
    ///   .AddTelegramBotFramework(config)
    ///   .AddConversationFlows(opts =>
    ///   {
    ///     opts.DefaultFlowTimeout = TimeSpan.FromMinutes(15);
    ///     opts.EnableFlowEvents = true;
    ///     opts.AbortKeyword = "/stop";
    ///   });
    /// </code>
    /// </example>
    public static IServiceCollection AddConversationFlows(
        this IServiceCollection services,
        Action<ConversationFlowOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = new ConversationFlowOptions();
        configure?.Invoke(options);

        services.AddSingleton(options);
        services.AddSingleton<IConversationStateStore, InMemoryConversationStateStore>();
        services.AddSingleton<IConversationFlowEngine, ConversationFlowEngine>();
        services.AddSingleton<ConversationFlowMiddleware>();

        return services;
    }

    /// <summary>
    /// Adds the conversation flow engine with a file-based state store that persists
    /// flow states to <paramref name="stateDirectory"/> as JSON files.
    /// Flow states survive process restarts and can be resumed on the next startup.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to configure.</param>
    /// <param name="stateDirectory">Directory path where <c>{userId}.json</c> state files are stored.</param>
    /// <param name="configure">Optional delegate to configure <see cref="ConversationFlowOptions"/>.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="services"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">
    /// Thrown if <paramref name="stateDirectory"/> is <see langword="null"/>, empty, or consists only of whitespace.
    /// </exception>
    public static IServiceCollection AddConversationFlowsWithFileStore(
        this IServiceCollection services,
        string stateDirectory,
        Action<ConversationFlowOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        if (string.IsNullOrWhiteSpace(stateDirectory))
            throw new ArgumentException(ConversationFlowExtensionsConstants.StateDirectoryCannotBeEmpty, nameof(stateDirectory));

        var options = new ConversationFlowOptions();
        configure?.Invoke(options);

        services.AddSingleton(options);
        services.AddSingleton<IConversationStateStore>(sp =>
            new FileConversationStateStore(
                stateDirectory,
                sp.GetService<ILogger<FileConversationStateStore>>()));
        services.AddSingleton<IConversationFlowEngine, ConversationFlowEngine>();
        services.AddSingleton<ConversationFlowMiddleware>();

        return services;
    }

    // -------------------------------------------------------------------------
    // Fluent Builder Factory
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a new <see cref="IFlowDefinitionBuilder"/> for constructing a
    /// <see cref="FlowDefinition"/> using a fluent API.
    /// </summary>
    /// <param name="flowId">
    /// The unique identifier for the flow. Must match the value passed to
    /// <see cref="IConversationFlowEngine.RegisterFlowAsync"/>.
    /// </param>
    /// <param name="name">The human-readable display name of the flow.</param>
    /// <returns>A new <see cref="IFlowDefinitionBuilder"/> instance.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="flowId"/> or <paramref name="name"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown if <paramref name="flowId"/> or <paramref name="name"/> is <see langword="null"/>, empty, or consists only of whitespace.
    /// </exception>
    /// <example>
    /// <code>
    /// var flow = ConversationFlowExtensions
    ///     .CreateFlow("onboarding", "User Onboarding")
    ///     .WithDescription("Collects name and contact details during first use.")
    ///     .WithTimeout(TimeSpan.FromMinutes(10))
    ///     .AddStep(new FlowStep
    ///     {
    ///         StepId = "ask_name",
    ///         Prompt = "Welcome! What is your name?",
    ///         InputType = FlowInputType.Text,
    ///         VariableName = "name",
    ///         DefaultNextStepId = "ask_email"
    ///     })
    ///     .AddStep(new FlowStep
    ///     {
    ///         StepId = "ask_email",
    ///         Prompt = "Great! What is your email address?",
    ///         InputType = FlowInputType.Email,
    ///         VariableName = "email",
    ///         IsTerminal = true
    ///     })
    ///     .Build();
    ///
    /// await engine.RegisterFlowAsync(flow);
    /// </code>
    /// </example>
    public static IFlowDefinitionBuilder CreateFlow(string flowId, string name)
    {
        ArgumentNullException.ThrowIfNull(flowId);
        ArgumentNullException.ThrowIfNull(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(flowId);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new FlowDefinitionBuilder(flowId, name);
    }
}

// ---------------------------------------------------------------------------
// Builder Implementation
// ---------------------------------------------------------------------------

/// <summary>
/// Default implementation of <see cref="IFlowDefinitionBuilder"/> returned by
/// <see cref="ConversationFlowExtensions.CreateFlow"/>.
/// </summary>
internal sealed class FlowDefinitionBuilder : IFlowDefinitionBuilder
{
    private readonly string _flowId;
    private readonly string _name;
    private string? _description;
    private TimeSpan? _timeout;
    private string? _completionMenuId;
    private bool _allowResume = true;
    private readonly List<FlowStep> _steps = [];
    private readonly Dictionary<string, string> _metadata = new();

    internal FlowDefinitionBuilder(string flowId, string name)
    {
        if (string.IsNullOrWhiteSpace(flowId))
            throw new ArgumentException(ConversationFlowExtensionsConstants.FlowIdMustNotBeEmpty, nameof(flowId));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(ConversationFlowExtensionsConstants.NameMustNotBeEmpty, nameof(name));

        _flowId = flowId;
        _name = name;
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="description"/> is <see langword="null"/>.</exception>
    public IFlowDefinitionBuilder WithDescription(string description)
    {
        ArgumentNullException.ThrowIfNull(description);
        _description = description;
        return this;
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="timeout"/> is not positive.</exception>
    public IFlowDefinitionBuilder WithTimeout(TimeSpan timeout)
    {
        if (timeout <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(timeout), ConversationFlowExtensionsConstants.TimeoutMustBePositive);

        _timeout = timeout;
        return this;
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentException">
    /// Thrown if <paramref name="menuId"/> is <see langword="null"/>, empty, or consists only of whitespace.
    /// </exception>
    public IFlowDefinitionBuilder OnCompletionNavigateTo(string menuId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(menuId);
        _completionMenuId = menuId;
        return this;
    }

    /// <inheritdoc/>
    public IFlowDefinitionBuilder AllowResume(bool allow = true)
    {
        _allowResume = allow;
        return this;
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="step"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">
    /// Thrown if <paramref name="step"/>.StepId is <see langword="null"/>, empty, or consists only of whitespace.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown if a step with the same StepId has already been added to this flow.
    /// </exception>
    public IFlowDefinitionBuilder AddStep(FlowStep step)
    {
        ArgumentNullException.ThrowIfNull(step);

        if (string.IsNullOrWhiteSpace(step.StepId))
            throw new ArgumentException(ConversationFlowExtensionsConstants.StepIdMustNotBeEmpty, nameof(step));

        if (_steps.Any(s => s.StepId == step.StepId))
            throw new InvalidOperationException(
                string.Format(ConversationFlowExtensionsConstants.DuplicateStepIdFormat, step.StepId));

        _steps.Add(step);
        return this;
    }

    /// <inheritdoc/>
    public IFlowDefinitionBuilder WithMetadata(string key, string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        _metadata[key] = value;
        return this;
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the flow has no steps.
    /// Thrown if any step references a non-existent step ID in transitions.
    /// </exception>
    public FlowDefinition Build()
    {
        if (_steps.Count == 0)
            throw new InvalidOperationException(
                string.Format(ConversationFlowExtensionsConstants.FlowMustHaveAtLeastOneStepFormat, _flowId));

        var initialStepId = _steps[0].StepId;

        // Validate all transition targets reference existing steps
        var stepIds = _steps.Select(s => s.StepId).ToHashSet(ConversationFlowExtensionsConstants.StepIdComparer);
        foreach (var step in _steps)
        {
            foreach (var transition in step.Transitions)
            {
                if (!stepIds.Contains(transition.TargetStepId))
                    throw new InvalidOperationException(
                        string.Format(ConversationFlowExtensionsConstants.TransitionTargetDoesNotExistFormat, step.StepId, transition.TargetStepId, _flowId));
            }

            if (step.DefaultNextStepId is not null && !stepIds.Contains(step.DefaultNextStepId))
                throw new InvalidOperationException(
                    string.Format(ConversationFlowExtensionsConstants.DefaultNextStepIdDoesNotExistFormat, step.StepId, step.DefaultNextStepId, _flowId));
        }

        return new FlowDefinition
        {
            FlowId = _flowId,
            Name = _name,
            Description = _description,
            InitialStepId = initialStepId,
            Steps = _steps.AsReadOnly(),
            Timeout = _timeout,
            AllowResume = _allowResume,
            CompletionMenuId = _completionMenuId,
            Metadata = new Dictionary<string, string>(_metadata)
        };
    }
}