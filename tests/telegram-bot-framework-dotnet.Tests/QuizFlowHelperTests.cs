using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using TelegramBotFramework.ConversationFlow.QuizFlow;
using Xunit;

namespace TelegramBotFramework.Tests;

public class QuizFlowHelperTests
{
    private const string FlowId = "quiz-1";
    private const string QuizName = "General Knowledge";

    [Fact]
    public void Constructor_WithRequiredArguments_ExposesFlowIdNameAndOptionalDefaults()
    {
        // Arrange & Act
        using var helper = new QuizFlowHelper(FlowId, QuizName);

        // Assert
        helper.FlowId.Should().Be(FlowId);
        helper.Name.Should().Be(QuizName);
        helper.Description.Should().BeNull();
        helper.CompletionMenuId.Should().BeNull();
        helper.GetQuestionCount().Should().Be(0);
    }

    [Fact]
    public void Constructor_NullFlowIdOrName_ThrowsArgumentNullException()
    {
        // Arrange, Act & Assert
        var noFlowId = Assert.Throws<ArgumentNullException>(() => new QuizFlowHelper(null!, QuizName));
        noFlowId.ParamName.Should().Be("flowId");

        var noName = Assert.Throws<ArgumentNullException>(() => new QuizFlowHelper(FlowId, null!));
        noName.ParamName.Should().Be("name");
    }

    [Fact]
    public void AddQuestion_ValidQuestion_IncrementsCountAndReturnsSameInstance()
    {
        // Arrange
        using var helper = new QuizFlowHelper(FlowId, QuizName) { Description = "Test", CompletionMenuId = "menu" };
        var question = CreateQuestion("q1");

        // Act
        var result = helper.AddQuestion(question);

        // Assert
        result.Should().BeSameAs(helper);
        helper.GetQuestionCount().Should().Be(1);
        helper.Description.Should().Be("Test");
        helper.CompletionMenuId.Should().Be("menu");
    }

    [Fact]
    public void AddQuestion_InvalidQuestion_ThrowsArgumentExceptionAndDoesNotAdd()
    {
        // Arrange
        using var helper = new QuizFlowHelper(FlowId, QuizName);
        var invalid = CreateQuestion("bad") with { CorrectAnswerIndex = 5 };

        // Act
        var exception = Assert.Throws<ArgumentException>(() => helper.AddQuestion(invalid));

        // Assert
        exception.ParamName.Should().Be("CorrectAnswerIndex");
        helper.GetQuestionCount().Should().Be(0);
    }

    [Fact]
    public void AddQuestions_EmptySequence_LeavesQuizUnchanged()
    {
        // Arrange
        using var helper = new QuizFlowHelper(FlowId, QuizName);

        // Act
        var result = helper.AddQuestions(Array.Empty<QuizQuestion>());

        // Assert
        result.Should().BeSameAs(helper);
        helper.GetQuestionCount().Should().Be(0);
        helper.GetQuestions().Should().BeEmpty();
    }

    [Fact]
    public void AddQuestions_MultipleQuestions_PreservesInsertionOrder()
    {
        // Arrange
        using var helper = new QuizFlowHelper(FlowId, QuizName);
        var questions = new List<QuizQuestion> { CreateQuestion("q1"), CreateQuestion("q2"), CreateQuestion("q3") };

        // Act
        helper.AddQuestions(questions).AddQuestion(CreateQuestion("q4"));

        // Assert
        helper.GetQuestionCount().Should().Be(4);
        helper.GetQuestions().Select(q => q.QuestionId).Should().Equal("q1", "q2", "q3", "q4");
    }

    [Fact]
    public void Dispose_CalledTwice_DoesNotThrow()
    {
        // Arrange
        var helper = new QuizFlowHelper(FlowId, QuizName);
        helper.AddQuestion(CreateQuestion("q1"));

        // Act
        helper.Dispose();
        var secondDispose = Record.Exception(() => helper.Dispose());

        // Assert
        secondDispose.Should().BeNull();
    }

    [Fact]
    public void Members_AfterDispose_ThrowObjectDisposedException()
    {
        // Arrange
        var helper = new QuizFlowHelper(FlowId, QuizName);
        helper.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => helper.AddQuestion(CreateQuestion("q1")));
        Assert.Throws<ObjectDisposedException>(() => helper.AddQuestions(new[] { CreateQuestion("q2") }));
        Assert.Throws<ObjectDisposedException>(() => helper.GetQuestionCount());
        Assert.Throws<ObjectDisposedException>(() => helper.GetQuestions());
    }

    private static QuizQuestion CreateQuestion(string id) => new()
    {
        QuestionId = id,
        Text = $"Question {id}?",
        Options = new[] { "A", "B", "C" },
        CorrectAnswerIndex = 1
    };
}
