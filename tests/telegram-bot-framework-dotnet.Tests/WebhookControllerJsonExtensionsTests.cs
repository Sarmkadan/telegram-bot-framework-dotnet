using System;
using System.Text.Json;
using System.Runtime.Serialization;
using TelegramBotFramework.Controllers;
using Xunit;

namespace TelegramBotFramework.Tests;

public class WebhookControllerJsonExtensionsTests
{
	/// <summary>
	/// Creates an instance of <see cref="WebhookController"/> without invoking its constructor.
	/// This works even if the real controller only has parameterised constructors.
	/// </summary>
	private static WebhookController CreateControllerInstance()
	{
		// The controller type is public, so we can use FormatterServices to get an uninitialized object.
		return (WebhookController)FormatterServices.GetUninitializedObject(typeof(WebhookController));
	}

	[Fact]
	public void ToJson_WithValidController_ReturnsNonEmptyJson()
	{
		// Arrange
		var controller = CreateControllerInstance();

		// Act
		string json = controller.ToJson();

		// Assert
		Assert.False(string.IsNullOrWhiteSpace(json));
		// Verify that the result is valid JSON.
		using var doc = JsonDocument.Parse(json);
		Assert.NotNull(doc.RootElement);
	}

	[Fact]
	public void ToJson_WithIndentation_ReturnsIndentedJson()
	{
		var controller = CreateControllerInstance();

		string json = controller.ToJson(indented: true);

		Assert.Contains("\n", json); // indented JSON contains line breaks
	}

	[Fact]
	public void ToJson_NullController_ThrowsArgumentNullException()
	{
		WebhookController? controller = null;
		Assert.Throws<ArgumentNullException>(() => controller!.ToJson());
	}

	[Fact]
	public void FromJson_ValidJson_ReturnsController()
	{
		// Arrange
		var original = CreateControllerInstance();
		string json = original.ToJson();

		// Act
		var deserialized = WebhookControllerJsonExtensions.FromJson(json);

		// Assert
		Assert.NotNull(deserialized);
	}

	[Fact]
	public void FromJson_NullOrEmptyJson_ThrowsArgumentException()
	{
		Assert.Throws<ArgumentException>(() => WebhookControllerJsonExtensions.FromJson(null!));
		Assert.Throws<ArgumentException>(() => WebhookControllerJsonExtensions.FromJson(string.Empty));
		Assert.Throws<ArgumentException>(() => WebhookControllerJsonExtensions.FromJson("   "));
	}

	[Fact]
	public void FromJson_InvalidJson_ThrowsJsonException()
	{
		const string invalidJson = "{ this is not valid json }";

		var ex = Assert.Throws<JsonException>(() => WebhookControllerJsonExtensions.FromJson(invalidJson));
		Assert.Contains("Failed to deserialize JSON to WebhookController", ex.Message);
	}

	[Fact]
	public void TryFromJson_ValidJson_ReturnsTrueAndValue()
	{
		var original = CreateControllerInstance();
		string json = original.ToJson();

		bool result = WebhookControllerJsonExtensions.TryFromJson(json, out var value);

		Assert.True(result);
		Assert.NotNull(value);
	}

	[Fact]
	public void TryFromJson_NullJson_ThrowsArgumentNullException()
	{
		Assert.Throws<ArgumentNullException>(() => WebhookControllerJsonExtensions.TryFromJson(null!, out _));
	}

	[Fact]
	public void TryFromJson_EmptyOrWhiteSpaceJson_ReturnsTrueAndNull()
	{
		bool resultEmpty = WebhookControllerJsonExtensions.TryFromJson(string.Empty, out var valueEmpty);
		bool resultWhiteSpace = WebhookControllerJsonExtensions.TryFromJson("   ", out var valueWhiteSpace);

		Assert.True(resultEmpty);
		Assert.Null(valueEmpty);

		Assert.True(resultWhiteSpace);
		Assert.Null(valueWhiteSpace);
	}

	[Fact]
	public void TryFromJson_InvalidJson_ReturnsFalseAndNull()
	{
		const string invalidJson = "{ bad json }";

		bool result = WebhookControllerJsonExtensions.TryFromJson(invalidJson, out var value);

		Assert.False(result);
		Assert.Null(value);
	}
}
