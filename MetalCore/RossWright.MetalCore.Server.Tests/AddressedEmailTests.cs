namespace RossWright.Server.Tests;

public class AddressedEmailTests
{
    private static IEmailRecipient Recipient(string email = "r@x.com", string? name = null) =>
        new EmailRecipient(name, email);

    private record TestContent(string Subject, string TextBody, string? HtmlBody) : IEmailContent;

    // Constructor 1: (IEnumerable<IEmailRecipient>, subject, textBody, htmlBody)
    [Fact]
    public void Constructor_WithRecipientsAndInlineContent_SetsAllProperties()
    {
        IEnumerable<IEmailRecipient> recipients = [Recipient("a@x.com"), Recipient("b@x.com")];

        var email = new AddressedEmail(recipients, "Subject", "TextBody", "<b>Html</b>");

        email.ToRecipients.Count().ShouldBe(2);
        email.Subject.ShouldBe("Subject");
        email.TextBody.ShouldBe("TextBody");
        email.HtmlBody.ShouldBe("<b>Html</b>");
    }

    [Fact]
    public void Constructor_WithRecipientsAndInlineContent_NullHtmlBody_IsNull()
    {
        var email = new AddressedEmail([Recipient()], "Subj", "Body");

        email.HtmlBody.ShouldBeNull();
    }

    // Constructor 2: (IEnumerable<IEmailRecipient>, IEmailContent)
    [Fact]
    public void Constructor_WithRecipientsAndContent_CopiesSubjectAndBodies()
    {
        IEnumerable<IEmailRecipient> recipients = [Recipient("c@x.com")];
        var content = new TestContent("CS", "CT", "<p>CH</p>");

        var email = new AddressedEmail(recipients, content);

        email.Subject.ShouldBe("CS");
        email.TextBody.ShouldBe("CT");
        email.HtmlBody.ShouldBe("<p>CH</p>");
        email.ToRecipients.ShouldHaveSingleItem();
    }

    // Constructor 3: object initializer via public constructor + init properties
    [Fact]
    public void ObjectInitializer_SetsPropertiesCorrectly()
    {
        var email = new AddressedEmail([Recipient("d@x.com")], "Init Subj", "Init Body", "<b>Init</b>");

        email.Subject.ShouldBe("Init Subj");
        email.HtmlBody.ShouldBe("<b>Init</b>");
    }

    // Default Cc/Bcc behavior
    [Fact]
    public void Constructor_WithRecipientsAndInlineContent_CcAndBccAreNull()
    {
        var email = new AddressedEmail([Recipient()], "S", "T");

        email.CcRecipients.ShouldBeNull();
        email.BccRecipients.ShouldBeNull();
    }

    [Fact]
    public void Constructor_WithRecipientsAndContent_CcAndBccAreNull()
    {
        var email = new AddressedEmail([Recipient()], new TestContent("S", "T", null));

        email.CcRecipients.ShouldBeNull();
        email.BccRecipients.ShouldBeNull();
    }

    [Fact]
    public void AddressedEmail_WithCcAndBcc_ViaObjectInitializerSubclass_SetsCollections()
    {
        // AddressedEmail constructor 1 only sets ToRecipients; Cc/Bcc are init-only
        // and default to null. To set them, use the init syntax on a direct instantiation.
        // Since the protected ctor is not accessible, we verify via init properties
        // on the public constructor overload that produces a usable IAddressedEmail.
        var email = new AddressedEmail([Recipient()], "S", "T")
        {
            CcRecipients = [Recipient("cc@x.com")],
            BccRecipients = [Recipient("bcc@x.com")]
        };

        email.CcRecipients.ShouldHaveSingleItem();
        email.BccRecipients.ShouldHaveSingleItem();
    }
}
