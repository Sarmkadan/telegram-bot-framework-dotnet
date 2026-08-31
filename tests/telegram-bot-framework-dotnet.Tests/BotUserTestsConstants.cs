#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Tests;

/// <summary>
/// Shared values used by the model tests.
/// </summary>
internal static class BotUserTestsConstants
{
	public const string TestFirstName = "Test";
	public const string CommandHandlerType = "Handler";
	public const string BanCommandName = "/ban";
	public const string StartCommandName = "/start";
	public const string TestCommandName = "/test";
	public const string FloodCommandName = "/flood";
	public const string SessionId = "abc123";
	public const string ShortSessionId = "s1";
	public const string MainMenuId = "main";
	public const string CompactMenuId = "m";
	public const string CompactMenuTitle = "T";
	public const string FirstContextKey = "k1";
	public const string SecondContextKey = "k2";
	public const string CommandHistoryFormat = "/cmd{0}";

	public const long DefaultTelegramId = 1;
	public const long DefaultUserId = 1;
	public const long DefaultChatId = 1;
	public const long SessionChatId = 100;
	public const int SingleItemCount = 1;
	public const int TwoItemCount = 2;
	public const int ThreeItemCount = 3;
	public const int PastExpirationMinutes = -5;
	public const int FutureExpirationHours = 1;
	public const int RateLimitPerMinute = 10;
	public const int ExecutionsBelowRateLimit = 9;
	public const int UnlimitedExecutionCount = 9999;
	public const int CommandsAddedBeyondHistoryLimit = 55;
	public const int CommandHistoryLimit = 50;
	public const int FiveButtonCount = 5;
	public const int SixButtonCount = 6;
	public const int TwoButtonsPerRow = 2;
	public const int ThreeButtonsPerRow = 3;
	public const long CreatedUserTelegramId = 1001;
	public const long ExistingUserTelegramId = 2002;
	public const long DeletedUserTelegramId = 3003;
	public const long MissingUserTelegramId = 9999;
	public const long MissingDeletedUserTelegramId = 99999;
}
