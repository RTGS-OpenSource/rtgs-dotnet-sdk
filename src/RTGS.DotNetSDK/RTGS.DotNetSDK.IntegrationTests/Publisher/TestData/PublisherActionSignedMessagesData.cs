namespace RTGS.DotNetSDK.IntegrationTests.Publisher.TestData;

public class PublisherActionSignedMessagesData : BaseSignedPublisherActionData
{
	public override IPublisherAction<PayawayCreationV1> PayawayCreate => PublisherActions.PayawayCreate;
	public override IPublisherAction<PayawayRejectionV1> PayawayReject => PublisherActions.PayawayRejection;
	public override IPublisherAction<PayawayConfirmationV1> PayawayConfirm => PublisherActions.PayawayConfirmation;
}
