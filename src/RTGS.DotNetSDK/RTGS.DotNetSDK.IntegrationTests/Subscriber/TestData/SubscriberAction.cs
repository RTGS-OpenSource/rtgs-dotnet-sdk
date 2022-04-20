using RTGS.DotNetSDK.Subscriber.Handlers;

namespace RTGS.DotNetSDK.IntegrationTests.Subscriber.TestData;

public class SubscriberAction<TMessage> : ISubscriberAction<TMessage>
{
	public SubscriberAction(
		IEnumerable<IHandler> allTestHandlers,
		Func<IEnumerable<IHandler>, IHandler> selector,
		string messageIdentifier,
		TMessage message,
		Dictionary<string, string> additionalHeaders = null)
	{
		AllTestHandlers = allTestHandlers.ToList();
		Handler = (ITestHandler<TMessage>)selector(AllTestHandlers);
		MessageIdentifier = messageIdentifier;
		Message = message;
		AdditionalHeaders = additionalHeaders ?? new Dictionary<string, string>();
	}

	public IReadOnlyCollection<IHandler> AllTestHandlers { get; }
	public ITestHandler<TMessage> Handler { get; }
	public string MessageIdentifier { get; }
	public TMessage Message { get; }
	public Dictionary<string, string> AdditionalHeaders { get; }
}
