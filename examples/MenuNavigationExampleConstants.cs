#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Examples
{
    /// <summary>
    /// Constants for MenuNavigationExample to avoid magic strings and numbers.
    /// </summary>
    internal static class MenuNavigationExampleConstants
    {
        // User and chat IDs
        public const long ExampleUserId = 123456789L;
        public const long ExampleChatId = 123456789L;

        // Menu IDs
        public const string MainMenuId = "main_menu";
        public const string SettingsMenuId = "settings_menu";
        public const string ProfileMenuId = "profile_menu";

        // Menu titles
        public const string MainMenuTitle = "👋 Welcome to Bot";
        public const string SettingsMenuTitle = "⚙️ Settings";
        public const string ProfileMenuTitle = "👤 Profile";
        public const string BackButtonLabel = "⬅️ Back";

        // Menu descriptions
        public const string MainMenuDescription = "Choose an option to get started";
        public const string SettingsMenuDescription = "Manage your preferences";
        public const string ProfileMenuDescription = "View and edit your profile";

        // Callback data values
        public const string CallbackDataMenuSettings = "menu:settings";
        public const string CallbackDataMenuHelp = "menu:help";
        public const string CallbackDataMenuProfile = "menu:profile";
        public const string CallbackDataMenuAdmin = "menu:admin";
        public const string CallbackDataMenuExit = "menu:exit";
        public const string CallbackDataSettingsNotifications = "settings:notifications";
        public const string CallbackDataSettingsLanguage = "settings:language";
        public const string CallbackDataSettingsPrivacy = "settings:privacy";
        public const string CallbackDataProfileEditName = "profile:edit_name";
        public const string CallbackDataProfileEditEmail = "profile:edit_email";
        public const string CallbackDataProfileStats = "profile:stats";
        public const string CallbackDataMenuMain = "menu:main";

        // Context data
        public const string ContextKeyMenuHistory = "menu_history";
        public const string ContextValueMenuHistoryMainSettings = "main_menu,settings_menu";
        public const string ContextValueMenuHistoryMain = "main_menu";
        public const string ContextValueMenuHistoryMainProfile = "main_menu,profile_menu";

        // UI settings
        public const int MainMenuMaxButtonsPerRow = 2;
        public const int SubMenuMaxButtonsPerRow = 1;
    }
}
