namespace RTGS.DotNetSDK.IntegrationTests.Publisher.TestData;

public class PublisherActionData : BasePublisherActionData
{
	public override IPublisherAction<AtomicLockRequestV1> AtomicLock => PublisherActions.AtomicLock;
	public override IPublisherAction<AtomicTransferRequestV1> AtomicTransfer => PublisherActions.AtomicTransfer;
	public override IPublisherAction<EarmarkConfirmationV1> EarmarkConfirmation => PublisherActions.EarmarkConfirmation;
	public override IPublisherAction<AtomicTransferConfirmationV1> AtomicTransferConfirmation => PublisherActions.AtomicTransferConfirmation;
	public override IPublisherAction<UpdateLedgerRequestV1> UpdateLedger => PublisherActions.UpdateLedger;
	public override IPublisherAction<PayawayCreationV1> PayawayCreate => PublisherActions.PayawayCreate;
	public override IPublisherAction<PayawayConfirmationV1> PayawayConfirmation => PublisherActions.PayawayConfirmation;
	public override IPublisherAction<PayawayRejectionV1> PayawayRejection => PublisherActions.PayawayRejection;
	public override IPublisherAction<BankPartnersRequestV1> BankPartnersRequest => PublisherActions.BankPartnersRequest;
	
	public override IPublisherAction<AtomicLockRequestV2> AtomicLockV2IBAN => PublisherActions.AtomicLockV2IBAN;
	public override IPublisherAction<AtomicLockRequestV2> AtomicLockV2OtherId => PublisherActions.AtomicLockV2OtherId;
}
