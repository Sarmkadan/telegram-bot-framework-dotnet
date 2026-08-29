#nullable enable

using System;

namespace TelegramBotFramework.Examples
{
    /// <summary>
    /// Builder for configuring StateManagementExample instances with fluent syntax.
    /// </summary>
    public class StateManagementExampleExtensionsBuilder
    {
        private string _firstName = string.Empty;
        private string _email = string.Empty;
        private string _phoneNumber = string.Empty;
        private int _satisfactionLevel;
        private string _improvementSuggestions = string.Empty;
        private bool _wouldRecommend;

        /// <summary>
        /// Sets the first name.
        /// </summary>
        /// <param name="firstName">The first name value.</param>
        /// <returns>The builder instance for method chaining.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="firstName"/> is <see langword="null"/>.</exception>
        public StateManagementExampleExtensionsBuilder WithFirstName(string firstName)
        {
            ArgumentNullException.ThrowIfNull(firstName);
            _firstName = firstName;
            return this;
        }

        /// <summary>
        /// Sets the email address.
        /// </summary>
        /// <param name="email">The email address value.</param>
        /// <returns>The builder instance for method chaining.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="email"/> is <see langword="null"/>.</exception>
        public StateManagementExampleExtensionsBuilder WithEmail(string email)
        {
            ArgumentNullException.ThrowIfNull(email);
            _email = email;
            return this;
        }

        /// <summary>
        /// Sets the phone number.
        /// </summary>
        /// <param name="phoneNumber">The phone number value.</param>
        /// <returns>The builder instance for method chaining.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="phoneNumber"/> is <see langword="null"/>.</exception>
        public StateManagementExampleExtensionsBuilder WithPhoneNumber(string phoneNumber)
        {
            ArgumentNullException.ThrowIfNull(phoneNumber);
            _phoneNumber = phoneNumber;
            return this;
        }

        /// <summary>
        /// Sets the satisfaction level (1-10).
        /// </summary>
        /// <param name="satisfactionLevel">The satisfaction level value (must be between 1 and 10 inclusive).</param>
        /// <returns>The builder instance for method chaining.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="satisfactionLevel"/> is less than 1 or greater than 10.</exception>
        public StateManagementExampleExtensionsBuilder WithSatisfactionLevel(int satisfactionLevel)
        {
            if (satisfactionLevel < 1 || satisfactionLevel > 10)
            {
                throw new ArgumentOutOfRangeException(nameof(satisfactionLevel), "Satisfaction level must be between 1 and 10.");
            }
            _satisfactionLevel = satisfactionLevel;
            return this;
        }

        /// <summary>
        /// Sets the improvement suggestions.
        /// </summary>
        /// <param name="improvementSuggestions">The improvement suggestions value.</param>
        /// <returns>The builder instance for method chaining.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="improvementSuggestions"/> is <see langword="null"/>.</exception>
        public StateManagementExampleExtensionsBuilder WithImprovementSuggestions(string improvementSuggestions)
        {
            ArgumentNullException.ThrowIfNull(improvementSuggestions);
            _improvementSuggestions = improvementSuggestions;
            return this;
        }

        /// <summary>
        /// Sets whether the user would recommend the service.
        /// </summary>
        /// <param name="wouldRecommend">Whether the user would recommend the service.</param>
        /// <returns>The builder instance for method chaining.</returns>
        public StateManagementExampleExtensionsBuilder WithWouldRecommend(bool wouldRecommend)
        {
            _wouldRecommend = wouldRecommend;
            return this;
        }

        /// <summary>
        /// Creates a StateManagementExampleExtensionsBuilder pre-populated from an existing StateManagementExample instance.
        /// </summary>
        /// <param name="template">The StateManagementExample instance to copy values from.</param>
        /// <returns>A new builder instance with values copied from the template.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="template"/> is <see langword="null"/>.</exception>
        public static StateManagementExampleExtensionsBuilder From(StateManagementExample template)
        {
            ArgumentNullException.ThrowIfNull(template);
            return new StateManagementExampleExtensionsBuilder()
                .WithFirstName(template.FirstName)
                .WithEmail(template.Email)
                .WithPhoneNumber(template.PhoneNumber)
                .WithSatisfactionLevel(template.SatisfactionLevel)
                .WithImprovementSuggestions(template.ImprovementSuggestions)
                .WithWouldRecommend(template.WouldRecommend);
        }

        /// <summary>
        /// Builds and returns a configured StateManagementExample instance.
        /// </summary>
        /// <returns>A new StateManagementExample instance with the configured values.</returns>
        /// <exception cref="ArgumentException">If required properties (FirstName, Email, PhoneNumber) are empty.</exception>
        public StateManagementExample Build()
        {
            if (string.IsNullOrWhiteSpace(_firstName))
            {
                throw new ArgumentException("FirstName is required and cannot be empty.", nameof(_firstName));
            }

            if (string.IsNullOrWhiteSpace(_email))
            {
                throw new ArgumentException("Email is required and cannot be empty.", nameof(_email));
            }

            if (string.IsNullOrWhiteSpace(_phoneNumber))
            {
                throw new ArgumentException("PhoneNumber is required and cannot be empty.", nameof(_phoneNumber));
            }

            return new StateManagementExample
            {
                FirstName = _firstName,
                Email = _email,
                PhoneNumber = _phoneNumber,
                SatisfactionLevel = _satisfactionLevel,
                ImprovementSuggestions = _improvementSuggestions,
                WouldRecommend = _wouldRecommend
            };
        }
    }
}