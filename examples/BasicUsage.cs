// =============================================================================
// Basic usage of the Telegram Bot Framework 3
// =============================================================================

using Microsoft.Extensions.DependencyInjection;
using TelegramBotFramework.Models;
using TelegramBotFramework.Services;

namespace TelegramBotFramework.Examples
{
	/// <summary>
	/// Provides basic usage examples for the Telegram Bot Framework.
	/// This class demonstrates how to register commands and configure the bot framework.
	/// </summary>
	public class BasicUsage
	{
	private readonly ICommandService _commandService;

		/// <summary>
		/// Initializes a new instance of the <see cref="BasicUsage"/> class.
		/// </summary>
		/// <param name="serviceProvider">The service provider used to resolve required services.</param>
		public BasicUsage(IServiceProvider serviceProvider)
		{
			_commandService = serviceProvider.GetRequiredService<ICommandService>();
		}

		/// <summary>
		/// Registers a minimal command that responds to the /hello command.
		/// This demonstrates the basic command registration pattern used in the framework.
		/// </summary>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		public async Task RegisterMinimalCommandAsync()
		{
			// A simple command registration
			var command = new Command
			{
				Name = "/hello",
				Description = "Says hello back",
				HandlerType = "HelloCommandHandler",
				IsEnabled = true
			};

			await _commandService.RegisterCommandAsync(command);
				}
	}
}
}