using RossWright.MetalChain;

namespace RossWright.MetalNexus;

public class SendVia<TRequest>(string connectionName, TRequest request) 
    : IRequest
    where TRequest : IRequest
{
    public string ConnectionName => connectionName;
    public TRequest Request => request;
}

public class SendVia<TRequest, TResponse>(string connectionName, TRequest request)
    : IRequest<TResponse> where TRequest : IRequest<TResponse>
{
    public string ConnectionName => connectionName;
    public TRequest Request => request;
}