# MetalChain API Index

Primary namespace: `RossWright`.

## RossWright.IRequest

Package: `RossWright.MetalChain.Abstractions`  
Namespace: `RossWright`  
Signature: `public interface IRequest`  
Summary: Marker interface for command requests that do not return a response.

## RossWright.IRequest<TResponse>

Package: `RossWright.MetalChain.Abstractions`  
Namespace: `RossWright`  
Signature: `public interface IRequest<TResponse>`  
Summary: Marker interface for query requests that return `TResponse`.

## RossWright.IRequestHandler<TRequest>

Package: `RossWright.MetalChain.Abstractions`  
Namespace: `RossWright`  
Summary: Handles command requests.

## RossWright.IRequestHandler<TRequest, TResponse>

Package: `RossWright.MetalChain.Abstractions`  
Namespace: `RossWright`  
Summary: Handles query requests and returns `TResponse`.

## RossWright.IMediator.Send

Package: `RossWright.MetalChain.Abstractions`  
Namespace: `RossWright`  
Summary: Dispatches a command or query request to its registered handler.

## RossWright.IMediator.SendOrDefault

Package: `RossWright.MetalChain.Abstractions`  
Namespace: `RossWright`  
Summary: Dispatches a query and returns default when no handler is registered.

## RossWright.IMediator.SendOrIgnore

Package: `RossWright.MetalChain.Abstractions`  
Namespace: `RossWright`  
Summary: Dispatches a command and completes silently when no handler/listener is registered.

## RossWright.MetalChainExtensions.AddMetalChain

Package: `RossWright.MetalChain`  
Namespace: `RossWright`  
Summary: Registers MetalChain and scans or explicitly registers request handlers.

## RossWright.AllowNoHandlerAttribute

Package: `RossWright.MetalChain.Abstractions`  
Namespace: `RossWright`  
Summary: Marks a request type as allowed to have no registered handler.

## RossWright.AllowMultipleHandlersAttribute

Package: `RossWright.MetalChain.Abstractions`  
Namespace: `RossWright`  
Summary: Allows multiple command handlers for a request type.
