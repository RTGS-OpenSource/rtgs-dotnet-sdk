using System.Collections.Generic;
using RTGS.DotNetSDK.Publisher.IntegrationTests.Logging;
using RTGS.DotNetSDK.Publisher.Messages;
using RTGS.ISO20022.Messages.Camt_054_001.V09;
using RTGS.ISO20022.Messages.Pacs_008_001.V10;

namespace RTGS.DotNetSDK.Publisher.IntegrationTests.TestData
{
	public class PublisherActionFailedAcknowledgementLogsData : BasePublisherActionData
	{
		public override IPublisherAction<AtomicLockRequest> AtomicLock =>
			new PublisherActionWithLogs<AtomicLockRequest>(
				PublisherActions.AtomicLock,
				new List<LogEntry>
				{
					new("Sending AtomicLockRequest to RTGS (SendAtomicLockRequestAsync)"),
					new("Sent AtomicLockRequest to RTGS (SendAtomicLockRequestAsync)"),
					new("Received AtomicLockRequest acknowledgement (success: False) from RTGS (SendAtomicLockRequestAsync)")
				});

		public override IPublisherAction<AtomicTransferRequest> AtomicTransfer =>
			new PublisherActionWithLogs<AtomicTransferRequest>(
				PublisherActions.AtomicTransfer,
				new List<LogEntry>
				{
					new("Sending AtomicTransferRequest to RTGS (SendAtomicTransferRequestAsync)"),
					new("Sent AtomicTransferRequest to RTGS (SendAtomicTransferRequestAsync)"),
					new("Received AtomicTransferRequest acknowledgement (success: False) from RTGS (SendAtomicTransferRequestAsync)")
				});

		public override IPublisherAction<EarmarkConfirmation> EarmarkConfirmation =>
			new PublisherActionWithLogs<EarmarkConfirmation>(
				PublisherActions.EarmarkConfirmation,
				new List<LogEntry>
				{
					new("Sending EarmarkConfirmation to RTGS (SendEarmarkConfirmationAsync)"),
					new("Sent EarmarkConfirmation to RTGS (SendEarmarkConfirmationAsync)"),
					new("Received EarmarkConfirmation acknowledgement (success: False) from RTGS (SendEarmarkConfirmationAsync)")
				});

		public override IPublisherAction<TransferConfirmation> TransferConfirmation =>
			new PublisherActionWithLogs<TransferConfirmation>(
				PublisherActions.TransferConfirmation,
				new List<LogEntry>
				{
					new("Sending TransferConfirmation to RTGS (SendTransferConfirmationAsync)"),
					new("Sent TransferConfirmation to RTGS (SendTransferConfirmationAsync)"),
					new("Received TransferConfirmation acknowledgement (success: False) from RTGS (SendTransferConfirmationAsync)")
				});

		public override IPublisherAction<UpdateLedgerRequest> UpdateLedger =>
			new PublisherActionWithLogs<UpdateLedgerRequest>(
				PublisherActions.UpdateLedger,
				new List<LogEntry>
				{
					new("Sending UpdateLedgerRequest to RTGS (SendUpdateLedgerRequestAsync)"),
					new("Sent UpdateLedgerRequest to RTGS (SendUpdateLedgerRequestAsync)"),
					new("Received UpdateLedgerRequest acknowledgement (success: False) from RTGS (SendUpdateLedgerRequestAsync)")
				});

		public override IPublisherAction<FIToFICustomerCreditTransferV10> PayawayCreate =>
			new PublisherActionWithLogs<FIToFICustomerCreditTransferV10>(
				PublisherActions.PayawayCreate,
				new List<LogEntry>
				{
					new("Sending FIToFICustomerCreditTransferV10 to RTGS (SendPayawayCreateAsync)"),
					new("Sent FIToFICustomerCreditTransferV10 to RTGS (SendPayawayCreateAsync)"),
					new("Received FIToFICustomerCreditTransferV10 acknowledgement (success: False) from RTGS (SendPayawayCreateAsync)")
				});

		public override IPublisherAction<BankToCustomerDebitCreditNotificationV09> PayawayConfirmation =>
			new PublisherActionWithLogs<BankToCustomerDebitCreditNotificationV09>(
				PublisherActions.PayawayConfirmation,
				new List<LogEntry>
					{
						new("Sending BankToCustomerDebitCreditNotificationV09 to RTGS (SendPayawayConfirmationAsync)"),
						new("Sent BankToCustomerDebitCreditNotificationV09 to RTGS (SendPayawayConfirmationAsync)"),
						new("Received BankToCustomerDebitCreditNotificationV09 acknowledgement (success: False) from RTGS (SendPayawayConfirmationAsync)")
					});
	}
}
