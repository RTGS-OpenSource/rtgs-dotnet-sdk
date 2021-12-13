﻿using RTGS.DotNetSDK.Publisher.Messages;
using RTGS.ISO20022.Messages.Camt_054_001.V09;
using RTGS.ISO20022.Messages.Pacs_008_001.V10;

namespace RTGS.DotNetSDK.Publisher.IntegrationTests.TestData
{
	public class PublisherActionWithMessageIdentifierData : BasePublisherActionData
	{
		public override IPublisherAction<AtomicLockRequest> AtomicLock => new PublisherActionWithMessageIdentifier<AtomicLockRequest>(PublisherActions.AtomicLock, "payment.lock.v2");
		public override IPublisherAction<AtomicTransferRequest> AtomicTransfer => new PublisherActionWithMessageIdentifier<AtomicTransferRequest>(PublisherActions.AtomicTransfer, "payment.block.v1");
		public override IPublisherAction<EarmarkConfirmation> EarmarkConfirmation => new PublisherActionWithMessageIdentifier<EarmarkConfirmation>(PublisherActions.EarmarkConfirmation, "payment.earmarkconfirmation.v1");
		public override IPublisherAction<AtomicTransferConfirmation> AtomicTransferConfirmation => new PublisherActionWithMessageIdentifier<AtomicTransferConfirmation>(PublisherActions.AtomicTransferConfirmation, "payment.blockconfirmation.v1");
		public override IPublisherAction<UpdateLedgerRequest> UpdateLedger => new PublisherActionWithMessageIdentifier<UpdateLedgerRequest>(PublisherActions.UpdateLedger, "payment.update.ledger.v1");
		public override IPublisherAction<FIToFICustomerCreditTransferV10> PayawayCreate => new PublisherActionWithMessageIdentifier<FIToFICustomerCreditTransferV10>(PublisherActions.PayawayCreate, "payaway.create.v1");
		public override IPublisherAction<BankToCustomerDebitCreditNotificationV09> PayawayConfirmation => new PublisherActionWithMessageIdentifier<BankToCustomerDebitCreditNotificationV09>(PublisherActions.PayawayConfirmation, "payaway.confirmation.v1");
	}
}