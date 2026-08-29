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
/// Interface for edge-case tests of <see cref="TelegramBotFramework.Keyboard.InlineKeyboardBuilder"/> class.
/// </summary>
public interface IInlineKeyboardBuilderEdgeCaseTests
{
    void Build_EmptyBuilder_ThrowsInvalidOperationException();
    void Build_MaxButtonsPerRowOne_CreatesOneButtonPerRow();
    void Build_MaxButtonsPerRowLarge_AllButtonsInOneRow();
    void Build_AfterNewRowWithNoButtons_DoesNotCreateEmptyRow();
    void Build_MultipleConsecutiveNewRows_DoesNotCreateEmptyRows();
    void AddButton_DuplicateCallbackData_Allowed();
    void AddButton_DuplicateText_Allowed();
    void AddButton_VeryLongText_Allowed();
    void AddButton_WhitespaceText_ThrowsArgumentException();
    void AddButton_NullText_ThrowsArgumentException();
    void AddUrlButton_NullUrl_ThrowsArgumentException();
    void AddUrlButton_WhitespaceUrl_ThrowsArgumentException();
    void AddSwitchInlineButton_EmptyQuery_Allowed();
    void AddSwitchInlineButton_WhitespaceQuery_Allowed();
    void AddButton_CallbackData64Bytes_Allowed();
    void AddButton_CallbackData65Bytes_ThrowsArgumentException();
    void AddButton_UnicodeCallbackData_CorrectByteCount();
    void AddButton_UnicodeCallbackDataExceedsLimit_ThrowsArgumentException();
    void ToButtonLabels_MultiRowKeyboard_ReturnsCorrectStructure();
    void ToButtonLabels_EmptyRow_NotIncludedInOutput();
}