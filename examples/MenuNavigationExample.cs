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
                var userId = 123456789L;
                var chatId = 123456789L;

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
                Id = "main_menu",
                Title = "👋 Welcome to Bot",
                Description = "Choose an option to get started",
                Type = MenuType.Inline,
                IsActive = true,
                MaxButtonsPerRow = 2
            };

            // Create buttons
            menu.AddButton(new MenuButton
            {
                Label = "📋 Settings",
                CallbackData = "menu:settings",
                Action = ButtonAction.NavigateMenu
            });

            menu.AddButton(new MenuButton
            {
                Label = "❓ Help",
                CallbackData = "menu:help",
                Action = ButtonAction.NavigateMenu
            });

            menu.AddButton(new MenuButton
            {
                Label = "👤 Profile",
                CallbackData = "menu:profile",
                Action = ButtonAction.NavigateMenu
            });

            menu.AddButton(new MenuButton
            {
                Label = "⚙️ Admin",
                CallbackData = "menu:admin",
                Action = ButtonAction.NavigateMenu
            });

            menu.AddButton(new MenuButton
            {
                Label = "🚪 Exit",
                CallbackData = "menu:exit",
                Action = ButtonAction.CloseMenu
            });

            await _sessionService.CreateMenuAsync(menu).ConfigureAwait(false);
            return menu;
        }

        private async Task<Menu> CreateSettingsMenuAsync()
        {
            var menu = new Menu
            {
                Id = "settings_menu",
                Title = "⚙️ Settings",
                Description = "Manage your preferences",
                Type = MenuType.Inline,
                IsActive = true,
                MaxButtonsPerRow = 1
            };

            menu.AddButton(new MenuButton
            {
                Label = "🔔 Notifications",
                CallbackData = "settings:notifications",
                Action = ButtonAction.NavigateMenu
            });

            menu.AddButton(new MenuButton
            {
                Label = "🌍 Language",
                CallbackData = "settings:language",
                Action = ButtonAction.NavigateMenu
            });

            menu.AddButton(new MenuButton
            {
                Label = "🔒 Privacy",
                CallbackData = "settings:privacy",
                Action = ButtonAction.NavigateMenu
            });

            menu.AddButton(new MenuButton
            {
                Label = "⬅️ Back",
                CallbackData = "menu:main",
                Action = ButtonAction.NavigateMenu
            });

            await _sessionService.CreateMenuAsync(menu).ConfigureAwait(false);
            return menu;
        }

        private async Task<Menu> CreateProfileMenuAsync()
        {
            var menu = new Menu
            {
                Id = "profile_menu",
                Title = "👤 Profile",
                Description = "View and edit your profile",
                Type = MenuType.Inline,
                IsActive = true,
                MaxButtonsPerRow = 1
            };

            menu.AddButton(new MenuButton
            {
                Label = "📝 Edit Name",
                CallbackData = "profile:edit_name",
                Action = ButtonAction.NavigateMenu
            });

            menu.AddButton(new MenuButton
            {
                Label = "📧 Edit Email",
                CallbackData = "profile:edit_email",
                Action = ButtonAction.NavigateMenu
            });

            menu.AddButton(new MenuButton
            {
                Label = "📊 Statistics",
                CallbackData = "profile:stats",
                Action = ButtonAction.NavigateMenu
            });

            menu.AddButton(new MenuButton
            {
                Label = "⬅️ Back",
                CallbackData = "menu:main",
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
            session.SetContextData("menu_history", "main_menu,settings_menu");
            await _sessionService.UpdateSessionAsync(session).ConfigureAwait(false);
            _logger.LogInformation("Navigated to settings menu");

            // Back to main menu
            session.CurrentMenuId = mainMenu.Id;
            session.SetContextData("menu_history", "main_menu");
            await _sessionService.UpdateSessionAsync(session).ConfigureAwait(false);
            _logger.LogInformation("Navigated back to main menu");

            // Navigate to profile menu
            var profileMenu = await CreateProfileMenuAsync().ConfigureAwait(false);
            session.CurrentMenuId = profileMenu.Id;
            session.SetContextData("menu_history", "main_menu,profile_menu");
            await _sessionService.UpdateSessionAsync(session).ConfigureAwait(false);
            _logger.LogInformation("Navigated to profile menu");
        }
    }
}