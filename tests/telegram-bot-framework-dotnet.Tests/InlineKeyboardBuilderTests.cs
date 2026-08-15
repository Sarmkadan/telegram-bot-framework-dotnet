using System;
using System.Linq;
using TelegramBotFramework.Keyboard;
using Xunit;

namespace TelegramBotFramework.Tests;

public class InlineKeyboardBuilderExtraTests
{
    [Fact]
    public void Constructor_InvalidMaxButtonsPerRow_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new InlineKeyboardBuilder(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new InlineKeyboardBuilder(-1));
    }

    [Fact]
    public void AddButton_ValidInput_AddsButtonToRow()
    {
        var builder = new InlineKeyboardBuilder();
        builder.AddButton("Button 1", "data1");
        
        var keyboard = builder.Build();
        Assert.Single(keyboard.InlineKeyboard);
        Assert.Single(keyboard.InlineKeyboard[0]);
        Assert.Equal("Button 1", keyboard.InlineKeyboard[0][0].Text);
    }

    [Fact]
    public void AddButton_EmptyText_ThrowsArgumentException()
    {
        var builder = new InlineKeyboardBuilder();
        Assert.Throws<ArgumentException>(() => builder.AddButton("", "data"));
    }

    [Fact]
    public void AddUrlButton_ValidInput_AddsButton()
    {
        var builder = new InlineKeyboardBuilder();
        builder.AddUrlButton("Google", "https://google.com");
        
        var keyboard = builder.Build();
        Assert.Equal("https://google.com", keyboard.InlineKeyboard[0][0].Url);
    }

    [Fact]
    public void NewRow_ForcesNewRow()
    {
        var builder = new InlineKeyboardBuilder(2);
        builder.AddButton("B1", "d1")
               .NewRow()
               .AddButton("B2", "d2");
        
        var keyboard = builder.Build();
        Assert.Equal(2, keyboard.RowCount);
    }

    [Fact]
    public void Build_EmptyBuilder_ThrowsInvalidOperationException()
    {
        var builder = new InlineKeyboardBuilder();
        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }

    [Fact]
    public void ToMenu_ValidInput_CreatesCorrectMenu()
    {
        var builder = new InlineKeyboardBuilder();
        builder.AddButton("B1", "d1");
        
        var menu = builder.ToMenu("menu1", "Title");
        
        Assert.Equal("menu1", menu.Id);
        Assert.Equal("Title", menu.Title);
        Assert.Single(menu.Buttons);
    }

    [Fact]
    public void ToButtonLabels_ReturnsCorrectStructure()
    {
        var builder = new InlineKeyboardBuilder(1);
        builder.AddButton("B1", "d1").NewRow().AddButton("B2", "d2");
        
        var labels = builder.Build().ToButtonLabels();
        
        Assert.Equal(2, labels.Length);
        Assert.Equal("B1", labels[0][0]);
        Assert.Equal("B2", labels[1][0]);
    }
}
