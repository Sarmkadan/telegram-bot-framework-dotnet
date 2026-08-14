using Xunit;
using TelegramBotFramework.Utilities;

namespace TelegramBotFrameworkDotnet.Tests
{
    public class StringExtensionsTests
    {
        [Fact]
        public void Truncate_HappyPath_ReturnsTruncatedString()
        {
            // Arrange
            var input = "This is a long string that needs to be truncated.";
            var maxLength = 20;

            // Act
            var result = input.Truncate(maxLength);

            // Assert
            Assert.True(result.Length <= maxLength);
        }

        [Fact]
        public void ToSlug_HappyPath_ReturnsSlug()
        {
            // Arrange
            var input = "Hello World!";

            // Act
            var result = input.ToSlug();

            // Assert
            Assert.Equal("hello-world", result);
        }

        [Fact]
        public void IsValidEmail_HappyPath_ReturnsTrue()
        {
            // Arrange
            var input = "test@example.com";

            // Act
            var result = input.IsValidEmail();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsAlphanumeric_HappyPath_ReturnsTrue()
        {
            // Arrange
            var input = "HelloWorld123";

            // Act
            var result = input.IsAlphanumeric();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Repeat_HappyPath_ReturnsRepeatedString()
        {
            // Arrange
            var input = "Hello";
            var count = 3;

            // Act
            var result = input.Repeat(count);

            // Assert
            Assert.Equal("HelloHelloHello", result);
        }

        [Fact]
        public void Reverse_HappyPath_ReturnsReversedString()
        {
            // Arrange
            var input = "Hello";

            // Act
            var result = input.Reverse();

            // Assert
            Assert.Equal("olleH", result);
        }

        [Fact]
        public void ExtractNumbers_HappyPath_ReturnsExtractedNumbers()
        {
            // Arrange
            var input = "Hello123World456";

            // Act
            var result = input.ExtractNumbers();

            // Assert
            Assert.Equal("123456", result);
        }

        [Fact]
        public void EnsureStartsWith_HappyPath_ReturnsStringWithPrefix()
        {
            // Arrange
            var input = "World";
            var prefix = "Hello ";

            // Act
            var result = input.EnsureStartsWith(prefix);

            // Assert
            Assert.Equal("Hello World", result);
        }

        [Fact]
        public void EnsureEndsWith_HappyPath_ReturnsStringWithSuffix()
        {
            // Arrange
            var input = "Hello";
            var suffix = " World";

            // Act
            var result = input.EnsureEndsWith(suffix);

            // Assert
            Assert.Equal("Hello World", result);
        }

        [Fact]
        public void Capitalize_HappyPath_ReturnsCapitalizedString()
        {
            // Arrange
            var input = "hello";

            // Act
            var result = input.Capitalize();

            // Assert
            Assert.Equal("Hello", result);
        }

        [Fact]
        public void Truncate_NullInput_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => ((string)null).Truncate(10));
        }

        [Fact]
        public void ToSlug_NullInput_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => ((string)null).ToSlug());
        }

        [Fact]
        public void IsValidEmail_NullInput_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => ((string)null).IsValidEmail());
        }

        [Fact]
        public void IsAlphanumeric_NullInput_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => ((string)null).IsAlphanumeric());
        }

        [Fact]
        public void Repeat_NullInput_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => ((string)null).Repeat(10));
        }

        [Fact]
        public void Reverse_NullInput_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => ((string)null).Reverse());
        }

        [Fact]
        public void ExtractNumbers_NullInput_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => ((string)null).ExtractNumbers());
        }

        [Fact]
        public void EnsureStartsWith_NullInput_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => ((string)null).EnsureStartsWith("prefix"));
        }

        [Fact]
        public void EnsureEndsWith_NullInput_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => ((string)null).EnsureEndsWith("suffix"));
        }

        [Fact]
        public void Capitalize_NullInput_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => ((string)null).Capitalize());
        }
    }
}
