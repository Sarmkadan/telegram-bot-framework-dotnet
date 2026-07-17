#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace TelegramBotFramework.Controllers;

/// <summary>
/// Provides System.Text.Json serialization extensions for <see cref="WebhookController"/>.
/// </summary>
public static class WebhookControllerJsonExtensions
{
	private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		WriteIndented = false,
		TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
	};

	/// <summary>
	/// Serializes the <see cref="WebhookController"/> instance to a JSON string.
	/// </summary>
	/// <param name="value">The controller instance to serialize.</param>
	/// <param name="indented">Whether to format the JSON with indentation for readability.</param>
	/// <returns>A JSON string representation of the controller.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
	public static string ToJson(this WebhookController value, bool indented = false)
		=> JsonSerializer.Serialize(value, indented
			? new JsonSerializerOptions(_jsonSerializerOptions) { WriteIndented = true }
			: _jsonSerializerOptions);

	/// <summary>
	/// Deserializes a JSON string to a <see cref="WebhookController"/> instance.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <returns>The deserialized <see cref="WebhookController"/> instance, or <see langword="null"/> if the JSON is invalid.</returns>
	/// <exception cref="ArgumentException"><paramref name="json"/> is <see langword="null"/>, empty, or consists only of whitespace.</exception>
	/// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
	public static WebhookController? FromJson(string json)
	{
		ArgumentException.ThrowIfNullOrEmpty(json);

		try
		{
			return JsonSerializer.Deserialize<WebhookController>(json, _jsonSerializerOptions);
		}
		catch (JsonException ex)
		{
			throw new JsonException("Failed to deserialize JSON to WebhookController", ex);
		}
	}

	/// <summary>
	/// Attempts to deserialize a JSON string to a <see cref="WebhookController"/> instance.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <param name="value">Receives the deserialized <see cref="WebhookController"/> instance if successful; otherwise, <see langword="null"/>.</param>
	/// <returns><see langword="true"/> if the JSON was successfully deserialized; otherwise, <see langword="false"/>.</returns>
	/// <exception cref="ArgumentException"><paramref name="json"/> is <see langword="null"/>.</exception>
	public static bool TryFromJson(string json, out WebhookController? value)
	{
		ArgumentNullException.ThrowIfNull(json);

		if (string.IsNullOrWhiteSpace(json))
		{
			value = null;
			return true;
		}

		try
		{
			value = JsonSerializer.Deserialize<WebhookController>(json, _jsonSerializerOptions);
			return value is not null;
		}
		catch (JsonException)
		{
			value = null;
			return false;
		}
	}
}
