using System.Linq;
using RTGS.DotNetSDK.Subscriber.IntegrationTests.TestHandlers;
using RTGS.DotNetSDK.Subscriber.Messages;
using RTGS.ISO20022.Messages.Admi_002_001.V01;
using RTGS.ISO20022.Messages.Camt_054_001.V09;
using RTGS.ISO20022.Messages.Pacs_008_001.V10;
using static RTGS.DotNetSDK.Subscriber.IntegrationTests.TestHandlers.AllTestHandlers;

namespace RTGS.DotNetSDK.Subscriber.IntegrationTests.TestData
{
	public static class SubscriberActions
	{
		public static readonly SubscriberAction<FIToFICustomerCreditTransferV10> PayawayFundsV1 =
			new(new AllTestHandlers(), handlers => handlers.OfType<TestPayawayFundsV1Handler>().Single(), "PayawayFunds", ValidMessages.PayawayFunds);

		public static readonly SubscriberAction<BankToCustomerDebitCreditNotificationV09> PayawayCompleteV1 =
			new(new AllTestHandlers(), handlers => handlers.OfType<TestPayawayCompleteV1Handler>().Single(), "PayawayComplete", ValidMessages.PayawayComplete);

		public static readonly SubscriberAction<Admi00200101> MessageRejectedV1 =
			new(new AllTestHandlers(), handlers => handlers.OfType<TestMessageRejectedV1Handler>().Single(), "MessageRejected", ValidMessages.MessageRejected);

		public static readonly SubscriberAction<AtomicLockResponseV1> AtomicLockResponseV1 =
			new(new AllTestHandlers(), handlers => handlers.OfType<TestAtomicLockResponseV1Handler>().Single(), "payment.lock.v2", ValidMessages.AtomicLockResponseV1);

		public static readonly SubscriberAction<AtomicTransferResponseV1> AtomicTransferResponseV1 =
			new(new AllTestHandlers(), handlers => handlers.OfType<TestAtomicTransferResponseV1Handler>().Single(), "BlockResponse", ValidMessages.AtomicTransferResponseV1);

		public static readonly SubscriberAction<BlockFundsV1> BlockFundsV1 =
			new(new AllTestHandlers(), handlers => handlers.OfType<TestBlockFundsV1Handler>().Single(), "BlockFunds", ValidMessages.BlockFundsV1);

		public static readonly SubscriberAction<EarmarkFundsV1> EarmarkFundsV1 =
			new(new AllTestHandlers(), handlers => handlers.OfType<TestEarmarkFundsV1Handler>().Single(), "EarmarkFunds", ValidMessages.EarmarkFundsV1);

		public static readonly SubscriberAction<EarmarkCompleteV1> EarmarkCompleteV1 =
			new(new AllTestHandlers(), handlers => handlers.OfType<TestEarmarkCompleteV1Handler>().Single(), "EarmarkComplete", ValidMessages.EarmarkCompleteV1);
	}
}
