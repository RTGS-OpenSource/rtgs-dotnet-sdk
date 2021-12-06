using System;

namespace RTGS.DotNetSDK.Subscriber.IntegrationTests.TestHandlers
{
	public interface ITestHandler<out TMessage> : IHandler
	{
		TMessage ReceivedMessage { get; }
		void WaitForMessage(TimeSpan timeout);
		void Reset();
	}
}
