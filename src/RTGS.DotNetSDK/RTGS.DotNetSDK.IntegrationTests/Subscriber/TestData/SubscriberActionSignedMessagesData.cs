namespace RTGS.DotNetSDK.IntegrationTests.Subscriber.TestData;

public class SubscriberActionSignedMessagesData : BaseSignedSubscriberActionData
{
	public override ISubscriberAction<PayawayFundsV1> PayawayFundsV1 =>
		SubscriberActions.PayawayFundsV1;

	public override ISubscriberAction<MessageRejectV1> MessageRejectV1 =>
		SubscriberActions.MessageRejectV1;

	public override ISubscriberAction<PayawayCompleteV1> PayawayCompleteV1 =>
		SubscriberActions.PayawayCompleteV1;
}
