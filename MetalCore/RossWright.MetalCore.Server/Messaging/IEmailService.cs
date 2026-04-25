namespace RossWright.Messaging;

/// <summary>
/// Abstracts email delivery. Application code depends on this interface; swap implementations by changing registration.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends an addressed email message.
    /// </summary>
    /// <param name="email">The fully addressed email to deliver.</param>
    /// <param name="cancellationToken">A token that can cancel the operation.</param>
    /// <returns>A <see cref="Task"/> that completes when the message has been submitted to the delivery provider.</returns>
    Task Send(IAddressedEmail email, CancellationToken cancellationToken = default);
}

/// <summary>
/// Email content contract: extends <see cref="IMessageContent"/> with a subject and optional HTML body.
/// </summary>
public interface IEmailContent : IMessageContent
{
    /// <summary>Gets the subject line of the email.</summary>
    string Subject { get; }
    /// <summary>Gets the optional HTML body. When <see langword="null"/>, only the text body is sent.</summary>
    string? HtmlBody { get; }
}

/// <summary>
/// A fully addressed email, extending <see cref="IEmailContent"/> with To, Cc, and Bcc recipient lists.
/// </summary>
public interface IAddressedEmail : IEmailContent
{
    /// <summary>Gets the primary recipients.</summary>
    IEnumerable<IEmailRecipient> ToRecipients { get; }
    /// <summary>Gets the CC recipients.</summary>
    IEnumerable<IEmailRecipient> CcRecipients { get; }
    /// <summary>Gets the BCC recipients.</summary>
    IEnumerable<IEmailRecipient> BccRecipients { get; }
}

/// <summary>
/// Contract for a single email recipient, providing an optional display name and a required email address.
/// </summary>
public interface IEmailRecipient
{
    /// <summary>Gets the optional display name shown in mail clients.</summary>
    public string? Name { get; }
    /// <summary>Gets the recipient's email address.</summary>
    public string Email { get; }
}

/// <summary>Concrete implementation of <see cref="IEmailRecipient"/>.</summary>
public class EmailRecipient : IEmailRecipient
{
    /// <summary>Initializes a new <see cref="EmailRecipient"/> with an optional display name and required email address.</summary>
    /// <param name="name">The optional display name.</param>
    /// <param name="email">The recipient's email address.</param>
    public EmailRecipient(string? name, string email) => (Name, Email) = (name, email);
    /// <inheritdoc/>
    public string? Name { get; }
    /// <inheritdoc/>
    public string Email { get; }
}

/// <summary>Concrete implementation of <see cref="IAddressedEmail"/> suitable for general use and object-initializer syntax.</summary>
public class AddressedEmail : IAddressedEmail
{
    /// <summary>
    /// Initializes a new <see cref="AddressedEmail"/> with explicit recipients and content.
    /// </summary>
    /// <param name="recipients">The To recipients.</param>
    /// <param name="subject">The email subject line.</param>
    /// <param name="textBody">The plain-text body.</param>
    /// <param name="htmlBody">The optional HTML body.</param>
    public AddressedEmail(IEnumerable<IEmailRecipient> recipients, 
        string subject, string textBody, string? htmlBody = null) =>
        (ToRecipients, Subject, TextBody, HtmlBody) = 
        (recipients, subject, textBody, htmlBody);

    /// <summary>
    /// Initializes a new <see cref="AddressedEmail"/> by combining recipients with an existing <see cref="IEmailContent"/>.
    /// </summary>
    /// <param name="recipients">The To recipients.</param>
    /// <param name="email">The email content to copy subject and body from.</param>
    public AddressedEmail(IEnumerable<IEmailRecipient> recipients,
        IEmailContent email) =>
        (ToRecipients, Subject, TextBody, HtmlBody) =
        (recipients, email.Subject, email.TextBody, email.HtmlBody);

    /// <summary>Initializes a new <see cref="AddressedEmail"/> for use with object-initializer syntax.</summary>
    protected AddressedEmail() { }
    /// <inheritdoc/>
    public IEnumerable<IEmailRecipient> ToRecipients { get; init; } = null!;
    /// <inheritdoc/>
    public IEnumerable<IEmailRecipient> CcRecipients { get; init; } = null!;
    /// <inheritdoc/>
    public IEnumerable<IEmailRecipient> BccRecipients { get; init; } = null!;
    /// <inheritdoc/>
    public string Subject { get; init; } = null!;
    /// <inheritdoc/>
    public string TextBody { get; init; } = null!;
    /// <inheritdoc/>
    public string? HtmlBody { get; init; }
}

/// <summary>
/// Convenience overloads for <see cref="IEmailService"/> that build <see cref="AddressedEmail"/> instances inline.
/// </summary>
public static class IEmailServiceExtensions
{
    /// <summary>Sends an email to a collection of recipients using an existing <see cref="IEmailContent"/>.</summary>
    /// <param name="emailSvc">The email service.</param>
    /// <param name="toRecipients">The To recipients.</param>
    /// <param name="content">The email content providing subject and body.</param>
    /// <param name="cancellationToken">A token that can cancel the operation.</param>
    /// <returns>A <see cref="Task"/> that completes when the message has been submitted.</returns>
    public static Task Send(this IEmailService emailSvc, IEnumerable<IEmailRecipient> toRecipients, IEmailContent content, CancellationToken cancellationToken = default) =>
        emailSvc.Send(new AddressedEmail(toRecipients, content.Subject, content.TextBody, content.HtmlBody), cancellationToken);

    /// <summary>Sends an email to a single <see cref="IEmailRecipient"/> using an existing <see cref="IEmailContent"/>.</summary>
    /// <param name="emailSvc">The email service.</param>
    /// <param name="toRecipient">The single To recipient.</param>
    /// <param name="content">The email content providing subject and body.</param>
    /// <param name="cancellationToken">A token that can cancel the operation.</param>
    /// <returns>A <see cref="Task"/> that completes when the message has been submitted.</returns>
    public static Task Send(this IEmailService emailSvc, IEmailRecipient toRecipient, IEmailContent content, CancellationToken cancellationToken = default) =>
        emailSvc.Send(new AddressedEmail([toRecipient], content.Subject, content.TextBody, content.HtmlBody), cancellationToken);

    /// <summary>Sends an email to a single address (no display name) using an existing <see cref="IEmailContent"/>.</summary>
    /// <param name="emailSvc">The email service.</param>
    /// <param name="email">The recipient email address.</param>
    /// <param name="content">The email content providing subject and body.</param>
    /// <param name="cancellationToken">A token that can cancel the operation.</param>
    /// <returns>A <see cref="Task"/> that completes when the message has been submitted.</returns>
    public static Task Send(this IEmailService emailSvc, string email, IEmailContent content, CancellationToken cancellationToken = default) =>
        emailSvc.Send(new AddressedEmail([new EmailRecipient(null, email)], content.Subject, content.TextBody, content.HtmlBody), cancellationToken);

    /// <summary>Sends an email to a named address using an existing <see cref="IEmailContent"/>.</summary>
    /// <param name="emailSvc">The email service.</param>
    /// <param name="name">The recipient display name.</param>
    /// <param name="email">The recipient email address.</param>
    /// <param name="content">The email content providing subject and body.</param>
    /// <param name="cancellationToken">A token that can cancel the operation.</param>
    /// <returns>A <see cref="Task"/> that completes when the message has been submitted.</returns>
    public static Task Send(this IEmailService emailSvc, string name, string email, IEmailContent content, CancellationToken cancellationToken = default) =>
        emailSvc.Send(new AddressedEmail([new EmailRecipient(name, email)], content.Subject, content.TextBody, content.HtmlBody), cancellationToken);

    /// <summary>Sends an email to a single <see cref="IEmailRecipient"/> with inline subject and body.</summary>
    /// <param name="emailSvc">The email service.</param>
    /// <param name="toRecipient">The single To recipient.</param>
    /// <param name="subject">The email subject line.</param>
    /// <param name="textBody">The plain-text body.</param>
    /// <param name="htmlBody">The optional HTML body.</param>
    /// <param name="cancellationToken">A token that can cancel the operation.</param>
    /// <returns>A <see cref="Task"/> that completes when the message has been submitted.</returns>
    public static Task Send(this IEmailService emailSvc, IEmailRecipient toRecipient, string subject, string textBody, string? htmlBody = null, CancellationToken cancellationToken = default) =>
        emailSvc.Send(new AddressedEmail([toRecipient], subject, textBody, htmlBody), cancellationToken);

    /// <summary>Sends an email to a single address (no display name) with inline subject and body.</summary>
    /// <param name="emailSvc">The email service.</param>
    /// <param name="email">The recipient email address.</param>
    /// <param name="subject">The email subject line.</param>
    /// <param name="textBody">The plain-text body.</param>
    /// <param name="htmlBody">The optional HTML body.</param>
    /// <param name="cancellationToken">A token that can cancel the operation.</param>
    /// <returns>A <see cref="Task"/> that completes when the message has been submitted.</returns>
    public static Task Send(this IEmailService emailSvc, string email, string subject, string textBody, string? htmlBody = null, CancellationToken cancellationToken = default) =>
        emailSvc.Send(new AddressedEmail([new EmailRecipient(null, email)], subject, textBody, htmlBody), cancellationToken);

    /// <summary>Sends an email to a named address with inline subject and body.</summary>
    /// <param name="emailSvc">The email service.</param>
    /// <param name="name">The recipient display name.</param>
    /// <param name="email">The recipient email address.</param>
    /// <param name="subject">The email subject line.</param>
    /// <param name="textBody">The plain-text body.</param>
    /// <param name="htmlBody">The optional HTML body.</param>
    /// <param name="cancellationToken">A token that can cancel the operation.</param>
    /// <returns>A <see cref="Task"/> that completes when the message has been submitted.</returns>
    public static Task Send(this IEmailService emailSvc, string name, string email, string subject, string textBody, string? htmlBody = null, CancellationToken cancellationToken = default) =>
        emailSvc.Send(new AddressedEmail([new EmailRecipient(name, email)], subject, textBody, htmlBody), cancellationToken);
}