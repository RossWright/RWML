namespace RossWright.MetalChain;

/// <summary>
/// Applied to an <see cref="IRequest"/> or <see cref="IRequest{TResponse}"/> type:
/// return default / complete silently when no handler is found, instead of throwing.
/// Overrides the global default. Use <see cref="RequireHandlerAttribute"/> to opt back in to strict behavior.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class AllowNoHandlerAttribute : Attribute { }
