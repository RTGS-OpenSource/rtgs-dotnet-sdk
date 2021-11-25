using System.Collections.Generic;
using RTGS.DotNetSDK.Publisher.IntegrationTests.Logging;
using RTGS.DotNetSDK.Publisher.Messages;
using RTGS.ISO20022.Messages.Camt_054_001.V09;
using RTGS.ISO20022.Messages.Pacs_008_001.V10;
using Serilog.Events;

namespace RTGS.DotNetSDK.Publisher.IntegrationTests.TestData
{
	public class PublisherActionRpcExceptionLogsData : BasePublisherActionData
	{
		public override IPublisherAction<AtomicLockRequest> AtomicLock =>
			new PublisherActionWithLogs<AtomicLockRequest>(
				PublisherActions.AtomicLock,
				new List<LogEntry>
				{
					new("Sending AtomicLockRequest to RTGS (SendAtomicLockRequestAsync)", LogEventLevel.Information),
					new("Sent AtomicLockRequest to RTGS (SendAtomicLockRequestAsync)", LogEventLevel.Information),
					new("Error received when sending AtomicLockRequest to RTGS (SendAtomicLockRequestAsync)", LogEventLevel.Error)
				});

		public override IPublisherAction<AtomicTransferRequest> AtomicTransfer =>
			new PublisherActionWithLogs<AtomicTransferRequest>(
				PublisherActions.AtomicTransfer,
				new List<LogEntry>
				{
					new("Sending AtomicTransferRequest to RTGS (SendAtomicTransferRequestAsync)", LogEventLevel.Information),
					new("Sent AtomicTransferRequest to RTGS (SendAtomicTransferRequestAsync)", LogEventLevel.Information),
					new("Error received when sending AtomicTransferRequest to RTGS (SendAtomicTransferRequestAsync)", LogEventLevel.Error)
				});

		public override IPublisherAction<EarmarkConfirmation> EarmarkConfirmation =>
			new PublisherActionWithLogs<EarmarkConfirmation>(
				PublisherActions.EarmarkConfirmation,
				new List<LogEntry>
				{
					new("Sending EarmarkConfirmation to RTGS (SendEarmarkConfirmationAsync)", LogEventLevel.Information),
					new("Sent EarmarkConfirmation to RTGS (SendEarmarkConfirmationAsync)", LogEventLevel.Information),
					new("Error received when sending EarmarkConfirmation to RTGS (SendEarmarkConfirmationAsync)", LogEventLevel.Error)
				});

		public override IPublisherAction<TransferConfirmation> TransferConfirmation =>
			new PublisherActionWithLogs<TransferConfirmation>(
				PublisherActions.TransferConfirmation,
				new List<LogEntry>
				{
					new("Sending TransferConfirmation to RTGS (SendTransferConfirmationAsync)", LogEventLevel.Information),
					new("Sent TransferConfirmation to RTGS (SendTransferConfirmationAsync)", LogEventLevel.Information),
					new("Error received when sending TransferConfirmation to RTGS (SendTransferConfirmationAsync)", LogEventLevel.Error)
				});

		public override IPublisherAction<UpdateLedgerRequest> UpdateLedger =>
			new PublisherActionWithLogs<UpdateLedgerRequest>(
				PublisherActions.UpdateLedger,
				new List<LogEntry>
				{
					new("Sending UpdateLedgerRequest to RTGS (SendUpdateLedgerRequestAsync)", LogEventLevel.Information),
					new("Sent UpdateLedgerRequest to RTGS (SendUpdateLedgerRequestAsync)", LogEventLevel.Information),
					new("Error received when sending UpdateLedgerRequest to RTGS (SendUpdateLedgerRequestAsync)", LogEventLevel.Error)
				});

		public override IPublisherAction<FIToFICustomerCreditTransferV10> PayawayCreate =>
			new PublisherActionWithLogs<FIToFICustomerCreditTransferV10>(
				PublisherActions.PayawayCreate,
				new List<LogEntry>
				{
					new("Sending FIToFICustomerCreditTransferV10 to RTGS (SendPayawayCreateAsync)", LogEventLevel.Information),
					new("Sent FIToFICustomerCreditTransferV10 to RTGS (SendPayawayCreateAsync)", LogEventLevel.Information),
					new("Error received when sending FIToFICustomerCreditTransferV10 to RTGS (SendPayawayCreateAsync)", LogEventLevel.Error)
				});

		public override IPublisherAction<BankToCustomerDebitCreditNotificationV09> PayawayConfirmation =>
			new PublisherActionWithLogs<BankToCustomerDebitCreditNotificationV09>(
				PublisherActions.PayawayConfirmation,
				new List<LogEntry>
					{
						new("Sending BankToCustomerDebitCreditNotificationV09 to RTGS (SendPayawayConfirmationAsync)", LogEventLevel.Information),
						new("Sent BankToCustomerDebitCreditNotificationV09 to RTGS (SendPayawayConfirmationAsync)", LogEventLevel.Information),
						new("Error received when sending BankToCustomerDebitCreditNotificationV09 to RTGS (SendPayawayConfirmationAsync)", LogEventLevel.Error)
					});
	}
}
