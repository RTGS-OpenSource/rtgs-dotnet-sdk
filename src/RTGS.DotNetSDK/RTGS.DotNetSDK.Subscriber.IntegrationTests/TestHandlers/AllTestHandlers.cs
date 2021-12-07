using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
			yield return new TestMessageRejectedV1Handler();
			yield return new TestPayawayCompleteV1Handler();
			yield return new TestPayawayFundsV1Handler();
			yield return new TestAtomicLockResponseV1Handler();
			yield return new TestAtomicTransferResponseV1Handler();
			yield return new TestBlockFundsV1Handler();
			yield return new TestEarmarkFundsV1Handler();
		}

		IEnumerator IEnumerable.GetEnumerator() =>
			GetEnumerator();

		public class TestMessageRejectedV1Handler : TestHandler<Admi00200101>, IMessageRejectV1Handler { }
		public class TestPayawayCompleteV1Handler : TestHandler<BankToCustomerDebitCreditNotificationV09>, IPayawayCompleteV1Handler { }
		public class TestPayawayFundsV1Handler : TestHandler<FIToFICustomerCreditTransferV10>, IPayawayFundsV1Handler { }
		public class TestAtomicLockResponseV1Handler : TestHandler<AtomicLockResponseV1>, IAtomicLockResponseV1Handler { }
		public class TestAtomicTransferResponseV1Handler : TestHandler<AtomicTransferResponseV1>, IAtomicTransferResponseV1Handler { }
		public class TestBlockFundsV1Handler : TestHandler<BlockFundsV1>, IBlockFundsV1Handler { }
		public class TestEarmarkFundsV1Handler : TestHandler<EarmarkFundsV1>, IEarmarkFundsV1Handler { }

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
