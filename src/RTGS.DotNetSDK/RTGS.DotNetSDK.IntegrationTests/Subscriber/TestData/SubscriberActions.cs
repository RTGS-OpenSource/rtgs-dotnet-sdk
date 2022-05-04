using RTGS.DotNetSDK.Publisher.IdCrypt.Messages;
using RTGS.ISO20022.Messages.Admi_002_001.V01;
using RTGS.ISO20022.Messages.Camt_054_001.V09;
using RTGS.ISO20022.Messages.Pacs_008_001.V10;

namespace RTGS.DotNetSDK.IntegrationTests.Subscriber.TestData;

public static class SubscriberActions
{
	private static readonly Dictionary<string, string> DefaultSigningHeaders = new()
	{
		{ "public-did-signature", "public-did-signature" },
		{ "pairwise-did-signature", "pairwise-did-signature" },
		{ "alias", "alias" }
	};

	public static readonly SubscriberAction<FIToFICustomerCreditTransferV10> PayawayFundsV1 =
		new("PayawayFunds", ValidMessages.PayawayFunds, DefaultSigningHeaders);

	public static readonly SubscriberAction<BankToCustomerDebitCreditNotificationV09> PayawayCompleteV1 =
		new("PayawayComplete", ValidMessages.PayawayComplete);

	public static readonly SubscriberAction<Admi00200101> MessageRejectedV1 =
		new("MessageRejected", ValidMessages.MessageRejected);

	public static readonly SubscriberAction<AtomicLockResponseV1> AtomicLockResponseV1 =
		new("payment.lock.v2", ValidMessages.AtomicLockResponseV1);

	public static readonly SubscriberAction<AtomicTransferResponseV1> AtomicTransferResponseV1 =
		new("payment.block.v2", ValidMessages.AtomicTransferResponseV1);

	public static readonly SubscriberAction<AtomicTransferFundsV1> AtomicTransferFundsV1 =
		new("payment.blockfunds.v1", ValidMessages.AtomicTransferFundsV1);

	public static readonly SubscriberAction<EarmarkFundsV1> EarmarkFundsV1 =
		new("EarmarkFunds", ValidMessages.EarmarkFundsV1);

	public static readonly SubscriberAction<EarmarkCompleteV1> EarmarkCompleteV1 =
		new("EarmarkComplete", ValidMessages.EarmarkCompleteV1);

	public static readonly SubscriberAction<EarmarkReleaseV1> EarmarkReleaseV1 =
		new("EarmarkRelease", ValidMessages.EarmarkReleaseV1);

	public static readonly SubscriberAction<BankPartnersResponseV1> BankPartnersResponseV1 =
		new("bank.partners.v1", ValidMessages.BankPartnersResponseV1);

	public static readonly SubscriberAction<IdCryptInvitationConfirmationV1> IdCryptInvitationConfirmationV1 =
		new("idcrypt.invitationconfirmation.v1", ValidMessages.IdCryptInvitationConfirmationV1);
}
