namespace RTGS.DotNetSDK.IntegrationTests.Subscriber.TestData;

public class SubscriberActionSignedMessagesWithLogsData : BaseSignedSubscriberActionData
{
	public override ISubscriberAction<PayawayFundsV1> PayawayFundsV1 =>
		new SubscriberActionWithLogs<PayawayFundsV1>(
			SubscriberActions.PayawayFundsV1,
			StandardLogs<PayawayFundsV1>());

	public override ISubscriberAction<MessageRejectV1> MessageRejectV1 =>
		new SubscriberActionWithLogs<MessageRejectV1>(
			SubscriberActions.MessageRejectV1,
			StandardLogs<MessageRejectV1>());

	private static List<LogEntry> StandardLogs<T>() =>
		new()
		{
			new("RTGS Subscriber started", LogEventLevel.Information),
			new($"{typeof(T).Name} message received from RTGS", LogEventLevel.Information),
			new($"Verifying {typeof(T).Name} message", LogEventLevel.Information),
			new($"Verified {typeof(T).Name} message", LogEventLevel.Information),
			new("RTGS Subscriber stopping", LogEventLevel.Information),
			new("RTGS Subscriber stopped", LogEventLevel.Information)
		};
}
