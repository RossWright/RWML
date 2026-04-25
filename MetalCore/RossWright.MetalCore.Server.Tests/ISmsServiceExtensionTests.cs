namespace RossWright.Server.Tests;

public class ISmsServiceExtensionTests
{
    private static ISmsService MockSvc() => Substitute.For<ISmsService>();

    private static IMessageContent MakeContent(string text = "Hello SMS") =>
        new TestMessageContent(text);

    private record TestMessageContent(string TextBody) : IMessageContent;

    private class TestRecipient : ISmsRecipient
    {
        public TestRecipient(string phone) => PhoneNumber = phone;
        public string PhoneNumber { get; }
    }

    // Overload 1: Send(IEnumerable<ISmsRecipient>, IMessageContent)
    [Fact]
    public async Task Send_MultipleRecipientsWithContent_DelegatesWithAllRecipients()
    {
        var svc = MockSvc();
        IEnumerable<ISmsRecipient> recipients = [
            new TestRecipient("+11111111111"),
            new TestRecipient("+12222222222")
        ];

        await svc.Send(recipients, MakeContent());

        await svc.Received(1).Send(
            Arg.Is<IAddressedSmsMessage>(m => m.Recipients.Count() == 2),
            Arg.Any<CancellationToken>());
    }

    // Overload 2: Send(ISmsRecipient, IMessageContent)
    [Fact]
    public async Task Send_SingleRecipientWithContent_DelegatesWithCorrectRecipient()
    {
        var svc = MockSvc();
        var recipient = new TestRecipient("+10000000001");

        await svc.Send(recipient, MakeContent("Body"));

        await svc.Received(1).Send(
            Arg.Is<IAddressedSmsMessage>(m =>
                m.Recipients.Single().PhoneNumber == "+10000000001" &&
                m.TextBody == "Body"),
            Arg.Any<CancellationToken>());
    }

    // Overload 3: Send(string phoneNumber, IMessageContent)
    [Fact]
    public async Task Send_PhoneStringWithContent_DelegatesWithPhoneNumber()
    {
        var svc = MockSvc();

        await svc.Send("+13333333333", MakeContent("Msg"));

        await svc.Received(1).Send(
            Arg.Is<IAddressedSmsMessage>(m =>
                m.Recipients.Single().PhoneNumber == "+13333333333" &&
                m.TextBody == "Msg"),
            Arg.Any<CancellationToken>());
    }

    // Overload 4: Send(ISmsRecipient, string textBody)
    [Fact]
    public async Task Send_SingleRecipientWithInlineBody_DelegatesWithBody()
    {
        var svc = MockSvc();
        var recipient = new TestRecipient("+14444444444");

        await svc.Send(recipient, "Inline body");

        await svc.Received(1).Send(
            Arg.Is<IAddressedSmsMessage>(m =>
                m.Recipients.Single().PhoneNumber == "+14444444444" &&
                m.TextBody == "Inline body"),
            Arg.Any<CancellationToken>());
    }

    // Overload 5: Send(string phoneNumber, string textBody)
    [Fact]
    public async Task Send_PhoneStringWithInlineBody_DelegatesWithPhoneAndBody()
    {
        var svc = MockSvc();

        await svc.Send("+15555555555", "Direct message");

        await svc.Received(1).Send(
            Arg.Is<IAddressedSmsMessage>(m =>
                m.Recipients.Single().PhoneNumber == "+15555555555" &&
                m.TextBody == "Direct message"),
            Arg.Any<CancellationToken>());
    }

    // Overload 6 — ISmsService.Send(IAddressedSmsMessage) — base interface, not extension
    // Verify the extension overloads always delegate to the base Send
    [Fact]
    public async Task Send_AllExtensionOverloads_DelegateToBaseInterfaceSend()
    {
        var svc = MockSvc();
        var recipient = new TestRecipient("+16666666666");
        var content = MakeContent("test");

        await svc.Send(new[] { recipient }, content);
        await svc.Send(recipient, content);
        await svc.Send("+16666666666", content);
        await svc.Send(recipient, "text");
        await svc.Send("+16666666666", "text");

        await svc.Received(5).Send(
            Arg.Any<IAddressedSmsMessage>(),
            Arg.Any<CancellationToken>());
    }
}
