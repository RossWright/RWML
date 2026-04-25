using System.Diagnostics.CodeAnalysis;

namespace RossWright.MetalGuardian;

public interface IPasswordValidator
{
    string GetPasswordRequirementsMessage(params string?[] forbiddenFragments);
    bool ValidatePassword([NotNullWhen(true)] string? password, params string?[] forbiddenFragments);
}