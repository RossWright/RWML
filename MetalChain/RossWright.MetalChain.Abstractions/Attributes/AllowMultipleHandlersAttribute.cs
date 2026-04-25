namespace RossWright.MetalChain;

/// <summary>
/// Applied to an <see cref="IRequest"/> type: allow multiple distinct handler types to be registered.
/// All registered handlers are called according to the specified <see cref="ExecutionMode"/>
/// when the command is dispatched.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class AllowMultipleHandlersAttribute : Attribute
{
    /// <summary>
    /// Controls how the registered handlers are invoked. Defaults to <see cref="MultipleHandlerExecutionMode.SequentialFailFast"/>.
    /// </summary>
    public MultipleHandlerExecutionMode ExecutionMode { get; init; } = MultipleHandlerExecutionMode.SequentialFailFast;
}
