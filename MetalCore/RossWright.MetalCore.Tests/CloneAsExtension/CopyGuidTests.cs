namespace RossWright.MetalCore.Tests.CloneAsExtension;

public class CopyGuidTests
{
    [Fact] public void GuidToString()
    {
        var source = new HasGuid() { Value = Guid.NewGuid() };
        var target = source.CloneAs<HasString>();
        target.Value.ShouldBe(source.Value.ToString());
    }

    [Fact] public void StringToGuid()
    {
        var source = new HasString() { Value = Guid.NewGuid().ToString() };
        var target = source.CloneAs<HasGuid>();
        target.Value.ShouldBe(Guid.Empty);
    }

    public class HasGuid
    {
        public Guid Value { get; set; }
    }

    public class HasString
    { 
        public string Value { get; set; } = null!;
    }
}
