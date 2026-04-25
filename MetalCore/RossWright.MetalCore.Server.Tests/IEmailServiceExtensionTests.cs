namespace RossWright.Server.Tests;

public class IEmailServiceExtensionTests
{
    private static IEmailService MockService() => Substitute.For<IEmailService>();

    private static IEmailContent MakeContent(
        string subject = "Subj", string text = "Body", string? html = null) =>
        new TestEmailContent(subject, text, html);

    private record TestEmailContent(string Subject, string TextBody, string? HtmlBody)
        : IEmailContent;

    // Overload 1: Send(IEnumerable<IEmailRecipient>, IEmailContent)
    [Fact]
    public async Task Send_MultipleRecipientsWithContent_PassesAllRecipientsToService()
    {
        var svc = MockService();
        IEnumerable<IEmailRecipient> recipients = [
            new EmailRecipient("A", "a@x.com"),
            new EmailRecipient("B", "b@x.com")
        ];

        await svc.Send(recipients, MakeContent());

        await svc.Received(1).Send(
            Arg.Is<IAddressedEmail>(e => e.ToRecipients.Count() == 2),
            Arg.Any<CancellationToken>());
    }

    // Overload 2: Send(IEmailRecipient, IEmailContent)
    [Fact]
    public async Task Send_SingleRecipientWithContent_PassesRecipientToService()
    {
        var svc = MockService();
        var recipient = new EmailRecipient("Alice", "alice@x.com");

        await svc.Send(recipient, MakeContent("Hi", "Text"));

        await svc.Received(1).Send(
            Arg.Is<IAddressedEmail>(e =>
                e.ToRecipients.Single().Email == "alice@x.com" &&
                e.Subject == "Hi" && e.TextBody == "Text"),
            Arg.Any<CancellationToken>());
    }

    // Overload 3: Send(string email, IEmailContent)
    [Fact]
    public async Task Send_EmailStringWithContent_SetsNullNameAndCorrectAddress()
    {
        var svc = MockService();

        await svc.Send("bob@x.com", MakeContent());

        await svc.Received(1).Send(
            Arg.Is<IAddressedEmail>(e =>
                e.ToRecipients.Single().Email == "bob@x.com" &&
                e.ToRecipients.Single().Name == null),
            Arg.Any<CancellationToken>());
    }

    // Overload 4: Send(string name, string email, IEmailContent)
    [Fact]
    public async Task Send_NameAndEmailWithContent_SetsNameAndAddress()
    {
        var svc = MockService();

        await svc.Send("Bob", "bob@x.com", MakeContent());

        await svc.Received(1).Send(
            Arg.Is<IAddressedEmail>(e =>
                e.ToRecipients.Single().Name == "Bob" &&
                e.ToRecipients.Single().Email == "bob@x.com"),
            Arg.Any<CancellationToken>());
    }

    // Overload 5: Send(IEmailRecipient, subject, textBody, htmlBody)
    [Fact]
    public async Task Send_RecipientWithInlineContent_SetsAllContentProperties()
    {
        var svc = MockService();
        var recipient = new EmailRecipient("Alice", "alice@x.com");

        await svc.Send(recipient, "Subject", "TextBody", "<b>Html</b>");

        await svc.Received(1).Send(
            Arg.Is<IAddressedEmail>(e =>
                e.Subject == "Subject" &&
                e.TextBody == "TextBody" &&
                e.HtmlBody == "<b>Html</b>"),
            Arg.Any<CancellationToken>());
    }

    // Overload 6: Send(string email, subject, textBody, htmlBody)
    [Fact]
    public async Task Send_EmailStringWithInlineContent_SetsNullNameAndAllContent()
    {
        var svc = MockService();

        await svc.Send("carol@x.com", "Subj", "Body", "<p>Html</p>", CancellationToken.None);

        await svc.Received(1).Send(
            Arg.Is<IAddressedEmail>(e =>
                e.ToRecipients.Single().Email == "carol@x.com" &&
                e.ToRecipients.Single().Name == null &&
                e.Subject == "Subj" &&
                e.HtmlBody == "<p>Html</p>"),
            Arg.Any<CancellationToken>());
    }

    // Overload 7: Send(string name, string email, subject, textBody, htmlBody)
    [Fact]
    public async Task Send_NameEmailWithInlineContent_SetsNameAddressAndContent()
    {
        var svc = MockService();

        await svc.Send("Dan", "dan@x.com", "Subj", "Body", "<p>Html</p>");

        await svc.Received(1).Send(
            Arg.Is<IAddressedEmail>(e =>
                e.ToRecipients.Single().Name == "Dan" &&
                e.ToRecipients.Single().Email == "dan@x.com" &&
                e.Subject == "Subj"),
            Arg.Any<CancellationToken>());
    }
}
