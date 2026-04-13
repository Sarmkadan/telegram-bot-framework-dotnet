// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using TelegramBotFramework.Models;
using TelegramBotFramework.Services;

namespace TelegramBotFramework.Examples
{
    /// <summary>
    /// State management example showing how to handle complex user flows with form data,
    /// multi-step processes, and conversation state tracking.
    /// </summary>
    public class StateManagementExample
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<StateManagementExample> _logger;
        private readonly ISessionAndMenuService _sessionService;
        private readonly IUserService _userService;

        public StateManagementExample(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _logger = serviceProvider.GetRequiredService<ILogger<StateManagementExample>>();
            _sessionService = serviceProvider.GetRequiredService<ISessionAndMenuService>();
            _userService = serviceProvider.GetRequiredService<IUserService>();
        }

        public async Task RunAsync()
        {
            _logger.LogInformation("Starting StateManagementExample");

            try
            {
                var userId = 123456789L;
                var chatId = 123456789L;

                // Create user and session
                await _userService.GetOrCreateUserAsync(userId, "John", "Doe");
                var session = await _sessionService.CreateSessionAsync(userId, chatId);

                // Simulate a registration form flow
                await ProcessRegistrationFlowAsync(session);

                // Simulate a feedback survey flow
                await ProcessFeedbackSurveyAsync(session);

                // Close session
                await _sessionService.CloseSessionAsync(session.SessionId);
                _logger.LogInformation("Session closed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in StateManagementExample");
                throw;
            }
        }

        private async Task ProcessRegistrationFlowAsync(UserSession session)
        {
            _logger.LogInformation("Processing registration flow");

            // Initialize form data
            var formData = new RegistrationForm();
            session.SetContextData("registration_form", JsonSerializer.Serialize(formData));
            session.SetContextData("registration_step", "1");
            await _sessionService.UpdateSessionAsync(session);

            _logger.LogInformation("Step 1: Asking for first name");
            var step1Form = GetFormData<RegistrationForm>(session, "registration_form");
            step1Form.FirstName = "John";
            session.SetContextData("registration_form", JsonSerializer.Serialize(step1Form));
            session.SetContextData("registration_step", "2");
            await _sessionService.UpdateSessionAsync(session);

            _logger.LogInformation("Step 2: Asking for email");
            var step2Form = GetFormData<RegistrationForm>(session, "registration_form");
            step2Form.Email = "john@example.com";
            session.SetContextData("registration_form", JsonSerializer.Serialize(step2Form));
            session.SetContextData("registration_step", "3");
            await _sessionService.UpdateSessionAsync(session);

            _logger.LogInformation("Step 3: Asking for phone");
            var step3Form = GetFormData<RegistrationForm>(session, "registration_form");
            step3Form.PhoneNumber = "+1234567890";
            session.SetContextData("registration_form", JsonSerializer.Serialize(step3Form));
            session.SetContextData("registration_step", "complete");
            await _sessionService.UpdateSessionAsync(session);

            var finalForm = GetFormData<RegistrationForm>(session, "registration_form");
            _logger.LogInformation("Registration completed: {FirstName} {Email} {Phone}",
                finalForm.FirstName, finalForm.Email, finalForm.PhoneNumber);
        }

        private async Task ProcessFeedbackSurveyAsync(UserSession session)
        {
            _logger.LogInformation("Processing feedback survey");

            // Initialize survey data
            var surveyData = new FeedbackSurvey();
            session.SetContextData("survey_data", JsonSerializer.Serialize(surveyData));
            session.SetContextData("survey_step", "1");
            await _sessionService.UpdateSessionAsync(session);

            _logger.LogInformation("Question 1: How satisfied are you?");
            var step1Survey = GetFormData<FeedbackSurvey>(session, "survey_data");
            step1Survey.SatisfactionLevel = 5;
            session.SetContextData("survey_data", JsonSerializer.Serialize(step1Survey));
            session.SetContextData("survey_step", "2");
            await _sessionService.UpdateSessionAsync(session);

            _logger.LogInformation("Question 2: What could be improved?");
            var step2Survey = GetFormData<FeedbackSurvey>(session, "survey_data");
            step2Survey.ImprovementSuggestions = "Better user interface";
            session.SetContextData("survey_data", JsonSerializer.Serialize(step2Survey));
            session.SetContextData("survey_step", "3");
            await _sessionService.UpdateSessionAsync(session);

            _logger.LogInformation("Question 3: Would you recommend this?");
            var step3Survey = GetFormData<FeedbackSurvey>(session, "survey_data");
            step3Survey.WouldRecommend = true;
            session.SetContextData("survey_data", JsonSerializer.Serialize(step3Survey));
            session.SetContextData("survey_step", "complete");
            await _sessionService.UpdateSessionAsync(session);

            var finalSurvey = GetFormData<FeedbackSurvey>(session, "survey_data");
            _logger.LogInformation("Survey completed: Satisfaction={Level}, Recommend={Recommend}",
                finalSurvey.SatisfactionLevel, finalSurvey.WouldRecommend);
        }

        private T GetFormData<T>(UserSession session, string key) where T : class
        {
            var json = session.GetContextData(key);
            if (string.IsNullOrEmpty(json))
                return Activator.CreateInstance<T>();

            return JsonSerializer.Deserialize<T>(json) ?? Activator.CreateInstance<T>();
        }

        private class RegistrationForm
        {
            public string FirstName { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string PhoneNumber { get; set; } = string.Empty;
        }

        private class FeedbackSurvey
        {
            public int SatisfactionLevel { get; set; }
            public string ImprovementSuggestions { get; set; } = string.Empty;
            public bool WouldRecommend { get; set; }
        }
    }
}
