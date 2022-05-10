namespace RTGS.DotNetSDK.IntegrationTests.Subscriber.TestData;

public class SubscriberActionSignedMessagesWithLogsData : BaseSignedSubscriberActionData
{
	public override ISubscriberAction<PayawayFundsV1> PayawayFundsV1 =>
		new SubscriberActionWithLogs<PayawayFundsV1>(
			SubscriberActions.PayawayFundsV1,
			new List<LogEntry>
			{
				new("RTGS Subscriber started", LogEventLevel.Information),
				new("PayawayFundsV1 message received from RTGS", LogEventLevel.Information),
				new("Verifying PayawayFundsV1 message", LogEventLevel.Information),
				new("Verified PayawayFundsV1 message", LogEventLevel.Information),
				new("RTGS Subscriber stopping", LogEventLevel.Information),
				new("RTGS Subscriber stopped", LogEventLevel.Information)
			});
}
