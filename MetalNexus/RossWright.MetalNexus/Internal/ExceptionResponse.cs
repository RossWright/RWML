using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Runtime.ExceptionServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RossWright.MetalNexus.Internal;

[EditorBrowsable(EditorBrowsableState.Never)]
public class ExceptionResponse
{
    public ExceptionResponse() { }
    public ExceptionResponse(Exception exception, 
        bool includeStackTrace = false, 
        bool defaultToBadRequest = true)
    {
        var exceptionType = exception.GetType();
        AssemblyName = exceptionType.Assembly.FullName!;
        TypeName = exceptionType.FullName!;
        Message = exception.Message;
        StackTrace = includeStackTrace ? exception.StackTrace : null;
        if (exception.InnerException != null)
        {
            Inner = new ExceptionResponse(exception.InnerException, includeStackTrace, defaultToBadRequest);
        }

        if (exceptionType.IsAssignableTo(typeof(NotAuthenticatedException)))
        {
            StatusCode = HttpStatusCode.Unauthorized;
        }
        else if (exceptionType.IsAssignableTo(typeof(NotAuthorizedException)))
        {
            StatusCode = HttpStatusCode.Forbidden;
        }
        else if (exceptionType.IsAssignableTo(typeof(NotFoundException)))
        {
            StatusCode = HttpStatusCode.NotFound;
        }
        else if (exceptionType.IsAssignableTo(typeof(NotImplementedException)))
        {
            StatusCode = HttpStatusCode.NotImplemented;
        }
        else if (exceptionType.IsAssignableTo(typeof(ValidationException)))
        {
            StatusCode = HttpStatusCode.UnprocessableEntity;
        }
        else if (exceptionType.IsAssignableTo(typeof(InternalServerErrorException)))
        {
            StatusCode = HttpStatusCode.InternalServerError;
        }
        else
        {
            StatusCode = defaultToBadRequest 
                ? HttpStatusCode.BadRequest 
                : HttpStatusCode.InternalServerError;
        }
    }

    public string AssemblyName { get; set; } = null!;
    public string TypeName { get; set; } = null!;
    public string Message { get; set; } = null!;
    public string? StackTrace { get; set; } = null!;
    public ExceptionResponse? Inner { get; set; }
    [JsonIgnore] public HttpStatusCode StatusCode { get; }

    public static Exception Deserialize(HttpStatusCode? statusCode, string? json, bool includeStackOnException)
    {
        if (json != null)
        {
            try
            {
                var exceptionResponse = JsonSerializer.Deserialize<ExceptionResponse>(json);
                if (exceptionResponse != null)
                {
                    return exceptionResponse.ToException(includeStackOnException);
                }
            }
            catch
            {
                // ignore if deserialization fails, we'll try to interopret based on status code below
            }
        }
        return statusCode switch
        {
            HttpStatusCode.Unauthorized => new NotAuthenticatedException(json ?? ""),
            HttpStatusCode.Forbidden => new NotAuthorizedException(json ?? ""),
            HttpStatusCode.NotFound => new NotFoundException(json ?? ""),
            HttpStatusCode.NotImplemented => new NotImplementedException(json),
            HttpStatusCode.UnprocessableEntity => new ValidationException(json),
            HttpStatusCode.InternalServerError => new InternalServerErrorException(json),
            _ => string.IsNullOrWhiteSpace(json)
                ? new MetalNexusException($"HTTP status code: {statusCode.ToString() ?? "null"}")
                : new MetalNexusException($"HTTP status code: {statusCode.ToString() ?? "null"} with response {json}")
        };
    }

    internal Exception ToException(bool includeStackOnException = false)
    {
        var assembly = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(_ => _.FullName == AssemblyName);
        var exceptionType = assembly?.GetType(TypeName);

        if (exceptionType != null)
        {
            Exception? exception = null;
            var constructor = exceptionType.GetConstructor([typeof(string), typeof(Exception)]);
            if (constructor != null) exception = (Exception)constructor.Invoke([Message, Inner?.ToException(includeStackOnException)]);
            if (exception == null)
            {
                constructor = exceptionType.GetConstructor([typeof(string)]);
                if (constructor != null) exception = (Exception)constructor.Invoke([Message]);
            }
            if (exception == null && Inner != null)
            {
                constructor = exceptionType.GetConstructor([typeof(Exception)]);
                if (constructor != null) exception = (Exception)constructor.Invoke([Inner?.ToException(includeStackOnException)]);
            }
            if (exception == null)
            {
                constructor = exceptionType.GetConstructor([]);
                if (constructor != null) exception = (Exception)constructor.Invoke([]);
            }
            if (exception != null)
            {
                if (StackTrace != null) exception = ExceptionDispatchInfo.SetRemoteStackTrace(exception, StackTrace);
                return exception;
            }
        }
        return new MetalNexusException($"Server Exception of type {TypeName}: {Message}", Inner?.ToException(includeStackOnException));
    }
}
