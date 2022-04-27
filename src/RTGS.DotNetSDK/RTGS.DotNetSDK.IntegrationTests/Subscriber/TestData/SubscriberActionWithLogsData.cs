using RTGS.DotNetSDK.Publisher.IdCrypt.Messages;
using RTGS.DotNetSDK.Subscriber.Messages;
using RTGS.ISO20022.Messages.Admi_002_001.V01;
using RTGS.ISO20022.Messages.Camt_054_001.V09;
using RTGS.ISO20022.Messages.Pacs_008_001.V10;

namespace RTGS.DotNetSDK.IntegrationTests.Subscriber.TestData;

public class SubscriberActionWithLogsData : BaseSubscriberActionData
{
	public override ISubscriberAction<FIToFICustomerCreditTransferV10> PayawayFundsV1 =>
		new SubscriberActionWithLogs<FIToFICustomerCreditTransferV10>(
			SubscriberActions.PayawayFundsV1,
			new List<LogEntry>
			{
				new("RTGS Subscriber started", LogEventLevel.Information),
				new("PayawayFunds message received from RTGS", LogEventLevel.Information),
				new("RTGS Subscriber stopping", LogEventLevel.Information),
				new("RTGS Subscriber stopped", LogEventLevel.Information)
			});

	public override ISubscriberAction<BankToCustomerDebitCreditNotificationV09> PayawayCompleteV1 =>
		new SubscriberActionWithLogs<BankToCustomerDebitCreditNotificationV09>(
			SubscriberActions.PayawayCompleteV1,
			new List<LogEntry>
			{
				new("RTGS Subscriber started", LogEventLevel.Information),
				new("PayawayComplete message received from RTGS", LogEventLevel.Information),
				new("RTGS Subscriber stopping", LogEventLevel.Information),
				new("RTGS Subscriber stopped", LogEventLevel.Information)
			});

	public override ISubscriberAction<Admi00200101> MessageRejectedV1 =>
		new SubscriberActionWithLogs<Admi00200101>(
			SubscriberActions.MessageRejectedV1,
			new List<LogEntry>
			{
				new("RTGS Subscriber started", LogEventLevel.Information),
				new("MessageRejected message received from RTGS", LogEventLevel.Information),
				new("RTGS Subscriber stopping", LogEventLevel.Information),
				new("RTGS Subscriber stopped", LogEventLevel.Information)
			});

	public override ISubscriberAction<AtomicLockResponseV1> AtomicLockResponseV1 =>
		new SubscriberActionWithLogs<AtomicLockResponseV1>(
			SubscriberActions.AtomicLockResponseV1,
			new List<LogEntry>
			{
				new("RTGS Subscriber started", LogEventLevel.Information),
				new("payment.lock.v2 message received from RTGS", LogEventLevel.Information),
				new("RTGS Subscriber stopping", LogEventLevel.Information),
				new("RTGS Subscriber stopped", LogEventLevel.Information)
			});

	public override ISubscriberAction<AtomicTransferResponseV1> AtomicTransferResponseV1 =>
		new SubscriberActionWithLogs<AtomicTransferResponseV1>(
			SubscriberActions.AtomicTransferResponseV1,
			new List<LogEntry>
			{
				new("RTGS Subscriber started", LogEventLevel.Information),
				new("payment.block.v2 message received from RTGS", LogEventLevel.Information),
				new("RTGS Subscriber stopping", LogEventLevel.Information),
				new("RTGS Subscriber stopped", LogEventLevel.Information)
			});

	public override ISubscriberAction<AtomicTransferFundsV1> AtomicTransferFundsV1 =>
		new SubscriberActionWithLogs<AtomicTransferFundsV1>(
			SubscriberActions.AtomicTransferFundsV1,
			new List<LogEntry>
			{
				new("RTGS Subscriber started", LogEventLevel.Information),
				new("payment.blockfunds.v1 message received from RTGS", LogEventLevel.Information),
				new("RTGS Subscriber stopping", LogEventLevel.Information),
				new("RTGS Subscriber stopped", LogEventLevel.Information)
			});

	public override ISubscriberAction<EarmarkFundsV1> EarmarkFundsV1 =>
		new SubscriberActionWithLogs<EarmarkFundsV1>(
			SubscriberActions.EarmarkFundsV1,
			new List<LogEntry>
			{
				new("RTGS Subscriber started", LogEventLevel.Information),
				new("EarmarkFunds message received from RTGS", LogEventLevel.Information),
				new("RTGS Subscriber stopping", LogEventLevel.Information),
				new("RTGS Subscriber stopped", LogEventLevel.Information)
			});

	public override ISubscriberAction<EarmarkCompleteV1> EarmarkCompleteV1 =>
		new SubscriberActionWithLogs<EarmarkCompleteV1>(
			SubscriberActions.EarmarkCompleteV1,
			new List<LogEntry>
			{
				new("RTGS Subscriber started", LogEventLevel.Information),
				new("EarmarkComplete message received from RTGS", LogEventLevel.Information),
				new("RTGS Subscriber stopping", LogEventLevel.Information),
				new("RTGS Subscriber stopped", LogEventLevel.Information)
			});

	public override ISubscriberAction<EarmarkReleaseV1> EarmarkReleaseV1 =>
		new SubscriberActionWithLogs<EarmarkReleaseV1>(
			SubscriberActions.EarmarkReleaseV1,
			new List<LogEntry>
			{
				new("RTGS Subscriber started", LogEventLevel.Information),
				new("EarmarkRelease message received from RTGS", LogEventLevel.Information),
				new("RTGS Subscriber stopping", LogEventLevel.Information),
				new("RTGS Subscriber stopped", LogEventLevel.Information)
			});

	public override ISubscriberAction<BankPartnersResponseV1> BankPartnersResponseV1 =>
		new SubscriberActionWithLogs<BankPartnersResponseV1>(
			SubscriberActions.BankPartnersResponseV1,
			new List<LogEntry>
			{
				new("RTGS Subscriber started", LogEventLevel.Information),
				new("bank.partners.v1 message received from RTGS", LogEventLevel.Information),
				new("RTGS Subscriber stopping", LogEventLevel.Information),
				new("RTGS Subscriber stopped", LogEventLevel.Information)
			});

	public override ISubscriberAction<IdCryptInvitationConfirmationV1> IdCryptInvitationConfirmationV1 =>
		new SubscriberActionWithLogs<IdCryptInvitationConfirmationV1>(
			SubscriberActions.IdCryptInvitationConfirmationV1,
			new List<LogEntry>
			{
				new("RTGS Subscriber started", LogEventLevel.Information),
				new("idcrypt.invitationconfirmation.v1 message received from RTGS", LogEventLevel.Information),
				new("RTGS Subscriber stopping", LogEventLevel.Information),
				new("RTGS Subscriber stopped", LogEventLevel.Information)
			});
}
