namespace RTGS.DotNetSDK.IntegrationTests.Subscriber.TestData;

public abstract class BaseSignedSubscriberActionData : BaseActionData
{
	public abstract ISubscriberAction<ISO20022.Messages.Pacs_008_001.V10.FIToFICustomerCreditTransferV10> PayawayFundsV1 { get; }
}
