using RossWright.MetalInjection;
using System.Collections.Concurrent;
namespace RossWright.MetalChain;

internal interface IMetalChainRegistry 
{
    void AddHandlers(params Type[] handlerTypes);
    bool HasHandlerFor(Type requestType);
    bool HasListenerFor(Type requestType);

    void AddListener(Type requestType, Func<object, CancellationToken, Task> listener);
    void RemoveListener(Type requestType, Func<object, CancellationToken, Task> listener);

    Task<object?> Handle(IServiceProvider serviceProvider, object request, CancellationToken cancellationToken);
}

internal class MetalChainRegistry : IMetalChainRegistry
{
    internal readonly ConcurrentDictionaryOfLists<Type, Type> _commandHandlers = new();
    internal readonly ConcurrentDictionary<Type, Type> _queryHandlers = new();
    internal readonly ConcurrentDictionaryOfLists<Type, Func<object, CancellationToken, Task>> _listeners = new();

    public ILoadLog? LoadLog { get; internal set; }

    internal bool AllowUnhandledQueries { get; set; }
    internal bool AllowUnhandledCommands { get; set; }
    internal bool AllowMultipleCommandHandlers { get; set; }
    internal MultipleHandlerExecutionMode DefaultCommandExecutionMode { get; set; } = MultipleHandlerExecutionMode.SequentialFailFast;
    internal HashSet<Type> IgnoredHandlers { get; } = [];

    public void AddHandlers(params Type[] handlerTypes)
    {
        foreach (var handlerType in handlerTypes)
        {
            if (IgnoredHandlers.Contains(handlerType))
                continue;

            var interfaces = handlerType.GetInterfaces();
            foreach (var handlerInterface in interfaces
                .Where(interfaceType => interfaceType.IsGenericType &&
                    (interfaceType.GetGenericTypeDefinition() == typeof(IRequestHandler<>) ||
                    interfaceType.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)) &&
                    !interfaces.Any(_ => _ != interfaceType && _.IsAssignableTo(interfaceType))))
            {
                LoadLog?.LogTrace($"Found Request Handler: {handlerType} implements {handlerInterface}");
                var handlerParams = handlerInterface.GetGenericArguments();
                var requestType = handlerParams[0];
                var responseType = handlerParams.Length > 1 ? handlerParams[1] : null;
                if (requestType.ContainsGenericParameters)
                    requestType = requestType.GetGenericTypeDefinition();

                if (handlerInterface.GetGenericTypeDefinition() == typeof(IRequestHandler<>))
                {
                    // Same handler type is always a silent skip (idempotent registration)
                    if (_commandHandlers.ContainsKey(requestType, handlerType))
                        continue;

                    // Different handler type: enforce multicast guard unless explicitly allowed
                    if (_commandHandlers.ContainsKey(requestType) &&
                        !AllowMultipleCommandHandlers &&
                        !requestType.IsDefined(typeof(AllowMultipleHandlersAttribute), inherit: false))
                    {
                        throw new MetalChainException(
                            $"A handler for command '{requestType.Name}' is already registered. " +
                            $"Multiple command handlers are not permitted by default. " +
                            $"Use AllowMultipleCommandHandlers() globally or [AllowMultipleHandlers] on the request type to enable multicast fan-out.");
                    }

                    _commandHandlers.Add(requestType, handlerType);
                }
                else
                {
                    // Duplicate query handler is always forbidden — nondeterministic dispatch
                    if (_queryHandlers.ContainsKey(requestType))
                    {
                        throw new MetalChainException(
                            $"A handler for query '{requestType.Name}' is already registered. " +
                            $"Multiple query handlers for the same type are not permitted. " +
                            $"Use Listen to fan out side-effects, or IgnoreHandler<T>() to suppress a conflicting handler from a scanned assembly.");
                    }

                    _queryHandlers.TryAdd(requestType, handlerType);
                }
            }
        }
    }

    public bool HasHandlerFor(Type requestType) =>
        _commandHandlers.ContainsKey(requestType) ||
        _queryHandlers.ContainsKey(requestType);

    public bool HasListenerFor(Type requestType) =>
        _listeners.ContainsKey(requestType);

    public void AddListener(Type requestType, Func<object, CancellationToken, Task> listener) =>
        _listeners.Add(requestType, listener);

    public void RemoveListener(Type requestType, Func<object, CancellationToken, Task> listener) =>
        _listeners.Remove(requestType, listener);

    public async Task<object?> Handle(IServiceProvider serviceProvider, object request, CancellationToken cancellationToken)
    {
        var requestType = request.GetType();

        var commandHandlers = _commandHandlers.GetValuesOrEmptySet(requestType)
           .Concat(!requestType.IsGenericType ? [] : _commandHandlers
                .GetValuesOrEmptySet(requestType.GetGenericTypeDefinition())
                .Select(_ => _.MakeGenericType(requestType.GetGenericArguments())))
           .ToList();

        var listenerTasks = _listeners
            .GetValuesOrEmptySet(requestType)
            .Select(listener => listener(request, cancellationToken))
            .ToList();

        // Unhandled command check
        if (!commandHandlers.Any() && !listenerTasks.Any())
        {
            var allowNoHandler = requestType.IsDefined(typeof(AllowNoHandlerAttribute), inherit: false);
            var requireHandler = requestType.IsDefined(typeof(RequireHandlerAttribute), inherit: false);
            if (requireHandler || (!allowNoHandler && !AllowUnhandledCommands))
            {
                // Only throw for pure commands (IRequest but not IRequest<TResponse>)
                var isQueryRequest = requestType.GetInterfaces()
                    .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>));
                if (!isQueryRequest)
                    throw new MetalChainException(
                        $"No handler or listener registered for command '{requestType.Name}'. " +
                        $"Implement and register IRequestHandler<{requestType.Name}>, use AllowUnhandledCommands(), or apply [AllowNoHandler] to the request type.");
            }
        }

        if (commandHandlers.Any())
            await DispatchCommandHandlers(serviceProvider, request, requestType, commandHandlers, cancellationToken);

        object? result = null;
        if (!_queryHandlers.TryGetValue(requestType, out var queryHandler) && requestType.IsGenericType &&
            _queryHandlers.TryGetValue(requestType.GetGenericTypeDefinition(), out var genericHandlerType))
        {
            queryHandler = genericHandlerType.MakeGenericType(requestType.GetGenericArguments());
        }

        // Unhandled query check
        if (queryHandler == null && requestType.GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>)))
        {
            var allowNoHandler = requestType.IsDefined(typeof(AllowNoHandlerAttribute), inherit: false);
            var requireHandler = requestType.IsDefined(typeof(RequireHandlerAttribute), inherit: false);
            if (requireHandler || (!allowNoHandler && !AllowUnhandledQueries))
                throw new MetalChainException(
                    $"No handler registered for query type '{requestType.Name}'. " +
                    $"Implement and register IRequestHandler<{requestType.Name}, TResponse>, use AllowUnhandledQueries(), or apply [AllowNoHandler] to the request type.");
        }

        if (queryHandler != null)
        {
            var handler = serviceProvider.CreateInstance(queryHandler);
            var queryInterface = queryHandler.GetInterfaces()
                .First(i => i.IsGenericType &&
                            i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>) &&
                            i.GetGenericArguments()[0] == requestType);
            var methodInfo = queryInterface.GetMethod("Handle", [requestType, typeof(CancellationToken)])!;
            var taskObj = methodInfo.Invoke(handler, [request, cancellationToken])!;
            var task = (Task)taskObj;
            await task;
            var resultProperty = task.GetType().GetProperty("Result")!;
            result = resultProperty.GetValue(task);
        }

        if (listenerTasks.Any())
            await Task.WhenAll(listenerTasks);

        return result;
    }

    private async Task DispatchCommandHandlers(
        IServiceProvider serviceProvider,
        object request,
        Type requestType,
        List<Type> commandHandlers,
        CancellationToken cancellationToken)
    {
        var executionMode = requestType.GetCustomAttributes(typeof(AllowMultipleHandlersAttribute), inherit: false)
            .Cast<AllowMultipleHandlersAttribute>()
            .FirstOrDefault()?.ExecutionMode ?? DefaultCommandExecutionMode;

        switch (executionMode)
        {
            case MultipleHandlerExecutionMode.SequentialFailFast:
                foreach (var handlerType in commandHandlers)
                    await InvokeCommandHandler(serviceProvider, handlerType, request, requestType, cancellationToken);
                break;

            case MultipleHandlerExecutionMode.SequentialCollectErrors:
            {
                List<Exception>? errors = null;
                foreach (var handlerType in commandHandlers)
                {
                    try
                    {
                        await InvokeCommandHandler(serviceProvider, handlerType, request, requestType, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        (errors ??= []).Add(ex);
                    }
                }
                if (errors != null)
                    throw new AggregateException(errors);
                break;
            }

            case MultipleHandlerExecutionMode.ParallelCollectErrors:
            {
                var tasks = commandHandlers
                    .Select(h => InvokeCommandHandler(serviceProvider, h, request, requestType, cancellationToken))
                    .ToList();
                try
                {
                    await Task.WhenAll(tasks);
                }
                catch
                {
                    var exceptions = tasks
                        .Where(t => t.IsFaulted)
                        .SelectMany(t => t.Exception!.InnerExceptions)
                        .ToList();
                    throw new AggregateException(exceptions);
                }
                break;
            }
        }
    }

    private static Task InvokeCommandHandler(
        IServiceProvider serviceProvider,
        Type handlerType,
        object request,
        Type requestType,
        CancellationToken cancellationToken)
    {
        var handler = serviceProvider.CreateInstance(handlerType);
        var commandInterface = handlerType.GetInterfaces()
            .First(i => i.IsGenericType &&
                        i.GetGenericTypeDefinition() == typeof(IRequestHandler<>) &&
                        i.GetGenericArguments()[0] == requestType);
        var methodInfo = commandInterface.GetMethod("Handle", [requestType, typeof(CancellationToken)])!;
        return (Task)methodInfo.Invoke(handler, [request, cancellationToken])!;
    }
}
