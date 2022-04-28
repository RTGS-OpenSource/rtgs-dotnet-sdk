using RTGS.ISO20022.Messages.Admi_002_001.V01;
using RTGS.ISO20022.Messages.Camt_054_001.V09;
using RTGS.ISO20022.Messages.Pacs_008_001.V10;

namespace RTGS.DotNetSDK.IntegrationTests.Publisher.TestData;

public abstract class BaseSignedPublisherActionData : BaseActionData
{
	public abstract IPublisherAction<FIToFICustomerCreditTransferV10> PayawayCreate { get; }
	public abstract IPublisherAction<Admi00200101> PayawayReject { get; }
	public abstract IPublisherAction<BankToCustomerDebitCreditNotificationV09> PayawayConfirm { get; }
}
