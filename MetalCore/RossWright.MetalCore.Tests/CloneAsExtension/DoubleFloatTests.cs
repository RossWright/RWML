namespace RossWright.MetalCore.Tests.CloneAsExtension;

public class DoubleFloatTests
{
    [Fact] public void DoubleToFloat()
    {
        HasDouble source = new()
        {
            Value = 1.234567890123456789
        };
        var target = source.CloneAs<HasFloat>();
        target.Value.ShouldBe(1.234567890123456789F);
    }

    [Fact] public void FloatToDouble()
    {
        HasFloat source = new()
        {
            Value = 1.23456789012345678F
        };
        var target = source.CloneAs<HasDouble>();
        ((float)target.Value).ShouldBe(1.234567890123456789F);
    }

    public class HasDouble
    {
        public double Value { get; set; }
    }

    public class HasFloat
    {
        public float Value { get; set; }
    }

    [Fact] public void NullableDoubleToFloat()
    {
        HasNullableDouble source = new()
        {
            Value = 1.234567890123456789
        };
        var target = source.CloneAs<HasFloat>();
        target.Value.ShouldBe(1.234567890123456789F);
    }

    [Fact] public void NullableDoubleToFloat_IsNull()
    {
        HasNullableDouble source = new();
        var target = source.CloneAs<HasFloat>();
        target.Value.ShouldBe(default);
    }

    [Fact] public void FloatToNullableDouble()
    {
        HasFloat source = new()
        {
            Value = 1.23456789012345678F
        };
        var target = source.CloneAs<HasNullableDouble>();
        ((float?)target.Value).ShouldBe(1.234567890123456789F);
    }

    [Fact] public void DoubleToNullableFloat()
    {
        HasDouble source = new()
        {
            Value = 1.234567890123456789
        };
        var target = source.CloneAs<HasNullableFloat>();
        target.Value.ShouldBe(1.234567890123456789F);
    }

    [Fact] public void NullableFloatToDouble_IsNull()
    {
        HasNullableFloat source = new();
        var target = source.CloneAs<HasDouble>();
        target.Value.ShouldBe(default);
    }

    [Fact] public void NullableFloatToDouble()
    {
        HasNullableFloat source = new()
        {
            Value = 1.23456789012345678F
        };
        var target = source.CloneAs<HasDouble>();
        ((float)target.Value).ShouldBe(1.234567890123456789F);
    }

    public class HasNullableDouble
    {
        public double? Value { get; set; }
    }

    public class HasNullableFloat
    {
        public float? Value { get; set; }
    }
}
