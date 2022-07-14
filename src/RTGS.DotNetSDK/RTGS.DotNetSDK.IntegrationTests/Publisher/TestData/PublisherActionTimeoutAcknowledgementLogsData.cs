namespace RTGS.DotNetSDK.IntegrationTests.Publisher.TestData;

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

	public override IPublisherAction<PayawayCreationV1> PayawayCreate =>
		new PublisherActionWithLogs<PayawayCreationV1>(
			PublisherActions.PayawayCreate,
			new List<LogEntry>
			{
				new("Timed out waiting for PayawayCreationV1 acknowledgement from RTGS (SendPayawayCreateAsync)", LogEventLevel.Error)
			});

	public override IPublisherAction<PayawayConfirmationV1> PayawayConfirmation =>
		new PublisherActionWithLogs<PayawayConfirmationV1>(
			PublisherActions.PayawayConfirmation,
			new List<LogEntry>
			{
				new("Timed out waiting for PayawayConfirmationV1 acknowledgement from RTGS (SendPayawayConfirmationAsync)", LogEventLevel.Error)
			});

	public override IPublisherAction<PayawayRejectionV1> PayawayRejection =>
		new PublisherActionWithLogs<PayawayRejectionV1>(
			PublisherActions.PayawayRejection,
			new List<LogEntry>
			{
				new("Timed out waiting for PayawayRejectionV1 acknowledgement from RTGS (SendPayawayRejectionAsync)", LogEventLevel.Error)
			});

	public override IPublisherAction<BankPartnersRequestV1> BankPartnersRequest =>
		new PublisherActionWithLogs<BankPartnersRequestV1>(
			PublisherActions.BankPartnersRequest,
			new List<LogEntry>
			{
					new("Timed out waiting for BankPartnersRequestV1 acknowledgement from RTGS (SendBankPartnersRequestAsync)", LogEventLevel.Error)
			});

	public override IPublisherAction<AtomicLockRequestV2> AtomicLockV2IBAN =>
		new PublisherActionWithLogs<AtomicLockRequestV2>(
			PublisherActions.AtomicLockV2IBAN,
			new List<LogEntry>
			{
				new("Timed out waiting for AtomicLockRequestV2 acknowledgement from RTGS (SendAtomicLockRequestAsync)", LogEventLevel.Error)
			});

	public override IPublisherAction<AtomicLockRequestV2> AtomicLockV2OtherId =>
		new PublisherActionWithLogs<AtomicLockRequestV2>(
			PublisherActions.AtomicLockV2OtherId,
			new List<LogEntry>
			{
				new("Timed out waiting for AtomicLockRequestV2 acknowledgement from RTGS (SendAtomicLockRequestAsync)", LogEventLevel.Error)
			});
}
