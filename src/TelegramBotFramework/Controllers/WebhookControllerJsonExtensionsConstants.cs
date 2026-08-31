namespace TelegramBotFramework.Controllers;

internal static class WebhookControllerJsonExtensionsConstants
{
	/// <summary>
	/// Error message for failed JSON deserialization to WebhookController.
	/// </summary>
	public const string FailedToDeserializeWebhookControllerMessage = "Failed to deserialize JSON to WebhookController";

	/// <summary>
	/// Default JSON serializer defaults.
	/// </summary>
	public static readonly System.Text.Json.JsonSerializerDefaults JsonSerializerDefaults = System.Text.Json.JsonSerializerDefaults.Web;

	/// <summary>
	/// Property naming policy for JSON serialization.
	/// </summary>
	public static readonly System.Text.Json.JsonNamingPolicy PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;

	/// <summary>
	/// Whether to write indented JSON by default.
	/// </summary>
	public const bool DefaultWriteIndented = false;
}
