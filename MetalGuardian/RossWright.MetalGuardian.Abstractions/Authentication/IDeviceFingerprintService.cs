namespace RossWright.MetalGuardian;

/// <summary>
/// Returns a stable device identifier string used for device-trust decisions.
/// The Blazor implementation derives a fingerprint from browser signals.
/// For non-Blazor applications, register <c>MachineDeviceFingerprintService</c> or provide a custom implementation.
/// </summary>
public interface IDeviceFingerprintService
{
    /// <summary>Returns a stable string that uniquely identifies the current device.</summary>
    Task<string> GetFingerprint();
}
