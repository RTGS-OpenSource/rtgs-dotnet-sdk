namespace RTGS.DotNetSDK.IntegrationTests.Publisher.TestData;

public abstract class BasePublisherActionData : BaseActionData
{
	public abstract IPublisherAction<AtomicLockRequestV1> AtomicLock { get; }
	public abstract IPublisherAction<AtomicTransferRequestV1> AtomicTransfer { get; }
	public abstract IPublisherAction<EarmarkConfirmationV1> EarmarkConfirmation { get; }
	public abstract IPublisherAction<AtomicTransferConfirmationV1> AtomicTransferConfirmation { get; }
	public abstract IPublisherAction<UpdateLedgerRequestV1> UpdateLedger { get; }
	public abstract IPublisherAction<PayawayCreationV1> PayawayCreate { get; }
	public abstract IPublisherAction<PayawayConfirmationV1> PayawayConfirmation { get; }
	public abstract IPublisherAction<PayawayRejectionV1> PayawayRejection { get; }
	public abstract IPublisherAction<BankPartnersRequestV1> BankPartnersRequest { get; }
	public abstract IPublisherAction<AtomicLockRequestV2> AtomicLockV2IBAN { get; }
	public abstract IPublisherAction<AtomicLockRequestV2> AtomicLockV2OtherId { get; }
}
