using RTGS.DotNetSDK.Subscriber.IntegrationTests.TestHandlers;
using RTGS.ISO20022.Messages.Admi_002_001.V01;
using RTGS.ISO20022.Messages.Camt_054_001.V09;

namespace RTGS.DotNetSDK.Subscriber.IntegrationTests.TestData
{
	public static class SubscriberActions
	{
		public static readonly SubscriberAction<ISO20022.Messages.Pacs_008_001.V10.FIToFICustomerCreditTransferV10> PayawayFunds =
			new(new TestPayawayFundsHandler(), "PayawayFunds", ValidMessages.PayawayFunds);

		public static readonly SubscriberAction<BankToCustomerDebitCreditNotificationV09> PayawayComplete =
			new(new TestPayawayCompleteHandler(), "PayawayComplete", ValidMessages.PayawayComplete);

		public static readonly SubscriberAction<Admi00200101> MessageRejected =
			new(new TestMessageRejectedHandler(), "MessageRejected", ValidMessages.MessageRejected);
	}
}
