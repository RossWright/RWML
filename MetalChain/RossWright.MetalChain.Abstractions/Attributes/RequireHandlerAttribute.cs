namespace RossWright.MetalChain;

/// <summary>
/// Applied to an <see cref="IRequest"/> or <see cref="IRequest{TResponse}"/> type:
/// throw if no handler (and no listener for commands) is found, even when the global
/// setting would otherwise be permissive.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class RequireHandlerAttribute : Attribute { }
