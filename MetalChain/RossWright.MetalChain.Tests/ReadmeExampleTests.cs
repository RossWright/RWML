using Microsoft.Extensions.DependencyInjection;

namespace RossWright.MetalChain.Tests;

#pragma warning disable CS8618

public class ReadmeExampleTests
{
    public class Builder
    {
        public IServiceCollection Services { get; }
    }

    public void JustSeeIfItCompiles()
    {
        var builder = new Builder();

        builder.Services.AddMetalChain(options =>
        {
            options.ScanThisAssembly();
            options.ScanAssemblyContaining<SendNotificationHandler>();
        });

        builder.Services.AddMetalChainHandlers(
            typeof(CreateUserCommandHandler),
            typeof(UpdateUserCommandHandler));

        builder.Services.AddMetalChain();

        builder.Services.AddMetalChainHandlers(typeof(GetUserByIdHandler));
    }

    public class CreateUserCommand : IRequest { }
    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand>
    {
        public Task Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
    public class UpdateUserCommand : IRequest { }
    public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand>
    {
        public Task Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }

    public class SendNotificationCommand : IRequest
    {
        public string UserId { get; set; }
        public string Message { get; set; }
    }

    public interface INotificationService
    {
        Task SendAsync(string userId, string message, CancellationToken cancellationToken);
    }

    public class SendNotificationHandler(
        INotificationService _notificationService)
    : IRequestHandler<SendNotificationCommand>
    {
        public async Task Handle(SendNotificationCommand request, CancellationToken cancellationToken)
        {
            await _notificationService.SendAsync(request.UserId, request.Message, cancellationToken);
        }
    }

    public class GetUserByIdQuery : IRequest<UserDto>
    {
        public string UserId { get; set; }
    }

    public class UserDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }

    public interface IUserRepository
    {
        Task<UserDto> GetByIdAsync(string userId, CancellationToken cancellationToken);
    }

    public class GetUserByIdHandler(
        IUserRepository _userRepository)
        : IRequestHandler<GetUserByIdQuery, UserDto>
    {
        public async Task<UserDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
            return new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email
            };
        }
    }

    public class SomeService(IMediator _mediator)
    {
        public async Task DoStuff(CancellationToken cancellationToken)
        {
            await _mediator.Send(new SendNotificationCommand(), cancellationToken);
        }
    }

    public class SomeService2(IMediator _mediator)
    {
        public async Task DoStuff(CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(new GetUserByIdQuery(), cancellationToken);
            // ... use response ...
        }
    }

    public class Outer
    {
        public async Task DoStuff(IMediator mediator)
        {
            var disposable = mediator.Listen<SendNotificationCommand>(
                async (request, cancellationToken) =>
                {
                    Console.WriteLine($"Notification sent to {request.UserId}: {request.Message}");
                    await Task.CompletedTask;
                });

            // Later, when no longer needed, dispose to stop listening
            disposable.Dispose();
        }
    }

    public class MyDbContent { }

    public class DataQueryHandler
        : IRequestHandler<GetUserByIdQuery, UserDto>,
        IRequestHandler<CreateUserCommand>
    {
        public async Task<UserDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            return new UserDto();
        }

        public async Task Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            // create the user using the DbContext
        }
    }

    public interface ISomeThing
    {
        Task Log(string context, CancellationToken cancellationToken);
    }

    public class PostResultCommand<TResult> : IRequest
        where TResult : ISomeThing
    {
        public TResult SomeThing { get; set; }
        public string Context { get; set; }
    }

    public class PostResultCommandHandler<TResult> : IRequestHandler<PostResultCommand<TResult>>
        where TResult : ISomeThing
    {
        public async Task Handle(PostResultCommand<TResult> request, CancellationToken cancellationToken)
        {
            await request.SomeThing.Log(request.Context, cancellationToken); //where DoStuff is defined on ISomeThing
        }
    }

    public class SomeThingImpl : ISomeThing
    {
        public Task Log(string context, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
    public class OtherSomeThingImpl : ISomeThing
    {
        public Task Log(string context, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }

    public class DoStuffClass(IMediator _mediator)
    {
        public async Task DoStuff(CancellationToken cancellationToken)
        {
            var someThing = new SomeThingImpl();
            var someOtherThing = new OtherSomeThingImpl();
            var postResultCommand = new PostResultCommand<SomeThingImpl>
            {
                SomeThing = someThing
            };
            await _mediator.Send(postResultCommand, cancellationToken);

            var postOtherResultCommand = new PostResultCommand<OtherSomeThingImpl>
            {
                SomeThing = someOtherThing
            };
            await _mediator.Send(postOtherResultCommand, cancellationToken);
        }
    }


    public class QueryWrapper<TRequest, TResponse> : IRequest<TResponse>
        where TRequest : IRequest<TResponse>
    {
        public TRequest Request { get; set; }
        public string UserId { get; set; }
        public string Context { get; set; }
    }

    public class QueryWrapperRequestHandler<TRequest, TResponse>(IMediator _mediator)
        : IRequestHandler<QueryWrapper<TRequest, TResponse>, TResponse>
        where TRequest : IRequest<TResponse>
    {
        public async Task<TResponse> Handle(
            QueryWrapper<TRequest, TResponse> wrapper,
            CancellationToken cancellationToken)
        {
            await _mediator.Send(new SendNotificationCommand()
            {
                UserId = wrapper.UserId,
                Message = wrapper.Context + " Starting..."
            }, cancellationToken);

            TResponse response;
            try
            {
                response = await _mediator.Send(wrapper.Request, cancellationToken);
            }
            catch (Exception exception)
            {
                await _mediator.Send(new SendNotificationCommand()
                {
                    UserId = wrapper.UserId,
                    Message = wrapper.Context + " Failed with error: " + exception.Message
                }, cancellationToken);
                return default!;
            }

            await _mediator.Send(new SendNotificationCommand()
            {
                UserId = wrapper.UserId,
                Message = wrapper.Context +
                    " Completed with response: " +
                    (response?.ToString() ?? "<null>")
            }, cancellationToken);

            return response;
        }
    }

    public class Blah
    {

        public async Task Foo(IMediator _mediator)
        {
            string queryUserId = Guid.NewGuid().ToString();
            string executingUserId = Guid.NewGuid().ToString();


            var wrapper = new QueryWrapper<GetUserByIdQuery, UserDto>
            {
                Request = new GetUserByIdQuery { UserId = queryUserId },
                UserId = executingUserId,
                Context = "Administaion Page"
            };

            var userDto = await _mediator.Send(wrapper);
        }
    }
}

#pragma warning restore CS8618