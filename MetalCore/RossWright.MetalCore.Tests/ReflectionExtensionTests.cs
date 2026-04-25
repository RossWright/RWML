namespace RossWright;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property)]
internal class ReflTestAttribute : Attribute { }

[ReflTest]
internal class ReflAttributedClass
{
    [ReflTest] public string AnnotatedProperty { get; set; } = "initial";
    [ReflTest] public string AnnotatedField = "field-initial";
}

internal class ReflPlainClass
{
    public int Value { get; set; }
}

internal abstract class ReflAbstractClass { }
internal interface IReflTestInterface { }

public class ReflectionExtensionTests
{
    // ── GetValue / SetValue ───────────────────────────────────────────────────────
    [Fact] public void GetValue_Property_ReturnsValue()
    {
        var obj = new ReflAttributedClass { AnnotatedProperty = "hello" };
        var prop = typeof(ReflAttributedClass).GetProperty(nameof(ReflAttributedClass.AnnotatedProperty))!;
        prop.GetValue(obj).ShouldBe("hello");
    }

    [Fact] public void GetValue_Field_ReturnsValue()
    {
        var obj = new ReflAttributedClass { AnnotatedField = "world" };
        var field = typeof(ReflAttributedClass).GetField(nameof(ReflAttributedClass.AnnotatedField))!;
        field.GetValue(obj).ShouldBe("world");
    }

    [Fact] public void SetValue_Property_SetsValue()
    {
        var obj = new ReflAttributedClass();
        var prop = typeof(ReflAttributedClass).GetProperty(nameof(ReflAttributedClass.AnnotatedProperty))!;
        prop.SetValue(obj, "updated");
        obj.AnnotatedProperty.ShouldBe("updated");
    }

    [Fact] public void SetValue_Field_SetsValue()
    {
        var obj = new ReflAttributedClass();
        var field = typeof(ReflAttributedClass).GetField(nameof(ReflAttributedClass.AnnotatedField))!;
        field.SetValue(obj, "updated-field");
        obj.AnnotatedField.ShouldBe("updated-field");
    }

    // ── GetReturnType ─────────────────────────────────────────────────────────────
    [Fact] public void GetReturnType_Property_ReturnsPropertyType()
    {
        var prop = typeof(ReflAttributedClass).GetProperty(nameof(ReflAttributedClass.AnnotatedProperty))!;
        prop.GetReturnType().ShouldBe(typeof(string));
    }

    [Fact] public void GetReturnType_Field_ReturnsFieldType()
    {
        var field = typeof(ReflAttributedClass).GetField(nameof(ReflAttributedClass.AnnotatedField))!;
        field.GetReturnType().ShouldBe(typeof(string));
    }

    [Fact] public void GetReturnType_Method_ReturnsReturnType()
    {
        var method = typeof(string).GetMethod(nameof(string.ToString), Type.EmptyTypes)!;
        method.GetReturnType().ShouldBe(typeof(string));
    }

    // ── HasAttribute (Type) ────────────────────────────────────────────────────────
    [Fact] public void TypeHasAttribute_Generic_ReturnsTrueWhenPresent() =>
        typeof(ReflAttributedClass).HasAttribute<ReflTestAttribute>().ShouldBeTrue();

    [Fact] public void TypeHasAttribute_Generic_ReturnsFalseWhenAbsent() =>
        typeof(ReflPlainClass).HasAttribute<ReflTestAttribute>().ShouldBeFalse();

    [Fact] public void TypeHasAttribute_NonGeneric_ReturnsTrueWhenPresent() =>
        typeof(ReflAttributedClass).HasAttribute(typeof(ReflTestAttribute)).ShouldBeTrue();

    [Fact] public void TypeHasAttribute_NonGeneric_ReturnsFalseWhenAbsent() =>
        typeof(ReflPlainClass).HasAttribute(typeof(ReflTestAttribute)).ShouldBeFalse();

    // ── HasAttribute (FieldInfo / PropertyInfo) ────────────────────────────────────
    [Fact] public void FieldHasAttribute_Generic_ReturnsTrueWhenPresent()
    {
        var field = typeof(ReflAttributedClass).GetField(nameof(ReflAttributedClass.AnnotatedField))!;
        field.HasAttribute<ReflTestAttribute>().ShouldBeTrue();
    }

    [Fact] public void PropertyHasAttribute_Generic_ReturnsTrueWhenPresent()
    {
        var prop = typeof(ReflAttributedClass).GetProperty(nameof(ReflAttributedClass.AnnotatedProperty))!;
        prop.HasAttribute<ReflTestAttribute>().ShouldBeTrue();
    }

    [Fact] public void PropertyHasAttribute_NonGeneric_ReturnsTrueWhenPresent()
    {
        var prop = typeof(ReflAttributedClass).GetProperty(nameof(ReflAttributedClass.AnnotatedProperty))!;
        prop.HasAttribute(typeof(ReflTestAttribute)).ShouldBeTrue();
    }

    // ── Parse ─────────────────────────────────────────────────────────────────────
    [Fact] public void Parse_Int_ReturnsInt() => typeof(int).Parse("42").ShouldBe(42);
    [Fact] public void Parse_Guid_ReturnsGuid()
    {
        var id = Guid.NewGuid();
        typeof(Guid).Parse(id.ToString()).ShouldBe(id);
    }
    [Fact] public void Parse_Enum_ReturnsEnumValue() =>
        typeof(DayOfWeek).Parse("Monday").ShouldBe(DayOfWeek.Monday);

    // ── TryConvert ────────────────────────────────────────────────────────────────
    [Fact] public void TryConvert_NullToValueType_ReturnsDefault() =>
        typeof(int).TryConvert(null).ShouldBe(0);

    [Fact] public void TryConvert_NullToReferenceType_ReturnsNull() =>
        typeof(string).TryConvert(null).ShouldBeNull();

    [Fact] public void TryConvert_SameType_ReturnsSameValue() =>
        typeof(string).TryConvert("hello").ShouldBe("hello");

    [Fact] public void TryConvert_StringToInt_ParsesValue() =>
        typeof(int).TryConvert("99").ShouldBe(99);

    [Fact] public void TryConvert_InvalidStringForInt_Throws() =>
        Should.Throw<Exception>(() => typeof(int).TryConvert("not-a-number"));

    // ── IsSimpleType ──────────────────────────────────────────────────────────────
    [Fact] public void IsSimpleType_Int_ReturnsTrue() => typeof(int).IsSimpleType().ShouldBeTrue();
    [Fact] public void IsSimpleType_String_ReturnsTrue() => typeof(string).IsSimpleType().ShouldBeTrue();
    [Fact] public void IsSimpleType_Enum_ReturnsTrue() => typeof(DayOfWeek).IsSimpleType().ShouldBeTrue();
    [Fact] public void IsSimpleType_ComplexClass_ReturnsFalse() => typeof(List<int>).IsSimpleType().ShouldBeFalse();

    // ── IsConcrete ────────────────────────────────────────────────────────────────
    [Fact] public void IsConcrete_ConcreteClass_ReturnsTrue() => typeof(ReflPlainClass).IsConcrete().ShouldBeTrue();
    [Fact] public void IsConcrete_AbstractClass_ReturnsFalse() => typeof(ReflAbstractClass).IsConcrete().ShouldBeFalse();
    [Fact] public void IsConcrete_Interface_ReturnsFalse() => typeof(IReflTestInterface).IsConcrete().ShouldBeFalse();

    // ── GetFullGenericName ─────────────────────────────────────────────────────────
    [Fact] public void GetFullGenericName_NonGeneric_ReturnsTypeName() =>
        typeof(int).GetFullGenericName().ShouldBe("Int32");

    [Fact] public void GetFullGenericName_SingleTypeParam_ReturnsFormattedName() =>
        typeof(List<int>).GetFullGenericName().ShouldBe("List<Int32>");

    [Fact] public void GetFullGenericName_NestedGenerics_ReturnsFormattedName() =>
        typeof(Dictionary<string, List<int>>).GetFullGenericName().ShouldBe("Dictionary<String,List<Int32>>");

    // ── P3-D: NotSupportedException paths and open-generic ────────────────────
    [Fact] public void GetValue_WhenMemberIsMethod_ThrowsNotSupportedException()
    {
        var method = typeof(string).GetMethod("ToUpper", Type.EmptyTypes)!;
        Should.Throw<NotSupportedException>(() => method.GetValue("hello"));
    }

    [Fact] public void SetValue_WhenMemberIsMethod_ThrowsNotSupportedException()
    {
        var method = typeof(string).GetMethod("ToUpper", Type.EmptyTypes)!;
        Should.Throw<NotSupportedException>(() => method.SetValue("hello", null));
    }

    [Fact] public void GetReturnType_WhenMemberIsEvent_ThrowsNotSupportedException()
    {
        var evt = typeof(Console).GetEvent("CancelKeyPress")!;
        Should.Throw<NotSupportedException>(() => evt.GetReturnType());
    }

    [Fact] public void GetFullGenericName_OpenGenericType_ReturnsNameWithPlaceholders()
    {
        var result = typeof(List<>).GetFullGenericName();
        result.ShouldNotBeNull();
        result.ShouldNotBeEmpty();
        result.ShouldContain("List");
    }
}
