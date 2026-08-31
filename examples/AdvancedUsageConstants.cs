namespace TelegramBotFramework.Examples
{
 	/// <summary>
	/// Constants for advanced usage examples.
	/// </summary>
	internal static class AdvancedUsageConstants
	{
		public const string AdminCommandName = "/admin";
		public const string AdminCommandDescription = "Perform administrative tasks";
		public const string AdminCommandHandlerType = "AdminCommandHandler";
		public const string ActionParameterName = "action";
		public const string StringParameterType = "string";
		public const string ActionParameterDescription = "The action to perform (e.g., 'ban', 'mute')";
		public const string AdminCommandRegisteredLogMessage = "Admin command registered successfully.";
		public const string AdminCommandRegistrationFailedLogMessage = "Failed to register admin command.";
		public const string RegistrationFailedExceptionMessage = "Registration failed";
		public const int AdminCommandRateLimitPerMinute = 5;
	}
}
