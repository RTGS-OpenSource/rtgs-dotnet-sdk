using RTGS.DotNetSDK.Subscriber.Handlers;

namespace RTGS.DotNetSDK.IntegrationTests.Subscriber.TestHandlers;

public interface ITestHandler<out TMessage> : IHandler
{
	TMessage ReceivedMessage { get; }
	void WaitForMessage(TimeSpan timeout);
	void Reset();
}
