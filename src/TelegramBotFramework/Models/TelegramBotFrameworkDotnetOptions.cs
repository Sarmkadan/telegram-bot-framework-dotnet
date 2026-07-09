using System.ComponentModel.DataAnnotations;

namespace TelegramBotFramework.Models
{
    public class TelegramBotFrameworkDotnetOptions
    {
        [Required]
        public string BotToken { get; set; } = null!;

        [Required]
        public string BotUsername { get; set; } = null!;

        [Url]
        public string? DatabaseConnectionString { get; set; }

        [Range(1, 60)]
        public int SessionTimeoutMinutes { get; set; } = 30;

        [Range(1, 300)]
        public int MessageProcessingTimeoutSeconds { get; set; } = 10;

        [Range(1, 100)]
        public int MaxConcurrentRequests { get; set; } = 10;

        public bool EnableLogging { get; set; } = true;

        public bool EnableRateLimiting { get; set; } = true;

        [Range(1, 600)]
        public int RateLimitPerMinute { get; set; } = 30;
    }
}
