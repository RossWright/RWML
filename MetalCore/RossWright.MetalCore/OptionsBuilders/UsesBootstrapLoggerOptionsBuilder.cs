using Microsoft.Extensions.Logging;

namespace RossWright;

/// <summary>
/// Extends <see cref="IOptionsBuilder"/> with the ability to attach an
/// <see cref="ILogger"/> for diagnostic output during options setup.
/// </summary>
public interface IUsesBootstrapLoggerOptionsBuilder : IOptionsBuilder
{
    /// <summary>
    /// Provides a factory to create a logger that receives diagnostic messages during configuration.
    /// Pass <see langword="null"/> to disable all diagnostic output.
    /// </summary>
    /// <param name="bootstrapLoggingFactory">
    /// The <see cref="ILoggerFactory"/> to use, or <see langword="null"/> to suppress output.
    /// </param>
    void UseBootstrapLogger(ILoggerFactory? bootstrapLoggingFactory);

    /// <summary>
    /// Used by IAssemblyScanningBuilderExtensions only.
    /// </summary>
    /// <returns></returns>
    internal ILogger? GetBootstrapLogger();
}

/// <summary>
/// Extension helpers for <see cref="IUsesBootstrapLoggerOptionsBuilder"/>.
/// </summary>
public static class IUsesBootstrapLoggerOptionsBuilderExtensions
{
    /// <summary>Disables all diagnostic bootstrap logging output for this options builder.</summary>
    /// <param name="builder">The options builder to silence.</param>
    public static void DoNotUseLogger(this IUsesBootstrapLoggerOptionsBuilder builder) => 
        builder.UseBootstrapLogger(null);

    /// <summary>
    /// Configures a bootstrap logger using the provided <see cref="ILoggingBuilder"/> delegate,
    /// creating the <see cref="ILoggerFactory"/> automatically.
    /// </summary>
    /// <param name="builder">The options builder to configure.</param>
    /// <param name="configure">A delegate to configure logging providers.</param>
    public static void UseBootstrapLogger(this IUsesBootstrapLoggerOptionsBuilder builder,
        Action<ILoggingBuilder> configure) =>
        builder.UseBootstrapLogger(LoggerFactory.Create(configure));
}

/// <summary>
/// Base implementation of <see cref="IUsesBootstrapLoggerOptionsBuilder"/> that stores an
/// optional <see cref="ILogger"/> and stamps each message with a module name.
/// </summary>
public class UsesBootstrapLoggerOptionsBuilder(string category)
    : OptionsBuilder,
    IUsesBootstrapLoggerOptionsBuilder
{
    /// <inheritdoc/>
    public void UseBootstrapLogger(ILoggerFactory? bootstrapLoggerFactory) =>
        _bootstrapLoggerFactory = bootstrapLoggerFactory;
    private ILoggerFactory? _bootstrapLoggerFactory =
#if DEBUG
    LoggerFactory.Create(logging =>
    {
        logging.ClearProviders();
        logging.AddMetalConsoleLogger();
        logging.AddDebug();
        logging.SetMinimumLevel(LogLevel.Debug);
    });
#else
    null;
#endif
    /// <inheritdoc/>
    public ILogger? GetBootstrapLogger() =>
        _bootstrapLoggerFactory?.CreateLogger(category);
}
