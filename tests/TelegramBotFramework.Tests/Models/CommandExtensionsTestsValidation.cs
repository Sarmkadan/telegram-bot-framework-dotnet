public static IReadOnlyList<string> Validate(CommandExtensionsTests value)
    {
        if (value == null)
        {
            return new List<string> { "Value cannot be null" };
        }

        var problems = new List<string>();

        if (value.HasParameters && value.Parameters == null)
        {
            problems.Add("Parameters cannot be null when HasParameters is true");
        }

        if (string.IsNullOrWhiteSpace(value.Name))
        {
            problems.Add("Name cannot be null, empty, or whitespace");
        }

        if (value.Type == CommandType.Standard && string.IsNullOrWhiteSpace(value.Description))
        {
            problems.Add("Description cannot be null, empty, or whitespace for standard commands");
        }

        return problems;
    }

    public static bool IsValid(CommandExtensionsTests value)
    {
        return Validate(value).Count == 0;
    }

    public static void EnsureValid(CommandExtensionsTests value)
    {
        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException("Invalid CommandExtensionsTests: " + string.Join("; ", problems));
        }
    }
}