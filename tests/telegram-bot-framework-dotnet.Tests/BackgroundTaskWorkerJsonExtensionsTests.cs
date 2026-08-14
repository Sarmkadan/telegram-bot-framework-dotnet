using System;
using System.Text.Json;
using TelegramBotFramework.BackgroundWorkers;
using Xunit;

namespace TelegramBotFramework.Tests;

public class BackgroundTaskWorkerJsonExtensionsTests
{
    // Helper to create a minimal BackgroundTaskWorker instance.
    // The real class may have many properties; we only need a
    // parameterless constructor (or the default values) for the
    // serialization tests. If the class does not expose a public
    // parameterless constructor, the test will fail – in that case
    // the production code should be adjusted accordingly.
    private static BackgroundTaskWorker CreateSampleWorker()
    {
        // The class is assumed to have a public parameterless constructor.
        // If it has required properties, they can be set here.
        return new BackgroundTaskWorker();
    }

    [Fact]
    public void ToJson_NullArgument_ThrowsArgumentNullException()
    {
        BackgroundTaskWorker? nullWorker = null;
        Assert.Throws<ArgumentNullException>(() => nullWorker!.ToJson());
    }

    [Fact]
    public void ToJson_ValidWorker_ReturnsNonEmptyJson()
    {
        var worker = CreateSampleWorker();
        var json = worker.ToJson();

        Assert.False(string.IsNullOrWhiteSpace(json));
        // The JSON should contain at least one opening brace.
        Assert.Contains("{", json);
    }

    [Fact]
    public void ToJson_WithIndentation_ProducesIndentedJson()
    {
        var worker = CreateSampleWorker();
        var json = worker.ToJson(indented: true);

        // When indented, the JSON should contain line‑break characters.
        Assert.Contains(Environment.NewLine, json);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void FromJson_NullOrWhiteSpace_ReturnsNull(string? input)
    {
        var result = BackgroundTaskWorkerJsonExtensions.FromJson(input);
        Assert.Null(result);
    }

    [Fact]
    public void FromJson_ValidJson_ReturnsDeserializedObject()
    {
        var original = CreateSampleWorker();
        var json = original.ToJson();

        var deserialized = BackgroundTaskWorkerJsonExtensions.FromJson(json);
        Assert.NotNull(deserialized);
        // Simple round‑trip equality check – if the type overrides Equals,
        // this will verify full fidelity; otherwise we compare JSON.
        if (original.Equals(deserialized))
        {
            Assert.Equal(original, deserialized);
        }
        else
        {
            var reJson = deserialized!.ToJson();
            Assert.Equal(json, reJson);
        }
    }

    [Fact]
    public void TryFromJson_NullArgument_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => BackgroundTaskWorkerJsonExtensions.TryFromJson(null!, out _));
    }

    [Fact]
    public void TryFromJson_InvalidJson_ReturnsFalse()
    {
        var invalidJson = "{ this is not valid json }";
        var success = BackgroundTaskWorkerJsonExtensions.TryFromJson(invalidJson, out var value);
        Assert.False(success);
        Assert.Null(value);
    }

    [Fact]
    public void TryFromJson_ValidJson_ReturnsTrueAndDeserializedObject()
    {
        var original = CreateSampleWorker();
        var json = original.ToJson();

        var success = BackgroundTaskWorkerJsonExtensions.TryFromJson(json, out var deserialized);
        Assert.True(success);
        Assert.NotNull(deserialized);
        // Verify round‑trip consistency as in the FromJson test.
        if (original.Equals(deserialized))
        {
            Assert.Equal(original, deserialized);
        }
        else
        {
            var reJson = deserialized!.ToJson();
            Assert.Equal(json, reJson);
        }
    }
}
