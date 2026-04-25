
namespace RossWright.Messaging.Smtp;

/// <summary>
/// Configuration model for the SMTP email delivery service.
/// Bind this from <c>appsettings.json</c> under <c>MetalCore.Smtp</c> (or a custom section).
/// </summary>
public class SmtpConfig
{
    /// <summary>Gets or sets the SMTP server hostname or IP address.</summary>
    public string Host { get; set; } = null!;

    /// <summary>Gets or sets the SMTP server port number (e.g., 587 for STARTTLS, 465 for SSL).</summary>
    public int Port { get; set; }

    /// <summary>Gets or sets the <c>From</c> email address used for all outgoing messages.</summary>
    public string FromEmail { get; set; } = null!;

    /// <summary>Gets or sets a value indicating whether SSL/TLS is enabled for the SMTP connection.</summary>
    public bool EnableSsl { get; set; }

    /// <summary>Gets or sets the display name shown in the <c>From</c> header.</summary>
    public string FromName { get; set; } = null!;

    /// <summary>
    /// Gets or sets the SMTP authentication username.
    /// When both <see cref="Username"/> and <see cref="Password"/> are provided, the client authenticates
    /// using <see cref="System.Net.NetworkCredential"/>. Leave <see langword="null"/> for unauthenticated relays.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Gets or sets the SMTP authentication password.
    /// Used in conjunction with <see cref="Username"/>. Leave <see langword="null"/> for unauthenticated relays.
    /// </summary>
    public string? Password { get; set; }
}
