#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using TelegramBotFramework.Keyboard;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBotFramework.Tests.Keyboard;

/// <summary>
/// Tests for the <see cref="ReplyKeyboardBuilder"/> class.
/// </summary>
public class ReplyKeyboardBuilderTests : IReplyKeyboardBuilderTests
{
/// <summary>
    /// Tests that the Create method returns a new builder instance.
/// </summary>
    public void Create_ShouldReturnNewBuilderInstance()
    {
        // Act
        var builder = ReplyKeyboardBuilder.Create();

        // Assert
        Assert.NotNull(builder);
    }

/// <summary>
/// Tests that create with max buttons per row should set max buttons per row.
/// </summary>
    public void Create_WithMaxButtonsPerRow_ShouldSetMaxButtonsPerRow()
    {
        // Act
        var builder = ReplyKeyboardBuilder.Create(maxButtonsPerRow: 5);

        // Assert
        Assert.NotNull(builder);
    }

/// <summary>
/// Tests that add button with text should add button to current row.
/// </summary>
    public void AddButton_WithText_ShouldAddButtonToCurrentRow()
    {
        // Arrange
        var builder = ReplyKeyboardBuilder.Create();

        // Act
        var result = builder.AddButton("Test Button");

        // Assert
        Assert.Same(builder, result);
    }

/// <summary>
/// Tests that add button with empty text should throw argument exception.
/// </summary>
    public void AddButton_WithEmptyText_ShouldThrowArgumentException()
    {
        // Arrange
        var builder = ReplyKeyboardBuilder.Create();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.AddButton(""));
        Assert.Throws<ArgumentException>(() => builder.AddButton("   "));
        Assert.Throws<ArgumentException>(() => builder.AddButton(null!));
    }

/// <summary>
/// Tests that add button with configure action should add button with configuration.
/// </summary>
    public void AddButton_WithConfigureAction_ShouldAddButtonWithConfiguration()
    {
        // Arrange
        var builder = ReplyKeyboardBuilder.Create();

        // Act
        var result = builder.AddButton("Share Location", button => button.RequestLocation = true);

        // Assert
        Assert.Same(builder, result);
    }

/// <summary>
/// Tests that add button with configure action null configure should throw argument null exception.
/// </summary>
    public void AddButton_WithConfigureAction_NullConfigure_ShouldThrowArgumentNullException()
    {
        // Arrange
        var builder = ReplyKeyboardBuilder.Create();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.AddButton("Test", null!));
    }

/// <summary>
/// Tests that new row should start new row.
/// </summary>
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

/// <summary>
/// Tests that one time should set one time keyboard to false.
/// </summary>
    public void OneTime_ShouldSetOneTimeKeyboardToFalse()
    {
        // Arrange
        var builder = ReplyKeyboardBuilder.Create();

        // Act
        var result = builder.OneTime();

        // Assert
        Assert.Same(builder, result);
    }

/// <summary>
/// Tests that persistent should set one time keyboard to true.
/// </summary>
    public void Persistent_ShouldSetOneTimeKeyboardToTrue()
    {
        // Arrange
        var builder = ReplyKeyboardBuilder.Create();

        // Act
        var result = builder.Persistent();

        // Assert
        Assert.Same(builder, result);
    }

/// <summary>
/// Tests that resize should set resize keyboard to true.
/// </summary>
    public void Resize_ShouldSetResizeKeyboardToTrue()
    {
        // Arrange
        var builder = ReplyKeyboardBuilder.Create();

        // Act
        var result = builder.Resize();

        // Assert
        Assert.Same(builder, result);
    }

/// <summary>
/// Tests that no resize should set resize keyboard to false.
/// </summary>
    public void NoResize_ShouldSetResizeKeyboardToFalse()
    {
        // Arrange
        var builder = ReplyKeyboardBuilder.Create();

        // Act
        var result = builder.NoResize();

        // Assert
        Assert.Same(builder, result);
    }

/// <summary>
/// Tests that build with no buttons should throw invalid operation exception.
/// </summary>
    public void Build_WithNoButtons_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var builder = ReplyKeyboardBuilder.Create();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }

/// <summary>
/// Tests that build with buttons should return reply keyboard markup.
/// </summary>
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

/// <summary>
/// Tests that build with max buttons per row should create multiple rows.
/// </summary>
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

/// <summary>
/// Tests that build with request contact should set request contact.
/// </summary>
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

/// <summary>
/// Tests that build with request location should set request location.
/// </summary>
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

/// <summary>
/// Tests that build with one time should set one time keyboard.
/// </summary>
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

/// <summary>
/// Tests that build with resize should set resize keyboard.
/// </summary>
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

/// <summary>
/// Tests that to menu should convert to menu.
/// </summary>
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

/// <summary>
/// Tests that to menu with empty menu id should throw argument exception.
/// </summary>
    public void ToMenu_WithEmptyMenuId_ShouldThrowArgumentException()
    {
        // Arrange
        var builder = ReplyKeyboardBuilder.Create();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.ToMenu("", "Test Menu"));
        Assert.Throws<ArgumentException>(() => builder.ToMenu(null!, "Test Menu"));
    }

/// <summary>
/// Tests that to menu with empty title should throw argument exception.
/// </summary>
    public void ToMenu_WithEmptyTitle_ShouldThrowArgumentException()
    {
        // Arrange
        var builder = ReplyKeyboardBuilder.Create();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.ToMenu("test-menu", ""));
        Assert.Throws<ArgumentException>(() => builder.ToMenu("test-menu", null!));
    }

/// <summary>
/// Tests that validation empty builder should have error.
/// </summary>
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

/// <summary>
/// Tests that validation valid builder should have no errors.
/// </summary>
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

/// <summary>
/// Tests that is valid valid builder should return true.
/// </summary>
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

/// <summary>
/// Tests that is valid invalid builder should return false.
/// </summary>
    public void IsValid_InvalidBuilder_ShouldReturnFalse()
    {
        // Arrange
        var builder = ReplyKeyboardBuilder.Create();

        // Act
        var result = builder.IsValid();

        // Assert
        Assert.False(result);
    }

/// <summary>
/// Tests that ensure valid valid builder should not throw.
/// </summary>
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

/// <summary>
/// Tests that ensure valid invalid builder should throw argument exception.
/// </summary>
    public void EnsureValid_InvalidBuilder_ShouldThrowArgumentException()
    {
        // Arrange
        var builder = ReplyKeyboardBuilder.Create();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => builder.EnsureValid());
        Assert.Contains("validation failed", exception.Message);
    }
}
