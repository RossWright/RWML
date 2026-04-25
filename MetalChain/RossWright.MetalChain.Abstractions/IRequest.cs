namespace RossWright.MetalChain;

/// <summary>Marker interface for commands that produce no response.</summary>
public interface IRequest { }
/// <summary>Marker interface for queries that produce a <typeparamref name="TResponse"/> result.</summary>
/// <typeparam name="TResponse">The type returned by the handler.</typeparam>
public interface IRequest<out TResponse> { }
