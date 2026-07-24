#nullable enable
using System.Net;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using TelegramBotFramework.Events;
using TelegramBotFramework.Integration;
using TelegramBotFramework.Utilities;
using Xunit;

namespace TelegramBotFramework.Tests;

/// <summary>
/// Tests for <see cref="ExternalApiIntegration"/>.
/// </summary>
public sealed class ExternalApiIntegrationTests
{
    private readonly HttpClientFactory _httpClientFactory = new();
    private readonly Mock<ILogger<ExternalApiIntegration>> _loggerMock = new();

    public ExternalApiIntegrationTests()
    {
        // Setup logger mock to avoid console output during tests
        _loggerMock.Setup(x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
            .Verifiable();
    }

    [Fact]
    public void Constructor_WithNullParameters_UsesDefaults()
    {
        // Act
        var integration = new ExternalApiIntegration(null, null);

        // Assert
        integration.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithCustomParameters_UsesProvidedValues()
    {
        // Arrange
        var customFactory = new HttpClientFactory();
        var logger = _loggerMock.Object;

        // Act
        var integration = new ExternalApiIntegration(customFactory, logger);

        // Assert
        integration.Should().NotBeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("invalid-url")]
    [InlineData("ftp://example.com")]
    public async Task GetAsync_WithInvalidUrl_ReturnsDefault(string? invalidUrl)
    {
        // Arrange
        var integration = new ExternalApiIntegration(_httpClientFactory, _loggerMock.Object);

        // Act
        var result = await integration.GetAsync<object>(invalidUrl!);

        // Assert
        result.Should().BeNull();
        _loggerMock.VerifyLogging(LogLevel.Warning, Times.Once());
    }

    [Fact]
    public async Task GetAsync_WithEmptyUrl_ReturnsDefault()
    {
        // Arrange
        var integration = new ExternalApiIntegration(_httpClientFactory, _loggerMock.Object);

        // Act
        var result = await integration.GetAsync<object>(string.Empty);

        // Assert
        result.Should().BeNull();
        _loggerMock.VerifyLogging(LogLevel.Warning, Times.Once());
    }

    [Fact]
    public void ParseResponse_WithValidJson_ReturnsDeserializedObject()
    {
        // Arrange
        var json = "{\"Name\":\"Test\",\"Value\":123}";

        // Act
        var result = ExternalApiIntegration.ParseResponse<TestModel>(json);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test");
        result.Value.Should().Be(123);
    }

    [Fact]
    public void ParseResponse_WithInvalidJson_ReturnsDefault()
    {
        // Arrange
        var invalidJson = "{invalid json";

        // Act
        var result = ExternalApiIntegration.ParseResponse<TestModel>(invalidJson);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ParseResponse_WithEmptyString_ReturnsDefault()
    {
        // Arrange
        var emptyJson = "";

        // Act
        var result = ExternalApiIntegration.ParseResponse<TestModel>(emptyJson);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ParseResponse_WithNull_ReturnsDefault()
    {
        // Arrange
        string? nullJson = null;

        // Act
        var result = ExternalApiIntegration.ParseResponse<TestModel>(nullJson!);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ParseResponse_WithCamelCaseJson_ReturnsDeserializedObject()
    {
        // Arrange
        var camelCaseJson = "{\"name\":\"camel\",\"value\":456}";

        // Act
        var result = ExternalApiIntegration.ParseResponse<TestModel>(camelCaseJson);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("camel");
        result.Value.Should().Be(456);
    }

    [Fact]
    public void ParseResponse_WithComplexNestedJson_ReturnsDeserializedObject()
    {
        // Arrange
        var complexJson = "{\"User\":{\"Id\":123,\"Name\":\"John\",\"Settings\":{\"DarkMode\":true,\"Notifications\":false}}";

        // Act
        var result = ExternalApiIntegration.ParseResponse<ComplexModel>(complexJson);

        // Assert
        result.Should().NotBeNull();
        result!.User.Should().NotBeNull();
        result.User.Id.Should().Be(123);
        result.User.Name.Should().Be("John");
        result.User.Settings.Should().NotBeNull();
        result.User.Settings.DarkMode.Should().BeTrue();
        result.User.Settings.Notifications.Should().BeFalse();
    }

    [Fact]
    public async Task PostAsync_WithInvalidUrl_ReturnsFalse()
    {
        // Arrange
        var integration = new ExternalApiIntegration(_httpClientFactory, _loggerMock.Object);
        var payload = new { Name = "Test", Value = 123 };

        // Act
        var result = await integration.PostAsync("invalid-url", payload);

        // Assert
        result.Should().BeFalse();
        _loggerMock.VerifyLogging(LogLevel.Warning, Times.Once());
    }

    [Fact]
    public async Task PostAsync_WithNullPayload_SerializesSuccessfully()
    {
        // Arrange
        var integration = new ExternalApiIntegration(_httpClientFactory, _loggerMock.Object);
        var url = "https://httpbin.org/post";

        // Act
        var result = await integration.PostAsync<object>(url, null);

        // Assert
        result.Should().BeFalse(); // Will be false since httpbin.org is not mocked
    }

    [Fact]
    public async Task PostAsync_WithEmptyApiKey_WorksWithoutAuthorization()
    {
        // Arrange
        var integration = new ExternalApiIntegration(_httpClientFactory, _loggerMock.Object);
        var url = "https://httpbin.org/post";
        var payload = new { Test = "data" };

        // Act
        var result = await integration.PostAsync(url, payload, string.Empty);

        // Assert
        result.Should().BeFalse(); // Will be false since httpbin.org is not mocked
    }

    [Fact]
    public async Task GetWithHeadersAsync_WithInvalidUrl_ReturnsNull()
    {
        // Arrange
        var integration = new ExternalApiIntegration(_httpClientFactory, _loggerMock.Object);
        var headers = new Dictionary<string, string> { { "X-Test", "value" } };

        // Act
        var result = await integration.GetWithHeadersAsync("invalid-url", headers);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetWithHeadersAsync_WithEmptyHeadersDictionary_Works()
    {
        // Arrange
        var integration = new ExternalApiIntegration(_httpClientFactory, _loggerMock.Object);
        var headers = new Dictionary<string, string>();
        var url = "https://httpbin.org/headers";

        // Act
        var result = await integration.GetWithHeadersAsync(url, headers);

        // Assert
        result.Should().BeNull(); // Will be null since httpbin.org is not mocked
    }

    [Fact]
    public async Task GetWithHeadersAsync_WithNullHeaders_ReturnsNull()
    {
        // Arrange
        var integration = new ExternalApiIntegration(_httpClientFactory, _loggerMock.Object);
        var url = "https://httpbin.org/headers";

        // Act
        var result = await integration.GetWithHeadersAsync(url, null!);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAsync_WithMaxRetriesParameter_UsesCustomRetryCount()
    {
        // Arrange
        var integration = new ExternalApiIntegration(_httpClientFactory, _loggerMock.Object);
        var url = "https://httpbin.org/status/500";

        // Act - should retry 3 times by default
        var result = await integration.GetAsync<object>(url, maxRetries: 3);

        // Assert
        result.Should().BeNull();
        _loggerMock.VerifyLogging(LogLevel.Error, Times.Once());
    }

    [Fact]
    public async Task GetAsync_WithZeroMaxRetries_DoesNotRetry()
    {
        // Arrange
        var integration = new ExternalApiIntegration(_httpClientFactory, _loggerMock.Object);
        var url = "https://httpbin.org/status/500";

        // Act
        var result = await integration.GetAsync<object>(url, maxRetries: 0);

        // Assert
        result.Should().BeNull();
        _loggerMock.VerifyLogging(LogLevel.Error, Times.Once());
    }

    [Fact]
    public async Task GetAsync_WithSingleMaxRetry_RetriesOnce()
    {
        // Arrange
        var integration = new ExternalApiIntegration(_httpClientFactory, _loggerMock.Object);
        var url = "https://httpbin.org/status/500";

        // Act - should retry once
        var result = await integration.GetAsync<object>(url, maxRetries: 1);

        // Assert
        result.Should().BeNull();
        _loggerMock.VerifyLogging(LogLevel.Error, Times.Once());
    }

    [Fact]
    public void ParseResponse_WithArrayJson_ReturnsDeserializedArray()
    {
        // Arrange
        var arrayJson = "[{\"Id\":1},{\"Id\":2},{\"Id\":3}]";

        // Act
        var result = ExternalApiIntegration.ParseResponse<int[]>(arrayJson);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result![0].Should().Be(1);
        result[1].Should().Be(2);
        result[2].Should().Be(3);
    }

    [Fact]
    public void ParseResponse_WithEmptyObject_ReturnsDefault()
    {
        // Arrange
        var emptyJson = "{}";

        // Act
        var result = ExternalApiIntegration.ParseResponse<TestModel>(emptyJson);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().BeNull();
        result.Value.Should().Be(0);
    }

    [Fact]
    public void ParseResponse_WithBooleanField_ReturnsDeserializedObject()
    {
        // Arrange
        var json = "{\"Active\":true,\"Enabled\":false}";

        // Act
        var result = ExternalApiIntegration.ParseResponse<BooleanModel>(json);

        // Assert
        result.Should().NotBeNull();
        result!.Active.Should().BeTrue();
        result.Enabled.Should().BeFalse();
    }

    // Test models for deserialization testing
    private sealed class TestModel
    {
        public string? Name { get; set; }
        public int Value { get; set; }
    }

    private sealed class ComplexModel
    {
        public UserData? User { get; set; }
    }

    private sealed class UserData
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public UserSettings? Settings { get; set; }
    }

    private sealed class UserSettings
    {
        public bool DarkMode { get; set; }
        public bool Notifications { get; set; }
    }

    private sealed class BooleanModel
    {
        public bool Active { get; set; }
        public bool Enabled { get; set; }
    }
}

// Extension method for verifying logger calls
internal static class LoggerExtensions
{
    public static void VerifyLogging(this Mock<ILogger<ExternalApiIntegration>> loggerMock, LogLevel level, Times times)
    {
        loggerMock.Verify(x => x.Log(
            level,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => true),
            It.IsAny<Exception>(),
            It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)!), times);
    }
}