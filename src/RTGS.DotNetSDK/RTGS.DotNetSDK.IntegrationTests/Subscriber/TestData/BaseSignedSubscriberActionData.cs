namespace RTGS.DotNetSDK.IntegrationTests.Subscriber.TestData;

public abstract class BaseSignedSubscriberActionData : BaseActionData
{
	public abstract ISubscriberAction<PayawayFundsV1> PayawayFundsV1 { get; }

	public abstract ISubscriberAction<MessageRejectV1> MessageRejectV1 { get; }

	public abstract ISubscriberAction<PayawayCompleteV1> PayawayCompleteV1 { get; }

}
