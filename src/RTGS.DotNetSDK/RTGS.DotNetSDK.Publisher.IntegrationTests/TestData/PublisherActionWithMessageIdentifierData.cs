using RTGS.DotNetSDK.Publisher.Messages;
using RTGS.ISO20022.Messages.Camt_054_001.V09;
using RTGS.ISO20022.Messages.Pacs_008_001.V10;

namespace RTGS.DotNetSDK.Publisher.IntegrationTests.TestData
{
	public class PublisherActionWithMessageIdentifierData : BasePublisherActionData
	{
		public override IPublisherAction<AtomicLockRequestV1> AtomicLock => new PublisherActionWithMessageIdentifier<AtomicLockRequestV1>(PublisherActions.AtomicLock, "payment.lock.v2");
		public override IPublisherAction<AtomicTransferRequestV1> AtomicTransfer => new PublisherActionWithMessageIdentifier<AtomicTransferRequestV1>(PublisherActions.AtomicTransfer, "payment.block.v1");
		public override IPublisherAction<EarmarkConfirmationV1> EarmarkConfirmation => new PublisherActionWithMessageIdentifier<EarmarkConfirmationV1>(PublisherActions.EarmarkConfirmation, "payment.earmarkconfirmation.v1");
		public override IPublisherAction<AtomicTransferConfirmationV1> AtomicTransferConfirmation => new PublisherActionWithMessageIdentifier<AtomicTransferConfirmationV1>(PublisherActions.AtomicTransferConfirmation, "payment.blockconfirmation.v1");
		public override IPublisherAction<UpdateLedgerRequestV1> UpdateLedger => new PublisherActionWithMessageIdentifier<UpdateLedgerRequestV1>(PublisherActions.UpdateLedger, "payment.update.ledger.v1");
		public override IPublisherAction<FIToFICustomerCreditTransferV10> PayawayCreate => new PublisherActionWithMessageIdentifier<FIToFICustomerCreditTransferV10>(PublisherActions.PayawayCreate, "payaway.create.v1");
		public override IPublisherAction<BankToCustomerDebitCreditNotificationV09> PayawayConfirmation => new PublisherActionWithMessageIdentifier<BankToCustomerDebitCreditNotificationV09>(PublisherActions.PayawayConfirmation, "payaway.confirmation.v1");
	}
}
