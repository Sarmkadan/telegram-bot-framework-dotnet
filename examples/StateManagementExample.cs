#nullable enable
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
public sealed class StateManagementExample : IStateManagementExample, IEquatable<StateManagementExample>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<StateManagementExample> _logger;
        private readonly ISessionAndMenuService _sessionService;
        private readonly IUserService _userService;

        public string FirstName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public int SatisfactionLevel { get; set; }
        public string ImprovementSuggestions { get; set; } = string.Empty;
        public bool WouldRecommend { get; set; }

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
                var userId = StateManagementExampleConstants.ExampleUserId;
                var chatId = StateManagementExampleConstants.ExampleChatId;

                // Create user and session
                await _userService.GetOrCreateUserAsync(userId, "John", "Doe").ConfigureAwait(false);
                var session = await _sessionService.CreateSessionAsync(userId, chatId).ConfigureAwait(false);

                // Simulate a registration form flow
                await ProcessRegistrationFlowAsync(session).ConfigureAwait(false);

                // Simulate a feedback survey flow
                await ProcessFeedbackSurveyAsync(session).ConfigureAwait(false);

                // Close session
                await _sessionService.CloseSessionAsync(session.SessionId).ConfigureAwait(false);
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
            session.SetContextData(StateManagementExampleConstants.RegistrationFormContextKey, JsonSerializer.Serialize(formData));
            session.SetContextData(StateManagementExampleConstants.RegistrationStepContextKey, StateManagementExampleConstants.FirstStep);
            await _sessionService.UpdateSessionAsync(session).ConfigureAwait(false);

            _logger.LogInformation("Step 1: Asking for first name");
            var step1Form = GetFormData<RegistrationForm>(session, StateManagementExampleConstants.RegistrationFormContextKey);
            step1Form.FirstName = "John";
            session.SetContextData(StateManagementExampleConstants.RegistrationFormContextKey, JsonSerializer.Serialize(step1Form));
            session.SetContextData(StateManagementExampleConstants.RegistrationStepContextKey, StateManagementExampleConstants.SecondStep);
            await _sessionService.UpdateSessionAsync(session).ConfigureAwait(false);

            _logger.LogInformation("Step 2: Asking for email");
            var step2Form = GetFormData<RegistrationForm>(session, StateManagementExampleConstants.RegistrationFormContextKey);
            step2Form.Email = "john@example.com";
            session.SetContextData(StateManagementExampleConstants.RegistrationFormContextKey, JsonSerializer.Serialize(step2Form));
            session.SetContextData(StateManagementExampleConstants.RegistrationStepContextKey, StateManagementExampleConstants.ThirdStep);
            await _sessionService.UpdateSessionAsync(session).ConfigureAwait(false);

            _logger.LogInformation("Step 3: Asking for phone");
            var step3Form = GetFormData<RegistrationForm>(session, StateManagementExampleConstants.RegistrationFormContextKey);
            step3Form.PhoneNumber = "+1234567890";
            session.SetContextData(StateManagementExampleConstants.RegistrationFormContextKey, JsonSerializer.Serialize(step3Form));
            session.SetContextData(StateManagementExampleConstants.RegistrationStepContextKey, StateManagementExampleConstants.CompleteStep);
            await _sessionService.UpdateSessionAsync(session).ConfigureAwait(false);

            var finalForm = GetFormData<RegistrationForm>(session, StateManagementExampleConstants.RegistrationFormContextKey);
            _logger.LogInformation("Registration completed: {FirstName} {Email} {Phone}",
                finalForm.FirstName, finalForm.Email, finalForm.PhoneNumber);
        }

        private async Task ProcessFeedbackSurveyAsync(UserSession session)
        {
            _logger.LogInformation("Processing feedback survey");

            // Initialize survey data
            var surveyData = new FeedbackSurvey();
            session.SetContextData(StateManagementExampleConstants.SurveyDataContextKey, JsonSerializer.Serialize(surveyData));
            session.SetContextData(StateManagementExampleConstants.SurveyStepContextKey, StateManagementExampleConstants.FirstStep);
            await _sessionService.UpdateSessionAsync(session).ConfigureAwait(false);

            _logger.LogInformation("Question 1: How satisfied are you?");
            var step1Survey = GetFormData<FeedbackSurvey>(session, StateManagementExampleConstants.SurveyDataContextKey);
            step1Survey.SatisfactionLevel = StateManagementExampleConstants.MaximumSatisfactionLevel;
            session.SetContextData(StateManagementExampleConstants.SurveyDataContextKey, JsonSerializer.Serialize(step1Survey));
            session.SetContextData(StateManagementExampleConstants.SurveyStepContextKey, StateManagementExampleConstants.SecondStep);
            await _sessionService.UpdateSessionAsync(session).ConfigureAwait(false);

            _logger.LogInformation("Question 2: What could be improved?");
            var step2Survey = GetFormData<FeedbackSurvey>(session, StateManagementExampleConstants.SurveyDataContextKey);
            step2Survey.ImprovementSuggestions = "Better user interface";
            session.SetContextData(StateManagementExampleConstants.SurveyDataContextKey, JsonSerializer.Serialize(step2Survey));
            session.SetContextData(StateManagementExampleConstants.SurveyStepContextKey, StateManagementExampleConstants.ThirdStep);
            await _sessionService.UpdateSessionAsync(session).ConfigureAwait(false);

            _logger.LogInformation("Question 3: Would you recommend this?");
            var step3Survey = GetFormData<FeedbackSurvey>(session, StateManagementExampleConstants.SurveyDataContextKey);
            step3Survey.WouldRecommend = true;
            session.SetContextData(StateManagementExampleConstants.SurveyDataContextKey, JsonSerializer.Serialize(step3Survey));
            session.SetContextData(StateManagementExampleConstants.SurveyStepContextKey, StateManagementExampleConstants.CompleteStep);
            await _sessionService.UpdateSessionAsync(session).ConfigureAwait(false);

            var finalSurvey = GetFormData<FeedbackSurvey>(session, StateManagementExampleConstants.SurveyDataContextKey);
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

        public bool Equals(StateManagementExample? other)
        {
            if (other is null) return false;
            return FirstName == other.FirstName &&
                   Email == other.Email &&
                   PhoneNumber == other.PhoneNumber &&
                   SatisfactionLevel == other.SatisfactionLevel &&
                   ImprovementSuggestions == other.ImprovementSuggestions &&
                   WouldRecommend == other.WouldRecommend;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as StateManagementExample);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(FirstName, Email, PhoneNumber, SatisfactionLevel, ImprovementSuggestions, WouldRecommend);
        }

        public static bool operator ==(StateManagementExample? left, StateManagementExample? right)
        {
            return EqualityComparer<StateManagementExample>.Default.Equals(left, right);
        }

        public static bool operator !=(StateManagementExample? left, StateManagementExample? right)
        {
            return !(left == right);
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
