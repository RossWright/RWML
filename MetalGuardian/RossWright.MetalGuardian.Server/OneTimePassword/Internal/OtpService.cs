using RossWright.Messaging;
using RossWright.MetalGuardian.Server.OneTimePassword;

namespace RossWright.MetalGuardian.OneTimePassword;

internal class OtpService : IOtpService
{
    public OtpService(
        IOtpRepository otpRepo,
        IEmailService? emailService = null,
        ISmsService? smsService = null) =>
        (_otpRepo, _emailService, _smsService) =
        (otpRepo, emailService, smsService);
    private readonly IOtpRepository _otpRepo;
    private readonly IEmailService? _emailService;
    private readonly ISmsService? _smsService;

    public async Task SendOtpViaEmail(string userIdentifier, Func<string, IAddressedEmail> makeEmail, CancellationToken cancellationToken = default)
    {
        if (_emailService == null) throw new NotSupportedException("Sending via E-mail is not supported");
        var otp = await _otpRepo.CreateOtp(userIdentifier);
        try
        {
            await _emailService.Send(makeEmail(otp), cancellationToken);
        }
        catch
        {
            await _otpRepo.RemoveOtp(userIdentifier, otp);
            throw;
        }
    }

    public async Task SendOtpViaSms(string userIdentifier, Func<string, IAddressedSmsMessage> makeSms, CancellationToken cancellationToken = default)
    {
        if (_smsService == null) throw new NotSupportedException("Sending via SMS is not supported");
        var otp = await _otpRepo.CreateOtp(userIdentifier);
        try
        {
            await _smsService.Send(makeSms(otp), cancellationToken);
        }
        catch
        {
            await _otpRepo.RemoveOtp(userIdentifier, otp);
            throw;
        }
    }

    public Task<OtpVerifyResult> VerifyOtp(string userIdentifier, string otp) =>
        _otpRepo.VerifyOpt(userIdentifier, otp);

    public Task RemoveOtp(string userIdentifier, string otp) =>
        _otpRepo.RemoveOtp(userIdentifier, otp);
}
