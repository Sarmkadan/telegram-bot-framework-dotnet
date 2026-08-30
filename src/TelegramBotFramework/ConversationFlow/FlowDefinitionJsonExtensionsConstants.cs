#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace TelegramBotFramework.ConversationFlow;

/// <summary>
/// Constants used throughout the FlowDefinitionJsonExtensions and related types.
/// </summary>
internal static class FlowDefinitionJsonExtensionsConstants
{
    /// <summary>
    /// The default <see cref="JsonSerializerOptions"/> configured for camelCase
    /// property naming and handling of the conversation-flow model types.
    /// </summary>
    public static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
        WriteIndented = false
    };
}