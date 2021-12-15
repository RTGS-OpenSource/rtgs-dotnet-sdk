using System.Collections.Generic;
using RTGS.DotNetSDK.Publisher.IntegrationTests.Logging;
using RTGS.DotNetSDK.Publisher.Messages;
using RTGS.ISO20022.Messages.Camt_054_001.V09;
using RTGS.ISO20022.Messages.Pacs_008_001.V10;
using Serilog.Events;

namespace RTGS.DotNetSDK.Publisher.IntegrationTests.TestData
{
	public class PublisherActionFailedAcknowledgementLogsData : BasePublisherActionData
	{
		public override IPublisherAction<AtomicLockRequestV1> AtomicLock =>
			new PublisherActionWithLogs<AtomicLockRequestV1>(
				PublisherActions.AtomicLock,
				new List<LogEntry>
				{
					new("Sending AtomicLockRequestV1 to RTGS (SendAtomicLockRequestAsync)", LogEventLevel.Information),
					new("Sent AtomicLockRequestV1 to RTGS (SendAtomicLockRequestAsync)", LogEventLevel.Information),
					new("Received AtomicLockRequestV1 acknowledgement (rejected) from RTGS (SendAtomicLockRequestAsync)", LogEventLevel.Error)
				});

		public override IPublisherAction<AtomicTransferRequestV1> AtomicTransfer =>
			new PublisherActionWithLogs<AtomicTransferRequestV1>(
				PublisherActions.AtomicTransfer,
				new List<LogEntry>
				{
					new("Sending AtomicTransferRequestV1 to RTGS (SendAtomicTransferRequestAsync)", LogEventLevel.Information),
					new("Sent AtomicTransferRequestV1 to RTGS (SendAtomicTransferRequestAsync)", LogEventLevel.Information),
					new("Received AtomicTransferRequestV1 acknowledgement (rejected) from RTGS (SendAtomicTransferRequestAsync)", LogEventLevel.Error)
				});

		public override IPublisherAction<EarmarkConfirmationV1> EarmarkConfirmation =>
			new PublisherActionWithLogs<EarmarkConfirmationV1>(
				PublisherActions.EarmarkConfirmation,
				new List<LogEntry>
				{
					new("Sending EarmarkConfirmationV1 to RTGS (SendEarmarkConfirmationAsync)", LogEventLevel.Information),
					new("Sent EarmarkConfirmationV1 to RTGS (SendEarmarkConfirmationAsync)", LogEventLevel.Information),
					new("Received EarmarkConfirmationV1 acknowledgement (rejected) from RTGS (SendEarmarkConfirmationAsync)", LogEventLevel.Error)
				});

		public override IPublisherAction<AtomicTransferConfirmationV1> AtomicTransferConfirmation =>
			new PublisherActionWithLogs<AtomicTransferConfirmationV1>(
				PublisherActions.AtomicTransferConfirmation,
				new List<LogEntry>
				{
					new("Sending AtomicTransferConfirmationV1 to RTGS (SendAtomicTransferConfirmationAsync)", LogEventLevel.Information),
					new("Sent AtomicTransferConfirmationV1 to RTGS (SendAtomicTransferConfirmationAsync)", LogEventLevel.Information),
					new("Received AtomicTransferConfirmationV1 acknowledgement (rejected) from RTGS (SendAtomicTransferConfirmationAsync)", LogEventLevel.Error)
				});

		public override IPublisherAction<UpdateLedgerRequestV1> UpdateLedger =>
			new PublisherActionWithLogs<UpdateLedgerRequestV1>(
				PublisherActions.UpdateLedger,
				new List<LogEntry>
				{
					new("Sending UpdateLedgerRequestV1 to RTGS (SendUpdateLedgerRequestAsync)", LogEventLevel.Information),
					new("Sent UpdateLedgerRequestV1 to RTGS (SendUpdateLedgerRequestAsync)", LogEventLevel.Information),
					new("Received UpdateLedgerRequestV1 acknowledgement (rejected) from RTGS (SendUpdateLedgerRequestAsync)", LogEventLevel.Error)
				});

		public override IPublisherAction<FIToFICustomerCreditTransferV10> PayawayCreate =>
			new PublisherActionWithLogs<FIToFICustomerCreditTransferV10>(
				PublisherActions.PayawayCreate,
				new List<LogEntry>
				{
					new("Sending FIToFICustomerCreditTransferV10 to RTGS (SendPayawayCreateAsync)", LogEventLevel.Information),
					new("Sent FIToFICustomerCreditTransferV10 to RTGS (SendPayawayCreateAsync)", LogEventLevel.Information),
					new("Received FIToFICustomerCreditTransferV10 acknowledgement (rejected) from RTGS (SendPayawayCreateAsync)", LogEventLevel.Error)
				});

		public override IPublisherAction<BankToCustomerDebitCreditNotificationV09> PayawayConfirmation =>
			new PublisherActionWithLogs<BankToCustomerDebitCreditNotificationV09>(
				PublisherActions.PayawayConfirmation,
				new List<LogEntry>
					{
						new("Sending BankToCustomerDebitCreditNotificationV09 to RTGS (SendPayawayConfirmationAsync)", LogEventLevel.Information),
						new("Sent BankToCustomerDebitCreditNotificationV09 to RTGS (SendPayawayConfirmationAsync)", LogEventLevel.Information),
						new("Received BankToCustomerDebitCreditNotificationV09 acknowledgement (rejected) from RTGS (SendPayawayConfirmationAsync)", LogEventLevel.Error)
					});
	}
}
