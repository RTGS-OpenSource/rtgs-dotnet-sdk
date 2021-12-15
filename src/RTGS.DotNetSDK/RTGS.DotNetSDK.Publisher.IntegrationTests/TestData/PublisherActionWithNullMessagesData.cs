using RTGS.ISO20022.Messages.Camt_054_001.V09;
using RTGS.ISO20022.Messages.Pacs_008_001.V10;

namespace RTGS.DotNetSDK.Publisher.IntegrationTests.TestData
{
	public class PublisherActionWithNullMessagesData : BasePublisherActionData
	{
		public override IPublisherAction<AtomicLockRequestV1> AtomicLock => PublisherActionsWithNullMessages.AtomicLock;
		public override IPublisherAction<AtomicTransferRequestV1> AtomicTransfer => PublisherActionsWithNullMessages.AtomicTransfer;
		public override IPublisherAction<EarmarkConfirmationV1> EarmarkConfirmation => PublisherActionsWithNullMessages.EarmarkConfirmation;
		public override IPublisherAction<AtomicTransferConfirmationV1> AtomicTransferConfirmation => PublisherActionsWithNullMessages.AtomicTransferConfirmation;
		public override IPublisherAction<UpdateLedgerRequestV1> UpdateLedger => PublisherActionsWithNullMessages.UpdateLedger;
		public override IPublisherAction<FIToFICustomerCreditTransferV10> PayawayCreate => PublisherActionsWithNullMessages.PayawayCreate;
		public override IPublisherAction<BankToCustomerDebitCreditNotificationV09> PayawayConfirmation => PublisherActionsWithNullMessages.PayawayConfirmation;
	}
}
