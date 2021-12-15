using System.Collections;
using RTGS.DotNetSDK.Subscriber.Handlers;
using RTGS.DotNetSDK.Subscriber.Messages;
using RTGS.ISO20022.Messages.Admi_002_001.V01;
using RTGS.ISO20022.Messages.Camt_054_001.V09;
using RTGS.ISO20022.Messages.Pacs_008_001.V10;

namespace RTGS.DotNetSDK.Subscriber.IntegrationTests.TestHandlers
{
	public class AllTestHandlers : IEnumerable<IHandler>
	{
		public IEnumerator<IHandler> GetEnumerator()
		{
			var types = typeof(AllTestHandlers)
				.GetNestedTypes()
				.Where(type => !type.IsAbstract)
				.Select(Activator.CreateInstance)
				.Cast<IHandler>();

			return types.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() =>
			GetEnumerator();

		public class TestMessageRejectedV1Handler : TestHandler<Admi00200101>, IMessageRejectV1Handler { }
		public class TestPayawayCompleteV1Handler : TestHandler<BankToCustomerDebitCreditNotificationV09>, IPayawayCompleteV1Handler { }
		public class TestPayawayFundsV1Handler : TestHandler<FIToFICustomerCreditTransferV10>, IPayawayFundsV1Handler { }
		public class TestAtomicLockResponseV1Handler : TestHandler<AtomicLockResponseV1>, IAtomicLockResponseV1Handler { }
		public class TestAtomicTransferResponseV1Handler : TestHandler<AtomicTransferResponseV1>, IAtomicTransferResponseV1Handler { }
		public class TestAtomicTransferFundsV1Handler : TestHandler<AtomicTransferFundsV1>, IAtomicTransferFundsV1Handler { }
		public class TestEarmarkFundsV1Handler : TestHandler<EarmarkFundsV1>, IEarmarkFundsV1Handler { }
		public class TestEarmarkCompleteV1Handler : TestHandler<EarmarkCompleteV1>, IEarmarkCompleteV1Handler { }
		public class TestEarmarkReleaseV1Handler : TestHandler<EarmarkReleaseV1>, IEarmarkReleaseV1Handler { }

		public abstract class TestHandler<TMessage> : ITestHandler<TMessage>
		{
			private readonly ManualResetEventSlim _handleSignal = new();

			public TMessage ReceivedMessage { get; private set; }

			public Task HandleMessageAsync(TMessage message)
			{
				ReceivedMessage = message;
				_handleSignal.Set();

				return Task.CompletedTask;
			}

			public void WaitForMessage(TimeSpan timeout) =>
				_handleSignal.Wait(timeout);

			public void Reset()
			{
				ReceivedMessage = default;
				_handleSignal.Reset();
			}
		}
	}
}
