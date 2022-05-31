namespace RTGS.DotNetSDK.IntegrationTests.Subscriber.TestData;

public class SubscriberActionData : BaseSubscriberActionData
{
	public override ISubscriberAction<PayawayFundsV1> PayawayFundsV1 =>
		SubscriberActions.PayawayFundsV1;

	public override ISubscriberAction<PayawayCompleteV1> PayawayCompleteV1 =>
		SubscriberActions.PayawayCompleteV1;

	public override ISubscriberAction<MessageRejectV1> MessageRejectedV1 =>
		SubscriberActions.MessageRejectV1;

	public override ISubscriberAction<AtomicLockResponseV1> AtomicLockResponseV1 =>
		SubscriberActions.AtomicLockResponseV1;

	public override ISubscriberAction<AtomicTransferResponseV1> AtomicTransferResponseV1 =>
		SubscriberActions.AtomicTransferResponseV1;

	public override ISubscriberAction<AtomicTransferFundsV1> AtomicTransferFundsV1 =>
		SubscriberActions.AtomicTransferFundsV1;

	public override ISubscriberAction<EarmarkCompleteV1> EarmarkCompleteV1 =>
		SubscriberActions.EarmarkCompleteV1;

	public override ISubscriberAction<EarmarkReleaseV1> EarmarkReleaseV1 =>
		SubscriberActions.EarmarkReleaseV1;

	public override ISubscriberAction<BankPartnersResponseV1> BankPartnersResponseV1 =>
		SubscriberActions.BankPartnersResponseV1;
}
