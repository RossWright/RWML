using RossWright.MetalChain;
using System.Reflection;

namespace RossWright.MetalNexus;

public static class SendViaExtensions
{
    public static Task SendVia(this IMediator mediator, string connectionName, IRequest request, CancellationToken cancellationToken)
    {
        var sendViaRequestType = typeof(SendVia<>).MakeGenericType(request.GetType());
        var sendViaRequest = MetalActivator.CreateInstance(sendViaRequestType, connectionName, request)!;
        return mediator.Send(sendViaRequest);
    }

    public static async Task<TResponse?> SendVia<TResponse>(this IMediator mediator, string connectionName, IRequest<TResponse> request, CancellationToken cancellationToken)
    {
        var sendViaRequestType = typeof(SendVia<,>).MakeGenericType(request.GetType(), typeof(TResponse));
        var sendViaRequest = MetalActivator.CreateInstance(sendViaRequestType, connectionName, request)!;
        return (TResponse?) await mediator.Send(sendViaRequest);
    }
}
