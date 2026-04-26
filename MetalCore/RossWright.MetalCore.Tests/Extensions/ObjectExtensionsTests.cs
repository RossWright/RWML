namespace RossWright;

public class ObjectExtensionsTests
{
    // ── In ────────────────────────────────────────────────────────────────────────
    [Fact]
    public void In_ValuePresentInSet_ReturnsTrue()
    {
        var result = 3.In(1, 2, 3, 4);
        result.ShouldBeTrue();
    }

    [Fact]
    public void In_ValueAbsentFromSet_ReturnsFalse()
    {
        var result = 9.In(1, 2, 3, 4);
        result.ShouldBeFalse();
    }

    [Fact]
    public void In_EmptySet_ReturnsFalse()
    {
        var result = 1.In<int>();
        result.ShouldBeFalse();
    }

    [Fact]
    public void In_StringValuePresent_ReturnsTrue()
    {
        var result = "b".In("a", "b", "c");
        result.ShouldBeTrue();
    }

    [Fact]
    public void In_NullableReferenceNullMatchesNull_ReturnsTrue()
    {
        string? value = null;
        var result = value.In(null, "a");
        result.ShouldBeTrue();
    }

    [Fact]
    public void In_NullableReferenceNullNotInSet_ReturnsFalse()
    {
        string? value = null;
        var result = value.In("a", "b");
        result.ShouldBeFalse();
    }

    [Fact]
    public void In_SingleValueMatch_ReturnsTrue()
    {
        var result = 5.In(5);
        result.ShouldBeTrue();
    }

    [Fact]
    public void In_SingleValueNoMatch_ReturnsFalse()
    {
        var result = 5.In(6);
        result.ShouldBeFalse();
    }

    [Fact]
    public void In_DuplicatesInSet_ReturnsTrue()
    {
        var result = 2.In(1, 2, 2, 3);
        result.ShouldBeTrue();
    }

    [Fact]
    public void In_ValueTypeNullable_ValuePresent_ReturnsTrue()
    {
        int? value = 5;
        var result = value.In(3, 5, 7);
        result.ShouldBeTrue();
    }

    [Fact]
    public void In_ValueTypeNullable_NullMatchesNull_ReturnsTrue()
    {
        int? value = null;
        var result = value.In(null, 3, 5);
        result.ShouldBeTrue();
    }

    [Fact]
    public void In_ValueTypeNullable_NullNotInSet_ReturnsFalse()
    {
        int? value = null;
        var result = value.In(3, 5);
        result.ShouldBeFalse();
    }

    [Fact]
    public void In_CustomObjectWithEquals_ReturnsTrue()
    {
        var obj = new TestObject { Id = 1 };
        var match = new TestObject { Id = 1 };
        var result = obj.In(new TestObject { Id = 2 }, match);
        result.ShouldBeTrue();
    }

    [Fact]
    public void In_CustomObjectWithEquals_ReturnsFalse()
    {
        var obj = new TestObject { Id = 1 };
        var result = obj.In(new TestObject { Id = 2 }, new TestObject { Id = 3 });
        result.ShouldBeFalse();
    }

    [Fact]
    public void In_EnumValue_Present_ReturnsTrue()
    {
        var result = TestEnum.Value2.In(TestEnum.Value1, TestEnum.Value2, TestEnum.Value3);
        result.ShouldBeTrue();
    }

    [Fact]
    public void In_EnumValue_Absent_ReturnsFalse()
    {
        var result = TestEnum.Value3.In(TestEnum.Value1, TestEnum.Value2);
        result.ShouldBeFalse();
    }

    [Fact]
    public void In_CharValue_Present_ReturnsTrue()
    {
        var result = 'b'.In('a', 'b', 'c');
        result.ShouldBeTrue();
    }

    [Fact]
    public void In_BoolValue_Present_ReturnsTrue()
    {
        var result = true.In(false, true);
        result.ShouldBeTrue();
    }

    [Fact]
    public void In_DoubleValue_Present_ReturnsTrue()
    {
        var result = 3.14.In(1.5, 3.14, 5.0);
        result.ShouldBeTrue();
    }

    [Fact]
    public void In_LongValue_Present_ReturnsTrue()
    {
        long value = 1000000000L;
        var result = value.In(500000000L, 1000000000L);
        result.ShouldBeTrue();
    }

    [Fact]
    public void In_FirstValueInSet_ReturnsTrue()
    {
        var result = 1.In(1, 2, 3);
        result.ShouldBeTrue();
    }

    [Fact]
    public void In_LastValueInSet_ReturnsTrue()
    {
        var result = 3.In(1, 2, 3);
        result.ShouldBeTrue();
    }

    [Fact]
    public void In_LargeSet_ValuePresent_ReturnsTrue()
    {
        var result = 50.In(1, 5, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100);
        result.ShouldBeTrue();
    }

    [Fact]
    public void In_LargeSet_ValueAbsent_ReturnsFalse()
    {
        var result = 55.In(1, 5, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100);
        result.ShouldBeFalse();
    }

    private class TestObject
    {
        public int Id { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is TestObject other && Id == other.Id;
        }

        public override int GetHashCode() => Id.GetHashCode();
    }

    private enum TestEnum
    {
        Value1,
        Value2,
        Value3
    }
}
