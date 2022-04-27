using RTGS.ISO20022.Messages.Pacs_008_001.V10;

namespace RTGS.DotNetSDK.IntegrationTests.Subscriber.TestData;

public class SubscriberActionSignedMessagesData : BaseSignedSubscriberActionData
{
	public override ISubscriberAction<FIToFICustomerCreditTransferV10> PayawayFundsV1 =>
		SubscriberActions.PayawayFundsV1;
}
