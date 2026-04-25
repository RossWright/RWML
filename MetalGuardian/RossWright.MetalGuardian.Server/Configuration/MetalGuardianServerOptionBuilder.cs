using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using RossWright.MetalGuardian.Authentication;
using RossWright.MetalGuardian.MetalNexus;
using RossWright.MetalGuardian.OneTimePassword;
using RossWright.MetalNexus;
using System.Linq.Expressions;

namespace RossWright.MetalGuardian;

internal sealed class MetalGuardianServerOptionBuilder
    : MetalGuardianOptionsBuilder,
    IMetalGuardianServerOptionBuilder
{
    public void UseJwtConfiguration(IMetalGuardianServerConfiguration configuration) =>
        _jwtConfiguration = configuration;
    private IMetalGuardianServerConfiguration? _jwtConfiguration;
    public void UseJwtConfigurationSection(string sectionName) =>
        _configurationSectionName = sectionName;
    private string? _configurationSectionName;

    public override void UseMetalNexusAuthenticationEndpoints() =>
        _includeMetalNexusRequestHandlers = true;
    private bool _includeMetalNexusRequestHandlers;

    public void UseAuthenticationRepository<TAuthenticationRepository>()
        where TAuthenticationRepository : class, IAuthenticationRepository
    {
        if (_authenticationRepositoryFactory != null)
        {
            throw new MetalGuardianException("You may only call one of: UseAuthenticationRepository, " +
                "MapDatabaseAuthentication or MapDatabaseAuthenticationWithDevices");
        }
        _authenticationRepositoryType = typeof(TAuthenticationRepository);
    }
    private Type? _authenticationRepositoryType;
    
    public void MapDatabaseAuthentication<TDbContext, TUser, TRefreshToken>(
        Func<string, Expression<Func<TUser, bool>>> userIdentityPredicate)
        where TDbContext : DbContext, IMetalGuardianDbContext<TUser, TRefreshToken>
        where TUser : class, IAuthenticationUser
        where TRefreshToken : class, IRefreshToken, new()
    {
        if (_authenticationRepositoryType != null)
        {
            throw new MetalGuardianException("You may only call one of: UseAuthenticationRepository, " +
                "MapDatabaseAuthentication or MapDatabaseAuthenticationWithDevices");
        }
        _authenticationRepositoryFactory = _ =>
            new AuthenticationRepository<TDbContext, TUser, TRefreshToken>(
                _.GetRequiredService<TDbContext>(), userIdentityPredicate);
    }
    private Func<IServiceProvider, IAuthenticationRepository>? _authenticationRepositoryFactory;

    public void UseUserDeviceRepository<TUserDeviceRepository>()
        where TUserDeviceRepository : class, IUserDeviceRepository
    {
        if (_userDeviceRepositoryFactory != null)
        {
            throw new MetalGuardianException("You may only call one of: UseUserDeviceRepository or MapDatabaseAuthenticationWithDevices");
        }
        _userDeviceRepositoryType = typeof(TUserDeviceRepository);
    }
    private Type? _userDeviceRepositoryType;
    
    public void MapDatabaseAuthenticationWithDevices<TDbContext, TUser, TRefreshToken, TUserDevice>(
        Func<string, Expression<Func<TUser, bool>>> userIdentityPredicate)
        where TDbContext : DbContext, IMetalGuardianDbContext<TUser, TRefreshToken, TUserDevice>
        where TUser : class, IAuthenticationUser
        where TRefreshToken : class, IRefreshToken, new()
        where TUserDevice : class, IUserDevice, new()
    {
        if (_authenticationRepositoryType != null)
        {
            throw new MetalGuardianException("You may only call one of: UseAuthenticationRepository, " +
                "MapDatabaseAuthentication or MapDatabaseAuthenticationWithDevices");
        }
        if (_userDeviceRepositoryType != null)
        {
            throw new MetalGuardianException("You may only call one of: UseUserDeviceRepository or MapDatabaseAuthenticationWithDevices");
        }
        MapDatabaseAuthentication<TDbContext, TUser, TRefreshToken>(userIdentityPredicate);
        _userDeviceRepositoryFactory = _ =>
            new UserDeviceRepository<TDbContext, TUser, TRefreshToken, TUserDevice>(
                _.GetRequiredService<TDbContext>());
    }
    private Func<IServiceProvider, IUserDeviceRepository>? _userDeviceRepositoryFactory;


    public void UseUserClaimsProvider<TUserClaimsProvider>()
        where TUserClaimsProvider : class, IUserClaimsProvider =>
        _userClaimsProviderTypes.Add(typeof(TUserClaimsProvider));
    private List<Type> _userClaimsProviderTypes = new();

    public void AddUserClaimMapping<TUser>(string claimName, Func<TUser, string?> getValue)
        where TUser : class, IAuthenticationUser
    {
        _simpleUserClaimsProvider ??= new SimpleUserClaimsProviderImpl<TUser>();
        if (_simpleUserClaimsProvider is SimpleUserClaimsProviderImpl<TUser> simpleUserClaimsProviderImpl)
        {
            simpleUserClaimsProviderImpl.ClaimFuncs.Add((claimName, getValue));
        }
        else
        { 
            var existingUserType = _simpleUserClaimsProvider.GetType().GetGenericArguments()[0];
            throw new MetalGuardianException($"Only one user data model can be used. " +
                $"This call used {typeof(TUser)} when {existingUserType} was already established");
        }        
    }

    public void AddUserClaimsArrayMapping<TUser>(string claimName, Func<TUser, string[]> getValues)
        where TUser : class, IAuthenticationUser
    {
        _simpleUserClaimsProvider ??= new SimpleUserClaimsProviderImpl<TUser>();
        if (_simpleUserClaimsProvider is SimpleUserClaimsProviderImpl<TUser> simpleUserClaimsProviderImpl)
        {
            simpleUserClaimsProviderImpl.ClaimArrayFuncs.Add((claimName, getValues));
        }
        else
        {
            var existingUserType = _simpleUserClaimsProvider.GetType().GetGenericArguments()[0];
            throw new MetalGuardianException($"Only one user data model can be used. " +
                $"This call used {typeof(TUser)} when {existingUserType} was already established");
        }
    }
    private IUserClaimsProvider? _simpleUserClaimsProvider;

    public sealed class SimpleUserClaimsProviderImpl<TUser> 
        : IUserClaimsProvider
        where TUser : class, IAuthenticationUser
    {
        public List<(string, Func<TUser, string?>)> ClaimFuncs { get; } = new();
        public List<(string, Func<TUser, string[]> getValues)> ClaimArrayFuncs { get; } = new();
        public Task<IEnumerable<(string, string)>?> GetClaims(
            IAuthenticationUser user, 
            CancellationToken cancellationToken)
        {
            var singleClaims = ClaimFuncs
                .Select(_ => (_.Item1, _.Item2((TUser)user)))
                .Where(_ => !string.IsNullOrWhiteSpace(_.Item2))
                .Select(_ => (_.Item1, _.Item2!));

            var arrayClaims = ClaimArrayFuncs
                .SelectMany(_ => _.getValues((TUser)user)
                    .Where(_ => !string.IsNullOrWhiteSpace(_))
                    .Select(value => (_.Item1, value.Trim('"'))));

            var claims = singleClaims.Concat(arrayClaims).ToArray();
            if (claims.Length == 0) claims = null;
            return Task.FromResult<IEnumerable<(string, string)>?>(claims);
        }
    }

    public void UseOneTimePassword(Action<OneTimePasswordOptions>? configure = null)
    {
        _oneTimePasswordOptions = new();
        if (configure != null) configure(_oneTimePasswordOptions);
    }
    private OneTimePasswordOptions? _oneTimePasswordOptions;

    public void InitializeServer(IServiceCollection services, IConfiguration configuration)
    {
        var jwtConfiguration = _jwtConfiguration ?? new MetalGuardianServerConfiguration();
        if (_jwtConfiguration == null)
            configuration.Bind(_configurationSectionName ?? "MetalGuardian", jwtConfiguration);
        if (string.IsNullOrWhiteSpace(jwtConfiguration.JwtIssuer)) 
            throw new MetalGuardianException("Invalid configuration MetalGuardian.JwtIssuer");
        if (string.IsNullOrWhiteSpace(jwtConfiguration.JwtAudience)) 
            throw new MetalGuardianException("Invalid configuration MetalGuardian.JwtAudience");
        if (string.IsNullOrWhiteSpace(jwtConfiguration.JwtIssuerSigningKey)) 
            throw new MetalGuardianException("Invalid configuration MetalGuardian.JwtIssuerSigningKey");
        services.AddSingleton(jwtConfiguration);

        if (_includeMetalNexusRequestHandlers)
        {
            services.AddMetalNexusEndpoints(
                typeof(LoginRequestHandler),
                typeof(LogoutRequestHandler),
                typeof(RefreshRequestHandler));
        }

        if (_authenticationRepositoryType != null)
        {
            services.AddScoped(typeof(IAuthenticationRepository), _authenticationRepositoryType);
        }
        else if (_authenticationRepositoryFactory != null)
        {
            services.AddScoped(typeof(IAuthenticationRepository), _authenticationRepositoryFactory);
        }
        else
        {
            throw new MetalGuardianException("An Authentication repository must be registered. " +
                "In the initialialization of MetalGuardian you must call UseAuthenticationRepository, " +
                "MapDatabaseAuthentication or MapDatabaseAuthenticationWithDevices");
        }

        if (_userDeviceRepositoryType != null)
        {
            services.AddScoped(typeof(IUserDeviceRepository), _userDeviceRepositoryType);
        }
        else if (_userDeviceRepositoryFactory != null)
        {
            services.AddScoped(typeof(IUserDeviceRepository), _userDeviceRepositoryFactory);
        }

        AccessTokenFactory accessTokenFactory = new(jwtConfiguration);
        services.AddSingleton<IAccessTokenFactory>(_ => accessTokenFactory);

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = accessTokenFactory.StrictTokenValidationParameters;
                options.Events = new JwtBearerEvents
                {
                    // This is for signalr and direct link authentication
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        if (!string.IsNullOrEmpty(accessToken))
                            context.Token = accessToken;
                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                            context.Response.Headers["Token-Expired"] = "true";
                        return Task.CompletedTask;
                    }
                };
            });

        services.AddScoped<IMetalGuardianAuthenticationService, MetalGuardianAuthenticationService>();

        foreach (var userClaimsProviderType in _userClaimsProviderTypes)
        {
            services.AddScoped(typeof(IUserClaimsProvider), userClaimsProviderType);
        }
        if (_simpleUserClaimsProvider != null)
        {
            services.AddScoped(typeof(IUserClaimsProvider),_ => _simpleUserClaimsProvider);
        }

        if (_oneTimePasswordOptions != null)
        {
            if (_oneTimePasswordOptions.NumberOfDigits < 4 ||
                _oneTimePasswordOptions.NumberOfDigits > 24)
                throw new MetalGuardianException("One time password length must be between 4 and 24 digits");

            services.AddSingleton<IOtpRepository>(_ => new InMemoryOtpRepository(
                _oneTimePasswordOptions.NumberOfDigits,
                _oneTimePasswordOptions.ExpirationInMinutes));
            services.AddScoped<IOtpService, OtpService>();
        }

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();
        AddServices(services);
    }
}
