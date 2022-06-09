namespace RTGS.DotNetSDK.IntegrationTests.Subscriber.TestData;

public static class SubscriberActions
{
	public static readonly Dictionary<string, string> DefaultSigningHeaders = new()
	{
		{ "public-did-signature", "public-did-signature" },
		{ "pairwise-did-signature", "pairwise-did-signature" },
		{ "alias", "alias" },
		{ "from-rtgs-global-id", "from-rtgs-global-id" }
	};

	public static readonly SubscriberAction<PayawayFundsV1> PayawayFundsV1 =
		new("PayawayFundsV1", ValidMessages.PayawayFunds, DefaultSigningHeaders);

	public static readonly SubscriberAction<PayawayCompleteV1> PayawayCompleteV1 =
		new("PayawayCompleteV1", ValidMessages.PayawayComplete, DefaultSigningHeaders);

	public static readonly SubscriberAction<MessageRejectV1> MessageRejectV1 =
		new("MessageRejectV1", ValidMessages.MessageRejected, DefaultSigningHeaders);

	public static readonly SubscriberAction<AtomicLockResponseV1> AtomicLockResponseV1 =
		new("AtomicLockResponseV1", ValidMessages.AtomicLockResponseV1);

	public static readonly SubscriberAction<AtomicTransferResponseV1> AtomicTransferResponseV1 =
		new("AtomicTransferResponseV1", ValidMessages.AtomicTransferResponseV1);

	public static readonly SubscriberAction<AtomicTransferFundsV1> AtomicTransferFundsV1 =
		new("AtomicTransferFundsV1", ValidMessages.AtomicTransferFundsV1);

	public static readonly SubscriberAction<EarmarkFundsV1> EarmarkFundsV1 =
		new("EarmarkFundsV1", ValidMessages.EarmarkFundsV1);

	public static readonly SubscriberAction<EarmarkCompleteV1> EarmarkCompleteV1 =
		new("EarmarkCompleteV1", ValidMessages.EarmarkCompleteV1);

	public static readonly SubscriberAction<EarmarkReleaseV1> EarmarkReleaseV1 =
		new("EarmarkReleaseV1", ValidMessages.EarmarkReleaseV1);

	public static readonly SubscriberAction<BankPartnersResponseV1> BankPartnersResponseV1 =
		new("BankPartnersResponseV1", ValidMessages.BankPartnersResponseV1);
}
