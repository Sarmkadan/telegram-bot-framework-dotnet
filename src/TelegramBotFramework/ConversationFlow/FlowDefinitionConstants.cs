#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.ConversationFlow;

/// <summary>
/// Constants used throughout the FlowDefinition and related types.
/// </summary>
internal static class FlowDefinitionConstants
{
    /// <summary>
    /// The default value for the <see cref="FlowDefinition.AllowResume"/> property.
    /// </summary>
    public const bool DefaultAllowResume = true;

    /// <summary>
    /// The default value for the <see cref="UserFlowState.Status"/> property.
    /// </summary>
    public const FlowStateStatus DefaultFlowStateStatus = FlowStateStatus.Active;
}