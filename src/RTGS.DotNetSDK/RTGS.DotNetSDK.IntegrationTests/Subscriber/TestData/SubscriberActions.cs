using RTGS.ISO20022.Messages.Pacs_008_001.V10;

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

	public static readonly VerifiableSubscriberAction<PayawayFundsV1, FIToFICustomerCreditTransferV10> PayawayFundsV1 =
		new("PayawayFundsV1", ValidMessages.PayawayFunds, ValidMessages.PayawayFundsVerifiable, DefaultSigningHeaders);

	public static readonly VerifiableSubscriberAction<PayawayCompleteV1, Dictionary<string, object>> PayawayCompleteV1 =
		new("PayawayCompleteV1", ValidMessages.PayawayComplete, ValidMessages.PayawayCompleteVerifiable,
			DefaultSigningHeaders);

	public static readonly VerifiableSubscriberAction<MessageRejectV1, Dictionary<string, object>> MessageRejectV1 =
		new("MessageRejectV1", ValidMessages.MessageRejected, ValidMessages.MessageRejectedVerifiable,
			DefaultSigningHeaders);

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

	public static readonly VerifiableSubscriberAction<AtomicLockApproveV2, Dictionary<string, object>>
		AtomicLockApproveV2IBAN =
			new("AtomicLockApproveV2", ValidMessages.AtomicLockApproveV2IBAN,
				ValidMessages.AtomicLockApproveV2IBANVerifiable, DefaultSigningHeaders);

	public static readonly VerifiableSubscriberAction<AtomicLockApproveV2, Dictionary<string, object>>
		AtomicLockApproveV2OtherId =
			new("AtomicLockApproveV2", ValidMessages.AtomicLockApproveV2OtherId,
				ValidMessages.AtomicLockApproveV2OtherIdVerifiable, DefaultSigningHeaders);
}
