using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;

namespace RossWright.MetalGuardian;

/// <summary>
/// Base implementation for MetalGuardian option builders.
/// This type is public for package-to-package infrastructure use; application code should use the public builder interfaces.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public abstract class MetalGuardianOptionsBuilder
    : OptionsBuilder,
    IMetalGuardianOptionsBuilder
{
    /// <inheritdoc />
    public void UsePasswordValidator(Action<PasswordRequirements>? configure = null)
    {
        PasswordRequirements passwordRequirements = new();
        if (configure != null) configure(passwordRequirements);
        AddServices(_ => _.AddSingleton<IPasswordValidator>(
            _ => new PasswordValidator(passwordRequirements)));
    }

    /// <inheritdoc />
    public abstract void UseMetalNexusAuthenticationEndpoints();
}
