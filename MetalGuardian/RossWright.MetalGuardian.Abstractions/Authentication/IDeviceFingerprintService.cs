namespace RossWright.MetalGuardian;

public interface IDeviceFingerprintService
{
    Task<string> GetFingerprint();
}
