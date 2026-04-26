namespace RossWright.MetalCore.Tests;

public class NotAuthorizedExceptionTests
{
    [Fact]
    public void DefaultConstructor_MessageIsNull()
    {
        var ex = new NotAuthorizedException();
        (ex.Message == "Exception of type 'RossWright.NotAuthorizedException' was thrown."
            ? null
            : ex.Message).ShouldBeNull();
        ex.InnerException.ShouldBeNull();
    }

    [Fact]
    public void MessageConstructor_RoundTrips()
    {
        var ex = new NotAuthorizedException("something went wrong");
        ex.Message.ShouldBe("something went wrong");
        ex.InnerException.ShouldBeNull();
    }

    [Fact]
    public void InnerExceptionConstructor_RoundTrips()
    {
        var inner = new InvalidOperationException("root cause");
        var ex = new NotAuthorizedException(inner);
        ex.InnerException.ShouldBeSameAs(inner);
    }

    [Fact]
    public void MessageAndInnerExceptionConstructor_RoundTrips()
    {
        var inner = new InvalidOperationException("root cause");
        var ex = new NotAuthorizedException("outer message", inner);
        ex.Message.ShouldBe("outer message");
        ex.InnerException.ShouldBeSameAs(inner);
    }

    [Fact]
    public void IsAssignableFromException()
    {
        var ex = new NotAuthorizedException("test");
        ex.ShouldBeAssignableTo<Exception>();
    }
}
