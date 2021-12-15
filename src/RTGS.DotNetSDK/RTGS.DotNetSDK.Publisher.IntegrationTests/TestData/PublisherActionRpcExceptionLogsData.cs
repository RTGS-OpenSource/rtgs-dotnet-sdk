using RTGS.ISO20022.Messages.Camt_054_001.V09;
using RTGS.ISO20022.Messages.Pacs_008_001.V10;

namespace RTGS.DotNetSDK.Publisher.IntegrationTests.TestData;

public class PublisherActionRpcExceptionLogsData : BasePublisherActionData
{
	public override IPublisherAction<AtomicLockRequestV1> AtomicLock =>
		new PublisherActionWithLogs<AtomicLockRequestV1>(
			PublisherActions.AtomicLock,
			new List<LogEntry>
			{
				new("Sending AtomicLockRequestV1 to RTGS (SendAtomicLockRequestAsync)", LogEventLevel.Information),
				new("Sent AtomicLockRequestV1 to RTGS (SendAtomicLockRequestAsync)", LogEventLevel.Information),
				new("Error received when sending AtomicLockRequestV1 to RTGS (SendAtomicLockRequestAsync)", LogEventLevel.Error, typeof(RpcException))
			});

	public override IPublisherAction<AtomicTransferRequestV1> AtomicTransfer =>
		new PublisherActionWithLogs<AtomicTransferRequestV1>(
			PublisherActions.AtomicTransfer,
			new List<LogEntry>
			{
				new("Sending AtomicTransferRequestV1 to RTGS (SendAtomicTransferRequestAsync)", LogEventLevel.Information),
				new("Sent AtomicTransferRequestV1 to RTGS (SendAtomicTransferRequestAsync)", LogEventLevel.Information),
				new("Error received when sending AtomicTransferRequestV1 to RTGS (SendAtomicTransferRequestAsync)", LogEventLevel.Error, typeof(RpcException))
			});

	public override IPublisherAction<EarmarkConfirmationV1> EarmarkConfirmation =>
		new PublisherActionWithLogs<EarmarkConfirmationV1>(
			PublisherActions.EarmarkConfirmation,
			new List<LogEntry>
			{
				new("Sending EarmarkConfirmationV1 to RTGS (SendEarmarkConfirmationAsync)", LogEventLevel.Information),
				new("Sent EarmarkConfirmationV1 to RTGS (SendEarmarkConfirmationAsync)", LogEventLevel.Information),
				new("Error received when sending EarmarkConfirmationV1 to RTGS (SendEarmarkConfirmationAsync)", LogEventLevel.Error, typeof(RpcException))
			});

	public override IPublisherAction<AtomicTransferConfirmationV1> AtomicTransferConfirmation =>
		new PublisherActionWithLogs<AtomicTransferConfirmationV1>(
			PublisherActions.AtomicTransferConfirmation,
			new List<LogEntry>
			{
				new("Sending AtomicTransferConfirmationV1 to RTGS (SendAtomicTransferConfirmationAsync)", LogEventLevel.Information),
				new("Sent AtomicTransferConfirmationV1 to RTGS (SendAtomicTransferConfirmationAsync)", LogEventLevel.Information),
				new("Error received when sending AtomicTransferConfirmationV1 to RTGS (SendAtomicTransferConfirmationAsync)", LogEventLevel.Error, typeof(RpcException))
			});

	public override IPublisherAction<UpdateLedgerRequestV1> UpdateLedger =>
		new PublisherActionWithLogs<UpdateLedgerRequestV1>(
			PublisherActions.UpdateLedger,
			new List<LogEntry>
			{
				new("Sending UpdateLedgerRequestV1 to RTGS (SendUpdateLedgerRequestAsync)", LogEventLevel.Information),
				new("Sent UpdateLedgerRequestV1 to RTGS (SendUpdateLedgerRequestAsync)", LogEventLevel.Information),
				new("Error received when sending UpdateLedgerRequestV1 to RTGS (SendUpdateLedgerRequestAsync)", LogEventLevel.Error, typeof(RpcException))
			});

	public override IPublisherAction<FIToFICustomerCreditTransferV10> PayawayCreate =>
		new PublisherActionWithLogs<FIToFICustomerCreditTransferV10>(
			PublisherActions.PayawayCreate,
			new List<LogEntry>
			{
				new("Sending FIToFICustomerCreditTransferV10 to RTGS (SendPayawayCreateAsync)", LogEventLevel.Information),
				new("Sent FIToFICustomerCreditTransferV10 to RTGS (SendPayawayCreateAsync)", LogEventLevel.Information),
				new("Error received when sending FIToFICustomerCreditTransferV10 to RTGS (SendPayawayCreateAsync)", LogEventLevel.Error, typeof(RpcException))
			});

	public override IPublisherAction<BankToCustomerDebitCreditNotificationV09> PayawayConfirmation =>
		new PublisherActionWithLogs<BankToCustomerDebitCreditNotificationV09>(
			PublisherActions.PayawayConfirmation,
			new List<LogEntry>
			{
				new("Sending BankToCustomerDebitCreditNotificationV09 to RTGS (SendPayawayConfirmationAsync)", LogEventLevel.Information),
				new("Sent BankToCustomerDebitCreditNotificationV09 to RTGS (SendPayawayConfirmationAsync)", LogEventLevel.Information),
				new("Error received when sending BankToCustomerDebitCreditNotificationV09 to RTGS (SendPayawayConfirmationAsync)", LogEventLevel.Error, typeof(RpcException))
			});
}
