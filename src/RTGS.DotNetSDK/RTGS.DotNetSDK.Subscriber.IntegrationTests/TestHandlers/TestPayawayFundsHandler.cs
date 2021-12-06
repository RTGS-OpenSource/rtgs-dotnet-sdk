using System;
using System.Threading;
using System.Threading.Tasks;
using RTGS.DotNetSDK.Subscriber.Handlers;
using RTGS.ISO20022.Messages.Pacs_008_001.V10;

namespace RTGS.DotNetSDK.Subscriber.IntegrationTests.TestHandlers
{
	public class TestPayawayFundsHandler : PayawayFundsV1HandlerBase, ITestHandler<FIToFICustomerCreditTransferV10>
	{
		private readonly ManualResetEventSlim _handleSignal = new();

		public FIToFICustomerCreditTransferV10 ReceivedMessage { get; private set; }

		protected override Task HandleMessageAsync(FIToFICustomerCreditTransferV10 message)
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
