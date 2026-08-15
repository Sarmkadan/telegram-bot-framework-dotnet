using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TelegramBotFramework.Utilities;
using Xunit;

namespace TelegramBotFramework.Tests;

public class ReflectionHelperTests
{
    // Test Fixtures
    private interface ITestInterface { }
    private class TestClass : ITestInterface { }
    private class TestClassWithArgs { public TestClassWithArgs(int i) { } }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    private class TestAttribute : Attribute { }

    [TestAttribute]
    private class AttributedClass { }

    private class ComplexClass
    {
        public int Id { get; set; }
        [TestAttribute]
        public string Name { get; set; } = string.Empty;
        public void PublicMethod() { }
        public const string Constant = "Value";
    }

    [Fact]
    public void GetTypesImplementing_And_GetTypesWithAttribute_ReturnsExpectedTypes()
    {
        // Act
        var implTypes = ReflectionHelper.GetTypesImplementing<ITestInterface>();
        var attrTypes = ReflectionHelper.GetTypesWithAttribute<TestAttribute>();

        // Assert
        Assert.Contains(typeof(TestClass), implTypes);
        Assert.Contains(typeof(AttributedClass), attrTypes);
    }

    [Fact]
    public void CreateInstance_WithAndWithoutArgs_ReturnsInstanceOrNull()
    {
        // Act & Assert - Default
        var instance1 = ReflectionHelper.CreateInstance<TestClass>(typeof(TestClass));
        Assert.NotNull(instance1);

        // Act & Assert - With Args
        var instance2 = ReflectionHelper.CreateInstance<TestClassWithArgs>(typeof(TestClassWithArgs), 1);
        Assert.NotNull(instance2);

        // Act & Assert - Null Type
        var instance3 = ReflectionHelper.CreateInstance<TestClass>(null);
        Assert.Null(instance3);

        // Act & Assert - Invalid Args
        var instance4 = ReflectionHelper.CreateInstance<TestClass>(typeof(TestClass), 1);
        Assert.Null(instance4);
    }

    [Fact]
    public void GetProperties_GetPublicMethods_GetConstants_ReturnsCorrectMembers()
    {
        // Act
        var props = ReflectionHelper.GetProperties<TestAttribute>(typeof(ComplexClass));
        var methods = ReflectionHelper.GetPublicMethods(typeof(ComplexClass));
        var constants = ReflectionHelper.GetConstants(typeof(ComplexClass));

        // Assert
        Assert.Single(props);
        Assert.Equal("Name", props.First().Name);
        Assert.Contains(methods, m => m.Name == "PublicMethod");
        Assert.Contains(constants, c => c.Name == "Constant");
    }

    [Fact]
    public void GetPropertyValue_SetPropertyValue_ManipulateObjectState()
    {
        // Arrange
        var obj = new ComplexClass { Id = 10 };

        // Act & Assert - Get
        var val = ReflectionHelper.GetPropertyValue(obj, "Id");
        Assert.Equal(10, val);

        // Act & Assert - Set
        var setResult = ReflectionHelper.SetPropertyValue(obj, "Id", 20);
        Assert.True(setResult);
        Assert.Equal(20, obj.Id);

        // Act & Assert - Set Invalid
        var invalidSet = ReflectionHelper.SetPropertyValue(obj, "InvalidProp", 0);
        Assert.False(invalidSet);

        // Act & Assert - Null Object
        var nullVal = ReflectionHelper.GetPropertyValue(null, "Id");
        Assert.Null(nullVal);
    }

    [Fact]
    public void IsSubclassOfGeneric_DetectsGenericInheritance()
    {
        // Act & Assert
        Assert.True(ReflectionHelper.IsSubclassOfGeneric(typeof(List<int>), typeof(List<>)));
        Assert.False(ReflectionHelper.IsSubclassOfGeneric(typeof(string), typeof(List<>)));
    }

    [Fact]
    public void GetDisplayName_FormatsComplexTypesCorrectly()
    {
        // Act & Assert
        Assert.Equal("List<Int32>", ReflectionHelper.GetDisplayName(typeof(List<int>)));
        Assert.Equal("Int32?", ReflectionHelper.GetDisplayName(typeof(int?)));
    }
}
