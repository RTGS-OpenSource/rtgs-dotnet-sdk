namespace RTGS.DotNetSDK.IntegrationTests.Subscriber.TestData;

public class SubscriberActionSignedMessagesData : BaseSignedSubscriberActionData
{
	public override ISubscriberAction<PayawayFundsV1> PayawayFundsV1 =>
		SubscriberActions.PayawayFundsV1;
}
