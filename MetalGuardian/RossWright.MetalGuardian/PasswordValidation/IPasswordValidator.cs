using System.Diagnostics.CodeAnalysis;

namespace RossWright.MetalGuardian;

/// <summary>
/// Validates passwords against a configured set of <see cref="PasswordRequirements"/>.
/// </summary>
public interface IPasswordValidator
{
    /// <summary>
    /// Returns a human-readable description of the active password requirements,
    /// optionally including a note that forbidden fragments are disallowed.
    /// </summary>
    string GetPasswordRequirementsMessage(params string?[] forbiddenFragments);

    /// <summary>
    /// Returns <c>true</c> if <paramref name="password"/> satisfies all active requirements
    /// and does not contain any of the supplied <paramref name="forbiddenFragments"/> (case-insensitive).
    /// Returns <c>false</c> if any requirement is unmet or a forbidden fragment is present.
    /// </summary>
    bool ValidatePassword([NotNullWhen(true)] string? password, params string?[] forbiddenFragments);
}