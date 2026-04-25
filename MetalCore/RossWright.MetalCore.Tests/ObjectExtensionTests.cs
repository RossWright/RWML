namespace RossWright;

public class ObjectExtensionTests
{
    [Fact] public void In_ValuePresentInSet_ReturnsTrue() => 3.In(1, 2, 3, 4).ShouldBeTrue();
    [Fact] public void In_ValueAbsentFromSet_ReturnsFalse() => 9.In(1, 2, 3, 4).ShouldBeFalse();
    [Fact] public void In_EmptySet_ReturnsFalse() => 1.In<int>().ShouldBeFalse();
    [Fact] public void In_String_ValuePresent_ReturnsTrue() => "b".In("a", "b", "c").ShouldBeTrue();
    [Fact] public void In_NullableReference_NullMatchesNull() =>
        ((string?)null).In(null, "a").ShouldBeTrue();
}
