#nullable enable

using TelegramBotFramework.Keyboard;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBotFramework.Tests.Keyboard;

public interface IReplyKeyboardBuilderTests
{
    void Create_ShouldReturnNewBuilderInstance();
    void Create_WithMaxButtonsPerRow_ShouldSetMaxButtonsPerRow();
    void AddButton_WithText_ShouldAddButtonToCurrentRow();
    void AddButton_WithEmptyText_ShouldThrowArgumentException();
    void AddButton_WithConfigureAction_ShouldAddButtonWithConfiguration();
    void AddButton_WithConfigureAction_NullConfigure_ShouldThrowArgumentNullException();
    void NewRow_ShouldStartNewRow();
    void OneTime_ShouldSetOneTimeKeyboardToFalse();
    void Persistent_ShouldSetOneTimeKeyboardToTrue();
    void Resize_ShouldSetResizeKeyboardToTrue();
    void NoResize_ShouldSetResizeKeyboardToFalse();
    void Build_WithNoButtons_ShouldThrowInvalidOperationException();
    void Build_WithButtons_ShouldReturnReplyKeyboardMarkup();
    void Build_WithMaxButtonsPerRow_ShouldCreateMultipleRows();
    void Build_WithRequestContact_ShouldSetRequestContact();
    void Build_WithRequestLocation_ShouldSetRequestLocation();
    void Build_WithOneTime_ShouldSetOneTimeKeyboard();
    void Build_WithResize_ShouldSetResizeKeyboard();
    void ToMenu_ShouldConvertToMenu();
    void ToMenu_WithEmptyMenuId_ShouldThrowArgumentException();
}