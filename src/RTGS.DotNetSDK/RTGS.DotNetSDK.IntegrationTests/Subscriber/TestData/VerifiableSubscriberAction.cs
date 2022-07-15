namespace RTGS.DotNetSDK.IntegrationTests.Subscriber.TestData;

public class VerifiableSubscriberAction<TMessage, TVerifyMessage> : SubscriberAction<TMessage>,
	IVerifiableSubscriberAction<TMessage, TVerifyMessage>
{
	public VerifiableSubscriberAction(string messageIdentifier,
		TMessage message,
		TVerifyMessage verifyMessage,
		Dictionary<string, string> additionalHeaders = null)
		: base(messageIdentifier, message, additionalHeaders)
	{
		VerifyMessage = verifyMessage;
	}

	public TVerifyMessage VerifyMessage { get; }
}
