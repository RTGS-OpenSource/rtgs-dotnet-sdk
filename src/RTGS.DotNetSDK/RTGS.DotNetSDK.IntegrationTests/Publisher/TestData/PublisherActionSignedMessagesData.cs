using RTGS.ISO20022.Messages.Pacs_008_001.V10;

namespace RTGS.DotNetSDK.IntegrationTests.Publisher.TestData;

public class PublisherActionSignedMessagesData : BaseSignedPublisherActionData
{
	public override IPublisherAction<FIToFICustomerCreditTransferV10> PayawayCreate => PublisherActions.PayawayCreate;
}
