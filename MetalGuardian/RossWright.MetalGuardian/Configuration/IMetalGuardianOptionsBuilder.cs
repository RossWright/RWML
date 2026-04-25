namespace RossWright.MetalGuardian;

public interface IMetalGuardianOptionsBuilder
{
    void UseMetalNexusAuthenticationEndpoints();

    void UsePasswordValidator(Action<PasswordRequirements>? configure = null);
}