using RTGS.DotNetSDK.Publisher.Messages;
using RTGS.ISO20022.Messages.Camt_054_001.V09;
using RTGS.ISO20022.Messages.Pacs_008_001.V10;

namespace RTGS.DotNetSDK.Publisher.IntegrationTests.TestData
{
	public class PublisherActionWithInstructionTypeData : BasePublisherActionData
	{
		public override IPublisherAction<AtomicLockRequest> AtomicLock => new PublisherActionWithInstructionType<AtomicLockRequest>(PublisherActions.AtomicLock, "payment.lock.v1");
		public override IPublisherAction<AtomicTransferRequest> AtomicTransfer => new PublisherActionWithInstructionType<AtomicTransferRequest>(PublisherActions.AtomicTransfer, "payment.block.v1");
		public override IPublisherAction<EarmarkConfirmation> EarmarkConfirmation => new PublisherActionWithInstructionType<EarmarkConfirmation>(PublisherActions.EarmarkConfirmation, "payment.earmarkconfirmation.v1");
		public override IPublisherAction<TransferConfirmation> TransferConfirmation => new PublisherActionWithInstructionType<TransferConfirmation>(PublisherActions.TransferConfirmation, "payment.blockconfirmation.v1");
		public override IPublisherAction<UpdateLedgerRequest> UpdateLedger => new PublisherActionWithInstructionType<UpdateLedgerRequest>(PublisherActions.UpdateLedger, "payment.update.ledger.v1");
		public override IPublisherAction<FIToFICustomerCreditTransferV10> PayawayCreate => new PublisherActionWithInstructionType<FIToFICustomerCreditTransferV10>(PublisherActions.PayawayCreate, "payaway.create.v1");
		public override IPublisherAction<BankToCustomerDebitCreditNotificationV09> PayawayConfirmation => new PublisherActionWithInstructionType<BankToCustomerDebitCreditNotificationV09>(PublisherActions.PayawayConfirmation, "payaway.confirmation.v1");
	}
}
