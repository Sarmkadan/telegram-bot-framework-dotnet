using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TelegramBotFramework.BackgroundWorkers;
using Xunit;

namespace TelegramBotFramework.Tests;

public sealed class ScheduledTaskManagerValidationTests
{
    private static ScheduledTask CreateValidTask()
    {
        // Use FormatterServices to bypass any constructor requirements.
        var task = (ScheduledTask)System.Runtime.Serialization.FormatterServices
            .GetUninitializedObject(typeof(ScheduledTask));

        // Populate with a set of valid values.
        task.Id = "valid-id";
        task.Name = "Valid Task";
        task.TaskFunc = () => Task.CompletedTask;
        task.Interval = TimeSpan.FromMinutes(1);
        task.CreatedAt = DateTime.UtcNow;
        task.ExecutionCount = 0;
        // Optional fields left null / default – they are valid in this state.
        return task;
    }

    [Fact]
    public void Validate_ValidTask_ReturnsEmptyList()
    {
        var task = CreateValidTask();

        IReadOnlyList<string> problems = task.Validate();

        Assert.Empty(problems);
    }

    [Fact]
    public void IsValid_ValidTask_ReturnsTrue()
    {
        var task = CreateValidTask();

        bool isValid = task.IsValid();

        Assert.True(isValid);
    }

    [Fact]
    public void EnsureValid_ValidTask_DoesNotThrow()
    {
        var task = CreateValidTask();

        var exception = Record.Exception(() => task.EnsureValid());

        Assert.Null(exception);
    }

    [Fact]
    public void Validate_NullTask_ThrowsArgumentNullException()
    {
        ScheduledTask? task = null;

        Assert.Throws<ArgumentNullException>(() => task!.Validate());
    }

    [Fact]
    public void EnsureValid_InvalidTask_ThrowsArgumentException_WithProblems()
    {
        var task = CreateValidTask();
        task.Id = ""; // make it invalid

        var ex = Assert.Throws<ArgumentException>(() => task.EnsureValid());

        Assert.Contains("Id is null or whitespace.", ex.Message);
    }

    [Fact]
    public void IsValid_InvalidTask_ReturnsFalse()
    {
        var task = CreateValidTask();
        task.Interval = TimeSpan.Zero; // invalid interval

        bool isValid = task.IsValid();

        Assert.False(isValid);
    }

    [Fact]
    public void Validate_TaskWithFutureCreatedAt_ReturnsProblem()
    {
        var task = CreateValidTask();
        task.CreatedAt = DateTime.UtcNow.AddHours(1); // future date

        var problems = task.Validate();

        Assert.Contains("CreatedAt is in the future.", problems);
    }

    [Fact]
    public void Validate_TaskWithLastErrorButNoErrorMessage_ReturnsProblem()
    {
        var task = CreateValidTask();
        task.LastErrorAt = DateTime.UtcNow;
        task.LastError = null; // missing error message

        var problems = task.Validate();

        Assert.Contains("LastError must be set when LastErrorAt is set.", problems);
    }
}
