#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using TelegramBotFramework.Keyboard;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBotFramework.Tests.Keyboard;

public class ReplyKeyboardBuilderTests
{
    [Fact]
    public void Create_ShouldReturnNewBuilderInstance()
    {
        // Act
        var builder = ReplyKeyboardBuilder.Create();

        // Assert
        Assert.NotNull(builder);
    }

    [Fact]
    public void Create_WithMaxButtonsPerRow_ShouldSetMaxButtonsPerRow()
    {
        // Act
        var builder = ReplyKeyboardBuilder.Create(maxButtonsPerRow: 5);

        // Assert
        Assert.NotNull(builder);
    }

    [Fact]
    public void AddButton_WithText_ShouldAddButtonToCurrentRow()
    {
        // Arrange
        var builder = ReplyKeyboardBuilder.Create();

        // Act
        var result = builder.AddButton("Test Button");

        // Assert
        Assert.Same(builder, result);
    }

    [Fact]
    public void AddButton_WithEmptyText_ShouldThrowArgumentException()
    {
        // Arrange
        var builder = ReplyKeyboardBuilder.Create();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.AddButton(""));
        Assert.Throws<ArgumentException>(() => builder.AddButton("   "));
        Assert.Throws<ArgumentException>(() => builder.AddButton(null!));
    }

    [Fact]
    public void AddButton_WithConfigureAction_ShouldAddButtonWithConfiguration()
    {
        // Arrange
        var builder = ReplyKeyboardBuilder.Create();

        // Act
        var result = builder.AddButton("Share Location", button => button.RequestLocation = true);

        // Assert
        Assert.Same(builder, result);
    }

    [Fact]
    public void AddButton_WithConfigureAction_NullConfigure_ShouldThrowArgumentNullException()
    {
        // Arrange
        var builder = ReplyKeyboardBuilder.Create();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.AddButton("Test", null!));
    }

    [Fact]
    public void NewRow_ShouldStartNewRow()
    {
        // Arrange
        var builder = ReplyKeyboardBuilder.Create();

        // Act
        var result = builder
            .AddButton("Button 1")
            .AddButton("Button 2")
            .NewRow()
            .AddButton("Button 3");

        // Assert
        Assert.Same(builder, result);
    }

    [Fact]
    public void OneTime_ShouldSetOneTimeKeyboardToFalse()
    {
        // Arrange
        var builder = ReplyKeyboardBuilder.Create();

        // Act
        var result = builder.OneTime();

        // Assert
        Assert.Same(builder, result);
    }

    [Fact]
    public void Persistent_ShouldSetOneTimeKeyboardToTrue()
    {
        // Arrange
        var builder = ReplyKeyboardBuilder.Create();

        // Act
        var result = builder.Persistent();

        // Assert
        Assert.Same(builder, result);
    }

    [Fact]
    public void Resize_ShouldSetResizeKeyboardToTrue()
    {
        // Arrange
        var builder = ReplyKeyboardBuilder.Create();

        // Act
        var result = builder.Resize();

        // Assert
        Assert.Same(builder, result);
    }

    [Fact]
    public void NoResize_ShouldSetResizeKeyboardToFalse()
    {
        // Arrange
        var builder = ReplyKeyboardBuilder.Create();

        // Act
        var result = builder.NoResize();

        // Assert
        Assert.Same(builder, result);
    }

    [Fact]
    public void Build_WithNoButtons_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var builder = ReplyKeyboardBuilder.Create();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }

    [Fact]
    public void Build_WithButtons_ShouldReturnReplyKeyboardMarkup()
    {
        // Arrange
        var builder = ReplyKeyboardBuilder.Create();

        // Act
        var result = builder
            .AddButton("Button 1")
            .AddButton("Button 2")
            .Build();

        // Assert
        Assert.NotNull(result);
        Assert.IsType<ReplyKeyboardMarkup>(result);
        Assert.Equal(2, result.Keyboard.Count);
        Assert.Equal(2, result.Keyboard[0].Count);
        Assert.Equal("Button 1", result.Keyboard[0][0].Text);
        Assert.Equal("Button 2", result.Keyboard[0][1].Text);
    }

    [Fact]
    public void Build_WithMaxButtonsPerRow_ShouldCreateMultipleRows()
    {
        // Arrange
        var builder = ReplyKeyboardBuilder.Create(maxButtonsPerRow: 2);

        // Act
        var result = builder
            .AddButton("Button 1")
            .AddButton("Button 2")
            .AddButton("Button 3")
            .AddButton("Button 4")
            .Build();

        // Assert
        Assert.Equal(2, result.Keyboard.Count);
        Assert.Equal(2, result.Keyboard[0].Count);
        Assert.Equal(2, result.Keyboard[1].Count);
    }

    [Fact]
    public void Build_WithRequestContact_ShouldSetRequestContact()
    {
        // Arrange
        var builder = ReplyKeyboardBuilder.Create();

        // Act
        var result = builder
            .AddButton("Share Contact", button => button.RequestContact = true)
            .Build();

        // Assert
        Assert.True(result.Keyboard[0][0].RequestContact);
    }

    [Fact]
    public void Build_WithRequestLocation_ShouldSetRequestLocation()
    {
        // Arrange
        var builder = ReplyKeyboardBuilder.Create();

        // Act
        var result = builder
            .AddButton("Share Location", button => button.RequestLocation = true)
            .Build();

        // Assert
        Assert.True(result.Keyboard[0][0].RequestLocation);
    }

    [Fact]
    public void Build_WithOneTime_ShouldSetOneTimeKeyboard()
    {
        // Arrange
        var builder = ReplyKeyboardBuilder.Create();

        // Act
        var result = builder
            .AddButton("Button 1")
            .OneTime()
            .Build();

        // Assert
        Assert.False(result.OneTimeKeyboard);
    }

    [Fact]
    public void Build_WithResize_ShouldSetResizeKeyboard()
    {
        // Arrange
        var builder = ReplyKeyboardBuilder.Create();

        // Act
        var result = builder
            .AddButton("Button 1")
            .Resize()
            .Build();

        // Assert
        Assert.True(result.ResizeKeyboard);
    }

    [Fact]
    public void ToMenu_ShouldConvertToMenu()
    {
        // Arrange
        var builder = ReplyKeyboardBuilder.Create();

        // Act
        var menu = builder
            .AddButton("Button 1")
            .AddButton("Button 2")
            .ToMenu("test-menu", "Test Menu");

        // Assert
        Assert.NotNull(menu);
        Assert.Equal("test-menu", menu.Id);
        Assert.Equal("Test Menu", menu.Title);
        Assert.Equal(Models.MenuType.ReplyKeyboard, menu.Type);
        Assert.Equal(2, menu.Buttons.Count);
    }

    [Fact]
    public void ToMenu_WithEmptyMenuId_ShouldThrowArgumentException()
    {
        // Arrange
        var builder = ReplyKeyboardBuilder.Create();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.ToMenu("", "Test Menu"));
        Assert.Throws<ArgumentException>(() => builder.ToMenu(null!, "Test Menu"));
    }

    [Fact]
    public void ToMenu_WithEmptyTitle_ShouldThrowArgumentException()
    {
        // Arrange
        var builder = ReplyKeyboardBuilder.Create();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.ToMenu("test-menu", ""));
        Assert.Throws<ArgumentException>(() => builder.ToMenu("test-menu", null!));
    }

    [Fact]
    public void Validation_EmptyBuilder_ShouldHaveError()
    {
        // Arrange
        var builder = ReplyKeyboardBuilder.Create();

        // Act
        var errors = builder.Validate();

        // Assert
        Assert.Single(errors);
        Assert.Contains("empty keyboard", errors[0]);
    }

    [Fact]
    public void Validation_ValidBuilder_ShouldHaveNoErrors()
    {
        // Arrange
        var builder = ReplyKeyboardBuilder.Create();

        // Act
        var result = builder
            .AddButton("Button 1")
            .AddButton("Button 2")
            .Validate();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void IsValid_ValidBuilder_ShouldReturnTrue()
    {
        // Arrange
        var builder = ReplyKeyboardBuilder.Create();

        // Act
        var result = builder
            .AddButton("Button 1")
            .IsValid();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValid_InvalidBuilder_ShouldReturnFalse()
    {
        // Arrange
        var builder = ReplyKeyboardBuilder.Create();

        // Act
        var result = builder.IsValid();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void EnsureValid_ValidBuilder_ShouldNotThrow()
    {
        // Arrange
        var builder = ReplyKeyboardBuilder.Create();

        // Act
        builder
            .AddButton("Button 1")
            .AddButton("Button 2")
            .EnsureValid();

        // Assert - no exception thrown
    }

    [Fact]
    public void EnsureValid_InvalidBuilder_ShouldThrowArgumentException()
    {
        // Arrange
        var builder = ReplyKeyboardBuilder.Create();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => builder.EnsureValid());
        Assert.Contains("validation failed", exception.Message);
    }
}
