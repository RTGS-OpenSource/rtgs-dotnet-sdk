namespace RTGS.DotNetSDK.IntegrationTests.Publisher.TestData;

public class PublisherActionSuccessAcknowledgementLogsData : BasePublisherActionData
{
	public override IPublisherAction<AtomicLockRequestV1> AtomicLock =>
		new PublisherActionWithLogs<AtomicLockRequestV1>(
			PublisherActions.AtomicLock,
			new List<LogEntry>
			{
				new("Signing AtomicLockRequestV1 message", LogEventLevel.Information),
				new("Signed AtomicLockRequestV1 message", LogEventLevel.Information),
				new("Sending AtomicLockRequestV1 to RTGS (SendAtomicLockRequestAsync)", LogEventLevel.Information),
				new("Sent AtomicLockRequestV1 to RTGS (SendAtomicLockRequestAsync)", LogEventLevel.Information),
				new("Received AtomicLockRequestV1 acknowledgement (acknowledged) from RTGS (SendAtomicLockRequestAsync)", LogEventLevel.Information)
			});

	public override IPublisherAction<AtomicTransferRequestV1> AtomicTransfer =>
		new PublisherActionWithLogs<AtomicTransferRequestV1>(
			PublisherActions.AtomicTransfer,
			new List<LogEntry>
			{
				new("Signing AtomicTransferRequestV1 message", LogEventLevel.Information),
				new("Signed AtomicTransferRequestV1 message", LogEventLevel.Information),
				new("Sending AtomicTransferRequestV1 to RTGS (SendAtomicTransferRequestAsync)", LogEventLevel.Information),
				new("Sent AtomicTransferRequestV1 to RTGS (SendAtomicTransferRequestAsync)", LogEventLevel.Information),
				new("Received AtomicTransferRequestV1 acknowledgement (acknowledged) from RTGS (SendAtomicTransferRequestAsync)", LogEventLevel.Information)
			});

	public override IPublisherAction<EarmarkConfirmationV1> EarmarkConfirmation =>
		new PublisherActionWithLogs<EarmarkConfirmationV1>(
			PublisherActions.EarmarkConfirmation,
			new List<LogEntry>
			{
				new("Sending EarmarkConfirmationV1 to RTGS (SendEarmarkConfirmationAsync)", LogEventLevel.Information),
				new("Sent EarmarkConfirmationV1 to RTGS (SendEarmarkConfirmationAsync)", LogEventLevel.Information),
				new("Received EarmarkConfirmationV1 acknowledgement (acknowledged) from RTGS (SendEarmarkConfirmationAsync)", LogEventLevel.Information)
			});

	public override IPublisherAction<AtomicTransferConfirmationV1> AtomicTransferConfirmation =>
		new PublisherActionWithLogs<AtomicTransferConfirmationV1>(
			PublisherActions.AtomicTransferConfirmation,
			new List<LogEntry>
			{
				new("Sending AtomicTransferConfirmationV1 to RTGS (SendAtomicTransferConfirmationAsync)", LogEventLevel.Information),
				new("Sent AtomicTransferConfirmationV1 to RTGS (SendAtomicTransferConfirmationAsync)", LogEventLevel.Information),
				new("Received AtomicTransferConfirmationV1 acknowledgement (acknowledged) from RTGS (SendAtomicTransferConfirmationAsync)", LogEventLevel.Information)
			});

	public override IPublisherAction<UpdateLedgerRequestV1> UpdateLedger =>
		new PublisherActionWithLogs<UpdateLedgerRequestV1>(
			PublisherActions.UpdateLedger,
			new List<LogEntry>
			{
				new("Sending UpdateLedgerRequestV1 to RTGS (SendUpdateLedgerRequestAsync)", LogEventLevel.Information),
				new("Sent UpdateLedgerRequestV1 to RTGS (SendUpdateLedgerRequestAsync)", LogEventLevel.Information),
				new("Received UpdateLedgerRequestV1 acknowledgement (acknowledged) from RTGS (SendUpdateLedgerRequestAsync)", LogEventLevel.Information)
			});

	public override IPublisherAction<PayawayCreationV1> PayawayCreate =>
		new PublisherActionWithLogs<PayawayCreationV1>(
			PublisherActions.PayawayCreate,
			new List<LogEntry>
			{
				new("Signing PayawayCreationV1 message", LogEventLevel.Information),
				new("Signed PayawayCreationV1 message", LogEventLevel.Information),
				new("Sending PayawayCreationV1 to RTGS (SendPayawayCreateAsync)", LogEventLevel.Information),
				new("Sent PayawayCreationV1 to RTGS (SendPayawayCreateAsync)", LogEventLevel.Information),
				new("Received PayawayCreationV1 acknowledgement (acknowledged) from RTGS (SendPayawayCreateAsync)", LogEventLevel.Information)
			});

	public override IPublisherAction<PayawayConfirmationV1> PayawayConfirmation =>
		new PublisherActionWithLogs<PayawayConfirmationV1>(
			PublisherActions.PayawayConfirmation,
			new List<LogEntry>
			{
				new("Signing PayawayConfirmationV1 message", LogEventLevel.Information),
				new("Signed PayawayConfirmationV1 message", LogEventLevel.Information),
				new("Sending PayawayConfirmationV1 to RTGS (SendPayawayConfirmationAsync)", LogEventLevel.Information),
				new("Sent PayawayConfirmationV1 to RTGS (SendPayawayConfirmationAsync)", LogEventLevel.Information),
				new("Received PayawayConfirmationV1 acknowledgement (acknowledged) from RTGS (SendPayawayConfirmationAsync)", LogEventLevel.Information)
			});

	public override IPublisherAction<PayawayRejectionV1> PayawayRejection =>
		new PublisherActionWithLogs<PayawayRejectionV1>(
			PublisherActions.PayawayRejection,
			new List<LogEntry>
			{
				new("Signing PayawayRejectionV1 message", LogEventLevel.Information),
				new("Signed PayawayRejectionV1 message", LogEventLevel.Information),
				new("Sending PayawayRejectionV1 to RTGS (SendPayawayRejectionAsync)", LogEventLevel.Information),
				new("Sent PayawayRejectionV1 to RTGS (SendPayawayRejectionAsync)", LogEventLevel.Information),
				new("Received PayawayRejectionV1 acknowledgement (acknowledged) from RTGS (SendPayawayRejectionAsync)", LogEventLevel.Information)
			});

	public override IPublisherAction<BankPartnersRequestV1> BankPartnersRequest =>
		new PublisherActionWithLogs<BankPartnersRequestV1>(
			PublisherActions.BankPartnersRequest,
			new List<LogEntry>
			{
				new("Sending BankPartnersRequestV1 to RTGS (SendBankPartnersRequestAsync)", LogEventLevel.Information),
				new("Sent BankPartnersRequestV1 to RTGS (SendBankPartnersRequestAsync)", LogEventLevel.Information),
				new("Received BankPartnersRequestV1 acknowledgement (acknowledged) from RTGS (SendBankPartnersRequestAsync)", LogEventLevel.Information)
			});

	public override IPublisherAction<AtomicLockRequestV2> AtomicLockV2 =>
		new PublisherActionWithLogs<AtomicLockRequestV2>(
			PublisherActions.AtomicLockV2,
			new List<LogEntry>
			{
				new("Signing AtomicLockRequestV2 message", LogEventLevel.Information),
				new("Signed AtomicLockRequestV2 message", LogEventLevel.Information),
				new("Sending AtomicLockRequestV2 to RTGS (SendAtomicLockRequestAsync)", LogEventLevel.Information),
				new("Sent AtomicLockRequestV2 to RTGS (SendAtomicLockRequestAsync)", LogEventLevel.Information),
				new("Received AtomicLockRequestV2 acknowledgement (acknowledged) from RTGS (SendAtomicLockRequestAsync)", LogEventLevel.Information)
			});
}
