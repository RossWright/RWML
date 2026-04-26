using System.Reflection;

namespace RossWright.MetalCore.Tests;

// Test attribute for reflection tests
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property)]
internal class TestReflectionAttribute : Attribute { }

// Test classes for attribute tests
[TestReflection]
internal class AttributedTestClass
{
    [TestReflection]
    public string AttributedField = "test";

    [TestReflection]
    public string AttributedProperty { get; set; } = "test";

    public string PlainField = "plain";

    public string PlainProperty { get; set; } = "plain";
}

internal class PlainTestClass
{
    public string Field = "test";
    public string Property { get; set; } = "test";
}

internal abstract class AbstractTestClass { }
internal interface ITestReflInterface { }

public class HasAttribute_Type_Tests
{
    [Fact]
    public void HasAttribute_TypeWithAttribute_ReturnsTrue()
    {
        // Arrange
        var type = typeof(AttributedTestClass);

        // Act
        var result = type.HasAttribute<TestReflectionAttribute>();

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void HasAttribute_TypeWithoutAttribute_ReturnsFalse()
    {
        // Arrange
        var type = typeof(PlainTestClass);

        // Act
        var result = type.HasAttribute<TestReflectionAttribute>();

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void HasAttribute_TypeWithAttributeInheritFalse_ReturnsTrue()
    {
        // Arrange
        var type = typeof(AttributedTestClass);

        // Act
        var result = type.HasAttribute<TestReflectionAttribute>(inherit: false);

        // Assert
        result.ShouldBeTrue();
    }
}

public class HasAttribute_FieldInfo_NonGeneric_Tests
{
    [Fact]
    public void HasAttribute_FieldWithAttribute_ReturnsTrue()
    {
        // Arrange
        var fieldInfo = typeof(AttributedTestClass).GetField(nameof(AttributedTestClass.AttributedField))!;

        // Act
        var result = fieldInfo.HasAttribute(typeof(TestReflectionAttribute));

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void HasAttribute_FieldWithoutAttribute_ReturnsFalse()
    {
        // Arrange
        var fieldInfo = typeof(AttributedTestClass).GetField(nameof(AttributedTestClass.PlainField))!;

        // Act
        var result = fieldInfo.HasAttribute(typeof(TestReflectionAttribute));

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void HasAttribute_FieldWithAttributeInheritFalse_ReturnsTrue()
    {
        // Arrange
        var fieldInfo = typeof(AttributedTestClass).GetField(nameof(AttributedTestClass.AttributedField))!;

        // Act
        var result = fieldInfo.HasAttribute(typeof(TestReflectionAttribute), inherit: false);

        // Assert
        result.ShouldBeTrue();
    }
}

public class HasAttribute_FieldInfo_Generic_Tests
{
    [Fact]
    public void HasAttribute_FieldWithAttribute_ReturnsTrue()
    {
        // Arrange
        var fieldInfo = typeof(AttributedTestClass).GetField(nameof(AttributedTestClass.AttributedField))!;

        // Act
        var result = fieldInfo.HasAttribute<TestReflectionAttribute>();

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void HasAttribute_FieldWithoutAttribute_ReturnsFalse()
    {
        // Arrange
        var fieldInfo = typeof(AttributedTestClass).GetField(nameof(AttributedTestClass.PlainField))!;

        // Act
        var result = fieldInfo.HasAttribute<TestReflectionAttribute>();

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void HasAttribute_FieldWithAttributeInheritFalse_ReturnsTrue()
    {
        // Arrange
        var fieldInfo = typeof(AttributedTestClass).GetField(nameof(AttributedTestClass.AttributedField))!;

        // Act
        var result = fieldInfo.HasAttribute<TestReflectionAttribute>(inherit: false);

        // Assert
        result.ShouldBeTrue();
    }
}

public class HasAttribute_PropertyInfo_Tests
{
    [Fact]
    public void HasAttribute_PropertyWithAttribute_ReturnsTrue()
    {
        // Arrange
        var propInfo = typeof(AttributedTestClass).GetProperty(nameof(AttributedTestClass.AttributedProperty))!;

        // Act
        var result = propInfo.HasAttribute<TestReflectionAttribute>();

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void HasAttribute_PropertyWithoutAttribute_ReturnsFalse()
    {
        // Arrange
        var propInfo = typeof(AttributedTestClass).GetProperty(nameof(AttributedTestClass.PlainProperty))!;

        // Act
        var result = propInfo.HasAttribute<TestReflectionAttribute>();

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void HasAttribute_PropertyWithAttributeInheritFalse_ReturnsTrue()
    {
        // Arrange
        var propInfo = typeof(AttributedTestClass).GetProperty(nameof(AttributedTestClass.AttributedProperty))!;

        // Act
        var result = propInfo.HasAttribute<TestReflectionAttribute>(inherit: false);

        // Assert
        result.ShouldBeTrue();
    }
}

public class TryConvert_Tests
{
    [Fact]
    public void TryConvert_NullToValueType_ReturnsDefault()
    {
        // Arrange
        var type = typeof(int);

        // Act
        var result = type.TryConvert(null);

        // Assert
        result.ShouldBe(0);
    }

    [Fact]
    public void TryConvert_NullToReferenceType_ReturnsNull()
    {
        // Arrange
        var type = typeof(string);

        // Act
        var result = type.TryConvert(null);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void TryConvert_SameType_ReturnsSameValue()
    {
        // Arrange
        var type = typeof(string);
        var value = "hello";

        // Act
        var result = type.TryConvert(value);

        // Assert
        result.ShouldBe(value);
    }

    [Fact]
    public void TryConvert_StringToTargetType_ParsesValue()
    {
        // Arrange
        var type = typeof(int);
        var value = "42";

        // Act
        var result = type.TryConvert(value);

        // Assert
        result.ShouldBe(42);
    }

    [Fact]
    public void TryConvert_NonStringDifferentType_ReturnsOriginalValue()
    {
        // Arrange
        var type = typeof(string);
        var value = 123;

        // Act
        var result = type.TryConvert(value);

        // Assert
        result.ShouldBe(123);
    }
}

public class ReflectionMemberTests
{
    // ── GetValue / SetValue ───────────────────────────────────────────────────────
    [Fact]
    public void GetValue_Property_ReturnsValue()
    {
        var obj = new AttributedTestClass { AttributedProperty = "hello" };
        var prop = typeof(AttributedTestClass).GetProperty(nameof(AttributedTestClass.AttributedProperty))!;
        prop.GetValue(obj).ShouldBe("hello");
    }

    [Fact]
    public void GetValue_Field_ReturnsValue()
    {
        var obj = new AttributedTestClass { AttributedField = "world" };
        var field = typeof(AttributedTestClass).GetField(nameof(AttributedTestClass.AttributedField))!;
        field.GetValue(obj).ShouldBe("world");
    }

    [Fact]
    public void SetValue_Property_SetsValue()
    {
        var obj = new AttributedTestClass();
        var prop = typeof(AttributedTestClass).GetProperty(nameof(AttributedTestClass.AttributedProperty))!;
        prop.SetValue(obj, "updated");
        obj.AttributedProperty.ShouldBe("updated");
    }

    [Fact]
    public void SetValue_Field_SetsValue()
    {
        var obj = new AttributedTestClass();
        var field = typeof(AttributedTestClass).GetField(nameof(AttributedTestClass.AttributedField))!;
        field.SetValue(obj, "updated-field");
        obj.AttributedField.ShouldBe("updated-field");
    }

    [Fact]
    public void GetValue_WhenMemberIsMethod_ThrowsNotSupportedException()
    {
        var method = typeof(string).GetMethod("ToUpper", Type.EmptyTypes)!;
        Should.Throw<NotSupportedException>(() => method.GetValue("hello"));
    }

    [Fact]
    public void SetValue_WhenMemberIsMethod_ThrowsNotSupportedException()
    {
        var method = typeof(string).GetMethod("ToUpper", Type.EmptyTypes)!;
        Should.Throw<NotSupportedException>(() => method.SetValue("hello", null));
    }

    // ── GetReturnType ─────────────────────────────────────────────────────────────
    [Fact]
    public void GetReturnType_Property_ReturnsPropertyType()
    {
        var prop = typeof(AttributedTestClass).GetProperty(nameof(AttributedTestClass.AttributedProperty))!;
        prop.GetReturnType().ShouldBe(typeof(string));
    }

    [Fact]
    public void GetReturnType_Field_ReturnsFieldType()
    {
        var field = typeof(AttributedTestClass).GetField(nameof(AttributedTestClass.AttributedField))!;
        field.GetReturnType().ShouldBe(typeof(string));
    }

    [Fact]
    public void GetReturnType_Method_ReturnsReturnType()
    {
        var method = typeof(string).GetMethod(nameof(string.ToString), Type.EmptyTypes)!;
        method.GetReturnType().ShouldBe(typeof(string));
    }

    [Fact]
    public void GetReturnType_WhenMemberIsEvent_ThrowsNotSupportedException()
    {
        var evt = typeof(Console).GetEvent("CancelKeyPress")!;
        Should.Throw<NotSupportedException>(() => evt.GetReturnType());
    }

    // ── Parse ─────────────────────────────────────────────────────────────────────
    [Fact]
    public void Parse_Int_ReturnsInt() => typeof(int).Parse("42").ShouldBe(42);

    [Fact]
    public void Parse_Guid_ReturnsGuid()
    {
        var id = Guid.NewGuid();
        typeof(Guid).Parse(id.ToString()).ShouldBe(id);
    }

    [Fact]
    public void Parse_Enum_ReturnsEnumValue() =>
        typeof(DayOfWeek).Parse("Monday").ShouldBe(DayOfWeek.Monday);

    // ── IsSimpleType ──────────────────────────────────────────────────────────────
    [Fact] public void IsSimpleType_Int_ReturnsTrue() => typeof(int).IsSimpleType().ShouldBeTrue();
    [Fact] public void IsSimpleType_String_ReturnsTrue() => typeof(string).IsSimpleType().ShouldBeTrue();
    [Fact] public void IsSimpleType_Enum_ReturnsTrue() => typeof(DayOfWeek).IsSimpleType().ShouldBeTrue();
    [Fact] public void IsSimpleType_ComplexClass_ReturnsFalse() => typeof(List<int>).IsSimpleType().ShouldBeFalse();

    // ── IsConcrete ────────────────────────────────────────────────────────────────
    [Fact] public void IsConcrete_ConcreteClass_ReturnsTrue() => typeof(PlainTestClass).IsConcrete().ShouldBeTrue();
    [Fact] public void IsConcrete_AbstractClass_ReturnsFalse() => typeof(AbstractTestClass).IsConcrete().ShouldBeFalse();
    [Fact] public void IsConcrete_Interface_ReturnsFalse() => typeof(ITestReflInterface).IsConcrete().ShouldBeFalse();

    // ── GetFullGenericName ─────────────────────────────────────────────────────────
    [Fact] public void GetFullGenericName_NonGeneric_ReturnsTypeName() =>
        typeof(int).GetFullGenericName().ShouldBe("Int32");

    [Fact] public void GetFullGenericName_SingleTypeParam_ReturnsFormattedName() =>
        typeof(List<int>).GetFullGenericName().ShouldBe("List<Int32>");

    [Fact] public void GetFullGenericName_NestedGenerics_ReturnsFormattedName() =>
        typeof(Dictionary<string, List<int>>).GetFullGenericName().ShouldBe("Dictionary<String,List<Int32>>");

    [Fact]
    public void GetFullGenericName_OpenGenericType_ReturnsNameWithPlaceholders()
    {
        var result = typeof(List<>).GetFullGenericName();
        result.ShouldNotBeNull();
        result.ShouldNotBeEmpty();
        result.ShouldContain("List");
    }

    // ── TryConvert throws path ────────────────────────────────────────────────────
    [Fact]
    public void TryConvert_InvalidStringForInt_Throws() =>
        Should.Throw<Exception>(() => typeof(int).TryConvert("not-a-number"));
}
