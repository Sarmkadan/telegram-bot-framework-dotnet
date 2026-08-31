#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TelegramBotFramework.Models;
using TelegramBotFramework.Services;

namespace TelegramBotFramework.Examples
{
    /// <summary>
    /// Interactive menu navigation example demonstrating nested menus, buttons, and navigation flows.
    /// Shows how to create rich user interfaces with inline keyboards and callback handling.
    /// </summary>
public sealed class MenuNavigationExample
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MenuNavigationExample> _logger;
        private readonly ISessionAndMenuService _sessionService;
        private readonly IUserService _userService;

        public MenuNavigationExample(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _logger = serviceProvider.GetRequiredService<ILogger<MenuNavigationExample>>();
            _sessionService = serviceProvider.GetRequiredService<ISessionAndMenuService>();
            _userService = serviceProvider.GetRequiredService<IUserService>();
        }

        public async Task RunAsync()
        {
            _logger.LogInformation("Starting MenuNavigationExample");

            try
            {
                var userId = MenuNavigationExampleConstants.ExampleUserId;
                var chatId = MenuNavigationExampleConstants.ExampleChatId;

                // Create user
                var user = await _userService.GetOrCreateUserAsync(userId, "John", "Doe").ConfigureAwait(false);

                // Create session for user
                var session = await _sessionService.CreateSessionAsync(userId, chatId).ConfigureAwait(false);
                _logger.LogInformation("Session created: {SessionId}", session.SessionId);

                // Build main menu
                var mainMenu = await CreateMainMenuAsync().ConfigureAwait(false);
                session.CurrentMenuId = mainMenu.Id;
                await _sessionService.UpdateSessionAsync(session).ConfigureAwait(false);

                _logger.LogInformation("Main menu created and set as current menu");

                // Simulate menu navigation
                await SimulateMenuNavigationAsync(session, mainMenu).ConfigureAwait(false);

                // Close session
                await _sessionService.CloseSessionAsync(session.SessionId).ConfigureAwait(false);
                _logger.LogInformation("Session closed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MenuNavigationExample");
                throw;
            }
        }

        private async Task<Menu> CreateMainMenuAsync()
        {
            var menu = new Menu
            {
                Id = MenuNavigationExampleConstants.MainMenuId,
                Title = MenuNavigationExampleConstants.MainMenuTitle,
                Description = MenuNavigationExampleConstants.MainMenuDescription,
                Type = MenuType.Inline,
                IsActive = true,
                MaxButtonsPerRow = MenuNavigationExampleConstants.MainMenuMaxButtonsPerRow
            };

            // Create buttons
            menu.AddButton(new MenuButton
            {
                Label = "📋 Settings",
                CallbackData = MenuNavigationExampleConstants.CallbackDataMenuSettings,
                Action = ButtonAction.NavigateMenu
            });

            menu.AddButton(new MenuButton
            {
                Label = "❓ Help",
                CallbackData = MenuNavigationExampleConstants.CallbackDataMenuHelp,
                Action = ButtonAction.NavigateMenu
            });

            menu.AddButton(new MenuButton
            {
                Label = MenuNavigationExampleConstants.ProfileMenuTitle,
                CallbackData = MenuNavigationExampleConstants.CallbackDataMenuProfile,
                Action = ButtonAction.NavigateMenu
            });

            menu.AddButton(new MenuButton
            {
                Label = "⚙️ Admin",
                CallbackData = MenuNavigationExampleConstants.CallbackDataMenuAdmin,
                Action = ButtonAction.NavigateMenu
            });

            menu.AddButton(new MenuButton
            {
                Label = "🚪 Exit",
                CallbackData = MenuNavigationExampleConstants.CallbackDataMenuExit,
                Action = ButtonAction.CloseMenu
            });

            await _sessionService.CreateMenuAsync(menu).ConfigureAwait(false);
            return menu;
        }

        private async Task<Menu> CreateSettingsMenuAsync()
        {
            var menu = new Menu
            {
                Id = MenuNavigationExampleConstants.SettingsMenuId,
                Title = MenuNavigationExampleConstants.SettingsMenuTitle,
                Description = MenuNavigationExampleConstants.SettingsMenuDescription,
                Type = MenuType.Inline,
                IsActive = true,
                MaxButtonsPerRow = MenuNavigationExampleConstants.SubMenuMaxButtonsPerRow
            };

            menu.AddButton(new MenuButton
            {
                Label = "🔔 Notifications",
                CallbackData = MenuNavigationExampleConstants.CallbackDataSettingsNotifications,
                Action = ButtonAction.NavigateMenu
            });

            menu.AddButton(new MenuButton
            {
                Label = "🌍 Language",
                CallbackData = MenuNavigationExampleConstants.CallbackDataSettingsLanguage,
                Action = ButtonAction.NavigateMenu
            });

            menu.AddButton(new MenuButton
            {
                Label = "🔒 Privacy",
                CallbackData = MenuNavigationExampleConstants.CallbackDataSettingsPrivacy,
                Action = ButtonAction.NavigateMenu
            });

            menu.AddButton(new MenuButton
            {
                Label = MenuNavigationExampleConstants.BackButtonLabel,
                CallbackData = MenuNavigationExampleConstants.CallbackDataMenuMain,
                Action = ButtonAction.NavigateMenu
            });

            await _sessionService.CreateMenuAsync(menu).ConfigureAwait(false);
            return menu;
        }

        private async Task<Menu> CreateProfileMenuAsync()
        {
            var menu = new Menu
            {
                Id = MenuNavigationExampleConstants.ProfileMenuId,
                Title = MenuNavigationExampleConstants.ProfileMenuTitle,
                Description = MenuNavigationExampleConstants.ProfileMenuDescription,
                Type = MenuType.Inline,
                IsActive = true,
                MaxButtonsPerRow = MenuNavigationExampleConstants.SubMenuMaxButtonsPerRow
            };

            menu.AddButton(new MenuButton
            {
                Label = "📝 Edit Name",
                CallbackData = MenuNavigationExampleConstants.CallbackDataProfileEditName,
                Action = ButtonAction.NavigateMenu
            });

            menu.AddButton(new MenuButton
            {
                Label = "📧 Edit Email",
                CallbackData = MenuNavigationExampleConstants.CallbackDataProfileEditEmail,
                Action = ButtonAction.NavigateMenu
            });

            menu.AddButton(new MenuButton
            {
                Label = "📊 Statistics",
                CallbackData = MenuNavigationExampleConstants.CallbackDataProfileStats,
                Action = ButtonAction.NavigateMenu
            });

            menu.AddButton(new MenuButton
            {
                Label = MenuNavigationExampleConstants.BackButtonLabel,
                CallbackData = MenuNavigationExampleConstants.CallbackDataMenuMain,
                Action = ButtonAction.NavigateMenu
            });

            await _sessionService.CreateMenuAsync(menu).ConfigureAwait(false);
            return menu;
        }

        private async Task SimulateMenuNavigationAsync(UserSession session, Menu mainMenu)
        {
            _logger.LogInformation("Simulating menu navigation");

            // Update session to settings menu
            var settingsMenu = await CreateSettingsMenuAsync().ConfigureAwait(false);
            session.CurrentMenuId = settingsMenu.Id;
            session.SetContextData(
                MenuNavigationExampleConstants.ContextKeyMenuHistory,
                MenuNavigationExampleConstants.ContextValueMenuHistoryMainSettings);
            await _sessionService.UpdateSessionAsync(session).ConfigureAwait(false);
            _logger.LogInformation("Navigated to settings menu");

            // Back to main menu
            session.CurrentMenuId = mainMenu.Id;
            session.SetContextData(
                MenuNavigationExampleConstants.ContextKeyMenuHistory,
                MenuNavigationExampleConstants.ContextValueMenuHistoryMain);
            await _sessionService.UpdateSessionAsync(session).ConfigureAwait(false);
            _logger.LogInformation("Navigated back to main menu");

            // Navigate to profile menu
            var profileMenu = await CreateProfileMenuAsync().ConfigureAwait(false);
            session.CurrentMenuId = profileMenu.Id;
            session.SetContextData(
                MenuNavigationExampleConstants.ContextKeyMenuHistory,
                MenuNavigationExampleConstants.ContextValueMenuHistoryMainProfile);
            await _sessionService.UpdateSessionAsync(session).ConfigureAwait(false);
            _logger.LogInformation("Navigated to profile menu");
        }
    }
}
