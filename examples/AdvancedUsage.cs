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
					Name = AdvancedUsageConstants.AdminCommandName,
					Description = AdvancedUsageConstants.AdminCommandDescription,
					HandlerType = AdvancedUsageConstants.AdminCommandHandlerType,
					Type = CommandType.Standard,
					IsEnabled = true,
					RequiresAdmin = true,
					RateLimitPerMinute = AdvancedUsageConstants.AdminCommandRateLimitPerMinute,
					Parameters = new List<CommandParameter>
					{
						new CommandParameter
						{
							Name = AdvancedUsageConstants.ActionParameterName,
							Type = AdvancedUsageConstants.StringParameterType,
							IsRequired = true,
							Description = AdvancedUsageConstants.ActionParameterDescription
						}
					}
				};

				await _commandService.RegisterCommandAsync(command);
				_logger.LogInformation(AdvancedUsageConstants.AdminCommandRegisteredLogMessage);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, AdvancedUsageConstants.AdminCommandRegistrationFailedLogMessage);
				// Proper error handling
				throw new InvalidOperationException(AdvancedUsageConstants.RegistrationFailedExceptionMessage, ex);
			}
		}
	}
}
