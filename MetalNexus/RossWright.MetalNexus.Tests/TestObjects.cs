using RossWright.MetalChain;

namespace RossWright.MetalNexus.Tests;

public static class EmptyCommand
{
    [ApiRequest(path: "command")]
    public class Request : IRequest { }

    public class Handler : IRequestHandler<Request>
    {
        public Task Handle(Request request, CancellationToken cancellationToken) => throw new NotImplementedException();
    }
}

public static class EmptyQuery
{
    [ApiRequest(path: "query")]
    public class Request : IRequest<Response> { }

    public class Response { }

    public class Handler : IRequestHandler<Request, Response>
    {
        public Task<Response> Handle(Request request, CancellationToken cancellationToken) => throw new NotImplementedException();
    }
}

[ApiRequest(path: "badrequest")]
public class BadRequestMissingIRequest { }

[ApiRequest(path: "filecommand")]
public class FileRequest : MetalNexusFileRequest, IRequest { }

[ApiRequest(path: "has-complex-property")]
public class HasComplexPropertyRequest : IRequest
{
    public Thing TheThing { get; set; } = null!;
}

[ApiRequest(path: "has-simple-array")]
public class SimpleArrayRequest : IRequest
{
    public string[] StringArray { get; set; } = null!;
}

[ApiRequest(path: "has-2d-array")]
public class Simple2dArrayRequest : IRequest
{
    public string[,] StringArray { get; set; } = null!;
}

[ApiRequest(path: "has-complex-array")]
public class ComplexArrayRequest : IRequest
{
    public Thing[] ThingArray { get; set; } = null!;
}
public class Thing { }


[ApiRequest]
public class RequestWith4Properties : IRequest
{
    public string? Prop1 { get; set; }
    public string? Prop2 { get; set; }
    public string? Prop3 { get; set; }
    public string? Prop4 { get; set; }
}

[ApiRequest]
public class RequestWith5Properties : IRequest
{
    public string? Prop1 { get; set; }
    public string? Prop2 { get; set; }
    public string? Prop3 { get; set; }
    public string? Prop4 { get; set; }
    public string? Prop5 { get; set; }
}