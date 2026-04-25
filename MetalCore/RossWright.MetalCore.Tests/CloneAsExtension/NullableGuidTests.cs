namespace RossWright.MetalCore.Tests.CloneAsExtension;

public class NullableGuidTests
{
    [Fact] public void OverwriteGuidWithNull()
    {
        var source = new HasNullableGuid() { Value = null };
        var target = new AlsoHasNullableGuid() { Value = Guid.NewGuid() };
        source.CopyTo(target);
        target.Value.ShouldBeNull();
    }
    [Fact] public void IgnoreChangeOnGuidWithNull()
    {
        var source = new HasNullableGuid() { Value = null };
        var initialValue = Guid.NewGuid();
        var target = new HasGuid() { Value = initialValue };
        source.CopyTo(target);
        target.Value.ShouldBe(initialValue);
    }
    public class HasNullableGuid
    {
        public Guid? Value { get; set; }
    }
    public class AlsoHasNullableGuid
    {
        public Guid? Value { get; set; }
    }
    public class HasGuid
    {
        public Guid Value { get; set; }
    }
}