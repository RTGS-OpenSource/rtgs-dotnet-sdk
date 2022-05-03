namespace RTGS.DotNetSDK.IntegrationTests.Subscriber.TestData;

public class SubscriberAction<TMessage> : ISubscriberAction<TMessage>
{
	public SubscriberAction(
		string messageIdentifier,
		TMessage message,
		Dictionary<string, string> additionalHeaders = null)
	{
		MessageIdentifier = messageIdentifier;
		Message = message;
		AdditionalHeaders = additionalHeaders ?? new Dictionary<string, string>();
	}

	public string MessageIdentifier { get; }
	public TMessage Message { get; }
	public Dictionary<string, string> AdditionalHeaders { get; }
}
