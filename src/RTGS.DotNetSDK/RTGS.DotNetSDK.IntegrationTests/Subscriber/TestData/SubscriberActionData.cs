using RTGS.DotNetSDK.Subscriber.Messages;
using RTGS.ISO20022.Messages.Admi_002_001.V01;
using RTGS.ISO20022.Messages.Camt_054_001.V09;
using RTGS.ISO20022.Messages.Pacs_008_001.V10;

namespace RTGS.DotNetSDK.IntegrationTests.Subscriber.TestData;

public class SubscriberActionData : BaseSubscriberActionData
{
	public override ISubscriberAction<FIToFICustomerCreditTransferV10> PayawayFundsV1 =>
		SubscriberActions.PayawayFundsV1;

	public override ISubscriberAction<BankToCustomerDebitCreditNotificationV09> PayawayCompleteV1 =>
		SubscriberActions.PayawayCompleteV1;

	public override ISubscriberAction<Admi00200101> MessageRejectedV1 =>
		SubscriberActions.MessageRejectedV1;

	public override ISubscriberAction<AtomicLockResponseV1> AtomicLockResponseV1 =>
		SubscriberActions.AtomicLockResponseV1;

	public override ISubscriberAction<AtomicTransferResponseV1> AtomicTransferResponseV1 =>
		SubscriberActions.AtomicTransferResponseV1;

	public override ISubscriberAction<AtomicTransferFundsV1> AtomicTransferFundsV1 =>
		SubscriberActions.AtomicTransferFundsV1;

	public override ISubscriberAction<EarmarkFundsV1> EarmarkFundsV1 =>
		SubscriberActions.EarmarkFundsV1;

	public override ISubscriberAction<EarmarkCompleteV1> EarmarkCompleteV1 =>
		SubscriberActions.EarmarkCompleteV1;

	public override ISubscriberAction<EarmarkReleaseV1> EarmarkReleaseV1 =>
		SubscriberActions.EarmarkReleaseV1;

	public override ISubscriberAction<BankPartnersResponseV1> BankPartnersResponseV1 =>
		SubscriberActions.BankPartnersResponseV1;

	public override ISubscriberAction<IdCryptInvitationConfirmationV1> IdCryptInvitationConfirmationV1 =>
		SubscriberActions.IdCryptInvitationConfirmationV1;
}
