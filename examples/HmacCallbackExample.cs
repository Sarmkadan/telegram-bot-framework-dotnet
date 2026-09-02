#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

// Example: Using HMAC signing for callback data security
// This demonstrates how to protect your bot from forged callback queries

// To use this example:
// 1. Add a secret key to your bot configuration (e.g., in appsettings.json)
// 2. Use AddSignedButton() or AddSignedButtons() instead of AddButton()
// 3. Validate incoming callbacks using CallbackDataSigner.TryValidate()

// Example configuration in appsettings.json:
// {
//   "BotConfiguration": {
//     "HmacSecret": "your-very-secure-secret-key-here"
//   }
// }

using TelegramBotFramework.Keyboard;
using TelegramBotFramework.Utilities;

namespace TelegramBotFramework.Examples;

/// <summary>
/// Provides examples of using HMAC signing for callback data security in Telegram bot framework.
/// </summary>
public static class HmacCallbackExample
{
    /// <summary>
    /// Demonstrates creating a simple inline keyboard with two HMAC-signed buttons.
    /// </summary>
    /// <remarks>
    /// The callback data for each button is signed using a secret key, preventing forgery.
    /// Example callback data format: "delete_account:123|{8-hex-chars}" and "cancel_action|{8-hex-chars}".
    /// </remarks>
    public static void Example1_SimpleSignedButton()
    {
        var secret = HmacCallbackExampleConstants.SimpleSecret;

        var keyboard = InlineKeyboardBuilder.Create()
            .AddSignedButton(HmacCallbackExampleConstants.DeleteAccountLabel, HmacCallbackExampleConstants.DeleteAccountCallbackData, secret)
            .AddSignedButton(HmacCallbackExampleConstants.CancelLabel, HmacCallbackExampleConstants.CancelActionCallbackData, secret)
            .Build();

        // The callback data sent to Telegram will be:
        // "delete_account:123|{8-hex-chars}" and "cancel_action|{8-hex-chars}"
        // These cannot be forged without knowing the secret key
    }

    /// <summary>
    /// Demonstrates creating a signed confirmation dialog with purchase and cancel buttons.
    /// </summary>
    /// <remarks>
    /// The confirmation button uses a formatted callback data string that includes a user ID,
    /// while the cancel button uses a static callback data string. Both are signed with the same secret.
    /// </remarks>
    public static void Example2_SignedConfirmation()
    {
        var secret = HmacCallbackExampleConstants.ConfirmationSecret;
        var userId = HmacCallbackExampleConstants.ExampleUserId;

        var keyboard = InlineKeyboardBuilder.Create()
            .AddButton(HmacCallbackExampleConstants.ConfirmPurchaseLabel, CallbackDataSigner.Sign(string.Format(HmacCallbackExampleConstants.PurchaseCallbackDataFormat, userId), secret))
            .AddButton(HmacCallbackExampleConstants.CancelPurchaseLabel, CallbackDataSigner.Sign(HmacCallbackExampleConstants.CancelPurchaseCallbackData, secret))
            .Build();
    }

    /// <summary>
    /// Demonstrates creating a paginated inline keyboard with HMAC-signed navigation buttons.
    /// </summary>
    /// <remarks>
    /// The keyboard includes buttons for previous page, current page display, and next page.
    /// All callback data is signed using a secret key to prevent tampering.
    /// </remarks>
    public static void Example3_SignedPagination()
    {
        var secret = HmacCallbackExampleConstants.PaginationSecret;
        var currentPage = HmacCallbackExampleConstants.CurrentPage;
        var totalPages = HmacCallbackExampleConstants.TotalPages;

        var keyboard = InlineKeyboardBuilder.Create()
            .AddButton(HmacCallbackExampleConstants.PreviousPageLabel, CallbackDataSigner.Sign(string.Format(HmacCallbackExampleConstants.PageCallbackDataFormat, currentPage - HmacCallbackExampleConstants.PageStep), secret))
            .AddButton(string.Format(HmacCallbackExampleConstants.CurrentPageLabelFormat, currentPage, totalPages), CallbackDataSigner.Sign(string.Format(HmacCallbackExampleConstants.CurrentPageCallbackDataFormat, currentPage), secret))
            .AddButton(HmacCallbackExampleConstants.NextPageLabel, CallbackDataSigner.Sign(string.Format(HmacCallbackExampleConstants.PageCallbackDataFormat, currentPage + HmacCallbackExampleConstants.PageStep), secret))
            .Build();
    }

    /// <summary>
    /// Demonstrates using extension methods for convenience when creating signed keyboards.
    /// </summary>
    /// <remarks>
    /// Shows how to use AddSignedConfirmationRow() to add pre-configured confirm/cancel buttons,
    /// and AddSignedButton() for custom actions like approve/reject.
    /// </remarks>
    public static void Example4_UsingExtensions()
    {
        var secret = HmacCallbackExampleConstants.ExtensionSecret;

        var keyboard = InlineKeyboardBuilder.Create()
            .AddSignedConfirmationRow(secret) // Adds ✅ Confirm and ❌ Cancel with signatures
            .NewRow()
            .AddSignedButton(HmacCallbackExampleConstants.ApproveLabel, HmacCallbackExampleConstants.ApproveRequestCallbackData, secret)
            .AddSignedButton(HmacCallbackExampleConstants.RejectLabel, HmacCallbackExampleConstants.RejectRequestCallbackData, secret)
            .Build();
    }

    /// <summary>
    /// Demonstrates how to validate an incoming signed callback query from Telegram.
    /// </summary>
    /// <param name="signedCallbackData">The callback data received from Telegram (including the HMAC signature).</param>
    /// <param name="secret">The secret key used to sign the callback data.</param>
    /// <returns>
    /// True if the callback data is authentic (signature valid) and corresponds to a known command;
    /// false if the signature is invalid or the data is tampered.
    /// </returns>
    /// <remarks>
    /// This method shows the pattern of validating the callback data, then parsing the original data
    /// to determine the action to take. In a real bot, you would replace the placeholder logic
    /// with actual command handling.
    /// </remarks>
    public static bool Example5_ValidateCallback(string signedCallbackData, string secret)
    {
        ArgumentException.ThrowIfNullOrEmpty(signedCallbackData);
        ArgumentException.ThrowIfNullOrEmpty(secret);
        // When your bot receives a callback query from Telegram:
        // callbackData = update.CallbackData

        if (CallbackDataSigner.TryValidate(signedCallbackData, secret, out var originalData))
        {
            // Validation successful - the data is authentic
            Console.WriteLine(HmacCallbackExampleConstants.AuthenticCallbackMessageFormat, originalData);

            // Parse and handle the original data
            var parts = originalData.Split(HmacCallbackExampleConstants.CallbackDataSeparator);
            if (parts[HmacCallbackExampleConstants.CommandPartIndex] == HmacCallbackExampleConstants.DeleteAccountCommand)
            {
                var userId = parts[HmacCallbackExampleConstants.ArgumentPartIndex];
                // Handle delete account logic
                return true;
            }

            return true;
        }
        else
        {
            // Validation failed - the callback may be forged!
            Console.WriteLine(HmacCallbackExampleConstants.InvalidSignatureMessage);
            return false;
        }
    }

    /// <summary>
    /// Demonstrates how to create a menu with HMAC-signed callback buttons.
    /// </summary>
    /// <remarks>
    /// This example shows creating a settings menu with buttons for changing password,
    /// updating email, notification settings, and language selection. All callback data
    /// is signed using a secret key to prevent forgery.
    /// </remarks>
    public static void Example6_MenuWithSignedCallbacks()
    {
        var secret = HmacCallbackExampleConstants.MenuSecret;
        var menuId = HmacCallbackExampleConstants.SettingsMenuId;

        // Create keyboard with signed buttons
        var keyboard = InlineKeyboardBuilder.Create()
            .AddSignedButton(HmacCallbackExampleConstants.ChangePasswordLabel, string.Format(HmacCallbackExampleConstants.ChangePasswordCallbackDataFormat, menuId), secret)
            .AddSignedButton(HmacCallbackExampleConstants.UpdateEmailLabel, string.Format(HmacCallbackExampleConstants.UpdateEmailCallbackDataFormat, menuId), secret)
            .NewRow()
            .AddSignedButton(HmacCallbackExampleConstants.NotificationSettingsLabel, string.Format(HmacCallbackExampleConstants.NotificationSettingsCallbackDataFormat, menuId), secret)
            .AddSignedButton(HmacCallbackExampleConstants.LanguageLabel, string.Format(HmacCallbackExampleConstants.LanguageCallbackDataFormat, menuId), secret)
            .Build();

        // Store menu with signed callbacks
        // menu.AddButton(...) would use the signed callback data
    }

    /// <summary>
    /// Demonstrates batch signing multiple buttons using the AddSignedButtons extension method.
    /// </summary>
    /// <remarks>
    /// This method shows how to sign multiple button callback data in a single call, which is
    /// more efficient than signing each button individually when you have many buttons.
    /// </remarks>
    public static void Example7_BatchSignedButtons()
    {
        var secret = HmacCallbackExampleConstants.BatchSecret;
        var actions = new[]
        {
            (HmacCallbackExampleConstants.ActionOneLabel, HmacCallbackExampleConstants.ActionOneCallbackData),
            (HmacCallbackExampleConstants.ActionTwoLabel, HmacCallbackExampleConstants.ActionTwoCallbackData),
            (HmacCallbackExampleConstants.ActionThreeLabel, HmacCallbackExampleConstants.ActionThreeCallbackData)
        };

        var keyboard = InlineKeyboardBuilder.Create()
            .AddSignedButtons(secret, actions)
            .Build();
    }

    /// <summary>
    /// Demonstrates security best practices for using HMAC signing in Telegram bots.
    /// </summary>
    /// <remarks>
    /// This example shows what not to do (using predictable secrets, hardcoding secrets) and what to do
    /// (using long random secrets, loading from configuration, rotating secrets).
    /// </remarks>
    public static void Example8_SecurityBestPractices()
    {
        // ❌ DON'T: Use predictable secrets
        // var weakSecret = "12345"; // BAD!

        // ✅ DO: Use long, random secrets
        var strongSecret = Guid.NewGuid().ToString(HmacCallbackExampleConstants.GuidCompactFormat); // 32-char hex string

        // ❌ DON'T: Store secret in code
        // var secret = "hardcoded_secret"; // BAD!

        // ✅ DO: Load from configuration
        // var secret = configuration["Bot:HmacSecret"];

        // ✅ DO: Rotate secrets periodically
        // Consider having a primary and secondary secret for rotation
    }
}

// Example usage in a bot handler:
/// <summary>
/// Example handler showing how to process validated callback queries in a bot.
/// </summary>
public class CallbackHandlerExample
{
    private readonly string _hmacSecret = HmacCallbackExampleConstants.ConfigurationSecretPlaceholder;

    public async Task HandleCallbackQueryAsync(string callbackData)
    {
        ArgumentException.ThrowIfNullOrEmpty(callbackData);
        if (CallbackDataSigner.TryValidate(callbackData, _hmacSecret, out var originalData))
        {
            // Process the authentic callback
            await ProcessValidCallback(originalData);
        }
        else
        {
            // Log security event and reject forged callback
            Console.WriteLine(HmacCallbackExampleConstants.HandlerInvalidSignatureMessage);
            // Optionally ban user or report to admin
        }
    }

    private async Task ProcessValidCallback(string callbackData)
    {
        var parts = callbackData.Split(HmacCallbackExampleConstants.CallbackDataSeparator);
        var command = parts[HmacCallbackExampleConstants.CommandPartIndex];

        switch (command)
        {
            case HmacCallbackExampleConstants.DeleteAccountCommand:
                var userId = parts[HmacCallbackExampleConstants.ArgumentPartIndex];
                await DeleteUserAccountAsync(userId);
                break;
            case HmacCallbackExampleConstants.PurchaseCommand:
                var purchaseId = parts[HmacCallbackExampleConstants.ArgumentPartIndex];
                await ConfirmPurchaseAsync(purchaseId);
                break;
            // Handle other commands
        }
    }

    private Task DeleteUserAccountAsync(string userId) => Task.CompletedTask;
    private Task ConfirmPurchaseAsync(string purchaseId) => Task.CompletedTask;
}
