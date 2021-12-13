using RTGS.DotNetSDK.Publisher.Messages;
using RTGS.ISO20022.Messages.Camt_054_001.V09;
using RTGS.ISO20022.Messages.Pacs_008_001.V10;

namespace RTGS.DotNetSDK.Publisher.IntegrationTests.TestData
{
	public class PublisherActionWithNullMessagesData : BasePublisherActionData
	{
		public override IPublisherAction<AtomicLockRequest> AtomicLock => PublisherActionsWithNullMessages.AtomicLock;
		public override IPublisherAction<AtomicTransferRequest> AtomicTransfer => PublisherActionsWithNullMessages.AtomicTransfer;
		public override IPublisherAction<EarmarkConfirmation> EarmarkConfirmation => PublisherActionsWithNullMessages.EarmarkConfirmation;
		public override IPublisherAction<AtomicTransferConfirmation> AtomicTransferConfirmation => PublisherActionsWithNullMessages.AtomicTransferConfirmation;
		public override IPublisherAction<UpdateLedgerRequest> UpdateLedger => PublisherActionsWithNullMessages.UpdateLedger;
		public override IPublisherAction<FIToFICustomerCreditTransferV10> PayawayCreate => PublisherActionsWithNullMessages.PayawayCreate;
		public override IPublisherAction<BankToCustomerDebitCreditNotificationV09> PayawayConfirmation => PublisherActionsWithNullMessages.PayawayConfirmation;
	}
}
