namespace RossWright.MetalCore.Tests;

public class NotAuthenticatedExceptionTests
{
    [Fact]
    public void DefaultConstructor_MessageIsNull()
    {
        var ex = new NotAuthenticatedException();
        (ex.Message == "Exception of type 'RossWright.NotAuthenticatedException' was thrown."
            ? null
            : ex.Message).ShouldBeNull();
        ex.InnerException.ShouldBeNull();
    }

    [Fact]
    public void MessageConstructor_RoundTrips()
    {
        var ex = new NotAuthenticatedException("something went wrong");
        ex.Message.ShouldBe("something went wrong");
        ex.InnerException.ShouldBeNull();
    }

    [Fact]
    public void InnerExceptionConstructor_RoundTrips()
    {
        var inner = new InvalidOperationException("root cause");
        var ex = new NotAuthenticatedException(inner);
        ex.InnerException.ShouldBeSameAs(inner);
    }

    [Fact]
    public void MessageAndInnerExceptionConstructor_RoundTrips()
    {
        var inner = new InvalidOperationException("root cause");
        var ex = new NotAuthenticatedException("outer message", inner);
        ex.Message.ShouldBe("outer message");
        ex.InnerException.ShouldBeSameAs(inner);
    }

    [Fact]
    public void IsAssignableFromException()
    {
        var ex = new NotAuthenticatedException("test");
        ex.ShouldBeAssignableTo<Exception>();
    }
}
