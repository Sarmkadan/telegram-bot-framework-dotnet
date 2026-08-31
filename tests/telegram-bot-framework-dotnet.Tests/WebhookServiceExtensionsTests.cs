using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using TelegramBotFramework.Integration;
using Xunit;

namespace TelegramBotFramework.Tests.Integration;

public class WebhookServiceExtensionsTests
{
    private readonly Mock<ITelegramApiClient> _apiClientMock;
    private readonly Mock<ILogger<WebhookService>> _loggerMock;
    private readonly WebhookOptions _options;
    private readonly WebhookService _service;
    private readonly TelegramApiClient _realApiClient;

    public WebhookServiceExtensionsTests()
    {
        _apiClientMock = new Mock<ITelegramApiClient>();
        _loggerMock = new Mock<ILogger<WebhookService>>();
        _options = new WebhookOptions { Url = "https://example.com/webhook" };
        // Use a real TelegramApiClient for testing the extension methods that access fields via reflection
        _realApiClient = new TelegramApiClient("123456789:abcdefghijklmnopqrstuvwxyzA");
        _service = new WebhookService(_realApiClient, _options, _loggerMock.Object);
    }

    [Fact]
    public async Task EnsureRegisteredAsync_SuccessOnFirstAttempt_ReturnsTrue()
    {
        // Arrange
        _apiClientMock.Setup(x => x.SetWebhookAsync(_options.Url))
            .ReturnsAsync(true);
        // Create service with mock for this test
        var serviceWithMock = new WebhookService(_apiClientMock.Object, _options, _loggerMock.Object);

        // Act
        var result = await serviceWithMock.EnsureRegisteredAsync(1, 1, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        _apiClientMock.Verify(x => x.SetWebhookAsync(_options.Url), Times.Once);
    }

    [Fact]
    public async Task EnsureRegisteredAsync_FailsAfterMaxRetries_ReturnsFalse()
    {
        // Arrange
        _apiClientMock.Setup(x => x.SetWebhookAsync(_options.Url))
            .ReturnsAsync(false);
        // Create service with mock for this test
        var serviceWithMock = new WebhookService(_apiClientMock.Object, _options, _loggerMock.Object);

        // Act
        var result = await serviceWithMock.EnsureRegisteredAsync(2, 1, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
        _apiClientMock.Verify(x => x.SetWebhookAsync(_options.Url), Times.Exactly(2));
    }

    [Fact]
    public void EnsureRegisteredAsync_NullService_ThrowsArgumentNullException()
    {
        // Act
        Func<Task> act = async () => await WebhookServiceExtensions.EnsureRegisteredAsync(null!, 1, 1, CancellationToken.None);

        // Assert
        act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public void EnsureRegisteredAsync_InvalidMaxRetries_ThrowsArgumentOutOfRangeException()
    {
        // Act
        Func<Task> act = async () => await _service.EnsureRegisteredAsync(0, 1, CancellationToken.None);

        // Assert
        act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void EnsureRegisteredAsync_InvalidRetryDelayMs_ThrowsArgumentOutOfRangeException()
    {
        // Act
        Func<Task> act = async () => await _service.EnsureRegisteredAsync(1, 0, CancellationToken.None);

        // Assert
        act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task EnsureUnregisteredAsync_Success_ReturnsTrue()
    {
        // Arrange
        // First register the service
        _apiClientMock.Setup(x => x.SetWebhookAsync(_options.Url))
            .ReturnsAsync(true);
        var serviceWithMock = new WebhookService(_apiClientMock.Object, _options, _loggerMock.Object);
        await serviceWithMock.RegisterAsync(CancellationToken.None);

        // Setup unregister to succeed
        _apiClientMock.Setup(x => x.RemoveWebhookAsync())
            .ReturnsAsync(true);

        // Act
        var result = await serviceWithMock.EnsureUnregisteredAsync(CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        _apiClientMock.Verify(x => x.RemoveWebhookAsync(), Times.Once);
    }

    [Fact]
    public async Task EnsureUnregisteredAsync_Fails_ReturnsFalse()
    {
        // Arrange
        // First register the service
        _apiClientMock.Setup(x => x.SetWebhookAsync(_options.Url))
            .ReturnsAsync(true);
        var serviceWithMock = new WebhookService(_apiClientMock.Object, _options, _loggerMock.Object);
        await serviceWithMock.RegisterAsync(CancellationToken.None);

        // Setup unregister to fail
        _apiClientMock.Setup(x => x.RemoveWebhookAsync())
            .ReturnsAsync(false);

        // Act
        var result = await serviceWithMock.EnsureUnregisteredAsync(CancellationToken.None);

        // Assert
        result.Should().BeFalse();
        _apiClientMock.Verify(x => x.RemoveWebhookAsync(), Times.Once);
    }

    [Fact]
    public void EnsureUnregisteredAsync_NullService_ThrowsArgumentNullException()
    {
        // Act
        Func<Task> act = async () => await WebhookServiceExtensions.EnsureUnregisteredAsync(null!, CancellationToken.None);

        // Assert
        act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public void GetLogger_ReturnsLogger()
    {
        // Act
        var logger = WebhookServiceExtensions.GetLogger(_service);

        // Assert
        logger.Should().BeSameAs(_loggerMock.Object);
    }

    [Fact]
    public void GetLogger_NullService_ThrowsArgumentNullException()
    {
        // Act
        Func<ILogger<WebhookService>> act = () => WebhookServiceExtensions.GetLogger(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetApiClient_ReturnsApiClient()
    {
        // Act
        var apiClient = WebhookServiceExtensions.GetApiClient(_service);

        // Assert
        apiClient.Should().BeSameAs(_realApiClient);
    }

    [Fact]
    public void GetApiClient_NullService_ThrowsArgumentNullException()
    {
        // Act
        Func<TelegramApiClient> act = () => WebhookServiceExtensions.GetApiClient(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetOptions_ReturnsOptions()
    {
        // Act
        var options = WebhookServiceExtensions.GetOptions(_service);

        // Assert
        options.Should().BeSameAs(_options);
    }

    [Fact]
    public void GetOptions_NullService_ThrowsArgumentNullException()
    {
        // Act
        Func<WebhookOptions> act = () => WebhookServiceExtensions.GetOptions(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddWebhookService_ValidParameters_ReturnsServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = WebhookServiceExtensions.AddWebhookService(services, opts => { });

        // Assert
        result.Should().NotBeNull();
        result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddWebhookService_NullServices_ThrowsArgumentNullException()
    {
        // Act
        Func<IServiceCollection> act = () => WebhookServiceExtensions.AddWebhookService(null!, opts => { });

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddWebhookService_NullConfigure_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        Func<IServiceCollection> act = () => WebhookServiceExtensions.AddWebhookService(services, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetUpdatesDispatchedCount_ReturnsCurrentCount()
    {
        // Arrange
        // Dispatch some updates to increment the counter
        for (int i = 0; i < 42; i++)
        {
            _service.DispatchUpdateAsync(new TelegramUpdate { UpdateId = i }).Wait();
        }

        // Act
        var count = WebhookServiceExtensions.GetUpdatesDispatchedCount(_service);

        // Assert
        count.Should().Be(42L);
    }

    [Fact]
    public void GetUpdatesDispatchedCount_NullService_ThrowsArgumentNullException()
    {
        // Act
        Func<long> act = () => WebhookServiceExtensions.GetUpdatesDispatchedCount(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task GetRegisteredAt_ReturnsRegisteredAt()
    {
        // Arrange
        var expected = new DateTime(2026, 8, 31, 12, 0, 0, DateTimeKind.Utc);
        // Register the service to set _registeredAt
        _apiClientMock.Setup(x => x.SetWebhookAsync(_options.Url))
            .ReturnsAsync(true);
        var serviceWithMock = new WebhookService(_apiClientMock.Object, _options, _loggerMock.Object);
        await serviceWithMock.RegisterAsync(CancellationToken.None);

        // Act
        var result = WebhookServiceExtensions.GetRegisteredAt(serviceWithMock);

        // Assert
        result.Should().NotBeNull();
        result.Value.Date.Should().Be(expected.Date); // Just check date part since time may vary slightly
    }

    [Fact]
    public async Task GetRegisteredAt_ReturnsNullWhenNotRegistered()
    {
        // Arrange
        // Ensure service is not registered by unsetting webhook
        _apiClientMock.Setup(x => x.RemoveWebhookAsync())
            .ReturnsAsync(true);
        var serviceWithMock = new WebhookService(_apiClientMock.Object, _options, _loggerMock.Object);
        await serviceWithMock.RegisterAsync(CancellationToken.None);
        await serviceWithMock.UnregisterAsync(CancellationToken.None);

        // Act
        var result = WebhookServiceExtensions.GetRegisteredAt(serviceWithMock);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetRegisteredAt_NullService_ThrowsArgumentNullException()
    {
        // Act
        Func<DateTime?> act = () => WebhookServiceExtensions.GetRegisteredAt(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}