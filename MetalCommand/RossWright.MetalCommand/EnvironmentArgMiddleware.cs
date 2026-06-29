using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace RossWright.MetalCommand;

/// <summary>
/// Middleware that enforces <see cref="EnvironmentPolicy"/> for properties decorated
/// with <see cref="EnvironmentArgAttribute"/>. Run after argument binding; invokes the
/// next pipeline step only when all policy checks pass.
/// </summary>
/// <remarks>
/// Confirmation for <see cref="EnvironmentPolicy.Dangerous"/> is scoped to a single
/// top-level command invocation via the <c>__runId</c> key in
/// <see cref="CommandContext.SessionContext"/>. Sub-commands called by an aggregate
/// command (e.g. Reload) share the same run ID and therefore share the confirmation —
/// the user is only prompted once. A fresh run ID is set for each new user-typed command,
/// so typing the same command again always re-prompts.
/// </remarks>
public sealed class EnvironmentArgMiddleware(IServiceProvider services) : ICommandMiddleware
{
    /// <inheritdoc />
    public async Task InvokeAsync(CommandContext context, Func<CommandContext, Task> next)
    {
        var optionsRegistries = services.GetServices<ICommandOptionsRegistry>().ToList();

        var envArgProperties = context.Command.GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.GetCustomAttribute<EnvironmentArgAttribute>() != null)
            .ToArray();

        foreach (var prop in envArgProperties)
        {
            var attr = prop.GetCustomAttribute<EnvironmentArgAttribute>()!;
            var effectivePolicy = optionsRegistries
                .Select(r => r.Get(context.Command.GetType())?.EnvironmentPolicy)
                .FirstOrDefault(p => p != null)
                ?? attr.Policy;

            if (effectivePolicy == EnvironmentPolicy.Benign)
                continue;

            var boundValue = prop.GetValue(context.Command) as string;
            if (boundValue is null)
                continue;

            var sourceType = attr.EnvironmentSourceType ?? typeof(IEnvironmentSource);
            var source = (IEnvironmentSource?)services.GetService(sourceType);
            if (source is null)
                continue;

            var entry = source.Environments.FirstOrDefault(
                e => string.Equals(e.Name, boundValue, StringComparison.OrdinalIgnoreCase));

            if (entry is null || !entry.IsProtected)
                continue;

            if (effectivePolicy == EnvironmentPolicy.Forbidden)
            {
                var safe = source.Environments.Where(e => !e.IsProtected).Select(e => e.Name).ToArray();
                context.Console.WriteErrorLine(
                    "That environment cannot be used with this command" +
                    (safe.Length > 0
                        ? $", try using {safe.CommaListJoin("or")}"
                        : ", no valid environments are available for this command"));
                context.Result = CommandResult.Fail();
                return;
            }

            // Dangerous — require typed confirmation, scoped to the current run ID.
            var runId = context.SessionContext.TryGetValue("__runId", out var id) ? id : null;
            var confirmationKey = $"__confirmed:{boundValue}:{runId}";

            if (runId != null && context.SessionContext.ContainsKey(confirmationKey))
                continue; // already confirmed within this top-level invocation

            var descriptor = CommandDescriptorFactory.FromAttributes(context.Command.GetType());
            context.Console.Write(
                $"You are about to run \"{descriptor.Name}\" against the protected environment \"{boundValue}\". " +
                $"Type \"yes\" to confirm: ");

            if (context.Console.ReadLine() != "yes")
            {
                context.Console.WriteErrorLine("Command aborted");
                context.Result = CommandResult.Fail();
                return;
            }

            if (runId != null)
                context.SessionContext[confirmationKey] = "true";
        }

        await next(context);
    }
}
