﻿namespace RTGS.DotNetSDK.IntegrationTests.Publisher.TestData;

public class PublisherActionWithMessageIdentifierData : BasePublisherActionData
{
	public override IPublisherAction<AtomicLockRequestV1> AtomicLock => new PublisherActionWithMessageIdentifier<AtomicLockRequestV1>(PublisherActions.AtomicLock, "payment.lock.v2");
	public override IPublisherAction<AtomicTransferRequestV1> AtomicTransfer => new PublisherActionWithMessageIdentifier<AtomicTransferRequestV1>(PublisherActions.AtomicTransfer, "payment.block.v2");
	public override IPublisherAction<EarmarkConfirmationV1> EarmarkConfirmation => new PublisherActionWithMessageIdentifier<EarmarkConfirmationV1>(PublisherActions.EarmarkConfirmation, "payment.earmarkconfirmation.v1");
	public override IPublisherAction<AtomicTransferConfirmationV1> AtomicTransferConfirmation => new PublisherActionWithMessageIdentifier<AtomicTransferConfirmationV1>(PublisherActions.AtomicTransferConfirmation, "payment.blockconfirmation.v1");
	public override IPublisherAction<UpdateLedgerRequestV1> UpdateLedger => new PublisherActionWithMessageIdentifier<UpdateLedgerRequestV1>(PublisherActions.UpdateLedger, "payment.update.ledger.v2");
	public override IPublisherAction<PayawayCreationV1> PayawayCreate => new PublisherActionWithMessageIdentifier<PayawayCreationV1>(PublisherActions.PayawayCreate, "PayawayCreationV1");
	public override IPublisherAction<PayawayConfirmationV1> PayawayConfirmation => new PublisherActionWithMessageIdentifier<PayawayConfirmationV1>(PublisherActions.PayawayConfirmation, "PayawayConfirmationV1");
	public override IPublisherAction<PayawayRejectionV1> PayawayRejection => new PublisherActionWithMessageIdentifier<PayawayRejectionV1>(PublisherActions.PayawayRejection, "PayawayRejectionV1");
	public override IPublisherAction<BankPartnersRequestV1> BankPartnersRequest => new PublisherActionWithMessageIdentifier<BankPartnersRequestV1>(PublisherActions.BankPartnersRequest, "bank.partners.v1", false);
}
