# LocalizationService

Provides a simple way to store and retrieve localized string templates within the Telegram bot framework. The service allows templates to be registered individually or in bulk and later fetched for use in message generation.

## API

### `public LocalizationService()`
Creates a new instance of the localization service. No parameters are required. This constructor does not throw any exceptions.

### `public string? GetTemplate()`
Retrieves the currently registered template string. If no template has been set, the method returns `null`. It does not throw exceptions.

### `public string Get()` (first overload)
Returns a localized string based on the service’s internal state. The exact resolution logic depends on the overload’s hidden parameters (not shown in the signature). The method returns the resolved string and does not throw exceptions.

### `public string Get()` (second overload)
An alternative overload of `Get` that resolves a localized string using a different set of hidden parameters. Like the first overload, it returns a string and does not throw exceptions.

### `public void RegisterTemplate()`
Registers a single template string with the service. The method does not take visible parameters in the provided signature; any required data is supplied through the service’s internal state. It does not throw exceptions.

### `public void RegisterTemplates(IDictionary<,>)`
Registers multiple template strings from a dictionary. The method accepts an `IDictionary` whose key and value types are not specified in the visible signature. If the supplied dictionary is `null`, an `ArgumentNullException` is thrown. If any key or value within the dictionary is `null`, an `ArgumentException` may be thrown. No other exceptions are expected under normal use.
