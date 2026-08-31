#nullable enable
using System;
using System.Collections.Generic;
using System.Text.Json;
using FluentAssertions;
using TelegramBotFramework.ConversationFlow;
using Xunit;

namespace TelegramBotFramework.Tests;

/// <summary>
/// Tests for <see cref="ConversationFlowExtensionsJsonExtensions"/>.
/// </summary>
public sealed class ConversationFlowExtensionsJsonExtensionsTests
{
    private static FlowDefinition CreateTestFlow()
    {
        return new FlowDefinition
        {
            FlowId = "test-flow",
            Name = "Test Flow",
            Description = "A test flow",
            InitialStepId = "step1",
            Steps = new List<FlowStep>
            {
                new FlowStep
                {
                    StepId = "step1",
                    Prompt = "Enter your name:",
                    InputType = FlowInputType.Text,
                    VariableName = "name",
                    IsTerminal = false,
                    Transitions = new List<FlowTransition>
                    {
                        new FlowTransition
                        {
                            TargetStepId = "step2",
                            Condition = new FlowCondition
                            {
                                VariableName = "name",
                                Operator = FlowConditionOperator.IsNotEmpty,
                                Value = ""
                            }
                        }
                    }
                },
                new FlowStep
                {
                    StepId = "step2",
                    Prompt = "Hello {name}! Flow completed.",
                    InputType = FlowInputType.Any,
                    IsTerminal = true
                }
            },
            AllowResume = true,
            Timeout = TimeSpan.FromMinutes(5),
            CompletionMenuId = "main-menu",
            Metadata = new Dictionary<string, string>
            {
                { "version", "1.0" },
                { "author", "test" }
            }
        };
    }

    [Fact]
    public void ToJson_NullInput_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => ConversationFlowExtensionsJsonExtensions.ToJson(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .Which.ParamName.Should().Be("value");
    }

    [Fact]
    public void ToJson_ValidFlow_ReturnsJsonString()
    {
        // Arrange
        var flow = CreateTestFlow();

        // Act
        var json = ConversationFlowExtensionsJsonExtensions.ToJson(flow);

        // Assert
        json.Should().NotBeNullOrEmpty();
        json.Should().Contain("\"flowId\":\"test-flow\"");
        json.Should().Contain("\"name\":\"Test Flow\"");
        json.Should().Contain("\"description\":\"A test flow\"");
    }

    [Fact]
    public void ToJson_IndentedTrue_ReturnsIndentedJson()
    {
        // Arrange
        var flow = CreateTestFlow();

        // Act
        var json = ConversationFlowExtensionsJsonExtensions.ToJson(flow, indented: true);

        // Assert
        json.Should().NotBeNullOrEmpty();
        json.Should().Contain("{");
        json.Should().Contain("  \"flowId\":");
        json.Should().Contain("\"test-flow\"");
    }

    [Fact]
    public void FromJson_NullInput_ReturnsNull()
    {
        // Act
        var result = ConversationFlowExtensionsJsonExtensions.FromJson(null!);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void FromJson_EmptyOrWhitespace_ReturnsNull()
    {
        // Act
        var result1 = ConversationFlowExtensionsJsonExtensions.FromJson(string.Empty);
        var result2 = ConversationFlowExtensionsJsonExtensions.FromJson("   ");
        var result3 = ConversationFlowExtensionsJsonExtensions.FromJson("\t\n\r");

        // Assert
        result1.Should().BeNull();
        result2.Should().BeNull();
        result3.Should().BeNull();
    }

    [Fact]
    public void FromJson_ValidJson_ReturnsFlowDefinition()
    {
        // Arrange
        var flow = CreateTestFlow();
        var json = ConversationFlowExtensionsJsonExtensions.ToJson(flow);

        // Act
        var result = ConversationFlowExtensionsJsonExtensions.FromJson(json);

        // Assert
        result.Should().NotBeNull();
        result!.FlowId.Should().Be(flow.FlowId);
        result.Name.Should().Be(flow.Name);
        result.Description.Should().Be(flow.Description);
        result.InitialStepId.Should().Be(flow.InitialStepId);
        result.Steps.Should().HaveCount(flow.Steps.Count);
        result.AllowResume.Should().Be(flow.AllowResume);
        result.Timeout.Should().Be(flow.Timeout);
        result.CompletionMenuId.Should().Be(flow.CompletionMenuId);
        result.Metadata.Should().BeEquivalentTo(flow.Metadata);
    }

    [Fact]
    public void FromJson_InvalidJson_ThrowsJsonException()
    {
        // Arrange
        var invalidJson = "{ invalid json";

        // Act
        Action act = () => ConversationFlowExtensionsJsonExtensions.FromJson(invalidJson);

        // Assert
        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void TryFromJson_NullInput_ReturnsFalse()
    {
        // Act
        var result = ConversationFlowExtensionsJsonExtensions.TryFromJson(null!, out _);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void TryFromJson_EmptyOrWhitespace_ReturnsFalse()
    {
        // Act
        var result1 = ConversationFlowExtensionsJsonExtensions.TryFromJson(string.Empty, out _);
        var result2 = ConversationFlowExtensionsJsonExtensions.TryFromJson("   ", out _);
        var result3 = ConversationFlowExtensionsJsonExtensions.TryFromJson("\t\n\r", out _);

        // Assert
        result1.Should().BeFalse();
        result2.Should().BeFalse();
        result3.Should().BeFalse();
    }

    [Fact]
    public void TryFromJson_ValidJson_ReturnsTrueAndValue()
    {
        // Arrange
        var flow = CreateTestFlow();
        var json = ConversationFlowExtensionsJsonExtensions.ToJson(flow);

        // Act
        var success = ConversationFlowExtensionsJsonExtensions.TryFromJson(json, out var result);

        // Assert
        success.Should().BeTrue();
        result.Should().NotBeNull();
        result!.FlowId.Should().Be(flow.FlowId);
        result.Name.Should().Be(flow.Name);
    }

    [Fact]
    public void TryFromJson_InvalidJson_ReturnsFalse()
    {
        // Arrange
        var invalidJson = "{ invalid json";

        // Act
        var success = ConversationFlowExtensionsJsonExtensions.TryFromJson(invalidJson, out var result);

        // Assert
        success.Should().BeFalse();
        result.Should().BeNull();
    }
}