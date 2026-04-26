namespace RossWright.MetalCore.Tests;

public class NotFoundExceptionTests
{
    [Fact]
    public void DefaultConstructor_MessageIsNull()
    {
        var ex = new NotFoundException();
        (ex.Message == "Exception of type 'RossWright.NotFoundException' was thrown."
            ? null
            : ex.Message).ShouldBeNull();
        ex.InnerException.ShouldBeNull();
    }

    [Fact]
    public void MessageConstructor_RoundTrips()
    {
        var ex = new NotFoundException("resource not found");
        ex.Message.ShouldBe("resource not found");
        ex.InnerException.ShouldBeNull();
    }

    [Fact]
    public void InnerExceptionConstructor_RoundTrips()
    {
        var inner = new InvalidOperationException("root cause");
        var ex = new NotFoundException(inner);
        ex.InnerException.ShouldBeSameAs(inner);
        (ex.Message == "Exception of type 'RossWright.NotFoundException' was thrown."
            ? null
            : ex.Message).ShouldBeNull();
    }

    [Fact]
    public void MessageAndInnerExceptionConstructor_RoundTrips()
    {
        var inner = new InvalidOperationException("root cause");
        var ex = new NotFoundException("outer message", inner);
        ex.Message.ShouldBe("outer message");
        ex.InnerException.ShouldBeSameAs(inner);
    }

    [Fact]
    public void IsAssignableFromException()
    {
        var ex = new NotFoundException("test");
        ex.ShouldBeAssignableTo<Exception>();
    }
}
