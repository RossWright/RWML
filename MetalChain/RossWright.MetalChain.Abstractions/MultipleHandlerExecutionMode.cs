namespace RossWright.MetalChain;

/// <summary>
/// Controls how multiple registered command handlers are invoked when multicast fan-out is enabled.
/// </summary>
public enum MultipleHandlerExecutionMode
{
    /// <summary>
    /// Handlers run one at a time in registration order.
    /// The first exception stops the chain and propagates to the caller.
    /// Default — safe for handlers that share resources or have ordering dependencies.
    /// </summary>
    SequentialFailFast,

    /// <summary>
    /// Handlers run one at a time in registration order.
    /// All handlers are given a chance to run; any exceptions are collected
    /// and thrown together as an AggregateException after the last handler completes.
    /// </summary>
    SequentialCollectErrors,

    /// <summary>
    /// Handlers run concurrently via Task.WhenAll.
    /// Any exceptions are collected and thrown as an AggregateException.
    /// Use only when handlers are independent — no shared connections, locks, or rate limits.
    /// </summary>
    ParallelCollectErrors,
}
