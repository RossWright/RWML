using RossWright.MetalGuardian;
using Shouldly;

namespace RossWright.MetalGuardian.Tests;

public class MetalGuardianExceptionTests
{
    [Fact]
    public void DefaultConstructor_MessageIsNull()
    {
        var ex = new MetalGuardianException();
        (ex.Message == "Exception of type 'RossWright.MetalGuardian.MetalGuardianException' was thrown."
            ? null
            : ex.Message).ShouldBeNull();
        ex.InnerException.ShouldBeNull();
    }

    [Fact]
    public void MessageConstructor_RoundTrips()
    {
        var ex = new MetalGuardianException("something went wrong");
        ex.Message.ShouldBe("something went wrong");
        ex.InnerException.ShouldBeNull();
    }

    [Fact]
    public void MessageAndInnerExceptionConstructor_RoundTrips()
    {
        var inner = new InvalidOperationException("root cause");
        var ex = new MetalGuardianException("outer message", inner);
        ex.Message.ShouldBe("outer message");
        ex.InnerException.ShouldBeSameAs(inner);
    }

    [Fact]
    public void MessageAndInnerExceptionConstructor_WithNullInner_RoundTrips()
    {
        var ex = new MetalGuardianException("outer message", null!);
        ex.Message.ShouldBe("outer message");
        ex.InnerException.ShouldBeNull();
    }

    [Fact]
    public void IsAssignableFromException()
    {
        var ex = new MetalGuardianException("test");
        ex.ShouldBeAssignableTo<Exception>();
    }
}
