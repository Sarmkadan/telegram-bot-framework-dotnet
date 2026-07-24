#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using FluentAssertions;
using TelegramBotFramework.Integration;
using Xunit;

namespace TelegramBotFramework.Tests;

public class HttpClientFactoryTests
{
    private readonly HttpClientFactory _factory = new();

    [Fact]
    public void GetClient_WithValidBaseUrlAndTimeout_ReturnsConfiguredClient()
    {
        // Arrange
        var baseUrl = "https://example.com";
        var timeout = TimeSpan.FromSeconds(10);

        // Act
        var client = _factory.GetClient(baseUrl, timeout);

        // Assert
        client.BaseAddress.Should().Be(new Uri(baseUrl));
        client.Timeout.Should().Be(timeout);
        client.DefaultRequestHeaders.Contains("User-Agent").Should().BeTrue();
        client.DefaultRequestHeaders.GetValues("User-Agent").Single().Should().Be("TelegramBotFramework/1.0");
        client.DefaultRequestHeaders.Contains("Accept").Should().BeTrue();
        client.DefaultRequestHeaders.GetValues("Accept").Single().Should().Be("application/json");
    }

    [Fact]
    public void GetTelegramClient_ReturnsClientWithTelegramBaseUrlAndDefaultTimeout()
    {
        // Act
        var client = _factory.GetTelegramClient();

        // Assert
        client.BaseAddress.Should().Be(new Uri("https://api.telegram.org"));
        client.Timeout.Should().Be(TimeSpan.FromSeconds(45));
    }

    [Fact]
    public void GetClientWithHeaders_WithValidHeaders_SetsHeadersOnClient()
    {
        // Arrange
        var baseUrl = "https://example.com";
        var headers = new Dictionary<string, string>
        {
            ["X-Custom-Header"] = "CustomValue",
            ["Another-Header"] = "123"
        };

        // Act
        var client = _factory.GetClientWithHeaders(baseUrl, headers);

        // Assert
        client.DefaultRequestHeaders.Contains("X-Custom-Header").Should().BeTrue();
        client.DefaultRequestHeaders.GetValues("X-Custom-Header").Single().Should().Be("CustomValue");
        client.DefaultRequestHeaders.Contains("Another-Header").Should().BeTrue();
        client.DefaultRequestHeaders.GetValues("Another-Header").Single().Should().Be("123");
    }

    [Fact]
    public void GetClientWithHeaders_NullHeaders_ThrowsArgumentNullException()
    {
        // Arrange
        var baseUrl = "https://example.com";

        // Act
        Action act = () => _factory.GetClientWithHeaders(baseUrl, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetClientWithAuth_WithValidToken_SetsAuthorizationHeader()
    {
        // Arrange
        var baseUrl = "https://example.com";
        var token = "my-secret-token";

        // Act
        var client = _factory.GetClientWithAuth(baseUrl, token);

        // Assert
        client.DefaultRequestHeaders.Authorization.Should().NotBeNull();
        client.DefaultRequestHeaders.Authorization!.Scheme.Should().Be("Bearer");
        client.DefaultRequestHeaders.Authorization.Parameter.Should().Be(token);
    }

    [Fact]
    public void GetClientWithAuth_EmptyToken_ThrowsArgumentException()
    {
        // Arrange
        var baseUrl = "https://example.com";

        // Act
        Action act = () => _factory.GetClientWithAuth(baseUrl, "");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("authToken");
    }

    [Fact]
    public void GetClient_WithEmptyBaseUrl_ThrowsArgumentException()
    {
        // Act
        Action act = () => _factory.GetClient(string.Empty, null);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("baseUrl");
    }

    [Fact]
    public void Dispose_ClearsCache_AndSubsequentCallsReturnNewInstances()
    {
        // Arrange
        var baseUrl = "https://example.com";
        var firstClient = _factory.GetClient(baseUrl);

        // Act
        _factory.Dispose();
        var secondClient = _factory.GetClient(baseUrl);

        // Assert
        secondClient.Should().NotBeSameAs(firstClient);
    }
}
