# Contributing to Telegram Bot Framework

Thank you for your interest in contributing! This document provides guidelines for contributing to the project.

## Code of Conduct

Be respectful, inclusive, and constructive in all interactions.

## How to Contribute

### Reporting Bugs

1. Check if the bug has already been reported in Issues
2. Provide a clear description of the bug
3. Include steps to reproduce
4. Specify your environment (.NET version, OS, etc.)

### Suggesting Enhancements

1. Check if the enhancement has already been suggested
2. Provide a clear description of the enhancement
3. Explain the use case and benefits
4. If possible, provide code examples

### Pull Requests

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/your-feature-name`
3. Make your changes
4. Add tests if applicable
5. Follow the coding standards (see below)
6. Commit with clear messages: `git commit -m "Add feature description"`
7. Push to your fork: `git push origin feature/your-feature-name`
8. Open a Pull Request with a clear description

## Coding Standards

### C# Style Guide

- Follow the Microsoft C# Coding Conventions
- Use meaningful variable and method names
- Keep methods focused on a single responsibility
- Maximum line length: 120 characters
- Use async/await for I/O operations

### File Structure

```
- Each class should be in its own file
- File name matches class name
- Use proper namespace organization
- Add author header to every file
```

### Comments

- Add meaningful comments for complex logic
- Avoid redundant comments that just restate the code
- Document public APIs with XML comments
- Add `///` XML documentation to public classes and methods

### Example

```csharp
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Services;

/// <summary>
/// Description of the class.
/// </summary>
public class MyService
{
    /// <summary>
    /// Description of the method.
    /// </summary>
    public void MyMethod()
    {
        // Implementation
    }
}
```

## Testing

- Write unit tests for new features
- Ensure existing tests pass
- Aim for >80% code coverage for new code
- Use descriptive test names

## Documentation

- Update README.md if adding new features
- Add code comments for complex logic
- Update XML documentation for public APIs
- Keep documentation in sync with code changes

## Commit Messages

Use clear, descriptive commit messages:

```
Add feature: Brief description of what was added

More detailed explanation of the change if needed.
- Bullet point 1
- Bullet point 2
```

## Review Process

1. Your PR will be reviewed by maintainers
2. Address any feedback or requested changes
3. Once approved, your PR will be merged

## Getting Help

- Check existing documentation and issues
- Open a discussion for questions
- Ask for help in PR reviews

## License

By contributing, you agree that your contributions will be licensed under the MIT License.

---

Thank you for contributing!
