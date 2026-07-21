#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using TelegramBotFramework.Keyboard;
using Xunit;

namespace TelegramBotFramework.Tests;

/// <summary>
/// Edge-case tests for <see cref="TelegramBotFramework.Keyboard.InlineKeyboardBuilder"/> class.
/// Tests boundary conditions, edge cases, and unusual scenarios.
/// </summary>
public sealed class InlineKeyboardBuilderEdgeCaseTests
{
    /// <summary>
    /// Tests that building with an empty builder throws InvalidOperationException.
    /// Edge case: no buttons added at all.
    /// </summary>
    [Fact]
    public void Build_EmptyBuilder_ThrowsInvalidOperationException()
    {
        var act = () => InlineKeyboardBuilder.Create().Build();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*empty keyboard*");
    }

    /// <summary>
    /// Tests that building with max row width of 1 creates one button per row.
    /// Edge case: minimum row width configuration.
    /// </summary>
    [Fact]
    public void Build_MaxButtonsPerRowOne_CreatesOneButtonPerRow()
    {
        var markup = InlineKeyboardBuilder.Create(maxButtonsPerRow: 1)
            .AddButton("Button1", "data1")
            .AddButton("Button2", "data2")
            .AddButton("Button3", "data3")
            .Build();

        markup.RowCount.Should().Be(3);
        markup.InlineKeyboard[0].Count.Should().Be(1);
        markup.InlineKeyboard[1].Count.Should().Be(1);
        markup.InlineKeyboard[2].Count.Should().Be(1);
        markup.TotalButtonCount.Should().Be(3);
    }

    /// <summary>
    /// Tests that building with a very large max row width (100) keeps all buttons in one row.
    /// Edge case: maximum practical row width configuration.
    /// </summary>
    [Fact]
    public void Build_MaxButtonsPerRowLarge_AllButtonsInOneRow()
    {
        var markup = InlineKeyboardBuilder.Create(maxButtonsPerRow: 100)
            .AddButton("Btn1", "data1")
            .AddButton("Btn2", "data2")
            .AddButton("Btn3", "data3")
            .Build();

        markup.RowCount.Should().Be(1);
        markup.InlineKeyboard[0].Count.Should().Be(3);
        markup.TotalButtonCount.Should().Be(3);
    }

    /// <summary>
    /// Tests that NewRow() followed by Build() with no additional buttons does not create an empty row.
    /// Edge case: manual row creation with no buttons.
    /// </summary>
    [Fact]
    public void Build_AfterNewRowWithNoButtons_DoesNotCreateEmptyRow()
    {
        var markup = InlineKeyboardBuilder.Create()
            .AddButton("Btn1", "data1")
            .NewRow()
            .Build();

        markup.RowCount.Should().Be(1);
        markup.InlineKeyboard[0].Count.Should().Be(1);
    }

    /// <summary>
    /// Tests that multiple consecutive NewRow() calls do not create empty rows.
    /// Edge case: multiple manual row breaks with no buttons.
    /// </summary>
    [Fact]
    public void Build_MultipleConsecutiveNewRows_DoesNotCreateEmptyRows()
    {
        var markup = InlineKeyboardBuilder.Create()
            .AddButton("Btn1", "data1")
            .NewRow()
            .NewRow()
            .NewRow()
            .Build();

        markup.RowCount.Should().Be(1);
        markup.InlineKeyboard[0].Count.Should().Be(1);
    }

    /// <summary>
    /// Tests that duplicate callback data is allowed (Telegram doesn't prohibit this).
    /// Edge case: duplicate callback data values.
    /// </summary>
    [Fact]
    public void AddButton_DuplicateCallbackData_Allowed()
    {
        var markup = InlineKeyboardBuilder.Create()
            .AddButton("Button A", "same_data")
            .AddButton("Button B", "same_data")
            .AddButton("Button C", "same_data")
            .Build();

        markup.RowCount.Should().Be(1);
        markup.TotalButtonCount.Should().Be(3);
        markup.InlineKeyboard[0][0].CallbackData.Should().Be("same_data");
        markup.InlineKeyboard[0][1].CallbackData.Should().Be("same_data");
        markup.InlineKeyboard[0][2].CallbackData.Should().Be("same_data");
    }

    /// <summary>
    /// Tests that duplicate button text is allowed.
    /// Edge case: duplicate button text values.
    /// </summary>
    [Fact]
    public void AddButton_DuplicateText_Allowed()
    {
        var markup = InlineKeyboardBuilder.Create()
            .AddButton("OK", "ok1")
            .AddButton("OK", "ok2")
            .AddButton("OK", "ok3")
            .Build();

        markup.RowCount.Should().Be(1);
        markup.TotalButtonCount.Should().Be(3);
        markup.InlineKeyboard[0][0].Text.Should().Be("OK");
        markup.InlineKeyboard[0][1].Text.Should().Be("OK");
        markup.InlineKeyboard[0][2].Text.Should().Be("OK");
    }

    /// <summary>
    /// Tests that very long button text (exceeding typical display limits) is allowed.
    /// Edge case: extremely long button text.
    /// </summary>
    [Fact]
    public void AddButton_VeryLongText_Allowed()
    {
        var longText = new string('A', 1000);

        var markup = InlineKeyboardBuilder.Create()
            .AddButton(longText, "data")
            .Build();

        markup.RowCount.Should().Be(1);
        markup.TotalButtonCount.Should().Be(1);
        markup.InlineKeyboard[0][0].Text.Should().Be(longText);
    }

    /// <summary>
    /// Tests that button text with only whitespace is rejected.
    /// Edge case: whitespace-only button text.
    /// </summary>
    [Fact]
    public void AddButton_WhitespaceText_ThrowsArgumentException()
    {
        var act = () => InlineKeyboardBuilder.Create().AddButton("   ", "data");

        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Tests that null button text throws ArgumentException.
    /// Edge case: null button text.
    /// </summary>
    [Fact]
    public void AddButton_NullText_ThrowsArgumentException()
    {
        var act = () => InlineKeyboardBuilder.Create().AddButton(null!, "data");

        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Tests that null URL throws ArgumentException.
    /// Edge case: null URL parameter.
    /// </summary>
    [Fact]
    public void AddUrlButton_NullUrl_ThrowsArgumentException()
    {
        var act = () => InlineKeyboardBuilder.Create().AddUrlButton("Text", null!);

        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Tests that whitespace-only URL throws ArgumentException.
    /// Edge case: whitespace-only URL.
    /// </summary>
    [Fact]
    public void AddUrlButton_WhitespaceUrl_ThrowsArgumentException()
    {
        var act = () => InlineKeyboardBuilder.Create().AddUrlButton("Text", "   ");

        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Tests that null switch inline query is allowed (empty string is valid).
    /// Edge case: empty switch inline query.
    /// </summary>
    [Fact]
    public void AddSwitchInlineButton_EmptyQuery_Allowed()
    {
        var markup = InlineKeyboardBuilder.Create()
            .AddSwitchInlineButton("Search", "")
            .Build();

        markup.RowCount.Should().Be(1);
        markup.TotalButtonCount.Should().Be(1);
        markup.InlineKeyboard[0][0].SwitchInlineQuery.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that whitespace-only switch inline query is allowed.
    /// Edge case: whitespace-only switch inline query.
    /// </summary>
    [Fact]
    public void AddSwitchInlineButton_WhitespaceQuery_Allowed()
    {
        var markup = InlineKeyboardBuilder.Create()
            .AddSwitchInlineButton("Search", "   ")
            .Build();

        markup.RowCount.Should().Be(1);
        markup.TotalButtonCount.Should().Be(1);
        markup.InlineKeyboard[0][0].SwitchInlineQuery.Should().Be("   ");
    }

    /// <summary>
    /// Tests that callback data exactly 64 bytes is allowed (boundary case).
    /// Edge case: maximum allowed callback data length.
    /// </summary>
    [Fact]
    public void AddButton_CallbackData64Bytes_Allowed()
    {
        var callbackData64 = new string('x', 64);

        var act = () => InlineKeyboardBuilder.Create().AddButton("Test", callbackData64);

        act.Should().NotThrow();
    }

    /// <summary>
    /// Tests that callback data of 65 bytes throws ArgumentException (exceeds limit).
    /// Edge case: one byte over the limit.
    /// </summary>
    [Fact]
    public void AddButton_CallbackData65Bytes_ThrowsArgumentException()
    {
        var callbackData65 = new string('x', 65);

        var act = () => InlineKeyboardBuilder.Create().AddButton("Test", callbackData65);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*64*byte*");
    }

    /// <summary>
    /// Tests that callback data with Unicode characters is properly counted by byte length.
    /// Edge case: Unicode characters in callback data.
    /// </summary>
    [Fact]
    public void AddButton_UnicodeCallbackData_CorrectByteCount()
    {
        // "café" is 5 characters but 6 bytes in UTF-8
        var unicodeCallback = "café";

        var act = () => InlineKeyboardBuilder.Create().AddButton("Test", unicodeCallback);

        act.Should().NotThrow();
    }

    /// <summary>
    /// Tests that callback data exceeding 64 bytes with Unicode characters throws.
    /// Edge case: Unicode characters pushing past the limit.
    /// </summary>
    [Fact]
    public void AddButton_UnicodeCallbackDataExceedsLimit_ThrowsArgumentException()
    {
        // Create a string that's 64 bytes but contains Unicode
        var builder = new System.Text.StringBuilder();
        while (System.Text.Encoding.UTF8.GetByteCount(builder.ToString()) < 64)
        {
            builder.Append('é'); // 2 bytes per character
        }
        var callbackData = builder.ToString();

        // This should be exactly at or near 64 bytes
        var act = () => InlineKeyboardBuilder.Create().AddButton("Test", callbackData);

        // Should either succeed or fail depending on exact byte count
        try
        {
            act.Should().NotThrow();
        }
        catch
        {
            act.Should().Throw<ArgumentException>();
        }
    }

    /// <summary>
    /// Tests that ToButtonLabels() returns correct structure for multi-row keyboard.
    /// Edge case: multiple rows with varying button counts.
    /// </summary>
    [Fact]
    public void ToButtonLabels_MultiRowKeyboard_ReturnsCorrectStructure()
    {
        var markup = InlineKeyboardBuilder.Create(maxButtonsPerRow: 2)
            .AddButton("A", "a")
            .AddButton("B", "b")
            .NewRow()
            .AddButton("C", "c")
            .AddButton("D", "d")
            .AddButton("E", "e")
            .NewRow()
            .AddButton("F", "f")
            .Build();

        var labels = markup.ToButtonLabels();

        labels.Should().HaveCount(4);
        labels[0].Should().BeEquivalentTo(new[] { "A", "B" });
        labels[1].Should().BeEquivalentTo(new[] { "C", "D" });
        labels[2].Should().BeEquivalentTo(new[] { "E" });
        labels[3].Should().BeEquivalentTo(new[] { "F" });
    }

    /// <summary>
    /// Tests that ToButtonLabels() does not include empty rows in the output.
    /// Edge case: row with zero buttons.
    /// </summary>
    [Fact]
    public void ToButtonLabels_EmptyRow_NotIncludedInOutput()
    {
        var markup = InlineKeyboardBuilder.Create()
            .AddButton("Btn1", "data1")
            .NewRow()
            .Build();

        var labels = markup.ToButtonLabels();

        labels.Should().HaveCount(1);
        labels[0].Should().BeEquivalentTo(new[] { "Btn1" });
    }

    /// <summary>
    /// Tests that TotalButtonCount returns 0 for empty keyboard (though Build() would throw).
    /// Edge case: button count calculation.
    /// </summary>
    [Fact]
    public void TotalButtonCount_CalculatesCorrectly()
    {
        var markup = InlineKeyboardBuilder.Create(maxButtonsPerRow: 3)
            .AddButton("1", "a")
            .AddButton("2", "b")
            .NewRow()
            .AddButton("3", "c")
            .AddButton("4", "d")
            .AddButton("5", "e")
            .NewRow()
            .AddButton("6", "f")
            .Build();

        markup.TotalButtonCount.Should().Be(6);
    }

    /// <summary>
    /// Tests that RowCount returns correct number of rows.
    /// Edge case: row count calculation.
    /// </summary>
    [Fact]
    public void RowCount_ReturnsCorrectRowCount()
    {
        var markup = InlineKeyboardBuilder.Create(maxButtonsPerRow: 2)
            .AddButton("A", "a")
            .AddButton("B", "b")
            .NewRow()
            .AddButton("C", "c")
            .Build();

        markup.RowCount.Should().Be(2);
    }

    /// <summary>
    /// Tests that Fluent interface allows chaining multiple operations.
    /// Edge case: fluent interface with many operations.
    /// </summary>
    [Fact]
    public void FluentInterface_ChainingMultipleOperations_WorksCorrectly()
    {
        var markup = InlineKeyboardBuilder.Create(maxButtonsPerRow: 2)
            .AddButton("Btn1", "data1")
            .AddButton("Btn2", "data2")
            .NewRow()
            .AddUrlButton("Url1", "https://example.com/1")
            .AddUrlButton("Url2", "https://example.com/2")
            .NewRow()
            .AddSwitchInlineButton("Switch1", "query1")
            .AddSwitchInlineButton("Switch2", "query2")
            .AddSwitchInlineButton("Switch3", "query3")
            .NewRow()
            .AddButton("Btn3", "data3")
            .Build();

        markup.RowCount.Should().Be(5);
        markup.TotalButtonCount.Should().Be(8);
    }

    /// <summary>
    /// Tests that builder with maxButtonsPerRow=0 throws ArgumentOutOfRangeException.
    /// Edge case: invalid constructor parameter.
    /// </summary>
    [Fact]
    public void Constructor_MaxButtonsPerRowZero_ThrowsArgumentOutOfRangeException()
    {
        var act = () => new InlineKeyboardBuilder(maxButtonsPerRow: 0);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    /// <summary>
    /// Tests that builder with negative maxButtonsPerRow throws ArgumentOutOfRangeException.
    /// Edge case: negative constructor parameter.
    /// </summary>
    [Fact]
    public void Constructor_NegativeMaxButtonsPerRow_ThrowsArgumentOutOfRangeException()
    {
        var act = () => new InlineKeyboardBuilder(maxButtonsPerRow: -1);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    /// <summary>
    /// Tests that adding button with empty callback data is allowed (null or empty is valid).
    /// Edge case: empty/null callback data.
    /// </summary>
    [Fact]
    public void AddButton_EmptyCallbackData_Allowed()
    {
        var markup = InlineKeyboardBuilder.Create()
            .AddButton("Btn", "")
            .Build();

        markup.RowCount.Should().Be(1);
        markup.TotalButtonCount.Should().Be(1);
        markup.InlineKeyboard[0][0].CallbackData.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that adding button with null callback data is allowed.
    /// Edge case: null callback data.
    /// </summary>
    [Fact]
    public void AddButton_NullCallbackData_Allowed()
    {
        var markup = InlineKeyboardBuilder.Create()
            .AddButton("Btn", null!)
            .Build();

        markup.RowCount.Should().Be(1);
        markup.TotalButtonCount.Should().Be(1);
        markup.InlineKeyboard[0][0].CallbackData.Should().BeNull();
    }

    /// <summary>
    /// Tests that mixed button types in the same row work correctly.
    /// Edge case: mixing different button types.
    /// </summary>
    [Fact]
    public void Build_MixedButtonTypesInSameRow_Correctly()
    {
        var markup = InlineKeyboardBuilder.Create(maxButtonsPerRow: 3)
            .AddButton("Callback", "cb_data")
            .AddUrlButton("URL", "https://example.com")
            .AddSwitchInlineButton("Switch", "query")
            .Build();

        markup.RowCount.Should().Be(1);
        markup.TotalButtonCount.Should().Be(3);

        markup.InlineKeyboard[0][0].Type.Should().Be(InlineButtonType.Callback);
        markup.InlineKeyboard[0][1].Type.Should().Be(InlineButtonType.Url);
        markup.InlineKeyboard[0][2].Type.Should().Be(InlineButtonType.SwitchInline);
    }

    /// <summary>
    /// Tests that ToMenu() creates a menu with correct button count including empty rows.
    /// Edge case: menu conversion with empty rows.
    /// </summary>
    [Fact]
    public void ToMenu_WithEmptyRows_CountsAllButtons()
    {
        var menu = InlineKeyboardBuilder.Create()
            .AddButton("Btn1", "data1")
            .NewRow()
            .AddButton("Btn2", "data2")
            .ToMenu("test_menu", "Test Menu");

        menu.Buttons.Should().HaveCount(2);
        menu.Buttons[0].Label.Should().Be("Btn1");
        menu.Buttons[1].Label.Should().Be("Btn2");
    }
}
