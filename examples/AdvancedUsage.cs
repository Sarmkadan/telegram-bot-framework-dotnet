// =============================================================================
// Advanced usage of the Telegram Bot Framework
// =============================================================================

using Microsoft.Extensions.Logging;
using TelegramBotFramework.Models;
using TelegramBotFramework.Services;

namespace TelegramBotFramework.Examples
{
	/// <summary>
	/// Provides advanced usage examples for the Telegram Bot Framework, demonstrating
	/// complex command registration, error handling, and service integration scenarios.
	/// </summary>
	public class AdvancedUsage
	{
		private readonly ICommandService _commandService;
		private readonly ILogger<AdvancedUsage> _logger;

		/// <summary>
		/// Initializes a new instance of the <see cref="AdvancedUsage"/> class.
		/// </summary>
		/// <param name="commandService">The command service used for registering commands.</param>
		/// <param name="logger">The logger for recording operational information and errors.</param>
		public AdvancedUsage(ICommandService commandService, ILogger<AdvancedUsage> logger)
		{
			_commandService = commandService;
			_logger = logger;
		}

		public async Task RegisterComplexCommandAsync()
		{
			try
			{
				// A complex command with parameters, rate limiting and admin requirements
				var command = new Command
				{
					Name = "/admin",
					Description = "Perform administrative tasks",
					HandlerType = "AdminCommandHandler",
					Type = CommandType.Standard,
					IsEnabled = true,
					RequiresAdmin = true,
					RateLimitPerMinute = 5,
					Parameters = new List<CommandParameter>
					{
						new CommandParameter
						{
							Name = "action",
							Type = "string",
							IsRequired = true,
							Description = "The action to perform (e.g., 'ban', 'mute')"
						}
					}
				};

				await _commandService.RegisterCommandAsync(command);
				_logger.LogInformation("Admin command registered successfully.");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to register admin command.");
				// Proper error handling
				throw new InvalidOperationException("Registration failed", ex);
			}
		}
	}
}