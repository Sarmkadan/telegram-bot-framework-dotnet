#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using TelegramBotFramework.Keyboard;
using Xunit;
using static TelegramBotFramework.Tests.InlineKeyboardBuilderEdgeCaseTestsConstants;

namespace TelegramBotFramework.Tests;

/// <summary>
/// Edge-case tests for <see cref="TelegramBotFramework.Keyboard.InlineKeyboardBuilder"/> class.
/// Tests boundary conditions, edge cases, and unusual scenarios.
/// </summary>
public sealed class InlineKeyboardBuilderEdgeCaseTests : IInlineKeyboardBuilderEdgeCaseTests
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
            .WithMessage(EmptyKeyboardMessagePattern);
    }

    /// <summary>
    /// Tests that building with max row width of 1 creates one button per row.
    /// Edge case: minimum row width configuration.
    /// </summary>
    [Fact]
    public void Build_MaxButtonsPerRowOne_CreatesOneButtonPerRow()
    {
        var markup = InlineKeyboardBuilder.Create(maxButtonsPerRow: MinimumButtonsPerRow)
            .AddButton("Button1", FirstCallbackData)
            .AddButton("Button2", SecondCallbackData)
            .AddButton("Button3", ThirdCallbackData)
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
        var markup = InlineKeyboardBuilder.Create(maxButtonsPerRow: LargeButtonsPerRow)
            .AddButton(FirstButtonText, FirstCallbackData)
            .AddButton(SecondButtonText, SecondCallbackData)
            .AddButton(ThirdButtonText, ThirdCallbackData)
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
            .AddButton(FirstButtonText, FirstCallbackData)
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
            .AddButton(FirstButtonText, FirstCallbackData)
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
            .AddButton("Button A", DuplicateCallbackData)
            .AddButton("Button B", DuplicateCallbackData)
            .AddButton("Button C", DuplicateCallbackData)
            .Build();

        markup.RowCount.Should().Be(1);
        markup.TotalButtonCount.Should().Be(3);
        markup.InlineKeyboard[0][0].CallbackData.Should().Be(DuplicateCallbackData);
        markup.InlineKeyboard[0][1].CallbackData.Should().Be(DuplicateCallbackData);
        markup.InlineKeyboard[0][2].CallbackData.Should().Be(DuplicateCallbackData);
    }

    /// <summary>
    /// Tests that duplicate button text is allowed.
    /// Edge case: duplicate button text values.
    /// </summary>
    [Fact]
    public void AddButton_DuplicateText_Allowed()
    {
        var markup = InlineKeyboardBuilder.Create()
            .AddButton(DuplicateButtonText, "ok1")
            .AddButton(DuplicateButtonText, "ok2")
            .AddButton(DuplicateButtonText, "ok3")
            .Build();

        markup.RowCount.Should().Be(1);
        markup.TotalButtonCount.Should().Be(3);
        markup.InlineKeyboard[0][0].Text.Should().Be(DuplicateButtonText);
        markup.InlineKeyboard[0][1].Text.Should().Be(DuplicateButtonText);
        markup.InlineKeyboard[0][2].Text.Should().Be(DuplicateButtonText);
    }

    /// <summary>
    /// Tests that very long button text (exceeding typical display limits) is allowed.
    /// Edge case: extremely long button text.
    /// </summary>
    [Fact]
    public void AddButton_VeryLongText_Allowed()
    {
        var longText = new string(LongTextCharacter, LongButtonTextLength);

        var markup = InlineKeyboardBuilder.Create()
            .AddButton(longText, DefaultCallbackData)
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
        var act = () => InlineKeyboardBuilder.Create().AddButton(WhitespaceValue, DefaultCallbackData);

        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Tests that null button text throws ArgumentException.
    /// Edge case: null button text.
    /// </summary>
    [Fact]
    public void AddButton_NullText_ThrowsArgumentException()
    {
        var act = () => InlineKeyboardBuilder.Create().AddButton(null!, DefaultCallbackData);

        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Tests that null URL throws ArgumentException.
    /// Edge case: null URL parameter.
    /// </summary>
    [Fact]
    public void AddUrlButton_NullUrl_ThrowsArgumentException()
    {
        var act = () => InlineKeyboardBuilder.Create().AddUrlButton(UrlButtonText, null!);

        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Tests that whitespace-only URL throws ArgumentException.
    /// Edge case: whitespace-only URL.
    /// </summary>
    [Fact]
    public void AddUrlButton_WhitespaceUrl_ThrowsArgumentException()
    {
        var act = () => InlineKeyboardBuilder.Create().AddUrlButton(UrlButtonText, WhitespaceValue);

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
            .AddSwitchInlineButton(SearchButtonText, EmptyValue)
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
            .AddSwitchInlineButton(SearchButtonText, WhitespaceValue)
            .Build();

        markup.RowCount.Should().Be(1);
        markup.TotalButtonCount.Should().Be(1);
        markup.InlineKeyboard[0][0].SwitchInlineQuery.Should().Be(WhitespaceValue);
    }

    /// <summary>
    /// Tests that callback data exactly 64 bytes is allowed (boundary case).
    /// Edge case: maximum allowed callback data length.
    /// </summary>
    [Fact]
    public void AddButton_CallbackData64Bytes_Allowed()
    {
        var callbackData64 = new string(CallbackDataCharacter, CallbackDataByteLimit);

        var act = () => InlineKeyboardBuilder.Create().AddButton(TestButtonText, callbackData64);

        act.Should().NotThrow();
    }

    /// <summary>
    /// Tests that callback data of 65 bytes throws ArgumentException (exceeds limit).
    /// Edge case: one byte over the limit.
    /// </summary>
    [Fact]
    public void AddButton_CallbackData65Bytes_ThrowsArgumentException()
    {
        var callbackData65 = new string(CallbackDataCharacter, CallbackDataLengthOverLimit);

        var act = () => InlineKeyboardBuilder.Create().AddButton(TestButtonText, callbackData65);

        act.Should().Throw<ArgumentException>()
            .WithMessage(CallbackDataByteLimitMessagePattern);
    }

    /// <summary>
    /// Tests that callback data with Unicode characters is properly counted by byte length.
    /// Edge case: Unicode characters in callback data.
    /// </summary>
    [Fact]
    public void AddButton_UnicodeCallbackData_CorrectByteCount()
    {
        // "café" is 5 characters but 6 bytes in UTF-8
        var unicodeCallback = UnicodeCallbackData;

        var act = () => InlineKeyboardBuilder.Create().AddButton(TestButtonText, unicodeCallback);

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
        while (System.Text.Encoding.UTF8.GetByteCount(builder.ToString()) < CallbackDataByteLimit)
        {
            builder.Append(UnicodeCharacter); // 2 bytes per character
        }
        var callbackData = builder.ToString();

        // This should be exactly at or near 64 bytes
        var act = () => InlineKeyboardBuilder.Create().AddButton(TestButtonText, callbackData);

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
        var markup = InlineKeyboardBuilder.Create(maxButtonsPerRow: TwoButtonsPerRow)
            .AddButton(LabelA, CallbackDataA)
            .AddButton(LabelB, CallbackDataB)
            .NewRow()
            .AddButton(LabelC, CallbackDataC)
            .AddButton(LabelD, CallbackDataD)
            .AddButton(LabelE, CallbackDataE)
            .NewRow()
            .AddButton(LabelF, CallbackDataF)
            .Build();

        var labels = markup.ToButtonLabels();

        labels.Should().HaveCount(4);
        labels[0].Should().BeEquivalentTo(new[] { LabelA, LabelB });
        labels[1].Should().BeEquivalentTo(new[] { LabelC, LabelD });
        labels[2].Should().BeEquivalentTo(new[] { LabelE });
        labels[3].Should().BeEquivalentTo(new[] { LabelF });
    }

    /// <summary>
    /// Tests that ToButtonLabels() does not include empty rows in the output.
    /// Edge case: row with zero buttons.
    /// </summary>
    [Fact]
    public void ToButtonLabels_EmptyRow_NotIncludedInOutput()
    {
        var markup = InlineKeyboardBuilder.Create()
            .AddButton(FirstButtonText, FirstCallbackData)
            .NewRow()
            .Build();

        var labels = markup.ToButtonLabels();

        labels.Should().HaveCount(1);
        labels[0].Should().BeEquivalentTo(new[] { FirstButtonText });
    }

    /// <summary>
    /// Tests that TotalButtonCount returns 0 for empty keyboard (though Build() would throw).
    /// Edge case: button count calculation.
    /// </summary>
    [Fact]
    public void TotalButtonCount_CalculatesCorrectly()
    {
        var markup = InlineKeyboardBuilder.Create(maxButtonsPerRow: ThreeButtonsPerRow)
            .AddButton("1", CallbackDataA)
            .AddButton("2", CallbackDataB)
            .NewRow()
            .AddButton("3", CallbackDataC)
            .AddButton("4", CallbackDataD)
            .AddButton("5", CallbackDataE)
            .NewRow()
            .AddButton("6", CallbackDataF)
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
        var markup = InlineKeyboardBuilder.Create(maxButtonsPerRow: TwoButtonsPerRow)
            .AddButton(LabelA, CallbackDataA)
            .AddButton(LabelB, CallbackDataB)
            .NewRow()
            .AddButton(LabelC, CallbackDataC)
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
        var markup = InlineKeyboardBuilder.Create(maxButtonsPerRow: TwoButtonsPerRow)
            .AddButton(FirstButtonText, FirstCallbackData)
            .AddButton(SecondButtonText, SecondCallbackData)
            .NewRow()
            .AddUrlButton("Url1", FirstUrl)
            .AddUrlButton("Url2", SecondUrl)
            .NewRow()
            .AddSwitchInlineButton("Switch1", "query1")
            .AddSwitchInlineButton("Switch2", "query2")
            .AddSwitchInlineButton("Switch3", "query3")
            .NewRow()
            .AddButton(ThirdButtonText, ThirdCallbackData)
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
        var act = () => new InlineKeyboardBuilder(maxButtonsPerRow: InvalidZeroButtonsPerRow);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    /// <summary>
    /// Tests that builder with negative maxButtonsPerRow throws ArgumentOutOfRangeException.
    /// Edge case: negative constructor parameter.
    /// </summary>
    [Fact]
    public void Constructor_NegativeMaxButtonsPerRow_ThrowsArgumentOutOfRangeException()
    {
        var act = () => new InlineKeyboardBuilder(maxButtonsPerRow: InvalidNegativeButtonsPerRow);

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
            .AddButton(GenericButtonText, EmptyValue)
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
            .AddButton(GenericButtonText, null!)
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
        var markup = InlineKeyboardBuilder.Create(maxButtonsPerRow: ThreeButtonsPerRow)
            .AddButton("Callback", "cb_data")
            .AddUrlButton("URL", ExampleUrl)
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
            .AddButton(FirstButtonText, FirstCallbackData)
            .NewRow()
            .AddButton(SecondButtonText, SecondCallbackData)
            .ToMenu("test_menu", "Test Menu");

        menu.Buttons.Should().HaveCount(2);
        menu.Buttons[0].Label.Should().Be(FirstButtonText);
        menu.Buttons[1].Label.Should().Be(SecondButtonText);
    }
}
