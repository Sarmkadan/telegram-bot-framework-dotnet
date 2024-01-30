# BotConfigurationTestsExtensions

Extension methods for creating and validating `BotConfiguration` instances in unit tests. These methods simplify the construction of valid and invalid configurations and provide fluent APIs for common test scenarios.

## API

### `CreateValidConfiguration()`
Creates a `BotConfiguration` instance with default valid values suitable for most test cases. The configuration includes a valid bot token, owner ID, and default settings.

**Returns**
`BotConfiguration` – A new instance with default valid values.

---

### `WithOwnerId(this BotConfiguration, long ownerId)`
Sets the owner ID of the configuration.

**Parameters**
- `ownerId` (long) – The owner ID to set.

**Returns**
`BotConfiguration` – The same instance for method chaining.

---

### `WithAdminIds(this BotConfiguration, params long[] adminIds)`
Sets the admin IDs of the configuration.

**Parameters**
- `adminIds` (params long[]) – The admin IDs to set.

**Returns**
`BotConfiguration` – The same instance for method chaining.

---
### `WithCustomSettings(this BotConfiguration, IDictionary<string, object> settings)`
Sets custom settings in the configuration.

**Parameters**
- `settings` (IDictionary<string, object>) – The custom settings to apply.

**Returns**
`BotConfiguration` – The same instance for method chaining.

---
### `ShouldBeValid(this BotConfiguration)`
Asserts that the configuration is valid according to the framework's validation rules.

**Parameters**
- `configuration` (BotConfiguration) – The configuration to validate.

**Throws**
`XunitException` – If the configuration is invalid.

---
### `ShouldThrowValidationException(this Action action)`
Asserts that the provided action throws a validation exception.

**Parameters**
- `action` (Action) – The action expected to throw a validation exception.

**Throws**
`XunitException` – If the action does not throw a validation exception.

---
### `WithWebhookEnabled(this BotConfiguration, bool enabled = true)`
Enables or disables webhook mode in the configuration.

**Parameters**
- `enabled` (bool) – Whether to enable webhook mode. Defaults to `true`.

**Returns**
`BotConfiguration` – The same instance for method chaining.

---
### `WithRateLimitingDisabled(this BotConfiguration)`
Disables rate limiting in the configuration.

**Returns**
`BotConfiguration` – The same instance for method chaining.

---
### `WithSessionTimeout(this BotConfiguration, TimeSpan timeout)`
Sets the session timeout for the configuration.

**Parameters**
- `timeout` (TimeSpan) – The session timeout to set.

**Returns**
`BotConfiguration` – The same instance for method chaining.

---
### `WithMaxConcurrentRequests(this BotConfiguration, int maxConcurrentRequests)`
Sets the maximum number of concurrent requests allowed.

**Parameters**
- `maxConcurrentRequests` (int) – The maximum number of concurrent requests.

**Returns**
`BotConfiguration` – The same instance for method chaining.

---
### `ShouldBeAdmin(this BotConfiguration, long userId)`
Asserts that the given user ID is an admin in the configuration.

**Parameters**
- `configuration` (BotConfiguration) – The configuration to check.
- `userId` (long) – The user ID to verify as an admin.

**Throws**
`XunitException` – If the user is not an admin.

---
### `ShouldNotBeAdmin(this BotConfiguration, long userId)`
Asserts that the given user ID is not an admin in the configuration.

**Parameters**
- `configuration` (BotConfiguration) – The configuration to check.
- `userId` (long) – The user ID to verify as a non-admin.

**Throws**
`XunitException` – If the user is an admin.

---
### `SessionTimeoutShouldBe(this BotConfiguration, TimeSpan expectedTimeout)`
Asserts that the session timeout matches the expected value.

**Parameters**
- `configuration` (BotConfiguration) – The configuration to check.
- `expectedTimeout` (TimeSpan) – The expected session timeout.

**Throws**
`XunitException` – If the session timeout does not match.

---
### `WithLoggingDisabled(this BotConfiguration)`
Disables logging in the configuration.

**Returns**
`BotConfiguration` – The same instance for method chaining.

---
### `WithLocalizationLanguage(this BotConfiguration, string languageCode)`
Sets the localization language for the configuration.

**Parameters**
- `languageCode` (string) – The language code to set (e.g., "en", "ru").

**Returns**
`BotConfiguration` – The same instance for method chaining.

## Usage
