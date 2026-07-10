# Menu

Represents a navigable menu in a Telegram bot conversation, containing buttons, layout rules, and runtime variables used to render dynamic content.

## API

### `public string Id`
Unique identifier for the menu. Used to reference the menu when navigating between menus or setting variables. Must be non-null and non-empty.

### `public string Title`
Display title shown at the top of the rendered menu. Intended for user-facing presentation.

### `public string? Description`
Optional descriptive text displayed below the title. Can be null if no description is needed.

### `public MenuType Type`
Categorizes the menu behavior (e.g., main, settings, help). Affects rendering and navigation logic.

### `public List<MenuButton> Buttons`
Collection of buttons displayed in the menu. Buttons are grouped into rows based on `MaxButtonsPerRow`. Must not be null.

### `public bool IsActive`
Indicates whether the menu is currently available for navigation. Inactive menus are not rendered or accessible.

### `public int DisplayOrder`
Determines the order in which menus are presented in navigation lists. Lower values appear first.

### `public DateTime CreatedAt`
Timestamp of when the menu was first created. Immutable after initialization.

### `public DateTime UpdatedAt`
Timestamp of the last modification to the menu. Updated whenever buttons, variables, or metadata change.

### `public string? BackMenuId`
Identifier of the menu to return to when a back button is pressed. Can be null if no back navigation is defined.

### `public Dictionary<string, string>? Variables`
Optional key-value store for dynamic content. Used to substitute placeholders in button labels or descriptions at runtime. Can be null.

### `public int MaxButtonsPerRow`
Maximum number of buttons allowed in a single row when rendering. Affects layout and pagination.

### `public bool Validate`
Flag indicating whether the menu should be validated for structural or logical errors upon rendering. Enables strict checks when true.

### `public void AddButton(MenuButton button)`
Adds a button to the `Buttons` list. Throws `ArgumentNullException` if `button` is null.

### `public bool RemoveButton(string buttonId)`
Removes the first button with the specified `buttonId`. Returns `true` if a button was removed; otherwise, `false`. Buttons are identified by their `Id` property.

### `public MenuButton? GetButton(string buttonId)`
Retrieves the button with the specified `buttonId`, or `null` if not found.

### `public void SetVariable(string key, string value)`
Stores a variable in the `Variables` dictionary. If `Variables` is null, initializes it. Throws `ArgumentNullException` if `key` is null.

### `public string? GetVariable(string key)`
Retrieves the value associated with `key` from `Variables`, or `null` if the key does not exist or `Variables` is null.

### `public List<List<MenuButton>> GetArrangedButtons()`
Returns the buttons grouped into rows based on `MaxButtonsPerRow`. Each inner list represents a row. Buttons are arranged in the order they appear in `Buttons`.

### `public string Label`
Display label for the menu, typically derived from `Title` or used for debugging. Must be non-null and non-empty.

## Usage
