# HmacCallbackExample

Utility class demonstrating HMAC-based callback signing for Telegram Bot API callbacks. Provides methods to generate signed callback data, validate incoming callbacks, and build interactive elements with signed callbacks to prevent tampering.

## API

### `public static void Example1_SimpleSignedButton`
Demonstrates creating a simple inline keyboard button with a signed callback data payload. The callback data includes a timestamp and HMAC signature to ensure integrity. No parameters are required; the example uses default signing settings.

### `public static void Example2_SignedConfirmation`
Shows how to generate a signed callback for a confirmation action (e.g., "Confirm Order"). The generated callback can be used in an inline button and later validated to ensure the confirmation originated from the bot and was not tampered with.

### `public static void Example3_SignedPagination`
Illustrates creating signed pagination buttons (e.g., "Next Page", "Previous Page") using HMAC. Each button includes a page identifier and signature, allowing safe navigation without server-side session storage.

### `public static void Example4_UsingExtensions`
Introduces extension methods for signing callback data directly on `InlineKeyboardButton` or `InlineKeyboardMarkup` instances. Simplifies integration by reducing boilerplate when building interactive messages.

### `public static bool Example5_ValidateCallback`
Validates an incoming callback query from Telegram. Returns `true` if the callback data is correctly signed and not expired; otherwise returns `false`. Throws `ArgumentException` if the callback data is malformed or missing required fields.

### `public static void Example6_MenuWithSignedCallbacks`
Builds a complete interactive menu using multiple signed callback buttons. Each button is signed with a shared secret, enabling secure user interaction without storing state on the server.

### `public static void Example7_BatchSignedButtons`
Demonstrates generating a batch of signed buttons with consistent signing parameters. Useful for large menus or dynamic content where multiple buttons share a common signing context.

### `public static void Example8_SecurityBestPractices`
Outlines recommended practices for using HMAC signing securely: rotating secrets, validating expiration, avoiding predictable payloads, and handling signature failures gracefully.

### `public async Task HandleCallbackQueryAsync`
Asynchronous handler for processing incoming callback queries. Validates the callback using `Example5_ValidateCallback`, extracts payload data, and dispatches actions accordingly. Does not throw for invalid callbacks—logs and ignores them instead.

## Usage
