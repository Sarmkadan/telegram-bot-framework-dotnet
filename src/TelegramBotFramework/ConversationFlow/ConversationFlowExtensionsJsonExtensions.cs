#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Text.Json;

namespace TelegramBotFramework.ConversationFlow;

/// <summary>
/// Provides System.Text.Json serialization extensions for conversation flow types.
/// <para>This class delegates to <see cref="FlowDefinitionJsonExtensions"/> for actual implementation.</para>
/// </summary>
public static class ConversationFlowExtensionsJsonExtensions
{
    /// <summary>
    /// Serializes the provided <see cref="FlowDefinition"/> to a JSON string using camelCase property naming.
    /// </summary>
    /// <param name="value">The flow definition to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the flow definition.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this FlowDefinition value, bool indented = false)
        => FlowDefinitionJsonExtensions.ToJson(value, indented);

    /// <summary>
    /// Deserializes a JSON string into a <see cref="FlowDefinition"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized <see cref="FlowDefinition"/> instance, or null if the JSON is empty or whitespace.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
    public static FlowDefinition? FromJson(string json)
        => FlowDefinitionJsonExtensions.FromJson(json);

    /// <summary>
    /// Attempts to deserialize a JSON string into a <see cref="FlowDefinition"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized instance if successful; otherwise, null.</param>
    /// <returns>True if deserialization succeeds; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    public static bool TryFromJson(string json, out FlowDefinition? value)
        => FlowDefinitionJsonExtensions.TryFromJson(json, out value);
}