namespace RossWright.Messaging;

/// <summary>
/// Abstracts SMS delivery. Application code depends on this interface; swap implementations by changing registration.
/// </summary>
public interface ISmsService
{
    /// <summary>
    /// Sends an addressed SMS message.
    /// </summary>
    /// <param name="message">The fully addressed SMS message to deliver.</param>
    /// <param name="cancellationToken">A token that can cancel the operation.</param>
    /// <returns>A <see cref="Task"/> that completes when the message has been submitted to the delivery provider.</returns>
    Task Send(IAddressedSmsMessage message, CancellationToken cancellationToken = default);
}

/// <summary>
/// A fully addressed SMS message, extending <see cref="IMessageContent"/> with a recipient list.
/// </summary>
public interface IAddressedSmsMessage : IMessageContent
{
    /// <summary>Gets the list of SMS recipients.</summary>
    IEnumerable<ISmsRecipient> Recipients { get; }
}

/// <summary>Contract for a single SMS recipient, identified by a phone number.</summary>
public interface ISmsRecipient
{
    /// <summary>Gets the recipient's phone number.</summary>
    public string PhoneNumber { get; }
}


/// <summary>
/// Convenience overloads for <see cref="ISmsService"/> that build addressed SMS messages inline.
/// </summary>
public static class ISmsServiceExtensions
{
    private class AddressedSmsMessage : IAddressedSmsMessage
    {
        public AddressedSmsMessage(IEnumerable<ISmsRecipient> recipients, string textBody) =>
            (Recipients, TextBody) = (recipients, textBody);

        public IEnumerable<ISmsRecipient> Recipients { get; }
        public string TextBody { get; }
    }

    /// <summary>Sends an SMS to a collection of <see cref="ISmsRecipient"/> recipients from an <see cref="IMessageContent"/>.</summary>
    /// <param name="smsSvc">The SMS service.</param>
    /// <param name="recipients">The recipients.</param>
    /// <param name="content">The message content.</param>
    /// <param name="cancellationToken">A token that can cancel the operation.</param>
    /// <returns>A <see cref="Task"/> that completes when the message has been submitted.</returns>
    public static Task Send(this ISmsService smsSvc, IEnumerable<ISmsRecipient> recipients, IMessageContent content, CancellationToken cancellationToken = default) =>
        smsSvc.Send(new AddressedSmsMessage(recipients, content.TextBody), cancellationToken);

    /// <summary>Sends an SMS to a single <see cref="ISmsRecipient"/> from an <see cref="IMessageContent"/>.</summary>
    /// <param name="smsSvc">The SMS service.</param>
    /// <param name="recipient">The single recipient.</param>
    /// <param name="content">The message content.</param>
    /// <param name="cancellationToken">A token that can cancel the operation.</param>
    /// <returns>A <see cref="Task"/> that completes when the message has been submitted.</returns>
    public static Task Send(this ISmsService smsSvc, ISmsRecipient recipient, IMessageContent content, CancellationToken cancellationToken = default) =>
        smsSvc.Send(new AddressedSmsMessage([recipient], content.TextBody), cancellationToken);

    /// <summary>Sends an SMS to a phone number string from an <see cref="IMessageContent"/>.</summary>
    /// <param name="smsSvc">The SMS service.</param>
    /// <param name="phoneNumber">The destination phone number.</param>
    /// <param name="content">The message content.</param>
    /// <param name="cancellationToken">A token that can cancel the operation.</param>
    /// <returns>A <see cref="Task"/> that completes when the message has been submitted.</returns>
    public static Task Send(this ISmsService smsSvc, string phoneNumber, IMessageContent content, CancellationToken cancellationToken = default) =>
        smsSvc.Send(new AddressedSmsMessage([new SmsRecipient(phoneNumber)], content.TextBody), cancellationToken);

    /// <summary>Sends an SMS to a single <see cref="ISmsRecipient"/> with an inline text body.</summary>
    /// <param name="smsSvc">The SMS service.</param>
    /// <param name="recipient">The single recipient.</param>
    /// <param name="textBody">The plain-text message body.</param>
    /// <param name="cancellationToken">A token that can cancel the operation.</param>
    /// <returns>A <see cref="Task"/> that completes when the message has been submitted.</returns>
    public static Task Send(this ISmsService smsSvc, ISmsRecipient recipient, string textBody, CancellationToken cancellationToken = default) =>
        smsSvc.Send(new AddressedSmsMessage([recipient], textBody), cancellationToken);

    /// <summary>Sends an SMS to a phone number string with an inline text body.</summary>
    /// <param name="smsSvc">The SMS service.</param>
    /// <param name="phoneNumber">The destination phone number.</param>
    /// <param name="textBody">The plain-text message body.</param>
    /// <param name="cancellationToken">A token that can cancel the operation.</param>
    /// <returns>A <see cref="Task"/> that completes when the message has been submitted.</returns>
    public static Task Send(this ISmsService smsSvc, string phoneNumber, string textBody, CancellationToken cancellationToken = default) =>
        smsSvc.Send(new AddressedSmsMessage([new SmsRecipient(phoneNumber)], textBody), cancellationToken);

    private class SmsRecipient : ISmsRecipient
    {
        public SmsRecipient(string phoneNumber) => PhoneNumber = phoneNumber;
        public string PhoneNumber { get; }
    }
}