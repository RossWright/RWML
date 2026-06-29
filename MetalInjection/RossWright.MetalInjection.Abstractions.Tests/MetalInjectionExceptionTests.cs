using RossWright.MetalInjection;
using Shouldly;

namespace RossWright.MetalInjection.Abstractions.UnitTests;

public class MetalInjectionExceptionTests
{
    [Fact]
    public void MessageConstructor_RoundTrips()
    {
        var ex = new MetalInjectionException("something went wrong");
        ex.Message.ShouldBe("something went wrong");
        ex.InnerException.ShouldBeNull();
    }

    [Fact]
    public void MessageConstructor_WithNull_CreatesExceptionWithDefaultMessage()
    {
        var ex = new MetalInjectionException(null);
        (ex.Message == "Exception of type 'RossWright.MetalInjection.MetalInjectionException' was thrown."
            ? null
            : ex.Message).ShouldBeNull();
        ex.InnerException.ShouldBeNull();
    }

    [Fact]
    public void MessageConstructor_WithEmptyString_RoundTrips()
    {
        var ex = new MetalInjectionException(string.Empty);
        ex.Message.ShouldBe(string.Empty);
        ex.InnerException.ShouldBeNull();
    }

    [Fact]
    public void IsAssignableFromException()
    {
        var ex = new MetalInjectionException("test");
        ex.ShouldBeAssignableTo<Exception>();
    }
}
