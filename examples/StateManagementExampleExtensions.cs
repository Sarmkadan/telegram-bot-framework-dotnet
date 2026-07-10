#nullable enable

using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace TelegramBotFramework.Examples
{
    /// <summary>
    /// Extension methods for StateManagementExample providing additional functionality
    /// for state management, data validation, and form processing.
    /// </summary>
    public static class StateManagementExampleExtensions
    {
        /// <summary>
        /// Validates the registration form data stored in the session context.
        /// </summary>
        /// <param name="example">The StateManagementExample instance</param>
        /// <param name="session">The user session</param>
        /// <returns>True if form is valid; false otherwise</returns>
        public static bool ValidateRegistrationForm(this StateManagementExample example, UserSession session)
        {
            var formData = example.GetFormData<RegistrationForm>(session, "registration_form");

            return !string.IsNullOrWhiteSpace(formData.FirstName) &&
                   !string.IsNullOrWhiteSpace(formData.Email) &&
                   !string.IsNullOrWhiteSpace(formData.PhoneNumber);
        }

        /// <summary>
        /// Gets the complete registration data as a formatted string for logging or display.
        /// </summary>
        /// <param name="example">The StateManagementExample instance</param>
        /// <param name="session">The user session</param>
        /// <returns>Formatted registration data string</returns>
        public static string GetRegistrationDataSummary(this StateManagementExample example, UserSession session)
        {
            var formData = example.GetFormData<RegistrationForm>(session, "registration_form");

            return $"Registration: Name='{formData.FirstName}', Email='{formData.Email}', Phone='{formData.PhoneNumber}'";
        }

        /// <summary>
        /// Validates the feedback survey data stored in the session context.
        /// </summary>
        /// <param name="example">The StateManagementExample instance</param>
        /// <param name="session">The user session</param>
        /// <returns>True if survey data is valid; false otherwise</returns>
        public static bool ValidateSurveyData(this StateManagementExample example, UserSession session)
        {
            var surveyData = example.GetFormData<FeedbackSurvey>(session, "survey_data");

            return surveyData.SatisfactionLevel >= 1 &&
                   surveyData.SatisfactionLevel <= 10 &&
                   !string.IsNullOrWhiteSpace(surveyData.ImprovementSuggestions);
        }

        /// <summary>
        /// Gets the complete survey results as a formatted string for logging or display.
        /// </summary>
        /// <param name="example">The StateManagementExample instance</param>
        /// <param name="session">The user session</param>
        /// <returns>Formatted survey results string</returns>
        public static string GetSurveyResultsSummary(this StateManagementExample example, UserSession session)
        {
            var surveyData = example.GetFormData<FeedbackSurvey>(session, "survey_data");

            return $"Survey Results: Satisfaction={surveyData.SatisfactionLevel}/10, " +
                   $"WouldRecommend={(surveyData.WouldRecommend ? "Yes" : "No")}, " +
                   $"Suggestions='{surveyData.ImprovementSuggestions}'";
        }

        /// <summary>
        /// Updates the satisfaction level in the survey data and persists it to the session.
        /// </summary>
        /// <param name="example">The StateManagementExample instance</param>
        /// <param name="session">The user session</param>
        /// <param name="level">The satisfaction level (1-10)</param>
        /// <returns>Task representing the async operation</returns>
        public static async Task UpdateSatisfactionLevelAsync(this StateManagementExample example, UserSession session, int level)
        {
            var surveyData = example.GetFormData<FeedbackSurvey>(session, "survey_data");
            surveyData.SatisfactionLevel = Math.Clamp(level, 1, 10);

            session.SetContextData("survey_data", JsonSerializer.Serialize(surveyData));
            await example.UpdateSessionAsync(session).ConfigureAwait(false);
        }

        /// <summary>
        /// Updates the improvement suggestions in the survey data and persists it to the session.
        /// </summary>
        /// <param name="example">The StateManagementExample instance</param>
        /// <param name="session">The user session</param>
        /// <param name="suggestions">The improvement suggestions text</param>
        /// <returns>Task representing the async operation</returns>
        public static async Task UpdateImprovementSuggestionsAsync(this StateManagementExample example, UserSession session, string suggestions)
        {
            var surveyData = example.GetFormData<FeedbackSurvey>(session, "survey_data");
            surveyData.ImprovementSuggestions = suggestions ?? string.Empty;

            session.SetContextData("survey_data", JsonSerializer.Serialize(surveyData));
            await example.UpdateSessionAsync(session).ConfigureAwait(false);
        }

        /// <summary>
        /// Updates the recommendation preference in the survey data and persists it to the session.
        /// </summary>
        /// <param name="example">The StateManagementExample instance</param>
        /// <param name="session">The user session</param>
        /// <param name="wouldRecommend">Whether user would recommend the service</param>
        /// <returns>Task representing the async operation</returns>
        public static async Task UpdateRecommendationAsync(this StateManagementExample example, UserSession session, bool wouldRecommend)
        {
            var surveyData = example.GetFormData<FeedbackSurvey>(session, "survey_data");
            surveyData.WouldRecommend = wouldRecommend;

            session.SetContextData("survey_data", JsonSerializer.Serialize(surveyData));
            await example.UpdateSessionAsync(session).ConfigureAwait(false);
        }

        private static async Task UpdateSessionAsync(this StateManagementExample example, UserSession session)
        {
            var sessionService = example.GetServiceProvider().GetRequiredService<ISessionAndMenuService>();
            await sessionService.UpdateSessionAsync(session).ConfigureAwait(false);
        }

        private static IServiceProvider GetServiceProvider(this StateManagementExample example)
        {
            var field = typeof(StateManagementExample).GetField(
                "_serviceProvider",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (IServiceProvider)field?.GetValue(example)!;
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