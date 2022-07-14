namespace RTGS.DotNetSDK.IntegrationTests.Publisher.TestData;

public class PublisherActionWithNullMessagesData : BasePublisherActionData
{
	public override IPublisherAction<AtomicLockRequestV1> AtomicLock => PublisherActionsWithNullMessages.AtomicLock;
	public override IPublisherAction<AtomicTransferRequestV1> AtomicTransfer => PublisherActionsWithNullMessages.AtomicTransfer;
	public override IPublisherAction<EarmarkConfirmationV1> EarmarkConfirmation => PublisherActionsWithNullMessages.EarmarkConfirmation;
	public override IPublisherAction<AtomicTransferConfirmationV1> AtomicTransferConfirmation => PublisherActionsWithNullMessages.AtomicTransferConfirmation;
	public override IPublisherAction<UpdateLedgerRequestV1> UpdateLedger => PublisherActionsWithNullMessages.UpdateLedger;
	public override IPublisherAction<PayawayCreationV1> PayawayCreate => PublisherActionsWithNullMessages.PayawayCreate;
	public override IPublisherAction<PayawayConfirmationV1> PayawayConfirmation => PublisherActionsWithNullMessages.PayawayConfirmation;
	public override IPublisherAction<PayawayRejectionV1> PayawayRejection => PublisherActionsWithNullMessages.PayawayRejection;
	public override IPublisherAction<BankPartnersRequestV1> BankPartnersRequest => PublisherActionsWithNullMessages.BankPartnersRequest;
	public override IPublisherAction<AtomicLockRequestV2> AtomicLockV2IBAN => PublisherActionsWithNullMessages.AtomicLockV2;
	public override IPublisherAction<AtomicLockRequestV2> AtomicLockV2OtherId => PublisherActionsWithNullMessages.AtomicLockV2;
}
