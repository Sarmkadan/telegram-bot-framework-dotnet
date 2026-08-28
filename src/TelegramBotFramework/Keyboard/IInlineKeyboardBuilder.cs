#nullable enable
namespace TelegramBotFramework.Keyboard;

/// <summary>
/// Interface for fluent builder constructing Telegram inline keyboard markups.
/// </summary>
public interface IInlineKeyboardBuilder
{
    InlineKeyboardBuilder AddButton(string text, string callbackData);
    InlineKeyboardBuilder AddUrlButton(string text, string url);
    InlineKeyboardBuilder AddSwitchInlineButton(string text, string query = "");
    InlineKeyboardBuilder NewRow();
    InlineKeyboardMarkup Build();
    Models.Menu ToMenu(string menuId, string title);
    IReadOnlyList<IReadOnlyList<InlineKeyboardButton>> InlineKeyboard { get; }
}