using System.Linq;
using RTGS.DotNetSDK.Subscriber.IntegrationTests.TestHandlers;
using RTGS.ISO20022.Messages.Admi_002_001.V01;
using RTGS.ISO20022.Messages.Camt_054_001.V09;
using static RTGS.DotNetSDK.Subscriber.IntegrationTests.TestHandlers.AllTestHandlers;

namespace RTGS.DotNetSDK.Subscriber.IntegrationTests.TestData
{
	public static class SubscriberActions
	{
		public static readonly SubscriberAction<ISO20022.Messages.Pacs_008_001.V10.FIToFICustomerCreditTransferV10> PayawayFundsV1 =
			new(new AllTestHandlers(), handlers => handlers.OfType<TestPayawayFundsV1Handler>().Single(), "PayawayFunds", ValidMessages.PayawayFunds);

		public static readonly SubscriberAction<BankToCustomerDebitCreditNotificationV09> PayawayCompleteV1 =
			new(new AllTestHandlers(), handlers => handlers.OfType<TestPayawayCompleteV1Handler>().Single(), "PayawayComplete", ValidMessages.PayawayComplete);

		public static readonly SubscriberAction<Admi00200101> MessageRejectedV1 =
			new(new AllTestHandlers(), handlers => handlers.OfType<TestMessageRejectedV1Handler>().Single(), "MessageRejected", ValidMessages.MessageRejected);
	}
}
