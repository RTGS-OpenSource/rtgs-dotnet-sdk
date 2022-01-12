using RTGS.ISO20022.Messages.Camt_054_001.V09;
using RTGS.ISO20022.Messages.Pacs_008_001.V10;

namespace RTGS.DotNetSDK.Publisher.IntegrationTests.TestData;

public class PublisherActionTimeoutAcknowledgementLogsData : BasePublisherActionData
{
	public override IPublisherAction<AtomicLockRequestV1> AtomicLock =>
		new PublisherActionWithLogs<AtomicLockRequestV1>(
			PublisherActions.AtomicLock,
			new List<LogEntry>
			{
				new("Timed out waiting for AtomicLockRequestV1 acknowledgement from RTGS (SendAtomicLockRequestAsync)", LogEventLevel.Error)
			});

	public override IPublisherAction<AtomicTransferRequestV1> AtomicTransfer =>
		new PublisherActionWithLogs<AtomicTransferRequestV1>(
			PublisherActions.AtomicTransfer,
			new List<LogEntry>
			{
				new("Timed out waiting for AtomicTransferRequestV1 acknowledgement from RTGS (SendAtomicTransferRequestAsync)", LogEventLevel.Error)
			});

	public override IPublisherAction<EarmarkConfirmationV1> EarmarkConfirmation =>
		new PublisherActionWithLogs<EarmarkConfirmationV1>(
			PublisherActions.EarmarkConfirmation,
			new List<LogEntry>
			{
				new("Timed out waiting for EarmarkConfirmationV1 acknowledgement from RTGS (SendEarmarkConfirmationAsync)", LogEventLevel.Error)
			});

	public override IPublisherAction<AtomicTransferConfirmationV1> AtomicTransferConfirmation =>
		new PublisherActionWithLogs<AtomicTransferConfirmationV1>(
			PublisherActions.AtomicTransferConfirmation,
			new List<LogEntry>
			{
				new("Timed out waiting for AtomicTransferConfirmationV1 acknowledgement from RTGS (SendAtomicTransferConfirmationAsync)", LogEventLevel.Error)
			});

	public override IPublisherAction<UpdateLedgerRequestV1> UpdateLedger =>
		new PublisherActionWithLogs<UpdateLedgerRequestV1>(
			PublisherActions.UpdateLedger,
			new List<LogEntry>
			{
				new("Timed out waiting for UpdateLedgerRequestV1 acknowledgement from RTGS (SendUpdateLedgerRequestAsync)", LogEventLevel.Error)
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

	public override IPublisherAction<BankPartnersRequestV1> BankPartnersRequest =>
		new PublisherActionWithLogs<BankPartnersRequestV1>(
			PublisherActions.BankPartnersRequest,
			new List<LogEntry>
			{
					new("Timed out waiting for BankPartnersRequestV1 acknowledgement from RTGS (SendBankPartnersRequestAsync)", LogEventLevel.Error)
			});
}
