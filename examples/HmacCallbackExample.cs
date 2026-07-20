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
        var secret = "my-secret-key-12345";

        var keyboard = InlineKeyboardBuilder.Create()
            .AddSignedButton("Delete Account", "delete_account:123", secret)
            .AddSignedButton("Cancel", "cancel_action", secret)
            .Build();

        // The callback data sent to Telegram will be:
        // "delete_account:123|{8-hex-chars}" and "cancel_action|{8-hex-chars}"
        // These cannot be forged without knowing the secret key
    }

    // Example 2: Signed confirmation dialog
    public static void Example2_SignedConfirmation()
    {
        var secret = "secure-secret-key";
        var userId = 42;

        var keyboard = InlineKeyboardBuilder.Create()
            .AddButton("✅ Confirm Purchase", CallbackDataSigner.Sign($"purchase:{userId}", secret))
            .AddButton("❌ Cancel", CallbackDataSigner.Sign("cancel", secret))
            .Build();
    }

    // Example 3: Signed pagination
    public static void Example3_SignedPagination()
    {
        var secret = "pagination-secret";
        var currentPage = 3;
        var totalPages = 10;

        var keyboard = InlineKeyboardBuilder.Create()
            .AddButton("⬅️ Previous", CallbackDataSigner.Sign($"page_{currentPage - 1}", secret))
            .AddButton($"📄 Page {currentPage} of {totalPages}", CallbackDataSigner.Sign($"page_{currentPage}_current", secret))
            .AddButton("Next ➡️", CallbackDataSigner.Sign($"page_{currentPage + 1}", secret))
            .Build();
    }

    // Example 4: Using extension methods for convenience
    public static void Example4_UsingExtensions()
    {
        var secret = "my-secret";

        var keyboard = InlineKeyboardBuilder.Create()
            .AddSignedConfirmationRow(secret) // Adds ✅ Confirm and ❌ Cancel with signatures
            .NewRow()
            .AddSignedButton("Approve", "approve_request", secret)
            .AddSignedButton("Reject", "reject_request", secret)
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
            Console.WriteLine($"Received authentic callback: {originalData}");

            // Parse and handle the original data
            var parts = originalData.Split(':');
            if (parts[0] == "delete_account")
            {
                var userId = parts[1];
                // Handle delete account logic
                return true;
            }

            return true;
        }
        else
        {
            // Validation failed - the callback may be forged!
            Console.WriteLine("⚠️ Invalid callback signature detected! Possible forgery attempt.");
            return false;
        }
    }

    // Example 6: Using with Menu system
    public static void Example6_MenuWithSignedCallbacks()
    {
        var secret = "menu-secret-key";
        var menuId = "settings_menu";

        // Create keyboard with signed buttons
        var keyboard = InlineKeyboardBuilder.Create()
            .AddSignedButton("🔒 Change Password", $"change_password:{menuId}", secret)
            .AddSignedButton("📧 Update Email", $"update_email:{menuId}", secret)
            .NewRow()
            .AddSignedButton("📱 Notification Settings", $"notifications:{menuId}", secret)
            .AddSignedButton("🔄 Language", $"language:{menuId}", secret)
            .Build();

        // Store menu with signed callbacks
        // menu.AddButton(...) would use the signed callback data
    }

    // Example 7: Batch signing multiple buttons
    public static void Example7_BatchSignedButtons()
    {
        var secret = "batch-secret";
        var actions = new[]
        {
            ("Action 1", "action1:param"),
            ("Action 2", "action2:param"),
            ("Action 3", "action3:param")
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
        var strongSecret = Guid.NewGuid().ToString("N"); // 32-char hex string

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
    private readonly string _hmacSecret = "your-secret-from-config";

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
            Console.WriteLine($"Security: Invalid callback signature detected!");
            // Optionally ban user or report to admin
        }
    }

    private async Task ProcessValidCallback(string callbackData)
    {
        var parts = callbackData.Split(':');
        var command = parts[0];

        switch (command)
        {
            case "delete_account":
                var userId = parts[1];
                await DeleteUserAccountAsync(userId);
                break;
            case "purchase":
                var purchaseId = parts[1];
                await ConfirmPurchaseAsync(purchaseId);
                break;
            // Handle other commands
        }
    }

    private Task DeleteUserAccountAsync(string userId) => Task.CompletedTask;
    private Task ConfirmPurchaseAsync(string purchaseId) => Task.CompletedTask;
}
