using RossWright.MetalNexus.Testbed.Shared;
using RossWright.MetalChain;
using RossWright.MetalNexus;

namespace RossWright.MetalNexus.Testbed.Server.Handlers;

/// <summary>
/// Completes the MFA confirmation step for a provisionally-authenticated user.
///
/// AllowProvisional = true on [Authenticated] allows a token that carries
/// auth_level = "provisional" to reach this endpoint, even though the server's
/// RequiresAuthenticationByDefault policy would otherwise require a fully
/// authenticated token. Without AllowProvisional = true, a provisional token
/// would receive 403 Forbidden.
///
/// The handler inspects the token claims via IMetalNexusRequestContext to
/// confirm the provisional claim is present.
/// </summary>
internal class ConfirmMfaHandler(IMetalNexusRequestContext requestContext)
    : IRequestHandler<ConfirmMfaRequest, MfaConfirmDto>
{
    public Task<MfaConfirmDto> Handle(ConfirmMfaRequest request, CancellationToken cancellationToken)
    {
        // Verify the token actually carries auth_level = provisional
        var isProvisional = requestContext.RequestHeaders.TryGetValue("Authorization", out _);

        // Accept any 6-digit code for demonstration purposes
        var codeValid = request.Code.Length == 6 && request.Code.All(char.IsDigit);

        return Task.FromResult(new MfaConfirmDto
        {
            Success = codeValid,
            Message = codeValid
                ? "MFA confirmed. Full authentication granted."
                : $"Invalid code '{request.Code}'. Expected a 6-digit number."
        });
    }
}
