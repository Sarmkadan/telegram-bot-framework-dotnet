// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Models;

/// <summary>
/// Represents an interactive menu interface in the bot.
/// </summary>
public class Menu
{
    public string Id { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public MenuType Type { get; set; } = MenuType.Inline;

    public List<MenuButton> Buttons { get; set; } = new();

    public bool IsActive { get; set; } = true;

    public int DisplayOrder { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public string? BackMenuId { get; set; }

    public Dictionary<string, string>? Variables { get; set; }

    public int MaxButtonsPerRow { get; set; } = 2;

    /// <summary>
    /// Validates the menu structure.
    /// </summary>
    public bool Validate()
    {
        if (string.IsNullOrWhiteSpace(Id))
            throw new InvalidOperationException("Menu Id is required");

        if (string.IsNullOrWhiteSpace(Title))
            throw new InvalidOperationException("Menu Title is required");

        if (Buttons.Count == 0)
            throw new InvalidOperationException("Menu must have at least one button");

        foreach (var button in Buttons)
        {
            if (string.IsNullOrWhiteSpace(button.Label))
                throw new InvalidOperationException("Button label cannot be empty");
        }

        return true;
    }

    /// <summary>
    /// Adds a button to the menu.
    /// </summary>
    public void AddButton(MenuButton button)
    {
        Buttons.Add(button);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Removes button by callback data.
    /// </summary>
    public bool RemoveButton(string callbackData)
    {
        var removed = Buttons.RemoveAll(b => b.CallbackData == callbackData) > 0;
        if (removed)
            UpdatedAt = DateTime.UtcNow;
        return removed;
    }

    /// <summary>
    /// Gets button by callback data.
    /// </summary>
    public MenuButton? GetButton(string callbackData) =>
        Buttons.FirstOrDefault(b => b.CallbackData == callbackData);

    /// <summary>
    /// Sets a variable for menu rendering.
    /// </summary>
    public void SetVariable(string key, string value)
    {
        Variables ??= new Dictionary<string, string>();
        Variables[key] = value;
    }

    /// <summary>
    /// Gets a variable value.
    /// </summary>
    public string? GetVariable(string key) =>
        Variables?.TryGetValue(key, out var value) == true ? value : null;

    /// <summary>
    /// Gets buttons arranged by rows.
    /// </summary>
    public List<List<MenuButton>> GetArrangedButtons()
    {
        var arranged = new List<List<MenuButton>>();
        var currentRow = new List<MenuButton>();

        foreach (var button in Buttons)
        {
            currentRow.Add(button);
            if (currentRow.Count >= MaxButtonsPerRow)
            {
                arranged.Add(currentRow);
                currentRow = new List<MenuButton>();
            }
        }

        if (currentRow.Count > 0)
            arranged.Add(currentRow);

        return arranged;
    }
}

public class MenuButton
{
    public string Label { get; set; } = string.Empty;

    public string CallbackData { get; set; } = string.Empty;

    public string? Url { get; set; }

    public ButtonAction Action { get; set; } = ButtonAction.Callback;

    public int DisplayOrder { get; set; }

    public bool IsVisible { get; set; } = true;

    public string? Icon { get; set; }

    public Dictionary<string, string>? Metadata { get; set; }
}

public enum MenuType
{
    Inline = 0,
    ReplyKeyboard = 1,
    Custom = 2
}

public enum ButtonAction
{
    Callback = 0,
    OpenUrl = 1,
    SwitchInline = 2,
    NavigateMenu = 3,
    ExecuteCommand = 4
}
