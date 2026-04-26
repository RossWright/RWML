namespace RossWright.MetalCore.Tests.CloneAsExtension;

public class NullableTests
{
    [Fact] public void CloneNonNullableToNullable()
    {
        WithNonNullable source = new()
        {
            Value = DayOfWeek.Tuesday
        };
        var target = source.CloneAs<WithNullable>();
        target.Value.ShouldBe(DayOfWeek.Tuesday);
    }

    [Fact] public void CloneNullableToNonNullable()
    {
        WithNullable source = new()
        {
            Value = DayOfWeek.Tuesday
        };
        var target = source.CloneAs<WithNonNullable>();
        target.Value.ShouldBe(DayOfWeek.Tuesday);
    }

    [Fact] public void CloneNullableToNonNullableWhenNull()
    {
        WithNullable source = new()
        {
            Value = null
        };
        var target = source.CloneAs<WithNonNullable>();
        target.Value.ShouldBe(default(DayOfWeek));
    }


    public class WithNonNullable
    {
        public DayOfWeek Value { get; set; }
    }

    public class WithNullable
    {
        public DayOfWeek? Value { get; set; }
    }
}
