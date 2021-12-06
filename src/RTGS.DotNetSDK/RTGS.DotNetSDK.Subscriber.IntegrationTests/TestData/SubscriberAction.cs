using RTGS.DotNetSDK.Subscriber.IntegrationTests.TestHandlers;

namespace RTGS.DotNetSDK.Subscriber.IntegrationTests.TestData
{
	public class SubscriberAction<TMessage> : ISubscriberAction<TMessage>
	{
		public SubscriberAction(ITestHandler<TMessage> handler, string messageIdentifier, TMessage message)
		{
			Handler = handler;
			MessageIdentifier = messageIdentifier;
			Message = message;
		}

		public ITestHandler<TMessage> Handler { get; }
		public string MessageIdentifier { get; }
		public TMessage Message { get; }
	}
}
