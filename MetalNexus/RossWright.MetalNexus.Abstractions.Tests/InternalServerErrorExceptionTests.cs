using RossWright.MetalNexus;
using Shouldly;

namespace RossWright.MetalNexus.Abstractions.UnitTests;

public class InternalServerErrorExceptionTests
{
    [Fact]
    public void DefaultConstructor_MessageIsNull()
    {
        var ex = new InternalServerErrorException();
        (ex.Message == "Exception of type 'RossWright.MetalNexus.InternalServerErrorException' was thrown."
            ? null
            : ex.Message).ShouldBeNull();
        ex.InnerException.ShouldBeNull();
    }

    [Fact]
    public void MessageConstructor_RoundTrips()
    {
        var ex = new InternalServerErrorException("something went wrong");
        ex.Message.ShouldBe("something went wrong");
        ex.InnerException.ShouldBeNull();
    }

    [Fact]
    public void MessageConstructor_WithNullMessage_RoundTrips()
    {
        var ex = new InternalServerErrorException(null);
        ex.Message.ShouldBe("Exception of type 'RossWright.MetalNexus.InternalServerErrorException' was thrown.");
        ex.InnerException.ShouldBeNull();
    }

    [Fact]
    public void MessageAndInnerExceptionConstructor_RoundTrips()
    {
        var inner = new InvalidOperationException("root cause");
        var ex = new InternalServerErrorException("outer message", inner);
        ex.Message.ShouldBe("outer message");
        ex.InnerException.ShouldBeSameAs(inner);
    }

    [Fact]
    public void MessageAndInnerExceptionConstructor_WithNullMessage_RoundTrips()
    {
        var inner = new InvalidOperationException("root cause");
        var ex = new InternalServerErrorException(null, inner);
        ex.Message.ShouldBe("Exception of type 'RossWright.MetalNexus.InternalServerErrorException' was thrown.");
        ex.InnerException.ShouldBeSameAs(inner);
    }

    [Fact]
    public void MessageAndInnerExceptionConstructor_WithNullInnerException_RoundTrips()
    {
        var ex = new InternalServerErrorException("outer message", null);
        ex.Message.ShouldBe("outer message");
        ex.InnerException.ShouldBeNull();
    }

    [Fact]
    public void MessageAndInnerExceptionConstructor_WithBothNull_RoundTrips()
    {
        var ex = new InternalServerErrorException(null, null);
        ex.Message.ShouldBe("Exception of type 'RossWright.MetalNexus.InternalServerErrorException' was thrown.");
        ex.InnerException.ShouldBeNull();
    }

    [Fact]
    public void IsAssignableFromException()
    {
        var ex = new InternalServerErrorException("test");
        ex.ShouldBeAssignableTo<Exception>();
    }
}
