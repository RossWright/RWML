using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using RossWright.MetalChain;
using System.Reflection;

namespace RossWright.MetalShout;

public static class MetalShoutServerExtensions
{
    public static WebApplicationBuilder AddMetalShoutServer(this WebApplicationBuilder appBuilder, 
        Action<IMetalShoutServerOptionsBuilder>? setOptions = null)
    {
        MetalShoutServerOptionsBuilder optionsBuilder = new();
        if (setOptions != null) setOptions(optionsBuilder);
        optionsBuilder.InitializeServer(appBuilder.Services, appBuilder.Configuration);
        appBuilder.Services.AddMetalChainHandlers(typeof(PushRequestHandler<>));
        return appBuilder;
    }

    public static void UseMetalShoutServer(this IEndpointRouteBuilder app, string hubName = "PushHub") =>
        app.MapHub<PushServiceHub>(hubName);

    public static Task Push(this IMediator mediator, IRequest request, string? refId, Guid[] userIds, CancellationToken cancellationToken)
    {
        var sendViaRequestType = typeof(Push<>).MakeGenericType(request.GetType());
        var sendViaRequest = MetalActivator.CreateInstance(sendViaRequestType, request, refId, userIds)!;
        return mediator.Send(sendViaRequest);
    }
}
