namespace RossWright.Messaging;

/// <summary>
/// Base contract for all message content. Provides a plain-text body accessible to any delivery provider.
/// </summary>
public interface IMessageContent
{
    /// <summary>Gets the plain-text body of the message.</summary>
    string TextBody { get; }
}
