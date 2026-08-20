// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using TelegramBotFramework.ConversationFlow;

namespace TelegramBotFramework.ConversationFlow.Builders;

/// <summary>
/// A builder for <see cref="FlowDefinition"/> instances.
/// </summary>
public sealed class FlowDefinitionBuilder
{
    private string? _flowId;
    private string? _name;
    private string? _description;
    private string? _initialStepId;
    private List<FlowStep>? _steps;
    private TimeSpan? _timeout;
    private bool _allowResume = true;
    private string? _completionMenuId;
    private Dictionary<string, string> _metadata = new();

    /// <summary>
    /// Initializes a new instance of <see cref="FlowDefinitionBuilder"/>.
    /// </summary>
    public FlowDefinitionBuilder()
    {
    }

    /// <summary>
    /// Creates a new <see cref="FlowDefinitionBuilder"/> pre-filled from an existing <see cref="FlowDefinition"/>.
    /// </summary>
    /// <param name="template">The template to pre-fill from.</param>
    /// <returns>A new <see cref="FlowDefinitionBuilder"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when template is null.</exception>
    public static FlowDefinitionBuilder From(FlowDefinition template)
    {
        ArgumentNullException.ThrowIfNull(template);

        return new FlowDefinitionBuilder
        {
            _flowId = template.FlowId,
            _name = template.Name,
            _description = template.Description,
            _initialStepId = template.InitialStepId,
            _steps = new List<FlowStep>(template.Steps),
            _timeout = template.Timeout,
            _allowResume = template.AllowResume,
            _completionMenuId = template.CompletionMenuId,
            _metadata = new Dictionary<string, string>(template.Metadata)
        };
    }

    /// <summary>Sets the flow ID.</summary>
    /// <param name="flowId">The flow ID.</param>
    /// <returns>The builder instance.</returns>
    /// <exception cref="ArgumentException">Thrown when flowId is null or empty.</exception>
    public FlowDefinitionBuilder WithFlowId(string flowId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(flowId);
        _flowId = flowId;
        return this;
    }

    /// <summary>Sets the flow name.</summary>
    /// <param name="name">The flow name.</param>
    /// <returns>The builder instance.</returns>
    /// <exception cref="ArgumentException">Thrown when name is null or empty.</exception>
    public FlowDefinitionBuilder WithName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        _name = name;
        return this;
    }

    /// <summary>Sets the flow description.</summary>
    /// <param name="description">The description.</param>
    /// <returns>The builder instance.</returns>
    public FlowDefinitionBuilder WithDescription(string? description)
    {
        _description = description;
        return this;
    }

    /// <summary>Sets the initial step ID.</summary>
    /// <param name="initialStepId">The initial step ID.</param>
    /// <returns>The builder instance.</returns>
    /// <exception cref="ArgumentException">Thrown when initialStepId is null or empty.</exception>
    public FlowDefinitionBuilder WithInitialStepId(string initialStepId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(initialStepId);
        _initialStepId = initialStepId;
        return this;
    }

    /// <summary>Sets the flow steps.</summary>
    /// <param name="steps">The flow steps.</param>
    /// <returns>The builder instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when steps is null.</exception>
    public FlowDefinitionBuilder WithSteps(List<FlowStep> steps)
    {
        ArgumentNullException.ThrowIfNull(steps);
        _steps = steps;
        return this;
    }

    /// <summary>Sets the flow timeout.</summary>
    /// <param name="timeout">The timeout.</param>
    /// <returns>The builder instance.</returns>
    public FlowDefinitionBuilder WithTimeout(TimeSpan? timeout)
    {
        _timeout = timeout;
        return this;
    }

    /// <summary>Sets whether to allow resume.</summary>
    /// <param name="allowResume">Whether to allow resume.</param>
    /// <returns>The builder instance.</returns>
    public FlowDefinitionBuilder WithAllowResume(bool allowResume)
    {
        _allowResume = allowResume;
        return this;
    }

    /// <summary>Sets the completion menu ID.</summary>
    /// <param name="completionMenuId">The completion menu ID.</param>
    /// <returns>The builder instance.</returns>
    public FlowDefinitionBuilder WithCompletionMenuId(string? completionMenuId)
    {
        _completionMenuId = completionMenuId;
        return this;
    }

    /// <summary>Sets the metadata.</summary>
    /// <param name="metadata">The metadata.</param>
    /// <returns>The builder instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when metadata is null.</exception>
    public FlowDefinitionBuilder WithMetadata(Dictionary<string, string> metadata)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        _metadata = metadata;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="FlowDefinition"/> instance.
    /// </summary>
    /// <returns>The configured <see cref="FlowDefinition"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when required properties are missing.</exception>
    public FlowDefinition Build()
    {
        if (string.IsNullOrWhiteSpace(_flowId))
            throw new ArgumentException("FlowId is required.", nameof(_flowId));
        if (string.IsNullOrWhiteSpace(_name))
            throw new ArgumentException("Name is required.", nameof(_name));
        if (string.IsNullOrWhiteSpace(_initialStepId))
            throw new ArgumentException("InitialStepId is required.", nameof(_initialStepId));
        if (_steps == null || _steps.Count == 0)
            throw new ArgumentException("Steps are required and must not be empty.", nameof(_steps));

        return new FlowDefinition
        {
            FlowId = _flowId,
            Name = _name,
            Description = _description,
            InitialStepId = _initialStepId,
            Steps = _steps.AsReadOnly(),
            Timeout = _timeout,
            AllowResume = _allowResume,
            CompletionMenuId = _completionMenuId,
            Metadata = _metadata
        };
    }
}
