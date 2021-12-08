using System.Collections.Generic;
using RTGS.DotNetSDK.Publisher.IntegrationTests.Logging;
using RTGS.DotNetSDK.Publisher.Messages;
using RTGS.ISO20022.Messages.Camt_054_001.V09;
using RTGS.ISO20022.Messages.Pacs_008_001.V10;
using Serilog.Events;

namespace RTGS.DotNetSDK.Publisher.IntegrationTests.TestData
{
	public class PublisherActionTimeoutAcknowledgementLogsData : BasePublisherActionData
	{
		public override IPublisherAction<AtomicLockRequest> AtomicLock =>
			new PublisherActionWithLogs<AtomicLockRequest>(
				PublisherActions.AtomicLock,
				new List<LogEntry>
				{
					new("Timed out waiting for AtomicLockRequest acknowledgement from RTGS (SendAtomicLockRequestAsync)", LogEventLevel.Error)
				});

		public override IPublisherAction<AtomicTransferRequest> AtomicTransfer =>
			new PublisherActionWithLogs<AtomicTransferRequest>(
				PublisherActions.AtomicTransfer,
				new List<LogEntry>
				{
					new("Timed out waiting for AtomicTransferRequest acknowledgement from RTGS (SendAtomicTransferRequestAsync)", LogEventLevel.Error)
				});

		public override IPublisherAction<EarmarkConfirmation> EarmarkConfirmation =>
			new PublisherActionWithLogs<EarmarkConfirmation>(
				PublisherActions.EarmarkConfirmation,
				new List<LogEntry>
				{
					new("Timed out waiting for EarmarkConfirmation acknowledgement from RTGS (SendEarmarkConfirmationAsync)", LogEventLevel.Error)
				});

		public override IPublisherAction<AtomicTransferConfirmation> AtomicTransferConfirmation =>
			new PublisherActionWithLogs<AtomicTransferConfirmation>(
				PublisherActions.AtomicTransferConfirmation,
				new List<LogEntry>
				{
					new("Timed out waiting for AtomicTransferConfirmation acknowledgement from RTGS (SendAtomicTransferConfirmationAsync)", LogEventLevel.Error)
				});

		public override IPublisherAction<UpdateLedgerRequest> UpdateLedger =>
			new PublisherActionWithLogs<UpdateLedgerRequest>(
				PublisherActions.UpdateLedger,
				new List<LogEntry>
				{
					new("Timed out waiting for UpdateLedgerRequest acknowledgement from RTGS (SendUpdateLedgerRequestAsync)", LogEventLevel.Error)
				});

		public override IPublisherAction<FIToFICustomerCreditTransferV10> PayawayCreate =>
			new PublisherActionWithLogs<FIToFICustomerCreditTransferV10>(
				PublisherActions.PayawayCreate,
				new List<LogEntry>
				{
					new("Timed out waiting for FIToFICustomerCreditTransferV10 acknowledgement from RTGS (SendPayawayCreateAsync)", LogEventLevel.Error)
				});

		public override IPublisherAction<BankToCustomerDebitCreditNotificationV09> PayawayConfirmation =>
			new PublisherActionWithLogs<BankToCustomerDebitCreditNotificationV09>(
				PublisherActions.PayawayConfirmation,
				new List<LogEntry>
					{
						new("Timed out waiting for BankToCustomerDebitCreditNotificationV09 acknowledgement from RTGS (SendPayawayConfirmationAsync)", LogEventLevel.Error)
					});
	}
}
