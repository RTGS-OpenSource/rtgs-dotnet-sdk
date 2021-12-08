using RTGS.DotNetSDK.Publisher.Messages;
using RTGS.ISO20022.Messages.Camt_054_001.V09;
using RTGS.ISO20022.Messages.Pacs_008_001.V10;

namespace RTGS.DotNetSDK.Publisher.IntegrationTests.TestData
{
	public class PublisherActionData : BasePublisherActionData
	{
		public override IPublisherAction<AtomicLockRequest> AtomicLock => PublisherActions.AtomicLock;
		public override IPublisherAction<AtomicTransferRequest> AtomicTransfer => PublisherActions.AtomicTransfer;
		public override IPublisherAction<EarmarkConfirmation> EarmarkConfirmation => PublisherActions.EarmarkConfirmation;
		public override IPublisherAction<AtomicTransferConfirmation> AtomicTransferConfirmation => PublisherActions.AtomicTransferConfirmation;
		public override IPublisherAction<UpdateLedgerRequest> UpdateLedger => PublisherActions.UpdateLedger;
		public override IPublisherAction<FIToFICustomerCreditTransferV10> PayawayCreate => PublisherActions.PayawayCreate;
		public override IPublisherAction<BankToCustomerDebitCreditNotificationV09> PayawayConfirmation => PublisherActions.PayawayConfirmation;
	}
}
