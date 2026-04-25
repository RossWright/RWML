using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;

namespace RossWright.MetalGuardian;

[EditorBrowsable(EditorBrowsableState.Never)]
public abstract class MetalGuardianOptionsBuilder
    : OptionsBuilder,
    IMetalGuardianOptionsBuilder
{
    public void UsePasswordValidator(Action<PasswordRequirements>? configure = null)
    {
        PasswordRequirements passwordRequirements = new();
        if (configure != null) configure(passwordRequirements);
        AddServices(_ => _.AddSingleton<IPasswordValidator>(
            _ => new PasswordValidator(passwordRequirements)));
    }

    public abstract void UseMetalNexusAuthenticationEndpoints();
}
