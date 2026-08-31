#nullable enable

using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;

namespace TelegramBotFramework.Examples
{
	/// <summary>
	/// Extension methods for <see cref="StateManagementExample"/> providing additional functionality
	/// for state management, data validation, and form processing.
	/// </summary>
	public static class StateManagementExampleExtensions
	{
		/// <summary>
		/// Validates the registration form data stored in the session context.
		/// </summary>
		/// <param name="example">The StateManagementExample instance.</param>
		/// <param name="session">The user session.</param>
		/// <returns>True if form is valid; false otherwise.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="example"/> or <paramref name="session"/> is <see langword="null"/>.</exception>
		public static bool ValidateRegistrationForm(this StateManagementExample example, UserSession session)
		{
			ArgumentNullException.ThrowIfNull(example);
			ArgumentNullException.ThrowIfNull(session);

			var formData = example.GetFormData<RegistrationForm>(session, StateManagementExampleExtensionsConstants.RegistrationFormKey);

			return !string.IsNullOrWhiteSpace(formData.FirstName) &&
				!string.IsNullOrWhiteSpace(formData.Email) &&
				!string.IsNullOrWhiteSpace(formData.PhoneNumber);
		}

		/// <summary>
		/// Gets the complete registration data as a formatted string for logging or display.
		/// </summary>
		/// <param name="example">The StateManagementExample instance.</param>
		/// <param name="session">The user session.</param>
		/// <returns>Formatted registration data string.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="example"/> or <paramref name="session"/> is <see langword="null"/>.</exception>
		public static string GetRegistrationDataSummary(this StateManagementExample example, UserSession session)
		{
			ArgumentNullException.ThrowIfNull(example);
			ArgumentNullException.ThrowIfNull(session);

			var formData = example.GetFormData<RegistrationForm>(session, StateManagementExampleExtensionsConstants.RegistrationFormKey);

			return $"Registration: Name='{formData.FirstName}', Email='{formData.Email}', Phone='{formData.PhoneNumber}'";
		}

		/// <summary>
		/// Validates the feedback survey data stored in the session context.
		/// </summary>
		/// <param name="example">The StateManagementExample instance.</param>
		/// <param name="session">The user session.</param>
		/// <returns>True if survey data is valid; false otherwise.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="example"/> or <paramref name="session"/> is <see langword="null"/>.</exception>
		public static bool ValidateSurveyData(this StateManagementExample example, UserSession session)
		{
			ArgumentNullException.ThrowIfNull(example);
			ArgumentNullException.ThrowIfNull(session);

			var surveyData = example.GetFormData<FeedbackSurvey>(session, StateManagementExampleExtensionsConstants.SurveyDataKey);

			return surveyData.SatisfactionLevel >= 1 &&
				surveyData.SatisfactionLevel <= 10 &&
				!string.IsNullOrWhiteSpace(surveyData.ImprovementSuggestions);
		}

		/// <summary>
		/// Gets the complete survey results as a formatted string for logging or display.
		/// </summary>
		/// <param name="example">The StateManagementExample instance.</param>
		/// <param name="session">The user session.</param>
		/// <returns>Formatted survey results string.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="example"/> or <paramref name="session"/> is <see langword="null"/>.</exception>
		public static string GetSurveyResultsSummary(this StateManagementExample example, UserSession session)
		{
			ArgumentNullException.ThrowIfNull(example);
			ArgumentNullException.ThrowIfNull(session);

			var surveyData = example.GetFormData<FeedbackSurvey>(session, StateManagementExampleExtensionsConstants.SurveyDataKey);

			return $"Survey Results: Satisfaction={surveyData.SatisfactionLevel}/10, " +
				$"WouldRecommend={(surveyData.WouldRecommend ? "Yes" : "No")}, " +
				$"Suggestions='{surveyData.ImprovementSuggestions}'";
		}

		/// <summary>
		/// Updates the satisfaction level in the survey data and persists it to the session.
		/// </summary>
		/// <param name="example">The StateManagementExample instance.</param>
		/// <param name="session">The user session.</param>
		/// <param name="level">The satisfaction level (1-10).</param>
		/// <returns>Task representing the async operation.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="example"/> or <paramref name="session"/> is <see langword="null"/>.</exception>
		public static async Task UpdateSatisfactionLevelAsync(this StateManagementExample example, UserSession session, int level)
		{
			ArgumentNullException.ThrowIfNull(example);
			ArgumentNullException.ThrowIfNull(session);

			var surveyData = example.GetFormData<FeedbackSurvey>(session, StateManagementExampleExtensionsConstants.SurveyDataKey);
			surveyData.SatisfactionLevel = Math.Clamp(level, 1, 10);

			session.SetContextData("survey_data", JsonSerializer.Serialize(surveyData));
			await example.UpdateSessionAsync(session).ConfigureAwait(false);
		}

		/// <summary>
		/// Updates the improvement suggestions in the survey data and persists it to the session.
		/// </summary>
		/// <param name="example">The StateManagementExample instance.</param>
		/// <param name="session">The user session.</param>
		/// <param name="suggestions">The improvement suggestions text.</param>
		/// <returns>Task representing the async operation.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="example"/> is <see langword="null"/>.</exception>
		public static async Task UpdateImprovementSuggestionsAsync(this StateManagementExample example, UserSession session, string? suggestions)
		{
			ArgumentNullException.ThrowIfNull(example);
			ArgumentNullException.ThrowIfNull(session);

			var surveyData = example.GetFormData<FeedbackSurvey>(session, StateManagementExampleExtensionsConstants.SurveyDataKey);
			surveyData.ImprovementSuggestions = suggestions ?? string.Empty;

			session.SetContextData("survey_data", JsonSerializer.Serialize(surveyData));
			await example.UpdateSessionAsync(session).ConfigureAwait(false);
		}

		/// <summary>
		/// Updates the recommendation preference in the survey data and persists it to the session.
		/// </summary>
		/// <param name="example">The StateManagementExample instance.</param>
		/// <param name="session">The user session.</param>
		/// <param name="wouldRecommend">Whether user would recommend the service.</param>
		/// <returns>Task representing the async operation.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="example"/> or <paramref name="session"/> is <see langword="null"/>.</exception>
		public static async Task UpdateRecommendationAsync(this StateManagementExample example, UserSession session, bool wouldRecommend)
		{
			ArgumentNullException.ThrowIfNull(example);
			ArgumentNullException.ThrowIfNull(session);

			var surveyData = example.GetFormData<FeedbackSurvey>(session, StateManagementExampleExtensionsConstants.SurveyDataKey);
			surveyData.WouldRecommend = wouldRecommend;

			session.SetContextData("survey_data", JsonSerializer.Serialize(surveyData));
			await example.UpdateSessionAsync(session).ConfigureAwait(false);
		}

		private static async Task UpdateSessionAsync(this StateManagementExample example, UserSession session)
		{
			ArgumentNullException.ThrowIfNull(example);
			ArgumentNullException.ThrowIfNull(session);

			var sessionService = example.GetRequiredService<ISessionAndMenuService>();
			await sessionService.UpdateSessionAsync(session).ConfigureAwait(false);
		}

		private static IServiceProvider GetRequiredService<T>(this StateManagementExample example) where T : notnull
		{
			ArgumentNullException.ThrowIfNull(example);

			return example.GetServiceProvider();
		}

		private static IServiceProvider GetServiceProvider(this StateManagementExample example)
		{
			ArgumentNullException.ThrowIfNull(example);

			return (IServiceProvider)typeof(StateManagementExample)
				.GetField("_serviceProvider", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(example)
				?? throw new InvalidOperationException("Service provider field not found in StateManagementExample");
		}

		private sealed class RegistrationForm
		{
			public string FirstName { get; set; } = string.Empty;
			public string Email { get; set; } = string.Empty;
			public string PhoneNumber { get; set; } = string.Empty;
		}

		private sealed class FeedbackSurvey
		{
			public int SatisfactionLevel { get; set; }
			public string ImprovementSuggestions { get; set; } = string.Empty;
			public bool WouldRecommend { get; set; }
		}
	}
}