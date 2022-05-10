using System.Collections;
using RTGS.DotNetSDK.Publisher.IdCrypt.Messages;
using RTGS.DotNetSDK.Subscriber.Handlers;

namespace RTGS.DotNetSDK.IntegrationTests.Subscriber.TestHandlers;

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

	public class TestMessageRejectedV1Handler : TestHandler<MessageRejectV1>, IMessageRejectV1Handler { }
	public class TestPayawayCompleteV1Handler : TestHandler<PayawayCompleteV1>, IPayawayCompleteV1Handler { }
	public class TestPayawayFundsV1Handler : TestHandler<PayawayFundsV1>, IPayawayFundsV1Handler { }
	public class TestAtomicLockResponseV1Handler : TestHandler<AtomicLockResponseV1>, IAtomicLockResponseV1Handler { }
	public class TestAtomicTransferResponseV1Handler : TestHandler<AtomicTransferResponseV1>, IAtomicTransferResponseV1Handler { }
	public class TestAtomicTransferFundsV1Handler : TestHandler<AtomicTransferFundsV1>, IAtomicTransferFundsV1Handler { }
	public class TestEarmarkFundsV1Handler : TestHandler<EarmarkFundsV1>, IEarmarkFundsV1Handler { }
	public class TestEarmarkCompleteV1Handler : TestHandler<EarmarkCompleteV1>, IEarmarkCompleteV1Handler { }
	public class TestEarmarkReleaseV1Handler : TestHandler<EarmarkReleaseV1>, IEarmarkReleaseV1Handler { }
	public class TestBankPartnersResponseV1 : TestHandler<BankPartnersResponseV1>, IBankPartnersResponseV1Handler { }
	public class TestIdCryptInvitationConfirmationV1 : TestHandler<IdCryptInvitationConfirmationV1>, IIdCryptInvitationConfirmationV1Handler { }
	public class TestIdCryptCreateInvitationNotificationV1 : TestHandler<IdCryptCreateInvitationNotificationV1>, IIdCryptCreateInvitationNotificationV1Handler { }
	public class TestIdCryptBankInvitationNotificationV1 : TestHandler<IdCryptBankInvitationNotificationV1>, IIdCryptBankInvitationNotificationV1Handler { }

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

		public void WaitForMessage(TimeSpan timeout)
		{
			_handleSignal.Wait(timeout);
			_handleSignal.Reset();
		}

		public void Reset()
		{
			ReceivedMessage = default;
			_handleSignal.Reset();
		}
	}
}
