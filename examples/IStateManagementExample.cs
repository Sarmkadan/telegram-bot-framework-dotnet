using System.Threading.Tasks;

namespace TelegramBotFramework.Examples
{
    public interface IStateManagementExample
    {
        Task RunAsync();
        string FirstName { get; set; }
        string Email { get; set; }
        string PhoneNumber { get; set; }
        int SatisfactionLevel { get; set; }
        string ImprovementSuggestions { get; set; }
        bool WouldRecommend { get; set; }
    }
}
