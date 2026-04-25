using OtpNet;
using QRCoder;

namespace RossWright.MetalGuardian;

internal class MetalGuardianTotpMfaService(
    string _issuer, int? _deviceRemmemberDays,
    IAuthenticationRepository _authRepo,
    IMetalGuardianAuthenticationService _authSvc,
    IUserDeviceRepository? _userDeviceRepository = null) 
    : IMetalGuardianTotpMfaService
{
    public async Task<string> GetSetupQrCode(Guid userId, CancellationToken cancellationToken)
    {
        var dbUser = await _authRepo.UpdateUser(userId, dbUser =>
        {
            var mfaUser =(ITotpMfaAuthenticationUser)dbUser;
            if (!string.IsNullOrEmpty(mfaUser.MfaTotpSecret)) return false;
            var key = KeyGeneration.GenerateRandomKey(20);
            mfaUser.MfaTotpSecret = Base32Encoding.ToString(key);
            return true;
        }, cancellationToken);
        if (dbUser == null) throw new MetalGuardianException("Unknown User ID");

        try
        {
            var mfaUser = (ITotpMfaAuthenticationUser)dbUser;
            var qrUri = $"otpauth://totp/{_issuer}:{mfaUser.Name}?secret={mfaUser.MfaTotpSecret}&issuer={_issuer}";
            using var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(qrUri, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);
            var qrBytes = qrCode.GetGraphic(20);
            return $"data:image/png;base64,{Convert.ToBase64String(qrBytes)}";
        }
        catch (Exception ex)
        {
            throw new MetalGuardianException("Failed To Generate QR Code", ex);
        }
    }
    
    public async Task<AuthenticationTokens?> VerifyCode(Guid userId, string code, string? deviceFingerprint, CancellationToken cancellationToken)
    {
        bool isVerified = false;

        var dbUser = await _authRepo.UpdateUser(userId, dbUser =>
        {
            var mfaUser = (ITotpMfaAuthenticationUser)dbUser;
            if (string.IsNullOrWhiteSpace(mfaUser.MfaTotpSecret)) return false;            
            var totp = new Totp(Base32Encoding.ToBytes(mfaUser.MfaTotpSecret));
            isVerified = totp.VerifyTotp(code, out long timeStepMatched,
                new VerificationWindow(1, 1)); // Allow ±1 time step for clock drift
            if (mfaUser.IsMfaTotpEnabled) return false;
            mfaUser.IsMfaTotpEnabled = true;
            return true;
        }, cancellationToken);
        if (dbUser == null) throw new MetalGuardianException("Unknown User ID");

        if (_userDeviceRepository != null && isVerified && deviceFingerprint != null)
        {
            var device = await _userDeviceRepository.Get(userId, deviceFingerprint, cancellationToken);
            if (device == null)
            {
                await _userDeviceRepository.Add(_ =>
                {
                    _.UserId = userId;
                    _.Fingerprint = deviceFingerprint!;
                    _.LastSeen = DateTime.UtcNow;
                    _.ExpiresOn = _deviceRemmemberDays == null? null 
                        : DateTime.UtcNow.AddDays(_deviceRemmemberDays.Value);
                }, cancellationToken);
            }
            else
            {
                await _userDeviceRepository.Update(userId, deviceFingerprint, _ =>
                {
                    _.LastSeen = DateTime.UtcNow;
                    _.ExpiresOn = _deviceRemmemberDays == null ? null
                        : DateTime.UtcNow.AddDays(_deviceRemmemberDays.Value);
                }, cancellationToken);
            }
        }

        return isVerified 
            ? await _authSvc.Login(dbUser, cancellationToken)
            : null;
    }

    public async Task ResetUser(Guid userId, CancellationToken cancellationToken)
    {
        await _authRepo.UpdateUser(userId, dbUser =>
        {
            var mfaUser = (ITotpMfaAuthenticationUser)dbUser;
            if (!mfaUser.IsMfaTotpEnabled &&
                mfaUser.MfaTotpSecret == null) return false;
            mfaUser.IsMfaTotpEnabled = false;
            mfaUser.MfaTotpSecret = null;
            return true;
        }, cancellationToken);
    }
}
