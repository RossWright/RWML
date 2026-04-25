using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using RossWright.MetalChain;
using RossWright.MetalGuardian;
using System.Text.Json;

namespace RossWright.MetalShout;

internal class PushClientService : IPushClientService, IPushServiceConnector
{
    public PushClientService(
        IMetalGuardianAuthenticationClient accessTokenProvider,
        IBaseAddressRepository baseAddressRepository, 
        IMediator? mediator,
        string hubName, 
        JsonSerializerOptions jsonOptions)
    {
        _accessTokenProvider = accessTokenProvider;
        _accessTokenProvider.AuthenticationChanged += OnAuthenticationChanged;
        _baseAddressRepository = baseAddressRepository;
        _mediator = mediator;
        _hubName = hubName;
        _jsonOptions = jsonOptions;
    }
    private readonly IMetalGuardianAuthenticationClient _accessTokenProvider;
    private readonly IBaseAddressRepository _baseAddressRepository;
    private readonly Dictionary<string, Connection> _connections = new();
    private readonly IMediator? _mediator;
    private readonly string _hubName;
    private readonly JsonSerializerOptions _jsonOptions;

    public async Task Connect(
        string? connectionName = null,
        CancellationToken cancellationToken = default)
    {
        Connection connection = SetupConnection(ref connectionName);
        await AuthenticateConnection(connection, cancellationToken);
    }

    public async Task Subscribe<TMessage>(
        IPushSubscriber<TMessage> subscriber, 
        string? connectionName = null, 
        CancellationToken cancellationToken = default) 
        where TMessage : class
    {
        Connection connection = SetupConnection(ref connectionName);
        connection.Subscriptions.AddToList(typeof(TMessage), subscriber);
        await AuthenticateConnection(connection, cancellationToken);
    }

    public async Task Unsubscribe<TMessage>(
        IPushSubscriber<TMessage>? subscriber, 
        string? connectionName = null, 
        CancellationToken cancellationToken = default) 
        where TMessage : class
    {
        if (subscriber == null) return;
        connectionName ??= _baseAddressRepository.DefaultConnectionName;
        if (_connections.TryGetValue(connectionName, out var connection))
        {
            connection.Subscriptions.RemoveFromList(typeof(TMessage), subscriber);
            if (!connection.Subscriptions.AnyInAnyList())
            {
                await connection.Disconnect(cancellationToken);
            }
        }
    }

    private Connection SetupConnection(ref string? connectionName)
    {
        connectionName ??= _baseAddressRepository.DefaultConnectionName;
        if (!_connections.TryGetValue(connectionName, out var connection))
        {
            connection = new Connection(_mediator, connectionName, _jsonOptions);
            _connections.Add(connectionName, connection);
        }
        return connection;
    }

    private async Task AuthenticateConnection(Connection connection, CancellationToken cancellationToken)
    {
        if (_accessTokenProvider.IsAuthenticated())
        {
            await connection.Connect(_accessTokenProvider,
                _baseAddressRepository.GetBaseAddress(connection.ConnectionName),
                _hubName, cancellationToken);
        }
    }

    private async Task OnAuthenticationChanged(string connectionName,
        IAuthenticationInformation? accessToken, CancellationToken cancellationToken = default)
    {
        connectionName ??= _baseAddressRepository.DefaultConnectionName;
        if (_connections.TryGetValue(connectionName, out var connection))
        {
            if (accessToken == null)
            {
                await connection.Disconnect(cancellationToken);
            }
            else if (connection.Subscriptions.AnyInAnyList())
            {
                await connection.Connect(_accessTokenProvider,
                    _baseAddressRepository.GetBaseAddress(connection.ConnectionName),
                    _hubName, cancellationToken);
            }
        }
    }

    private class Connection(
            IMediator? _mediator,
            string _connectionName,
            JsonSerializerOptions _jsonOptions)
    {
        public string ConnectionName { get; } = _connectionName;
        public Dictionary<Type, IList<object>> Subscriptions { get; } = new();

        private HubConnection? _hubConnection;

        public async Task Connect(
            IMetalGuardianAuthenticationClient accessTokenProvider, 
            string serverUrlBase, 
            string hubName,
            CancellationToken cancellationToken)
        {
            if (_hubConnection != null) return;
            var url = Tools.CombineUrl(serverUrlBase, hubName);
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(url, opt =>
                {
                    opt.Transports = HttpTransportType.WebSockets;
                    opt.SkipNegotiation = true;
                    opt.AccessTokenProvider = async () =>
                        (await accessTokenProvider.Authenticate(ConnectionName))?.Token;
                })
                .WithAutomaticReconnect()
                .AddJsonProtocol(options => options.PayloadSerializerOptions = _jsonOptions)
                .Build();
            _hubConnection.On<string, JsonElement>("Push", OnPushRecevied);
            _hubConnection.HandshakeTimeout = TimeSpan.FromSeconds(30);
            await _hubConnection.StartAsync(cancellationToken);
        }

        public async Task Disconnect(CancellationToken cancellationToken)
        {
            if (_hubConnection == null) return;
            await _hubConnection.StopAsync(cancellationToken);
            await _hubConnection.DisposeAsync();
            _hubConnection = null;
        }

        public async Task OnPushRecevied(string typeName, JsonElement jsonElement)
        {;
            var type = Type.GetType(typeName);
            if (type is null) return;

            var json = jsonElement.GetRawText();
            var value = JsonSerializer.Deserialize(json, type, jsonOpts);
            if (value is null) return;
                        
            if (_mediator != null) await _mediator.Send(value);

            var handlers = Subscriptions.GetList(type);
            if (handlers != null)
            {
                var handlerMethod = typeof(IPushSubscriber<>)
                    .MakeGenericType(type)
                    .GetMethod(nameof(IPushSubscriber<object>.OnPushReceived))!;
                foreach (var handler in handlers)
                {
                    await (Task)handlerMethod.Invoke(handler, [value])!;
                }
            }
        }

        private readonly static JsonSerializerOptions jsonOpts =
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
    }
}