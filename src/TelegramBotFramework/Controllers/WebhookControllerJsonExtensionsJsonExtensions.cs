#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace TelegramBotFramework.Controllers;

/// <summary>
/// Provides System.Text.Json serialization extensions for <see cref="WebhookControllerJsonExtensions"/>.
/// </summary>
public static class WebhookControllerJsonExtensionsJsonExtensions
{
	private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		WriteIndented = false,
		TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
	};
}