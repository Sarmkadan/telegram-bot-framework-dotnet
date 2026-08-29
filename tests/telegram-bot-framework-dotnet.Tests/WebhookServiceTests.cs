#nullable enable
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using TelegramBotFramework.Integration;
using Xunit;

namespace TelegramBotFramework.Tests;

public class WebhookServiceTests
{
    private readonly Mock<ITelegramApiClient> _apiClientMock;
    private readonly Mock<ILogger<WebhookService>> _loggerMock;
    private readonly WebhookOptions _options;

    public WebhookServiceTests()
    {
        _apiClientMock = new Mock<ITelegramApiClient>();
        _loggerMock = new Mock<ILogger<WebhookService>>();
        _options = new WebhookOptions { Url = "https://example.com", SecretToken = "secret" };
    }

    [Fact]
    public async Task StartAsync_RegistersWebhook()
    {
        // Arrange
        var service = new WebhookService(_apiClientMock.Object, _options, _loggerMock.Object);

        // Act
        await service.StartAsync(CancellationToken.None);

        // Assert
        _apiClientMock.Verify(x => x.SetWebhookAsync(_options.Url), Times.Once);
    }

    [Fact]
    public async Task StopAsync_UnregistersWebhook()
    {
        // Arrange
        var service = new WebhookService(_apiClientMock.Object, _options, _loggerMock.Object);
        await service.StartAsync(CancellationToken.None);

        // Act
        await service.StopAsync(CancellationToken.None);

        // Assert
        _apiClientMock.Verify(x => x.RemoveWebhookAsync(), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_ThrowsArgumentNullException_ForNullOptions()
    {
        // Act and Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => new WebhookService(_apiClientMock.Object, null!, _loggerMock.Object).RegisterAsync());
    }

    [Fact]
    public async Task DispatchUpdateAsync_ThrowsArgumentNullException_ForNullUpdate()
    {
        // Arrange
        var service = new WebhookService(_apiClientMock.Object, _options, _loggerMock.Object);

        // Act and Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => service.DispatchUpdateAsync(null!));
    }

    [Fact]
    public async Task ParseAndValidateAsync_ReturnsNull_ForInvalidSecretToken()
    {
        // Arrange
        var service = new WebhookService(_apiClientMock.Object, _options, _loggerMock.Object);

        // Act
        var result = await service.ParseAndValidateAsync("{}", "invalid-token", CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetInfo_ReturnsWebhookInfo()
    {
        // Arrange
        var service = new WebhookService(_apiClientMock.Object, _options, _loggerMock.Object);

        // Act
        var info = service.GetInfo();

        // Assert
        info.IsRegistered.Should().BeFalse();
        info.Url.Should().BeNull();
    }
}
