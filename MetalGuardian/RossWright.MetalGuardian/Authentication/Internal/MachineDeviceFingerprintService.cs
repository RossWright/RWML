using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace RossWright.MetalGuardian.Authentication;

/// <summary>
/// A built-in <see cref="IDeviceFingerprintService"/> for console and desktop clients.
/// Produces a stable SHA-256 hash of machine-level signals: machine name, OS description,
/// processor count, and OS architecture. The fingerprint survives process restarts and
/// is suitable for MFA device-trust without any browser dependency.
///
/// Non-Blazor clients can register this via
/// <c>builder.UseDeviceFingerprinting&lt;MachineDeviceFingerprintService&gt;()</c>.
/// </summary>
internal class MachineDeviceFingerprintService : IDeviceFingerprintService
{
    // Computed once per process; the signals are constant for the lifetime of the machine.
    private static readonly Lazy<string> _fingerprint = new(Compute);

    public Task<string> GetFingerprint() => Task.FromResult(_fingerprint.Value);

    private static string Compute()
    {
        var signals = string.Join("|",
            Environment.MachineName,
            RuntimeInformation.OSDescription,
            RuntimeInformation.OSArchitecture.ToString(),
            Environment.ProcessorCount.ToString());

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(signals));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
