namespace RTGS.DotNetSDK.IntegrationTests.Subscriber.TestData;

public abstract class BaseSubscriberActionData : BaseActionData
{
	public abstract ISubscriberAction<PayawayFundsV1> PayawayFundsV1 { get; }
	public abstract ISubscriberAction<PayawayCompleteV1> PayawayCompleteV1 { get; }
	public abstract ISubscriberAction<MessageRejectV1> MessageRejectedV1 { get; }
	public abstract ISubscriberAction<AtomicLockResponseV1> AtomicLockResponseV1 { get; }
	public abstract ISubscriberAction<AtomicTransferResponseV1> AtomicTransferResponseV1 { get; }
	public abstract ISubscriberAction<AtomicTransferFundsV1> AtomicTransferFundsV1 { get; }
	public abstract ISubscriberAction<EarmarkFundsV1> EarmarkFundsV1 { get; }
	public abstract ISubscriberAction<EarmarkCompleteV1> EarmarkCompleteV1 { get; }
	public abstract ISubscriberAction<EarmarkReleaseV1> EarmarkReleaseV1 { get; }
	public abstract ISubscriberAction<BankPartnersResponseV1> BankPartnersResponseV1 { get; }
	public abstract ISubscriberAction<AtomicLockApproveV2> AtomicLockApproveV2 { get; }
}
