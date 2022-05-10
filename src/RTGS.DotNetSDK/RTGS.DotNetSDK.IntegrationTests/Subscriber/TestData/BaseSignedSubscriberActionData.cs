namespace RTGS.DotNetSDK.IntegrationTests.Subscriber.TestData;

public abstract class BaseSignedSubscriberActionData : BaseActionData
{
	public abstract ISubscriberAction<PayawayFundsV1> PayawayFundsV1 { get; }
}
