namespace RTGS.DotNetSDK.IntegrationTests.Subscriber.TestData;

public class SubscriberActionWithLogsData : BaseSubscriberActionData
{
	public override ISubscriberAction<PayawayFundsV1> PayawayFundsV1 =>
		new SubscriberActionWithLogs<PayawayFundsV1>(
			SubscriberActions.PayawayFundsV1,
			new List<LogEntry>
			{
				new("RTGS Subscriber started", LogEventLevel.Information),
				new("PayawayFundsV1 message received from RTGS", LogEventLevel.Information),
				new("RTGS Subscriber stopping", LogEventLevel.Information),
				new("RTGS Subscriber stopped", LogEventLevel.Information)
			});

	public override ISubscriberAction<PayawayCompleteV1> PayawayCompleteV1 =>
		new SubscriberActionWithLogs<PayawayCompleteV1>(
			SubscriberActions.PayawayCompleteV1,
			new List<LogEntry>
			{
				new("RTGS Subscriber started", LogEventLevel.Information),
				new("PayawayCompleteV1 message received from RTGS", LogEventLevel.Information),
				new("RTGS Subscriber stopping", LogEventLevel.Information),
				new("RTGS Subscriber stopped", LogEventLevel.Information)
			});

	public override ISubscriberAction<MessageRejectV1> MessageRejectedV1 =>
		new SubscriberActionWithLogs<MessageRejectV1>(
			SubscriberActions.MessageRejectV1,
			new List<LogEntry>
			{
				new("RTGS Subscriber started", LogEventLevel.Information),
				new("MessageRejectV1 message received from RTGS", LogEventLevel.Information),
				new("RTGS Subscriber stopping", LogEventLevel.Information),
				new("RTGS Subscriber stopped", LogEventLevel.Information)
			});

	public override ISubscriberAction<AtomicLockResponseV1> AtomicLockResponseV1 =>
		new SubscriberActionWithLogs<AtomicLockResponseV1>(
			SubscriberActions.AtomicLockResponseV1,
			new List<LogEntry>
			{
				new("RTGS Subscriber started", LogEventLevel.Information),
				new("AtomicLockResponseV1 message received from RTGS", LogEventLevel.Information),
				new("RTGS Subscriber stopping", LogEventLevel.Information),
				new("RTGS Subscriber stopped", LogEventLevel.Information)
			});

	public override ISubscriberAction<AtomicTransferResponseV1> AtomicTransferResponseV1 =>
		new SubscriberActionWithLogs<AtomicTransferResponseV1>(
			SubscriberActions.AtomicTransferResponseV1,
			new List<LogEntry>
			{
				new("RTGS Subscriber started", LogEventLevel.Information),
				new("AtomicTransferResponseV1 message received from RTGS", LogEventLevel.Information),
				new("RTGS Subscriber stopping", LogEventLevel.Information),
				new("RTGS Subscriber stopped", LogEventLevel.Information)
			});

	public override ISubscriberAction<AtomicTransferFundsV1> AtomicTransferFundsV1 =>
		new SubscriberActionWithLogs<AtomicTransferFundsV1>(
			SubscriberActions.AtomicTransferFundsV1,
			new List<LogEntry>
			{
				new("RTGS Subscriber started", LogEventLevel.Information),
				new("AtomicTransferFundsV1 message received from RTGS", LogEventLevel.Information),
				new("RTGS Subscriber stopping", LogEventLevel.Information),
				new("RTGS Subscriber stopped", LogEventLevel.Information)
			});

	public override ISubscriberAction<EarmarkFundsV1> EarmarkFundsV1 =>
		new SubscriberActionWithLogs<EarmarkFundsV1>(
			SubscriberActions.EarmarkFundsV1,
			new List<LogEntry>
			{
				new("RTGS Subscriber started", LogEventLevel.Information),
				new("EarmarkFundsV1 message received from RTGS", LogEventLevel.Information),
				new("RTGS Subscriber stopping", LogEventLevel.Information),
				new("RTGS Subscriber stopped", LogEventLevel.Information)
			});

	public override ISubscriberAction<EarmarkCompleteV1> EarmarkCompleteV1 =>
		new SubscriberActionWithLogs<EarmarkCompleteV1>(
			SubscriberActions.EarmarkCompleteV1,
			new List<LogEntry>
			{
				new("RTGS Subscriber started", LogEventLevel.Information),
				new("EarmarkCompleteV1 message received from RTGS", LogEventLevel.Information),
				new("RTGS Subscriber stopping", LogEventLevel.Information),
				new("RTGS Subscriber stopped", LogEventLevel.Information)
			});

	public override ISubscriberAction<EarmarkReleaseV1> EarmarkReleaseV1 =>
		new SubscriberActionWithLogs<EarmarkReleaseV1>(
			SubscriberActions.EarmarkReleaseV1,
			new List<LogEntry>
			{
				new("RTGS Subscriber started", LogEventLevel.Information),
				new("EarmarkReleaseV1 message received from RTGS", LogEventLevel.Information),
				new("RTGS Subscriber stopping", LogEventLevel.Information),
				new("RTGS Subscriber stopped", LogEventLevel.Information)
			});

	public override ISubscriberAction<BankPartnersResponseV1> BankPartnersResponseV1 =>
		new SubscriberActionWithLogs<BankPartnersResponseV1>(
			SubscriberActions.BankPartnersResponseV1,
			new List<LogEntry>
			{
				new("RTGS Subscriber started", LogEventLevel.Information),
				new("BankPartnersResponseV1 message received from RTGS", LogEventLevel.Information),
				new("RTGS Subscriber stopping", LogEventLevel.Information),
				new("RTGS Subscriber stopped", LogEventLevel.Information)
			});
}
