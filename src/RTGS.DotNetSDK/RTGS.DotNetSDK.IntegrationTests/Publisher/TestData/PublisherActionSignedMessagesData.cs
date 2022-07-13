namespace RTGS.DotNetSDK.IntegrationTests.Publisher.TestData;

public class PublisherActionSignedMessagesData : BaseSignedPublisherActionData
{
	public override IPublisherAction<AtomicLockRequestV1> AtomicLock => PublisherActions.AtomicLock;
	public override IPublisherAction<AtomicTransferRequestV1> AtomicTransfer => PublisherActions.AtomicTransfer;
	public override IPublisherAction<PayawayCreationV1> PayawayCreate => PublisherActions.PayawayCreate;
	public override IPublisherAction<PayawayRejectionV1> PayawayReject => PublisherActions.PayawayRejection;
	public override IPublisherAction<PayawayConfirmationV1> PayawayConfirm => PublisherActions.PayawayConfirmation;
	public override IPublisherAction<AtomicLockRequestV2> AtomicLockV2 => PublisherActions.AtomicLockV2;
}
