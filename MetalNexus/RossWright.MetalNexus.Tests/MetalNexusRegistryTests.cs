using RossWright.MetalNexus.Internal;
using Shouldly;

namespace RossWright.MetalNexus.Tests;

public class MetalNexusRegistryTests
{
    [Fact] public void AddEndpoints_ToClient()
    {
        List<Type> addedToChain = new();
        MetalNexusRegistry registry = new(addedToChain.Add, new EndpointSchemaOptions(), null, _isServer: false, null);

        registry.AddEndpoints(
            typeof(EmptyQuery.Request),
            typeof(EmptyQuery.Response),
            typeof(EmptyQuery.Handler),
            typeof(EmptyCommand.Request),
            typeof(EmptyCommand.Handler));

        registry.AddEndpoints(typeof(EmptyCommand.Request)); // send this one again

        Assert.Collection(addedToChain,
            _ => _.ShouldBe(typeof(ApiRequestHandlerWithResponse<EmptyQuery.Request, EmptyQuery.Response>)),
            _ => _.ShouldBe(typeof(ApiRequestHandler<EmptyCommand.Request>)),
            _ => _.ShouldBe(typeof(ApiRequestHandler<EmptyCommand.Request>)));

        Assert.Collection(registry.Endpoints.OrderBy(_ => _.Path),
            _ =>
            {
                _.RequestType.ShouldBe(typeof(EmptyCommand.Request));
                _.ResponseType.ShouldBeNull();
            },
            _ =>
            {
                _.RequestType.ShouldBe(typeof(EmptyQuery.Request));
                _.ResponseType.ShouldBe(typeof(EmptyQuery.Response));
            });

        registry.FindEndpoint(typeof(EmptyCommand.Request)).ShouldNotBeNull();
        registry.FindEndpoint(typeof(EmptyCommand.Handler)).ShouldBeNull();
        registry.FindEndpoint(typeof(EmptyQuery.Request)).ShouldNotBeNull();
        registry.FindEndpoint(typeof(EmptyQuery.Response)).ShouldBeNull();
        registry.FindEndpoint(typeof(EmptyQuery.Handler)).ShouldBeNull();
    }

    [Fact] public void AddEndpoints_ToServer()
    {
        List<Type> addedToChain = new();
        MetalNexusRegistry registry = new(addedToChain.Add, new EndpointSchemaOptions(), null, _isServer: true, null);

        registry.AddEndpoints(
            typeof(EmptyQuery.Request),
            typeof(EmptyQuery.Response),
            typeof(EmptyQuery.Handler),
            typeof(EmptyCommand.Request),
            typeof(EmptyCommand.Handler));

        registry.AddEndpoints(typeof(EmptyCommand.Handler)); // send this one again

        Assert.Collection(addedToChain,
            _ => _.ShouldBe(typeof(EmptyQuery.Handler)),
            _ => _.ShouldBe(typeof(EmptyCommand.Handler)),
            _ => _.ShouldBe(typeof(EmptyCommand.Handler)));

        Assert.Collection(registry.Endpoints.OrderBy(_ => _.Path),
            _ =>
            {
                _.RequestType.ShouldBe(typeof(EmptyCommand.Request));
                _.ResponseType.ShouldBeNull();
            },
            _ =>
            {
                _.RequestType.ShouldBe(typeof(EmptyQuery.Request));
                _.ResponseType.ShouldBe(typeof(EmptyQuery.Response));
            });

        registry.FindEndpoint(typeof(EmptyCommand.Request)).ShouldNotBeNull();
        registry.FindEndpoint(typeof(EmptyCommand.Handler)).ShouldBeNull();
        registry.FindEndpoint(typeof(EmptyQuery.Request)).ShouldNotBeNull();
        registry.FindEndpoint(typeof(EmptyQuery.Response)).ShouldBeNull();
        registry.FindEndpoint(typeof(EmptyQuery.Handler)).ShouldBeNull();
    }

    [Fact] public void DefineEndpoint_Defaults()
    {
        MetalNexusRegistry registry = new(_ => { }, new EndpointSchemaOptions(), null, _isServer: false, null);
        var ep = registry.DefineEndpoint(typeof(EmptyCommand.Request));
        ep.ShouldNotBeNull();
        ep.RequestType.ShouldBe(typeof(EmptyCommand.Request));
        ep.ResponseType.ShouldBeNull();
        ep.HttpClientName.ShouldBeNull();
        ep.HttpMethod.ShouldBe(HttpMethod.Get);
        ep.Path.ShouldBe("/command");
        ep.RequestAsQueryParams.ShouldBeTrue();
        ep.HasPathParams.ShouldBeFalse();
        ep.Tag.ShouldBe("Command");
        ep.RequiresAuthentication.ShouldBeTrue();
        ep.AuthorizedRoles.ShouldBeNull();
        ep.AllowProvisional.ShouldBeFalse();
        ep.HttpClientTimeout.ShouldBeNull();
        ep.HeaderProperties.ShouldBeEmpty();


        Should.Throw<MetalNexusException>(() => registry.DefineEndpoint(typeof(BadRequestMissingIRequest)));
    }


    [Fact] public void IsValidBracketUrlParsing()
    {
        MetalNexusRegistry.IsValidBracketUrlParsing("{First}", typeof(Test)).ShouldBeTrue();
        MetalNexusRegistry.IsValidBracketUrlParsing("/first", typeof(Test)).ShouldBeTrue();
        MetalNexusRegistry.IsValidBracketUrlParsing("/first/second", typeof(Test)).ShouldBeTrue();
        MetalNexusRegistry.IsValidBracketUrlParsing("/first/second/third", typeof(Test)).ShouldBeTrue();
        MetalNexusRegistry.IsValidBracketUrlParsing("/{First}", typeof(Test)).ShouldBeTrue();
        MetalNexusRegistry.IsValidBracketUrlParsing("/first/{Second}", typeof(Test)).ShouldBeTrue();
        MetalNexusRegistry.IsValidBracketUrlParsing("/{First}/second", typeof(Test)).ShouldBeTrue();
        MetalNexusRegistry.IsValidBracketUrlParsing("/{First}/{Second}", typeof(Test)).ShouldBeTrue();
        MetalNexusRegistry.IsValidBracketUrlParsing("/{First}/second/third", typeof(Test)).ShouldBeTrue();
        MetalNexusRegistry.IsValidBracketUrlParsing("/first/{Second}/third", typeof(Test)).ShouldBeTrue();
        MetalNexusRegistry.IsValidBracketUrlParsing("/first/second/{Third}", typeof(Test)).ShouldBeTrue();
        MetalNexusRegistry.IsValidBracketUrlParsing("/{First}/{Second}/third", typeof(Test)).ShouldBeTrue();
        MetalNexusRegistry.IsValidBracketUrlParsing("/first/{Second}/{Third}", typeof(Test)).ShouldBeTrue();
        MetalNexusRegistry.IsValidBracketUrlParsing("/{First}/second/{Third}", typeof(Test)).ShouldBeTrue();
        MetalNexusRegistry.IsValidBracketUrlParsing("/{First}/{Second}/{Third}", typeof(Test)).ShouldBeTrue();

        MetalNexusRegistry.IsValidBracketUrlParsing("first{Second}", typeof(Test)).ShouldBeFalse();
        MetalNexusRegistry.IsValidBracketUrlParsing("{First}second", typeof(Test)).ShouldBeFalse();
        MetalNexusRegistry.IsValidBracketUrlParsing("}{", typeof(Test)).ShouldBeFalse();
        MetalNexusRegistry.IsValidBracketUrlParsing("/{First", typeof(Test)).ShouldBeFalse();
        MetalNexusRegistry.IsValidBracketUrlParsing("/first/second}", typeof(Test)).ShouldBeFalse();
        MetalNexusRegistry.IsValidBracketUrlParsing("/{First/second", typeof(Test)).ShouldBeFalse();
        MetalNexusRegistry.IsValidBracketUrlParsing("/{Obj}", typeof(Test)).ShouldBeFalse();
        MetalNexusRegistry.IsValidBracketUrlParsing("/{{Obj}", typeof(Test)).ShouldBeFalse();
    }

    public class Test
    {
        public string? First { get; set; }
        public string? Second { get; set; }
        public string? Third { get; set; }
        public object? Obj { get; set; }
    }
}