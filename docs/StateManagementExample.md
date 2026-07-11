# StateManagementExample
The `StateManagementExample` class is designed to demonstrate the management of state in a telegram bot, allowing for the collection and storage of user input data. This class provides properties to store user information, such as first name, email, phone number, satisfaction level, improvement suggestions, and whether the user would recommend the bot. It also includes a method to run the state management process asynchronously.

## API
* `public StateManagementExample`: The constructor for the `StateManagementExample` class, used to create a new instance.
* `public async Task RunAsync`: Runs the state management process asynchronously. This method does not take any parameters and does not return a value. It may throw exceptions if there are issues with the asynchronous operation.
* `public string FirstName`: Gets or sets the user's first name.
* `public string Email`: Gets or sets the user's email address.
* `public string PhoneNumber`: Gets or sets the user's phone number.
* `public int SatisfactionLevel`: Gets or sets the user's satisfaction level, represented as an integer.
* `public string ImprovementSuggestions`: Gets or sets the user's suggestions for improvement.
* `public bool WouldRecommend`: Gets or sets a boolean indicating whether the user would recommend the bot.

## Usage
The following examples demonstrate how to use the `StateManagementExample` class:
```csharp
// Example 1: Creating a new instance and setting properties
var stateManagement = new StateManagementExample();
stateManagement.FirstName = "John";
stateManagement.Email = "john@example.com";
stateManagement.PhoneNumber = "123-456-7890";
stateManagement.SatisfactionLevel = 5;
stateManagement.ImprovementSuggestions = "Add more features";
stateManagement.WouldRecommend = true;

// Example 2: Running the state management process asynchronously
var stateManagement = new StateManagementExample();
await stateManagement.RunAsync();
Console.WriteLine($"First Name: {stateManagement.FirstName}, Email: {stateManagement.Email}");
```

## Notes
When using the `StateManagementExample` class, consider the following edge cases and thread-safety remarks:
* The `RunAsync` method is asynchronous, so it should be used with caution in single-threaded environments or when working with sensitive data.
* The properties of the `StateManagementExample` class are not thread-safe, so access to them should be synchronized when used in multi-threaded environments.
* The `SatisfactionLevel` property is an integer, so it may not be suitable for storing satisfaction levels with decimal points.
* The `WouldRecommend` property is a boolean, so it may not be suitable for storing more nuanced user feedback.
