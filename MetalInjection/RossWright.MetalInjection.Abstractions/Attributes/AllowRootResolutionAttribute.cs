namespace RossWright.MetalInjection;

/// <summary>
/// Applied to a scoped service implementation class: suppresses the scoped-from-root guard
/// for this specific type, allowing it to be resolved directly from the root
/// <see cref="System.IServiceProvider"/> without creating a scope first.
/// </summary>
/// <remarks>
/// This attribute is a no-op on singleton and transient implementations — the guard never
/// fires for those lifetimes and the attribute is silently ignored.
/// Use <c>options.AllowRootScopedResolution()</c> to suppress the guard globally for all
/// scoped services.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class AllowRootResolutionAttribute : Attribute { }
