using System.ComponentModel;

namespace RossWright.MetalGuardian.Internal;


[EditorBrowsable(EditorBrowsableState.Never)]
public static class MetalGuardianClaimTypes
{
    public const string ProvisionalLogin = "http://schemas.rosswright.com/ws/2026/01/metalguardian/claims/provisionallogin";
    public const string IsKnownDevice = "http://schemas.rosswright.com/ws/2026/01/metalguardian/claims/isknowndevice";
}
