namespace RTGS.DotNetSDK.IntegrationTests.Publisher.TestData;

public abstract class BaseSignedPublisherActionData : BaseActionData
{
	public abstract IPublisherAction<AtomicLockRequestV1> AtomicLock { get; }
	public abstract IPublisherAction<AtomicTransferRequestV1> AtomicTransfer { get; }
	public abstract IPublisherAction<PayawayCreationV1> PayawayCreate { get; }
	public abstract IPublisherAction<PayawayRejectionV1> PayawayReject { get; }
	public abstract IPublisherAction<PayawayConfirmationV1> PayawayConfirm { get; }
	public abstract IPublisherAction<AtomicLockRequestV2> AtomicLockV2IBAN { get; }
	public abstract IPublisherAction<AtomicLockRequestV2> AtomicLockV2OtherId { get; }
}
