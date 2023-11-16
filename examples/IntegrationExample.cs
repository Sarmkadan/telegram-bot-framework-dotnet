// =============================================================================
// ASP.NET Core Integration Example
// =============================================================================

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using TelegramBotFramework.Configuration;
using TelegramBotFramework.Models;

namespace TelegramBotFramework.Examples
{
    public static class IntegrationExample
    {
        public static void Setup(WebApplicationBuilder builder)
        {
            // 1. Configure the BotConfiguration
            var botConfig = new BotConfiguration
            {
                ApiKey = "YOUR_TELEGRAM_API_KEY",
                WebhookUrl = "https://your-domain.com/webhook"
            };

            // 2. Wire up the framework in DI
            builder.Services.AddTelegramBotFramework(botConfig);
            
            // 3. Add controllers to handle API/Webhook requests
            builder.Services.AddControllers();
        }
    }
}
