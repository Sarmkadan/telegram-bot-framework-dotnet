#nullable enable

using System;
using System.Collections.Generic;
using FluentAssertions;
using TelegramBotFramework.Integration;
using Xunit;

namespace TelegramBotFramework.Integration.InternalTests;

internal class ExternalApiIntegrationValidationTestsHelper
{
    [Fact]
    public void Validate_ReturnsEmptyList_WhenInstanceIsValid()
    {
        // Arrange
        var integration = new ExternalApiIntegration();

        // Act
        IReadOnlyList<string> result = integration.Validate();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void Validate_ThrowsArgumentNullException_WhenInstanceIsNull()
    {
        // Arrange
        ExternalApiIntegration? integration = null;

        // Act
        Action act = () => integration!.Validate();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void IsValid_ReturnsTrue_WhenInstanceIsNotNull()
    {
        // Arrange
        var integration = new ExternalApiIntegration();

        // Act
        bool isValid = integration.IsValid();

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void IsValid_ReturnsFalse_WhenInstanceIsNull()
    {
        // Arrange
        ExternalApiIntegration? integration = null;

        // Act
        bool isValid = integration.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void EnsureValid_DoesNotThrow_WhenInstanceIsNotNull()
    {
        // Arrange
        var integration = new ExternalApiIntegration();

        // Act
        Action act = () => integration.EnsureValid();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureValid_ThrowsArgumentNullException_WhenInstanceIsNull()
    {
        // Arrange
        ExternalApiIntegration? integration = null;

        // Act
        Action act = () => integration!.EnsureValid();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
