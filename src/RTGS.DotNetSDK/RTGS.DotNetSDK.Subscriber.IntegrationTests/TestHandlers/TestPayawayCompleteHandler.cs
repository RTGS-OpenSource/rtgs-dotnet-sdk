using System;
using System.Threading;
using System.Threading.Tasks;
using RTGS.DotNetSDK.Subscriber.Handlers;
using RTGS.ISO20022.Messages.Camt_054_001.V09;

namespace RTGS.DotNetSDK.Subscriber.IntegrationTests.TestHandlers
{
	public class TestPayawayCompleteHandler : PayawayCompleteV1HandlerBase, ITestHandler<BankToCustomerDebitCreditNotificationV09>
	{
		private readonly ManualResetEventSlim _handleSignal = new();

		public BankToCustomerDebitCreditNotificationV09 ReceivedMessage { get; private set; }

		protected override Task HandleMessageAsync(BankToCustomerDebitCreditNotificationV09 message)
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
