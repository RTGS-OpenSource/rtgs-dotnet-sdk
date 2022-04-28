using RTGS.ISO20022.Messages.Admi_002_001.V01;
using RTGS.ISO20022.Messages.Camt_054_001.V09;
using RTGS.ISO20022.Messages.Pacs_008_001.V10;

namespace RTGS.DotNetSDK.IntegrationTests.Publisher.TestData;

public class PublisherActionSignedMessagesData : BaseSignedPublisherActionData
{
	public override IPublisherAction<FIToFICustomerCreditTransferV10> PayawayCreate => PublisherActions.PayawayCreate;
	public override IPublisherAction<Admi00200101> PayawayReject => PublisherActions.PayawayRejection;
	public override IPublisherAction<BankToCustomerDebitCreditNotificationV09> PayawayConfirm =>
		PublisherActions.PayawayConfirmation;
}
