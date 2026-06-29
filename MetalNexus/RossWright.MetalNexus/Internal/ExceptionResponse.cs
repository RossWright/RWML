using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Runtime.ExceptionServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RossWright.MetalNexus.Internal;

/// <summary>
/// Serialized representation of a server exception used by MetalNexus clients to reconstruct typed exceptions.
/// This type is public for cross-assembly serialization and should not be used directly by application code.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public class ExceptionResponse
{
    /// <summary>
    /// Creates an empty response for JSON deserialization.
    /// </summary>
    public ExceptionResponse() { }

    /// <summary>
    /// Creates a serialized response from an exception thrown by a server handler.
    /// </summary>
    /// <param name="exception">The exception to serialize.</param>
    /// <param name="includeStackTrace">Whether to include the server stack trace.</param>
    /// <param name="defaultToBadRequest">Whether unknown exception types map to 400 instead of 500.</param>
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

    /// <summary>
    /// Returns the HTTP status code that MetalNexus maps to <paramref name="exceptionType"/>.
    /// Unrecognised types map to <see cref="HttpStatusCode.BadRequest"/> (400).
    /// </summary>
    public static HttpStatusCode GetStatusCode(Type exceptionType)
    {
        if (exceptionType.IsAssignableTo(typeof(NotAuthenticatedException))) return HttpStatusCode.Unauthorized;
        if (exceptionType.IsAssignableTo(typeof(NotAuthorizedException)))    return HttpStatusCode.Forbidden;
        if (exceptionType.IsAssignableTo(typeof(NotFoundException)))         return HttpStatusCode.NotFound;
        if (exceptionType.IsAssignableTo(typeof(NotImplementedException)))   return HttpStatusCode.NotImplemented;
        if (exceptionType.IsAssignableTo(typeof(ValidationException)))       return HttpStatusCode.UnprocessableEntity;
        if (exceptionType.IsAssignableTo(typeof(InternalServerErrorException))) return HttpStatusCode.InternalServerError;
        return HttpStatusCode.BadRequest;
    }

    /// <summary>Gets or sets the assembly name containing the exception type.</summary>
    public string AssemblyName { get; set; } = null!;

    /// <summary>Gets or sets the full CLR type name of the exception.</summary>
    public string TypeName { get; set; } = null!;

    /// <summary>Gets or sets the exception message.</summary>
    public string Message { get; set; } = null!;

    /// <summary>Gets or sets the optional server stack trace.</summary>
    public string? StackTrace { get; set; } = null!;

    /// <summary>Gets or sets the serialized inner exception.</summary>
    public ExceptionResponse? Inner { get; set; }

    /// <summary>Gets the HTTP status code mapped to the exception.</summary>
    [JsonIgnore] public HttpStatusCode StatusCode { get; }

    /// <summary>
    /// Deserializes a MetalNexus error response into the closest matching exception type available on the client.
    /// </summary>
    /// <param name="statusCode">The HTTP response status code, when available.</param>
    /// <param name="json">The serialized exception response or raw error body.</param>
    /// <param name="includeStackOnException">Whether to attach a remote stack trace when one is present.</param>
    /// <returns>The reconstructed exception.</returns>
    public static Exception Deserialize(HttpStatusCode? statusCode, string? json, bool includeStackOnException)
    {
        if (json != null)
        {
            try
            {
                var exceptionResponse = JsonSerializer.Deserialize<ExceptionResponse>(json);
                if (exceptionResponse != null)
                {
                    var ex = exceptionResponse.ToException(includeStackOnException);
                    // If type-resolution fell back to MetalNexusException (unknown assembly/type),
                    // use the HTTP status code to reconstruct the correct typed exception instead.
                    if (ex is MetalNexusException && statusCode.HasValue)
                        return FromStatusCode(statusCode.Value, exceptionResponse.Message);
                    return ex;
                }
            }
            catch
            {
                // ignore if deserialization fails, we'll try to interpret based on status code below
            }
        }
        return statusCode switch
        {
            HttpStatusCode statusValue => FromStatusCode(statusValue, json),
            _ => string.IsNullOrWhiteSpace(json)
                ? new MetalNexusException("HTTP status code: ")
                : new MetalNexusException($"HTTP status code:  with response {json}")
        };
    }

    private static Exception FromStatusCode(HttpStatusCode statusCode, string? message) =>
        statusCode switch
        {
            HttpStatusCode.Unauthorized => new NotAuthenticatedException(message ?? ""),
            HttpStatusCode.Forbidden => new NotAuthorizedException(message ?? ""),
            HttpStatusCode.NotFound => new NotFoundException(message ?? ""),
            HttpStatusCode.NotImplemented => new NotImplementedException(message),
            HttpStatusCode.UnprocessableEntity => new ValidationException(message),
            HttpStatusCode.InternalServerError => new InternalServerErrorException(message),
            _ => string.IsNullOrWhiteSpace(message)
                ? new MetalNexusException($"HTTP status code: {statusCode}")
                : new MetalNexusException($"HTTP status code: {statusCode} with response {message}")
        };

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
                // Prefer (string, bool) overload when present: it is the message-passthrough
                // reconstruction constructor used by exceptions whose (string) ctor would
                // re-wrap the message (e.g. DuplicateEmailException treats the string as an email).
                constructor = exceptionType.GetConstructor([typeof(string), typeof(bool)]);
                if (constructor != null) exception = (Exception)constructor.Invoke([Message, true]);
            }
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
