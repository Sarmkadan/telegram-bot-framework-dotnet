using FluentAssertions;
using TelegramBotFramework.Keyboard;
using Xunit;

namespace TelegramBotFramework.Tests;

public class ReplyKeyboardBuilderValidationTests
{
    [Fact]
    public void Validate_ValidBuilder_ReturnsEmptyList()
    {
        var builder = ReplyKeyboardBuilder.Create()
            .AddButton("Button 1")
            .NewRow()
            .AddButton("Button 2");

        var errors = builder.Validate();

        errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_EmptyBuilder_ReturnsError()
    {
        var builder = ReplyKeyboardBuilder.Create();

        var errors = builder.Validate();

        errors.Should().NotBeEmpty();
        errors.Should().Contain(e => e.Contains("keyboard"));
    }

    [Fact]
    public void Validate_InvalidButtonText_ReturnsError()
    {
        var builder = ReplyKeyboardBuilder.Create()
            .AddButton(new string('a', 65));

        var errors = builder.Validate();

        errors.Should().NotBeEmpty();
        errors.Should().Contain(e => e.Contains("longer than 64"));
    }

    [Fact]
    public void IsValid_ValidBuilder_ReturnsTrue()
    {
        var builder = ReplyKeyboardBuilder.Create()
            .AddButton("Valid");

        builder.IsValid().Should().BeTrue();
    }

    [Fact]
    public void IsValid_InvalidBuilder_ReturnsFalse()
    {
        var builder = ReplyKeyboardBuilder.Create()
            .AddButton(new string('a', 65));

        builder.IsValid().Should().BeFalse();
    }

    [Fact]
    public void EnsureValid_ValidBuilder_DoesNotThrow()
    {
        var builder = ReplyKeyboardBuilder.Create()
            .AddButton("Valid");

        Action act = () => builder.EnsureValid();

        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureValid_InvalidBuilder_ThrowsArgumentException()
    {
        var builder = ReplyKeyboardBuilder.Create()
            .AddButton(new string('a', 65));

        Action act = () => builder.EnsureValid();

        act.Should().Throw<ArgumentException>()
            .WithMessage("*validation failed*");
    }
}
