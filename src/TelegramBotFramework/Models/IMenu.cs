namespace TelegramBotFramework.Models;

/// <summary>
/// Represents an interactive menu interface in the bot.
/// </summary>
public interface IMenu
{
    string Id { get; set; }
    string Title { get; set; }
    string? Description { get; set; }
    MenuType Type { get; set; }
    List<MenuButton> Buttons { get; set; }
    bool IsActive { get; set; }
    int DisplayOrder { get; set; }
    DateTime CreatedAt { get; set; }
    DateTime UpdatedAt { get; set; }
    string? BackMenuId { get; set; }
    Dictionary<string, string>? Variables { get; set; }
    int MaxButtonsPerRow { get; set; }
    bool Validate();
    void AddButton(MenuButton button);
    bool RemoveButton(string callbackData);
    MenuButton? GetButton(string callbackData);
    void SetVariable(string key, string value);
    string? GetVariable(string key);
    List<List<MenuButton>> GetArrangedButtons();
}