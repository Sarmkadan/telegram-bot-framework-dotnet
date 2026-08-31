#nullable enable

namespace TelegramBotFramework.Tests;

internal static class InlineKeyboardBuilderTestsConstants
{
	public const string ClickButtonText = "Click me";
	public const string ClickCallbackData = "click";
	public const string VisitButtonText = "Visit";
	public const string ExampleUrl = "https://example.com";
	public const string SearchButtonText = "Search";
	public const string SearchQuery = "my query";
	public const string FirstButtonText = "A";
	public const string FirstCallbackData = "a";
	public const string SecondButtonText = "B";
	public const string SecondCallbackData = "b";
	public const string ThirdButtonText = "C";
	public const string ThirdCallbackData = "c";
	public const string YesButtonText = "Yes";
	public const string YesCallbackData = "yes";
	public const string NoButtonText = "No";
	public const string NoCallbackData = "no";
	public const string HelpButtonText = "Help";
	public const string HelpCallbackData = "help";
	public const string DocumentationButtonText = "Docs";
	public const string DocumentationUrl = "https://docs.example.com";
	public const string MainMenuId = "main_menu";
	public const string MainMenuTitle = "Main Menu";
	public const string EmptyKeyboardMessagePattern = "*empty keyboard*";
	public const string TestButtonText = "Test";
	public const string DefaultCallbackData = "data";
	public const string CallbackDataByteLimitMessagePattern = "*64*byte*";
	public const char CallbackDataCharacter = 'x';
	public const int FirstIndex = 0;
	public const int SecondIndex = 1;
	public const int SingleItemCount = 1;
	public const int TwoItemCount = 2;
	public const int TwoButtonsPerRow = 2;
	public const int ThreeButtonsPerRow = 3;
	public const int CallbackDataLengthOverLimit = 65;
}
