using RTGS.ISO20022.Messages.Admi_002_001.V01;
using RTGS.ISO20022.Messages.Camt_054_001.V09;

namespace RTGS.DotNetSDK.Subscriber.IntegrationTests.TestData
{
	public class SubscriberActionData : BaseSubscriberActionData
	{
		public override ISubscriberAction<ISO20022.Messages.Pacs_008_001.V10.FIToFICustomerCreditTransferV10> PayawayFundsV1 =>
			SubscriberActions.PayawayFundsV1;

		public override ISubscriberAction<BankToCustomerDebitCreditNotificationV09> PayawayCompleteV1 =>
			SubscriberActions.PayawayCompleteV1;

		public override ISubscriberAction<Admi00200101> MessageRejectedV1 =>
			SubscriberActions.MessageRejectedV1;
	}
}
