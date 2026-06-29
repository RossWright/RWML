using RossWright.MetalNexus;
using Shouldly;

namespace RossWright.MetalNexus.Abstractions.UnitTests;

public class MetalNexusExceptionTests
{
    [Fact]
    public void DefaultConstructor_MessageIsNull()
    {
        var ex = new MetalNexusException();
        (ex.Message == "Exception of type 'RossWright.MetalNexus.MetalNexusException' was thrown."
            ? null
            : ex.Message).ShouldBeNull();
        ex.InnerException.ShouldBeNull();
    }

    [Fact]
    public void MessageConstructor_RoundTrips()
    {
        var ex = new MetalNexusException("something went wrong");
        ex.Message.ShouldBe("something went wrong");
        ex.InnerException.ShouldBeNull();
    }

    [Fact]
    public void MessageAndInnerExceptionConstructor_RoundTrips()
    {
        var inner = new InvalidOperationException("root cause");
        var ex = new MetalNexusException("outer message", inner);
        ex.Message.ShouldBe("outer message");
        ex.InnerException.ShouldBeSameAs(inner);
    }

    [Fact]
    public void MessageAndInnerExceptionConstructor_WithNullInner_RoundTrips()
    {
        var ex = new MetalNexusException("outer message", null);
        ex.Message.ShouldBe("outer message");
        ex.InnerException.ShouldBeNull();
    }

    [Fact]
    public void IsAssignableFromException()
    {
        var ex = new MetalNexusException("test");
        ex.ShouldBeAssignableTo<Exception>();
    }
}
