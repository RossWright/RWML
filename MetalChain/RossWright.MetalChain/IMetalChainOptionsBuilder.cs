namespace RossWright.MetalChain;

/// <summary>
/// Provides configuration options for MetalChain DI registration, including assembly scanning
/// and per-type handler behaviour settings.
/// </summary>
public interface IMetalChainOptionsBuilder : IAssemblyScanningOptionsBuilder
{
    /// <summary>
    /// Allow queries with no registered handler to return <see langword="default"/> instead of throwing.
    /// Per-type <see cref="RequireHandlerAttribute"/> overrides this for specific types.
    /// </summary>
    IMetalChainOptionsBuilder AllowUnhandledQueries();

    /// <summary>
    /// Allow commands with no registered handler and no active listener to complete silently instead of throwing.
    /// Per-type <see cref="RequireHandlerAttribute"/> overrides this for specific types.
    /// </summary>
    IMetalChainOptionsBuilder AllowUnhandledCommands();

    /// <summary>
    /// Allow multiple distinct handler types to be registered for the same command type.
    /// All registered handlers are called in the specified <paramref name="mode"/> on dispatch.
    /// Per-type <see cref="AllowMultipleHandlersAttribute"/> overrides the execution mode for specific types.
    /// </summary>
    /// <param name="mode">The execution strategy to use when multiple handlers are registered. Defaults to <see cref="MultipleHandlerExecutionMode.SequentialFailFast"/> when omitted.</param>
    IMetalChainOptionsBuilder AllowMultipleCommandHandlers(
        MultipleHandlerExecutionMode mode = MultipleHandlerExecutionMode.SequentialFailFast);

    /// <summary>
    /// Excludes the specified handler type from all scanning and registration passes.
    /// Use to suppress a handler shipped by a scanned assembly that conflicts with your own.
    /// </summary>
    /// <param name="handlerType">The handler type to exclude from registration.</param>
    IMetalChainOptionsBuilder IgnoreHandler(Type handlerType);

    /// <summary>
    /// Excludes <typeparamref name="THandler"/> from all scanning and registration passes.
    /// </summary>
    /// <typeparam name="THandler">The handler type to exclude from registration.</typeparam>
    IMetalChainOptionsBuilder IgnoreHandler<THandler>();
}
