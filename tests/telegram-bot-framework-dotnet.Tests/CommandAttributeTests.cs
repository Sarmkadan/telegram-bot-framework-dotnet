using Xunit;
using TelegramBotFramework.Attributes;

namespace TelegramBotFrameworkDotnet.Tests
{
    public class CommandAttributeTests
    {
        [Fact]
        public void Constructor_ValidName_SetsName()
        {
            // Arrange and Act
            var attribute = new CommandAttribute("test");

            // Assert
            Assert.Equal("test", attribute.Name);
        }

        [Fact]
        public void Constructor_NullName_ThrowsArgumentException()
        {
            // Act and Assert
            Assert.Throws<ArgumentException>(() => new CommandAttribute(null));
        }

        [Fact]
        public void Constructor_EmptyName_ThrowsArgumentException()
        {
            // Act and Assert
            Assert.Throws<ArgumentException>(() => new CommandAttribute(""));
        }

        [Fact]
        public void Constructor_LeadingSlashName_SetsNameWithoutSlash()
        {
            // Arrange and Act
            var attribute = new CommandAttribute("/test");

            // Assert
            Assert.Equal("test", attribute.Name);
        }

        [Fact]
        public void Description_DefaultValueIsNull()
        {
            // Arrange and Act
            var attribute = new CommandAttribute("test");

            // Assert
            Assert.Null(attribute.Description);
        }

        [Fact]
        public void Aliases_DefaultValueIsEmptyArray()
        {
            // Arrange and Act
            var attribute = new CommandAttribute("test");

            // Assert
            Assert.Empty(attribute.Aliases);
        }
    }
}
