using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using TelegramBotFramework.Caching;
using Xunit;

namespace TelegramBotFrameworkDotnet.Tests
{
    public class DistributedCacheProviderValidationTests
    {
        [Fact]
        public async Task Validate_HappyPath_ReturnsEmptyList()
        {
            // Arrange
            var cacheProvider = new Mock<DistributedCacheProvider>();
            cacheProvider.Setup(p => p.GetStatisticsAsync()).ReturnsAsync(new CacheStatistics());

            // Act
            var result = DistributedCacheProviderValidation.Validate(cacheProvider.Object);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task IsValid_HappyPath_ReturnsTrue()
        {
            // Arrange
            var cacheProvider = new Mock<DistributedCacheProvider>();
            cacheProvider.Setup(p => p.GetStatisticsAsync()).ReturnsAsync(new CacheStatistics());

            // Act
            var result = DistributedCacheProviderValidation.IsValid(cacheProvider.Object);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task EnsureValid_HappyPath_DoesNotThrow()
        {
            // Arrange
            var cacheProvider = new Mock<DistributedCacheProvider>();
            cacheProvider.Setup(p => p.GetStatisticsAsync()).ReturnsAsync(new CacheStatistics());

            // Act and Assert
            DistributedCacheProviderValidation.EnsureValid(cacheProvider.Object);
        }

        [Fact]
        public async Task Validate_NullInput_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => DistributedCacheProviderValidation.Validate(null));
        }

        [Fact]
        public async Task IsValid_NullInput_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => DistributedCacheProviderValidation.IsValid(null));
        }

        [Fact]
        public async Task EnsureValid_NullInput_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => DistributedCacheProviderValidation.EnsureValid(null));
        }

        [Fact]
        public async Task Validate_InvalidCacheProvider_ReturnsErrorList()
        {
            // Arrange
            var cacheProvider = new Mock<DistributedCacheProvider>();
            cacheProvider.Setup(p => p.GetStatisticsAsync()).ReturnsAsync(new CacheStatistics { HitCount = -1 });

            // Act
            var result = DistributedCacheProviderValidation.Validate(cacheProvider.Object);

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task IsValid_InvalidCacheProvider_ReturnsFalse()
        {
            // Arrange
            var cacheProvider = new Mock<DistributedCacheProvider>();
            cacheProvider.Setup(p => p.GetStatisticsAsync()).ReturnsAsync(new CacheStatistics { HitCount = -1 });

            // Act
            var result = DistributedCacheProviderValidation.IsValid(cacheProvider.Object);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task EnsureValid_InvalidCacheProvider_ThrowsArgumentException()
        {
            // Arrange
            var cacheProvider = new Mock<DistributedCacheProvider>();
            cacheProvider.Setup(p => p.GetStatisticsAsync()).ReturnsAsync(new CacheStatistics { HitCount = -1 });

            // Act and Assert
            Assert.Throws<ArgumentException>(() => DistributedCacheProviderValidation.EnsureValid(cacheProvider.Object));
        }
    }
}
