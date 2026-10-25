# MenuBuilder

Fluent builder for `Menu`. Each `With*` method sets one property and returns the same builder, and `Build()` checks that the menu has an ID, a title, and at least one button before returning a new `Menu`.

Namespace: `TelegramBotFramework.Models`
Visibility: `public sealed class`

## Purpose

`Menu` is a plain class with settable properties. It can be created with an object initializer, but nothing checks that `Id`, `Title`, or `Buttons` were filled in. `MenuBuilder` sets the same values through a chain of `With*` calls, and `Build()` rejects an incomplete menu before it is used to render a keyboard.

Use the builder when you want to:

- Create a menu in one expression, for example when defining the bot's navigation in startup code or in tests.
- Copy an existing menu and change a few values with `From(...)`.
- Get an early failure from `Build()` when the ID, title, or buttons are missing.

## Creating a builder

The constructor is public and takes no arguments. A new builder starts with these values:

| Property | Initial value |
|----------|---------------|
| `_id`, `_title` | `string.Empty` (invalid until set) |
| `_buttons` | An empty list (invalid until set) |
| `_type` | `MenuType.Inline` |
| `_isActive` | `true` |
| `_displayOrder` | `0` |
| `_createdAt`, `_updatedAt` | `DateTime.UtcNow` at construction |
| `_description`, `_backMenuId` | `null` |

Start from an existing menu with the static method `From(Menu template)`. It copies the template's values into a new builder and throws `ArgumentNullException` if `template` is `null`. The template is not modified. `From` copies `CreatedAt` and `UpdatedAt` as they are; it does not change `UpdatedAt` to the current time.

## Builder methods

Every `With*` method returns the same builder instance, so calls chain.

| Method | Parameter | Stored as | Checked when |
|--------|-----------|-----------|--------------|
| `WithId(string id)` | Menu identifier | `Id` | Immediately: throws `ArgumentException` if `null` or empty. |
| `WithTitle(string title)` | Menu title | `Title` | Immediately: throws `ArgumentException` if `null` or empty. |
| `WithDescription(string? description)` | Description, or `null` | `Description` | Not checked. |
| `WithType(MenuType type)` | Menu type | `Type` | Not checked. |
| `WithButtons(IEnumerable<MenuButton> buttons)` | Buttons | `Buttons` | Immediately: throws `ArgumentNullException` if `null`. |
| `WithIsActive(bool isActive)` | Active flag | `IsActive` | Not checked. |
| `WithDisplayOrder(int displayOrder)` | Display order | `DisplayOrder` | Not checked. |
| `WithCreatedAt(DateTime createdAt)` | Creation timestamp | `CreatedAt` | Not checked. |
| `WithUpdatedAt(DateTime updatedAt)` | Last update timestamp | `UpdatedAt` | Not checked. |
| `WithBackMenuId(string? backMenuId)` | ID of the back menu, or `null` | `BackMenuId` | Not checked. |

Only `WithId`, `WithTitle`, and `WithButtons` check their input when you call them. `WithId` and `WithTitle` reject `null` and empty strings but accept whitespace. The stricter checks run in `Build()`.

`WithButtons` copies the sequence into a new list, so later changes to the source collection do not affect the builder.

## Static method

```csharp
public static MenuBuilder From(Menu template)
```

Copies `Id`, `Title`, `Description`, `Type`, `Buttons`, `IsActive`, `DisplayOrder`, `CreatedAt`, `UpdatedAt`, and `BackMenuId` from `template`. The buttons are copied into a new list, so the builder does not share the template's list.

`From` does not copy `Variables` or `MaxButtonsPerRow`, because the builder has no methods for them. A menu created with `From(...).Build()` therefore has the default values for those two properties.

## Build

```csharp
public Menu Build()
```

`Build()` runs these checks in order and throws `ArgumentException` on the first failure:

1. `_id` must not be `null`, empty, or whitespace. The message is `Menu Id is required`.
2. `_title` must not be `null`, empty, or whitespace. The message is `Menu Title is required`.
3. `_buttons` must contain at least one button. The message is `Menu must have at least one button`.

The exception's parameter name is the builder's internal field name (`_id`, `_title`, or `_buttons`). Use the message text to tell the checks apart.

If all checks pass, `Build()` returns a new `Menu`. The returned menu uses the builder's button list directly, not a copy. Calling `WithButtons` again on the same builder replaces the reference, so the earlier menu keeps its own list.

### What Build does not check

`Build()` does not call `Menu.Validate()`. It does not check the following:

- Button labels. `Menu.Validate()` rejects empty labels.
- Callback data length. Telegram limits `callback_data` to 64 UTF-8 bytes (`Menu.MaxCallbackDataBytes`). `Menu.AddButton` enforces this, but `Build()` does not check buttons that were passed to `WithButtons`.

To check these, call `Validate()` on the built menu.

## Usage example

```csharp
using TelegramBotFramework.Models;

var settingsMenu = new MenuBuilder()
    .WithId("settings")
    .WithTitle("Settings")
    .WithDescription("Choose a section to configure.")
    .WithType(MenuType.Inline)
    .WithButtons(new[]
    {
        new MenuButton { Label = "Language", CallbackData = "settings:language" },
        new MenuButton { Label = "Notifications", CallbackData = "settings:notifications" },
        new MenuButton { Label = "Back", CallbackData = "menu:main", Action = ButtonAction.NavigateMenu }
    })
    .WithBackMenuId("main")
    .Build();

settingsMenu.Validate();

// Copy the menu and change only the title.
var renamedMenu = MenuBuilder.From(settingsMenu)
    .WithTitle("Preferences")
    .Build();
```

`Build()` throws `ArgumentException` if the ID, title, or button list is missing:

```csharp
// Throws ArgumentException: "Menu must have at least one button"
var empty = new MenuBuilder()
    .WithId("empty")
    .WithTitle("Empty")
    .Build();
```
