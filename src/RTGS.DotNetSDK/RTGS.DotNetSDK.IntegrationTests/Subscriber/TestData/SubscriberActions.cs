﻿using RTGS.DotNetSDK.Subscriber.Messages;
using RTGS.ISO20022.Messages.Admi_002_001.V01;
using RTGS.ISO20022.Messages.Camt_054_001.V09;
using RTGS.ISO20022.Messages.Pacs_008_001.V10;
using static RTGS.DotNetSDK.IntegrationTests.Subscriber.TestHandlers.AllTestHandlers;

namespace RTGS.DotNetSDK.IntegrationTests.Subscriber.TestData;

public static class SubscriberActions
{
	public static readonly SubscriberAction<FIToFICustomerCreditTransferV10> PayawayFundsV1 =
		new(new AllTestHandlers(), handlers => handlers.OfType<TestPayawayFundsV1Handler>().Single(), "PayawayFunds", ValidMessages.PayawayFunds);

	public static readonly SubscriberAction<BankToCustomerDebitCreditNotificationV09> PayawayCompleteV1 =
		new(new AllTestHandlers(), handlers => handlers.OfType<TestPayawayCompleteV1Handler>().Single(), "PayawayComplete", ValidMessages.PayawayComplete);

	public static readonly SubscriberAction<Admi00200101> MessageRejectedV1 =
		new(new AllTestHandlers(), handlers => handlers.OfType<TestMessageRejectedV1Handler>().Single(), "MessageRejected", ValidMessages.MessageRejected);

	public static readonly SubscriberAction<AtomicLockResponseV1> AtomicLockResponseV1 =
		new(new AllTestHandlers(), handlers => handlers.OfType<TestAtomicLockResponseV1Handler>().Single(), "payment.lock.v2", ValidMessages.AtomicLockResponseV1);

	public static readonly SubscriberAction<AtomicTransferResponseV1> AtomicTransferResponseV1 =
		new(new AllTestHandlers(), handlers => handlers.OfType<TestAtomicTransferResponseV1Handler>().Single(), "payment.block.v2", ValidMessages.AtomicTransferResponseV1);

	public static readonly SubscriberAction<AtomicTransferFundsV1> AtomicTransferFundsV1 =
		new(new AllTestHandlers(), handlers => handlers.OfType<TestAtomicTransferFundsV1Handler>().Single(), "payment.blockfunds.v1", ValidMessages.AtomicTransferFundsV1);

	public static readonly SubscriberAction<EarmarkFundsV1> EarmarkFundsV1 =
		new(new AllTestHandlers(), handlers => handlers.OfType<TestEarmarkFundsV1Handler>().Single(), "EarmarkFunds", ValidMessages.EarmarkFundsV1);

	public static readonly SubscriberAction<EarmarkCompleteV1> EarmarkCompleteV1 =
		new(new AllTestHandlers(), handlers => handlers.OfType<TestEarmarkCompleteV1Handler>().Single(), "EarmarkComplete", ValidMessages.EarmarkCompleteV1);

	public static readonly SubscriberAction<EarmarkReleaseV1> EarmarkReleaseV1 =
		new(new AllTestHandlers(), handlers => handlers.OfType<TestEarmarkReleaseV1Handler>().Single(), "EarmarkRelease", ValidMessages.EarmarkReleaseV1);

	public static readonly SubscriberAction<BankPartnersResponseV1> BankPartnersResponseV1 =
		new(new AllTestHandlers(), handlers => handlers.OfType<TestBankPartnersResponseV1>().Single(), "bank.partners.v1", ValidMessages.BankPartnersResponseV1);

	public static readonly SubscriberAction<IdCryptInvitationConfirmationV1> IdCryptInvitationConfirmationV1 =
		new(new AllTestHandlers(), handlers => handlers.OfType<TestIdCryptInvitationConfirmationV1>().Single(), "idcrypt.invitation.v1", ValidMessages.IdCryptInvitationConfirmationV1);
}