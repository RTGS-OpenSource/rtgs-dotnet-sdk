using System;
using System.Threading;
using System.Threading.Tasks;
using RTGS.DotNetSDK.Subscriber.Handlers;
using RTGS.ISO20022.Messages.Admi_002_001.V01;

namespace RTGS.DotNetSDK.Subscriber.IntegrationTests.TestHandlers
{
	public class TestMessageRejectedHandler : MessageRejectedV1HandlerBase, ITestHandler<Admi00200101>
	{
		private readonly ManualResetEventSlim _handleSignal = new();

		public Admi00200101 ReceivedMessage { get; private set; }

		protected override Task HandleMessageAsync(Admi00200101 message)
		{
			ReceivedMessage = message;
			_handleSignal.Set();

			return Task.CompletedTask;
		}

		public void WaitForMessage(TimeSpan timeout) =>
			_handleSignal.Wait(timeout);

		public void Reset()
		{
			ReceivedMessage = null;
			_handleSignal.Reset();
		}
	}
}
