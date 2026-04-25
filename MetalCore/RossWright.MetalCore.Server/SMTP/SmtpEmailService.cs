using System.Net.Mail;
using System.Net.Mime;

namespace RossWright.Messaging.Smtp;

internal class SmtpEmailService : IEmailService
{
    public SmtpEmailService(SmtpConfig config) => _config = config;
    private readonly SmtpConfig _config;

    public async Task Send(IAddressedEmail email, CancellationToken cancellationToken = default)
    {
        if (email == null) return;

        var message = new MailMessage();
        message.From = new MailAddress(_config.FromEmail, _config.FromName);
        message.Subject = email.Subject;

        if (!string.IsNullOrEmpty(email.TextBody))
        {
            message.AlternateViews.Add(
                AlternateView.CreateAlternateViewFromString(
                    email.TextBody, null, MediaTypeNames.Text.Plain));
        }

        if (!string.IsNullOrEmpty(email.HtmlBody))
        {
            message.AlternateViews.Add(
                AlternateView.CreateAlternateViewFromString(
                    email.HtmlBody, null, MediaTypeNames.Text.Html));
        }

        if (email.ToRecipients != null)
        {
            foreach (var to in email.ToRecipients)
                message.To.Add(new MailAddress(to.Email, to.Name));
        }
        if (email.CcRecipients != null)
        {
            foreach (var to in email.CcRecipients)
                message.CC.Add(new MailAddress(to.Email, to.Name));
        }
        if (email.BccRecipients != null)
        {
            foreach (var to in email.BccRecipients)
                message.Bcc.Add(new MailAddress(to.Email, to.Name));
        }

        using var smtpClient = new SmtpClient(_config.Host, _config.Port);
        smtpClient.EnableSsl = _config.EnableSsl;
        if (!string.IsNullOrWhiteSpace(_config.Username) && !string.IsNullOrWhiteSpace(_config.Password))
            smtpClient.Credentials = new System.Net.NetworkCredential(_config.Username, _config.Password);
        try
        {
            await smtpClient.SendMailAsync(message);
        }
        catch (Exception ex)
        {
            throw new MetalCoreException("Email failed to send", ex);
        }
    }
}
