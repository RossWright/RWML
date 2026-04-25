namespace RossWright.MetalCore.Tests.CloneAsExtension;

class BasicTypeOneField
{
    public int Value;
}

class BasicTypeOneProp
{
    public int Value { get; set; }
}

class BasicTypeTwoProp
{
    public int Value { get; set; }
    public int OtherValue { get; set; }
}
class BasicTypeTwoPropOneUnknown
{
    public int Value { get; set; }
    public int Unknown { get; set; }
}
class BasicTypeTwoPropOneIgnore
{
    public int Value { get; set; }
    [Ignore] public int OtherValue { get; set; }
}
class BasicTypeOneAliasProp
{
    [Aka("Value")] public int OtherValue { get; set; }
}
class BasicTypeOneAliasPropWithNameCollision
{
    public int Value { get; set; }
    [Aka("Value")] public int OtherValue { get; set; }
}

class BasicTypeOnePropWithMethod
{
    public int Value { get; set; }
    public int Foo() => 0;
}
class ComplexTypeWithFieldField
{
    public BasicTypeOneField? Obj;
}
class ComplexTypeWithFieldProp
{
    public BasicTypeOneField? Obj { get; set; }
}
class ComplexTypeWithPropProp
{
    public BasicTypeOneProp? Obj { get; set; }
}
class ComplexTypeWithPropField
{
    public BasicTypeOneProp? Obj;
}

class ComplexTypeWithDerivedType
{
    public DerivedType? Obj { get; set; }
}
class DerivedType : BasicTypeOneField
{
    public int OtherValue { get; set; }
}

class BasicTypeOneReadOnlyField
{
    public BasicTypeOneReadOnlyField(int value) => Value = value;
    public readonly int Value;
}

class BasicTypeOneReadOnlyProp
{
    public BasicTypeOneReadOnlyProp(int value) => Value = value;
    public int Value { get; }
}

class BasicTypeOneWriteOnlyProp
{
    public int Value { private get; set; }
    public int GetValue() { return Value; }
}

class BasicTypeOneStringField
{
    public string Value = string.Empty;
}

class BasicTypeOneStringProp
{
    public string Value { get; set; } = null!;
}

class BasicTypeWithPrivateField
{
    public BasicTypeWithPrivateField(int value, int otherValue) => 
        (Value, OtherValue) = (value, otherValue);
    public int Value;
    private int OtherValue;
    public int GetOtherValue() => OtherValue;
}

class BasicTypeWithPrivateProp
{
    public BasicTypeWithPrivateProp(int value, int otherValue, int anotherValue) =>
        (Value, OtherValue, AnotherValue) = (value, otherValue, anotherValue);
    public int Value { get; set; }
    private int OtherValue { get; set; }
    protected int AnotherValue { get; private set; }
    public int GetOtherValue() => OtherValue;
    public int GetAnotherValue() => AnotherValue;
}

class ObjWithoutNullableEnum
{
    public DayOfWeek DayOfWeek { get; set; }
}
class ObjWithNullableEnum
{
    public DayOfWeek? DayOfWeek { get; set; }
}