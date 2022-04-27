using RTGS.ISO20022.Messages.Pacs_008_001.V10;

namespace RTGS.DotNetSDK.IntegrationTests.Subscriber.TestData;

public class SubscriberActionSignedMessagesWithLogsData : BaseSignedSubscriberActionData
{
	public override ISubscriberAction<FIToFICustomerCreditTransferV10> PayawayFundsV1 =>
		new SubscriberActionWithLogs<FIToFICustomerCreditTransferV10>(
			SubscriberActions.PayawayFundsV1,
			new List<LogEntry>
			{
				new("RTGS Subscriber started", LogEventLevel.Information),
				new("PayawayFunds message received from RTGS", LogEventLevel.Information),
				new("Verifying PayawayFunds message", LogEventLevel.Information),
				new("Verified PayawayFunds message", LogEventLevel.Information),
				new("RTGS Subscriber stopping", LogEventLevel.Information),
				new("RTGS Subscriber stopped", LogEventLevel.Information)
			});
}
