using RossWright.MetalChain;
using Shouldly;

namespace RossWright.MetalChain.Abstractions.UnitTests;

public class MetalChainExceptionTests
{
    [Fact]
    public void DefaultConstructor_MessageIsNull()
    {
        var ex = new MetalChainException();
        (ex.Message == "Exception of type 'RossWright.MetalChain.MetalChainException' was thrown."
            ? null
            : ex.Message).ShouldBeNull();
        ex.InnerException.ShouldBeNull();
    }

    [Fact]
    public void MessageConstructor_RoundTrips()
    {
        var ex = new MetalChainException("something went wrong");
        ex.Message.ShouldBe("something went wrong");
        ex.InnerException.ShouldBeNull();
    }

    [Fact]
    public void MessageAndInnerExceptionConstructor_RoundTrips()
    {
        var inner = new InvalidOperationException("root cause");
        var ex = new MetalChainException("outer message", inner);
        ex.Message.ShouldBe("outer message");
        ex.InnerException.ShouldBeSameAs(inner);
    }

    [Fact]
    public void MessageAndInnerExceptionConstructor_WithNullInner_RoundTrips()
    {
        var ex = new MetalChainException("outer message", null);
        ex.Message.ShouldBe("outer message");
        ex.InnerException.ShouldBeNull();
    }

    [Fact]
    public void IsAssignableFromException()
    {
        var ex = new MetalChainException("test");
        ex.ShouldBeAssignableTo<Exception>();
    }
}
