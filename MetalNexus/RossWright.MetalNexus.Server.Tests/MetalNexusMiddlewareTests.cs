using RossWright.MetalNexus.Schemna;
using RossWright.MetalNexus.Server;

namespace RossWright.MetalNexus.Tests;

public class MetalNexusMiddlewareTests
{

    [Fact] public void FillSlotValues_HappyPath()
    {
        var obj = new Test();
        MetalNexusMiddleware.FillSlotValues(new StubEndpoint<Test>("/{First}"), "/firstValue", obj);
        obj.First.ShouldBe("firstValue");

        MetalNexusMiddleware.FillSlotValues(new StubEndpoint<Test>("/First/{Second}/{Third}"), "/first/secondValue/thirdValue", obj);
        obj.Second.ShouldBe("secondValue");
        obj.Third.ShouldBe("thirdValue");
    }

    public class Test
    {
        public string? First { get; set; }
        public string? Second { get; set; }
        public string? Third { get; set; }
        public object? Obj { get; set; }
    }

    public class StubEndpoint<TRequest>(string path) : IEndpoint
    {

        public string Path { get; } = path;

        public Type RequestType { get; } = typeof(TRequest);

        public Type? ResponseType => throw new NotImplementedException();

        public string? HttpClientName => throw new NotImplementedException();

        public HttpMethod HttpMethod => throw new NotImplementedException();

        public bool RequestAsQueryParams => throw new NotImplementedException();

        public string? Tag => throw new NotImplementedException();

        public bool RequiresAuthentication => throw new NotImplementedException();

        public string[]? AuthorizedRoles => throw new NotImplementedException();
        public bool IsProvisional => throw new NotImplementedException();
        public bool AllowProvisional => throw new NotImplementedException();

        public TimeSpan? HttpClientTimeout => throw new NotImplementedException();

        public bool HasPathParams => throw new NotImplementedException();

        public string[] HeaderProperties => throw new NotImplementedException();
    }
}

