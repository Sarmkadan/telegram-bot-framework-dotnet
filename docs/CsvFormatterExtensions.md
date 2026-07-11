# CsvFormatterExtensions
The `CsvFormatterExtensions` class provides a set of extension methods for formatting data into CSV (Comma Separated Values) format. These methods allow for easy conversion of data into a CSV string, making it simple to work with CSV data in various applications, such as data exchange, logging, or reporting.

## API
The `CsvFormatterExtensions` class includes the following public members:
* `FormatWithProperties<T>`: Formats an object of type `T` into a CSV string using its properties. The method takes an object of type `T` as a parameter and returns a `string` representing the CSV data. It throws an exception if the object is null or if there is an error during formatting.
* `FormatWithProperties<T>` (overload): This is an overloaded version of the previous method, providing an alternative way to format an object of type `T` into a CSV string.
* `FormatWithHeaders<T>`: Formats an object of type `T` into a CSV string including headers. The method takes an object of type `T` as a parameter and returns a `string` representing the CSV data with headers. It throws an exception if the object is null or if there is an error during formatting.
* `FormatWithDelimiter<T>`: Formats an object of type `T` into a CSV string using a specified delimiter. The method takes an object of type `T` and a delimiter character as parameters and returns a `string` representing the CSV data. It throws an exception if the object is null, if the delimiter is not a single character, or if there is an error during formatting.

## Usage
Here are some examples of using the `CsvFormatterExtensions` class:
```csharp
public class Person
{
    public string Name { get; set; }
    public int Age { get; set; }
}

// Example 1: Format a Person object into a CSV string
Person person = new Person { Name = "John Doe", Age = 30 };
string csv = person.FormatWithProperties<Person>();
Console.WriteLine(csv);  // Output: "Name,Age\nJohn Doe,30"

// Example 2: Format a list of Person objects into a CSV string with headers
List<Person> people = new List<Person>
{
    new Person { Name = "John Doe", Age = 30 },
    new Person { Name = "Jane Doe", Age = 25 }
};
string csvWithHeaders = people[0].FormatWithHeaders<Person>();
foreach (var personItem in people)
{
    csvWithHeaders += $"\n{personItem.Name},{personItem.Age}";
}
Console.WriteLine(csvWithHeaders);  // Output: "Name,Age\nJohn Doe,30\nJane Doe,25"
```

## Notes
When using the `CsvFormatterExtensions` class, be aware of the following:
* The `FormatWithProperties<T>` and `FormatWithHeaders<T>` methods use the default delimiter (comma) to separate values. If you need to use a different delimiter, use the `FormatWithDelimiter<T>` method.
* The `FormatWithDelimiter<T>` method throws an exception if the delimiter is not a single character.
* The `CsvFormatterExtensions` class is thread-safe, as it does not maintain any internal state. However, the methods may throw exceptions if the input data is invalid or if there is an error during formatting.
* When working with large datasets, consider using a `StringBuilder` to build the CSV string instead of concatenating strings to improve performance.
