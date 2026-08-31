#nullable enable

namespace TelegramBotFramework.Examples;

/// <summary>
/// Constants for the HMAC callback examples.
/// </summary>
internal static class HmacCallbackExampleConstants
{
    public const string SimpleSecret = "my-secret-key-12345";
    public const string DeleteAccountLabel = "Delete Account";
    public const string DeleteAccountCallbackData = "delete_account:123";
    public const string CancelLabel = "Cancel";
    public const string CancelActionCallbackData = "cancel_action";

    public const string ConfirmationSecret = "secure-secret-key";
    public const int ExampleUserId = 42;
    public const string ConfirmPurchaseLabel = "✅ Confirm Purchase";
    public const string PurchaseCallbackDataFormat = "purchase:{0}";
    public const string CancelPurchaseLabel = "❌ Cancel";
    public const string CancelPurchaseCallbackData = "cancel";

    public const string PaginationSecret = "pagination-secret";
    public const int CurrentPage = 3;
    public const int TotalPages = 10;
    public const int PageStep = 1;
    public const string PreviousPageLabel = "⬅️ Previous";
    public const string PageCallbackDataFormat = "page_{0}";
    public const string CurrentPageLabelFormat = "📄 Page {0} of {1}";
    public const string CurrentPageCallbackDataFormat = "page_{0}_current";
    public const string NextPageLabel = "Next ➡️";

    public const string ExtensionSecret = "my-secret";
    public const string ApproveLabel = "Approve";
    public const string ApproveRequestCallbackData = "approve_request";
    public const string RejectLabel = "Reject";
    public const string RejectRequestCallbackData = "reject_request";

    public const string AuthenticCallbackMessageFormat = "Received authentic callback: {0}";
    public const string InvalidSignatureMessage = "⚠️ Invalid callback signature detected! Possible forgery attempt.";
    public const char CallbackDataSeparator = ':';
    public const int CommandPartIndex = 0;
    public const int ArgumentPartIndex = 1;
    public const string DeleteAccountCommand = "delete_account";
    public const string PurchaseCommand = "purchase";

    public const string MenuSecret = "menu-secret-key";
    public const string SettingsMenuId = "settings_menu";
    public const string ChangePasswordLabel = "🔒 Change Password";
    public const string ChangePasswordCallbackDataFormat = "change_password:{0}";
    public const string UpdateEmailLabel = "📧 Update Email";
    public const string UpdateEmailCallbackDataFormat = "update_email:{0}";
    public const string NotificationSettingsLabel = "📱 Notification Settings";
    public const string NotificationSettingsCallbackDataFormat = "notifications:{0}";
    public const string LanguageLabel = "🔄 Language";
    public const string LanguageCallbackDataFormat = "language:{0}";

    public const string BatchSecret = "batch-secret";
    public const string ActionOneLabel = "Action 1";
    public const string ActionOneCallbackData = "action1:param";
    public const string ActionTwoLabel = "Action 2";
    public const string ActionTwoCallbackData = "action2:param";
    public const string ActionThreeLabel = "Action 3";
    public const string ActionThreeCallbackData = "action3:param";

    public const string GuidCompactFormat = "N";
    public const string ConfigurationSecretPlaceholder = "your-secret-from-config";
    public const string HandlerInvalidSignatureMessage = "Security: Invalid callback signature detected!";
}
