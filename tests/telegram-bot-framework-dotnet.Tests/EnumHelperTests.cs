using System;
using System.ComponentModel;
using System.Linq;
using Xunit;
using TelegramBotFramework.Utilities;

namespace TelegramBotFramework.Tests.Utilities;

public class EnumHelperTests
{
    private enum TestEnum { First, Second, Third }

    [Flags]
    private enum TestFlagsEnum { None = 0, A = 1, B = 2 }

    private enum TestDescribedEnum
    {
        [Description("Custom First")]
        Val1,
        Val2
    }

    private class DummyAttribute : Attribute { }

    private enum TestAttributedEnum
    {
        [Dummy]
        Item1,
        Item2
    }

    [Fact]
    public void GetAllValues_And_EnumToDictionary_ReturnsCorrectData()
    {
        var values = EnumHelper.GetAllValues<TestEnum>();
        Assert.Equal(3, values.Count());
        Assert.Contains(TestEnum.First, values);

        var dict = EnumHelper.EnumToDictionary<TestEnum>();
        Assert.Equal(3, dict.Count);
        Assert.True(dict.ContainsKey("First"));
        Assert.Equal(TestEnum.First, dict["First"]);
    }

    [Fact]
    public void TryParse_And_IsValid_HandleVariousInputs()
    {
        // TryParse
        Assert.Equal(TestEnum.Second, EnumHelper.TryParse<TestEnum>("second", TestEnum.First));
        Assert.Equal(TestEnum.First, EnumHelper.TryParse<TestEnum>("invalid", TestEnum.First));
        Assert.Equal(TestEnum.First, EnumHelper.TryParse<TestEnum>(null, TestEnum.First));

        // IsValid
        Assert.True(EnumHelper.IsValid<TestEnum>("First"));
        Assert.False(EnumHelper.IsValid<TestEnum>("Invalid"));
        Assert.False(EnumHelper.IsValid<TestEnum>(null));
    }

    [Fact]
    public void GetDescription_And_EnumToDisplayDictionary_WorkCorrectly()
    {
        // GetDescription
        Assert.Equal("Custom First", TestDescribedEnum.Val1.GetDescription());
        Assert.Equal("Val2", TestDescribedEnum.Val2.GetDescription());

        // EnumToDisplayDictionary
        var dict = EnumHelper.EnumToDisplayDictionary<TestDescribedEnum>();
        Assert.Equal(2, dict.Count);
        Assert.Equal("Custom First", dict[TestDescribedEnum.Val1]);
        Assert.Equal("Val2", dict[TestDescribedEnum.Val2]);
    }

    [Fact]
    public void GetNumericValue_And_GetName_ReturnExpectedResults()
    {
        // GetNumericValue
        Assert.Equal(0, TestEnum.First.GetNumericValue());
        Assert.Equal(1, TestEnum.Second.GetNumericValue());

        // GetName
        Assert.Equal("First", EnumHelper.GetName(TestEnum.First));
        Assert.Equal("Second", EnumHelper.GetName(TestEnum.Second));
    }

    [Fact]
    public void GetAttributes_And_HasFlag_FunctionCorrectly()
    {
        // GetAttributes
        var attrs = TestAttributedEnum.Item1.GetAttributes<DummyAttribute>();
        Assert.Single(attrs);

        var noAttrs = TestAttributedEnum.Item2.GetAttributes<DummyAttribute>();
        Assert.Empty(noAttrs);

        // HasFlag
        var combined = TestFlagsEnum.A | TestFlagsEnum.B;
        Assert.True(combined.HasFlag(TestFlagsEnum.A));
        Assert.False(combined.HasFlag(TestFlagsEnum.None));
    }
}
