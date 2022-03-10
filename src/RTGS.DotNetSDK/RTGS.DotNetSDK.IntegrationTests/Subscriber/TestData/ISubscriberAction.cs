using RTGS.DotNetSDK.Subscriber.Handlers;

namespace RTGS.DotNetSDK.IntegrationTests.Subscriber.TestData;

public interface ISubscriberAction<out TMessage>
{
	ITestHandler<TMessage> Handler { get; }
	string MessageIdentifier { get; }
	TMessage Message { get; }
	IReadOnlyCollection<IHandler> AllTestHandlers { get; }
}
