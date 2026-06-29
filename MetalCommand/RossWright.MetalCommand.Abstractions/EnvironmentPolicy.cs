namespace RossWright.MetalCommand;

/// <summary>
/// Controls how the MetalCommand environment middleware behaves when the selected environment
/// is marked as protected (e.g. production).
/// </summary>
public enum EnvironmentPolicy
{
    /// <summary>
    /// No restriction. The command proceeds without prompting, regardless of whether the
    /// environment is protected. Suitable for read-only or otherwise safe operations.
    /// </summary>
    Benign,

    /// <summary>
    /// The user must type <c>yes</c> at a confirmation prompt before the command runs against
    /// a protected environment. Suitable for operations that are reversible but impactful.
    /// </summary>
    Dangerous,

    /// <summary>
    /// Hard block — the command cannot run against a protected environment at all.
    /// The user is shown the list of permitted (non-protected) environments.
    /// Suitable for destructive operations such as database drops.
    /// </summary>
    Forbidden,
}
