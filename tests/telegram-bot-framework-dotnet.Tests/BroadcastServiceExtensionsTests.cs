using Microsoft.Extensions.DependencyInjection;
using TelegramBotFramework.Services;
using Xunit;

namespace TelegramBotFramework.Tests
{
    public class BroadcastServiceExtensionsTests
    {
        [Fact]
        public void AddBroadcastService_HappyPath_ServiceCollectionReturned()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            var result = BroadcastServiceExtensions.AddBroadcastService(services);

            // Assert
            Assert.Same(services, result);
        }

        [Fact]
        public void AddBroadcastService_ConfigurationActionNotNull_ThrowsArgumentNullException()
        {
            // Arrange
            var services = new ServiceCollection();
            Action<BroadcastOptions> configure = null;

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => BroadcastServiceExtensions.AddBroadcastService(services, configure));
        }

        [Fact]
        public void AddBroadcastServiceSingleton_HappyPath_ServiceCollectionReturned()
        {
            // Arrange
            var services = new ServiceCollection();
            Action<BroadcastOptions> configure = options => { };

            // Act
            var result = BroadcastServiceExtensions.AddBroadcastServiceSingleton(services, configure);

            // Assert
            Assert.Same(services, result);
        }

        [Fact]
        public void AddBroadcastServiceSingleton_ConfigurationActionNotNull_ThrowsArgumentNullException()
        {
            // Arrange
            var services = new ServiceCollection();
            Action<BroadcastOptions> configure = null;

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => BroadcastServiceExtensions.AddBroadcastServiceSingleton(services, configure));
        }
    }
}
