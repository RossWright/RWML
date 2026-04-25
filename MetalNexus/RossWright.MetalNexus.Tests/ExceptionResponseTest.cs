using RossWright.MetalNexus.Internal;
using System.Net;

namespace RossWright.MetalNexus.Tests;

public class ExceptionResponseTest
{
    Exception Throw(Exception exception)
    {
        try
        {
            throw exception;
        }
        catch (Exception ex)
        {
            return ex;
        }
    }

    [Fact] public void SerializeException()
    {
        Exception? sentException = Throw(new ApplicationException("Top",
            new KeyNotFoundException("Second", new NotImplementedException())));
        var exceptionResponse = new ExceptionResponse(sentException);
        exceptionResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        Exception? receivedException = exceptionResponse.ToException();

        receivedException.ShouldBeOfType<ApplicationException>();
        receivedException.Message.ShouldBe("Top");
        receivedException.InnerException.ShouldBeOfType<KeyNotFoundException>();
        receivedException.InnerException.Message.ShouldBe("Second");
        receivedException.InnerException.InnerException.ShouldBeOfType<NotImplementedException>();
    }

    [Fact] public void SerializeExceptionWithEmptyConstructor()
    {
        Exception? sentException = Throw(new ExceptionWithEmptyConstructor());
        var exceptionResponse = new ExceptionResponse(sentException);
        Exception? receivedException = exceptionResponse.ToException();

        receivedException.ShouldBeOfType<ExceptionWithEmptyConstructor>();
    }
    public class ExceptionWithEmptyConstructor : Exception { }

    [Fact] public void SerializeExceptionWithInnerConstructor()
    {
        Exception? sentException = Throw(new ExceptionWithInnerOnlyConstructor(
            new ApplicationException("Boom")));
        var exceptionResponse = new ExceptionResponse(sentException);
        Exception? receivedException = exceptionResponse.ToException();

        receivedException.ShouldBeOfType<ExceptionWithInnerOnlyConstructor>();
        receivedException.InnerException!.Message.ShouldBe("Boom");
    }
    public class ExceptionWithInnerOnlyConstructor : Exception
    {
        public ExceptionWithInnerOnlyConstructor(Exception? inner) : base(string.Empty, inner) { }
    }

    [Fact] public void SerializeExceptionWithMessageOnlyConstructor()
    {
        Exception? sentException = Throw(new ExceptionWithMessageOnlyConstructor("Boom"));
        var exceptionResponse = new ExceptionResponse(sentException);
        Exception? receivedException = exceptionResponse.ToException();

        receivedException.ShouldBeOfType<ExceptionWithMessageOnlyConstructor>();
        receivedException.Message.ShouldBe("Boom");
    }
    public class ExceptionWithMessageOnlyConstructor : Exception
    {
        public ExceptionWithMessageOnlyConstructor(string message) : base(message) { }
    }

    [Fact] public void SerializeUnauthorizedException()
    {
        Exception? sentException = Throw(new NotAuthenticatedException());
        var exceptionResponse = new ExceptionResponse(sentException);
        exceptionResponse.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        Exception? receivedException = exceptionResponse.ToException();
        receivedException.ShouldBeOfType<NotAuthenticatedException>();
    }

    [Fact] public void SerializeForbiddenException()
    {
        Exception? sentException = Throw(new NotAuthorizedException());
        var exceptionResponse = new ExceptionResponse(sentException);
        exceptionResponse.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        Exception? receivedException = exceptionResponse.ToException();
        receivedException.ShouldBeOfType<NotAuthorizedException>();
    }

    [Fact] public void SerializeNotFoundException()
    {
        Exception? sentException = Throw(new NotFoundException());
        var exceptionResponse = new ExceptionResponse(sentException);
        exceptionResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        Exception? receivedException = exceptionResponse.ToException();
        receivedException.ShouldBeOfType<NotFoundException>();
    }


    [Fact] public void SerializeExceptionWithOptionalParameterJustMessage()
    {
        Exception? sentException = Throw(new MetalNexusException("Test Message"));
        var exceptionResponse = new ExceptionResponse(sentException);
        Exception? receivedException = exceptionResponse.ToException();
        receivedException.ShouldBeOfType<MetalNexusException>();
        receivedException.Message.ShouldBe("Test Message");
    }

    [Fact] public void SerializeExceptionWithOptionalParameterWithBoth()
    {
        Exception? sentException = Throw(new MetalNexusException("Test Message", new NotImplementedException()));
        var exceptionResponse = new ExceptionResponse(sentException);
        Exception? receivedException = exceptionResponse.ToException();
        receivedException.ShouldBeOfType<MetalNexusException>();
        receivedException.Message.ShouldBe("Test Message");
        receivedException.InnerException.ShouldBeOfType<NotImplementedException>();
    }
}