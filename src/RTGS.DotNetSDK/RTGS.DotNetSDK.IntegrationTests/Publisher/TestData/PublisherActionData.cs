using RTGS.ISO20022.Messages.Admi_002_001.V01;
using RTGS.ISO20022.Messages.Camt_054_001.V09;
using RTGS.ISO20022.Messages.Pacs_008_001.V10;

namespace RTGS.DotNetSDK.IntegrationTests.Publisher.TestData;

public class PublisherActionData : BasePublisherActionData
{
	public override IPublisherAction<AtomicLockRequestV1> AtomicLock => PublisherActions.AtomicLock;
	public override IPublisherAction<AtomicTransferRequestV1> AtomicTransfer => PublisherActions.AtomicTransfer;
	public override IPublisherAction<EarmarkConfirmationV1> EarmarkConfirmation => PublisherActions.EarmarkConfirmation;
	public override IPublisherAction<AtomicTransferConfirmationV1> AtomicTransferConfirmation => PublisherActions.AtomicTransferConfirmation;
	public override IPublisherAction<UpdateLedgerRequestV1> UpdateLedger => PublisherActions.UpdateLedger;
	public override IPublisherAction<FIToFICustomerCreditTransferV10> PayawayCreate => PublisherActions.PayawayCreate;
	public override IPublisherAction<BankToCustomerDebitCreditNotificationV09> PayawayConfirmation => PublisherActions.PayawayConfirmation;
	public override IPublisherAction<Admi00200101> PayawayRejection => PublisherActions.PayawayRejection;
	public override IPublisherAction<BankPartnersRequestV1> BankPartnersRequest => PublisherActions.BankPartnersRequest;
}
