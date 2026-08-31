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

public static class HmacCallbackExample
{
    // Example 1: Simple signed button
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

    // Example 2: Signed confirmation dialog
    public static void Example2_SignedConfirmation()
    {
        var secret = HmacCallbackExampleConstants.ConfirmationSecret;
        var userId = HmacCallbackExampleConstants.ExampleUserId;

        var keyboard = InlineKeyboardBuilder.Create()
            .AddButton(HmacCallbackExampleConstants.ConfirmPurchaseLabel, CallbackDataSigner.Sign(string.Format(HmacCallbackExampleConstants.PurchaseCallbackDataFormat, userId), secret))
            .AddButton(HmacCallbackExampleConstants.CancelPurchaseLabel, CallbackDataSigner.Sign(HmacCallbackExampleConstants.CancelPurchaseCallbackData, secret))
            .Build();
    }

    // Example 3: Signed pagination
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

    // Example 4: Using extension methods for convenience
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

    // Example 5: Validating incoming callback queries
    public static bool Example5_ValidateCallback(string signedCallbackData, string secret)
    {
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

    // Example 6: Using with Menu system
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

    // Example 7: Batch signing multiple buttons
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

    // Example 8: Security considerations
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
public class CallbackHandlerExample
{
    private readonly string _hmacSecret = HmacCallbackExampleConstants.ConfigurationSecretPlaceholder;

    public async Task HandleCallbackQueryAsync(string callbackData)
    {
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
