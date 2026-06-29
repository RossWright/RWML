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

// Positional records for CloneAs primary-constructor tests
record PositionalSingleProp(int Value);
record PositionalTwoProps(int Value, string Name);
record PositionalWithNullable(int Id, string? Description);
record PositionalWithDateOnly(int Id, DateOnly BirthDate);

// Hybrid: positional ctor params + extra init-only property
record HybridRecord(int Id, string Name)
{
    public string? Extra { get; init; }
}

// Source type whose property name doesn't match the ctor parameter
record MismatchedCtorRecord(int UnknownParam);

// Source type with an incompatible type for a ctor parameter (DateOnly vs int — no TypeConverter)
record TypeMismatchRecord(DateOnly Value);

// Two constructors: prefer the one with more satisfiable params
class TwoCtorClass
{
    public int Id { get; }
    public string Name { get; }
    public TwoCtorClass(int id) { Id = id; Name = string.Empty; }
    public TwoCtorClass(int id, string name) { Id = id; Name = name; }
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

// init-only property — CopyTo cannot set it, CloneAs can via SetValue reflection
class BasicTypeOneInitProp
{
    public int Value { get; init; }
}

// source type with matching name in different case to exercise case-sensitivity of RunThroughDataMembers
class BasicTypeCasedProp
{
    public int value { get; set; }   // lowercase — does NOT match "Value" in RunThroughDataMembers (case-sensitive)
}

// for HasChangedFrom: type with a public field
class BasicTypeOnePublicField
{
    public int Value = default;
    public string? Name = default;
}

// for positional record: target with nullable ctor param
record PositionalWithNullableCtorParam(int Id, string? Tag);

// for positional record: target where only the smaller ctor can be satisfied
// (the larger ctor needs "Extra" which the source doesn't have)
class TwoCtorOnlySmallSatisfiable
{
    public int Id { get; }
    public string Name { get; }
    public TwoCtorOnlySmallSatisfiable(int id) { Id = id; Name = string.Empty; }
    public TwoCtorOnlySmallSatisfiable(int id, string name, string extra) { Id = id; Name = name; _ = extra; }
}
