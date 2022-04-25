using RTGS.ISO20022.Messages.Admi_002_001.V01;
using RTGS.ISO20022.Messages.Camt_054_001.V09;
using RTGS.ISO20022.Messages.Pacs_008_001.V10;

namespace RTGS.DotNetSDK.IntegrationTests.Publisher.TestData;

public class PublisherActionSuccessAcknowledgementLogsData : BasePublisherActionData
{
	public override IPublisherAction<AtomicLockRequestV1> AtomicLock =>
		new PublisherActionWithLogs<AtomicLockRequestV1>(
			PublisherActions.AtomicLock,
			new List<LogEntry>
			{
				new("No message signer found for AtomicLockRequestV1 message, skipping signing", LogEventLevel.Debug),
				new("Sending AtomicLockRequestV1 to RTGS (SendAtomicLockRequestAsync)", LogEventLevel.Information),
				new("Sent AtomicLockRequestV1 to RTGS (SendAtomicLockRequestAsync)", LogEventLevel.Information),
				new("Received AtomicLockRequestV1 acknowledgement (acknowledged) from RTGS (SendAtomicLockRequestAsync)", LogEventLevel.Information)
			});

	public override IPublisherAction<AtomicTransferRequestV1> AtomicTransfer =>
		new PublisherActionWithLogs<AtomicTransferRequestV1>(
			PublisherActions.AtomicTransfer,
			new List<LogEntry>
			{
				new("No message signer found for AtomicTransferRequestV1 message, skipping signing", LogEventLevel.Debug),
				new("Sending AtomicTransferRequestV1 to RTGS (SendAtomicTransferRequestAsync)", LogEventLevel.Information),
				new("Sent AtomicTransferRequestV1 to RTGS (SendAtomicTransferRequestAsync)", LogEventLevel.Information),
				new("Received AtomicTransferRequestV1 acknowledgement (acknowledged) from RTGS (SendAtomicTransferRequestAsync)", LogEventLevel.Information)
			});

	public override IPublisherAction<EarmarkConfirmationV1> EarmarkConfirmation =>
		new PublisherActionWithLogs<EarmarkConfirmationV1>(
			PublisherActions.EarmarkConfirmation,
			new List<LogEntry>
			{
				new("No message signer found for EarmarkConfirmationV1 message, skipping signing", LogEventLevel.Debug),
				new("Sending EarmarkConfirmationV1 to RTGS (SendEarmarkConfirmationAsync)", LogEventLevel.Information),
				new("Sent EarmarkConfirmationV1 to RTGS (SendEarmarkConfirmationAsync)", LogEventLevel.Information),
				new("Received EarmarkConfirmationV1 acknowledgement (acknowledged) from RTGS (SendEarmarkConfirmationAsync)", LogEventLevel.Information)
			});

	public override IPublisherAction<AtomicTransferConfirmationV1> AtomicTransferConfirmation =>
		new PublisherActionWithLogs<AtomicTransferConfirmationV1>(
			PublisherActions.AtomicTransferConfirmation,
			new List<LogEntry>
			{
				new("No message signer found for AtomicTransferConfirmationV1 message, skipping signing", LogEventLevel.Debug),
				new("Sending AtomicTransferConfirmationV1 to RTGS (SendAtomicTransferConfirmationAsync)", LogEventLevel.Information),
				new("Sent AtomicTransferConfirmationV1 to RTGS (SendAtomicTransferConfirmationAsync)", LogEventLevel.Information),
				new("Received AtomicTransferConfirmationV1 acknowledgement (acknowledged) from RTGS (SendAtomicTransferConfirmationAsync)", LogEventLevel.Information)
			});

	public override IPublisherAction<UpdateLedgerRequestV1> UpdateLedger =>
		new PublisherActionWithLogs<UpdateLedgerRequestV1>(
			PublisherActions.UpdateLedger,
			new List<LogEntry>
			{
				new("No message signer found for UpdateLedgerRequestV1 message, skipping signing", LogEventLevel.Debug),
				new("Sending UpdateLedgerRequestV1 to RTGS (SendUpdateLedgerRequestAsync)", LogEventLevel.Information),
				new("Sent UpdateLedgerRequestV1 to RTGS (SendUpdateLedgerRequestAsync)", LogEventLevel.Information),
				new("Received UpdateLedgerRequestV1 acknowledgement (acknowledged) from RTGS (SendUpdateLedgerRequestAsync)", LogEventLevel.Information)
			});

	public override IPublisherAction<FIToFICustomerCreditTransferV10> PayawayCreate =>
		new PublisherActionWithLogs<FIToFICustomerCreditTransferV10>(
			PublisherActions.PayawayCreate,
			new List<LogEntry>
			{
				new("Signing FIToFICustomerCreditTransferV10 message", LogEventLevel.Information),
				new("Signed FIToFICustomerCreditTransferV10 message", LogEventLevel.Information),
				new("Sending FIToFICustomerCreditTransferV10 to RTGS (SendPayawayCreateAsync)", LogEventLevel.Information),
				new("Sent FIToFICustomerCreditTransferV10 to RTGS (SendPayawayCreateAsync)", LogEventLevel.Information),
				new("Received FIToFICustomerCreditTransferV10 acknowledgement (acknowledged) from RTGS (SendPayawayCreateAsync)", LogEventLevel.Information)
			});

	public override IPublisherAction<BankToCustomerDebitCreditNotificationV09> PayawayConfirmation =>
		new PublisherActionWithLogs<BankToCustomerDebitCreditNotificationV09>(
			PublisherActions.PayawayConfirmation,
			new List<LogEntry>
			{
				new("No message signer found for BankToCustomerDebitCreditNotificationV09 message, skipping signing", LogEventLevel.Debug),
				new("Sending BankToCustomerDebitCreditNotificationV09 to RTGS (SendPayawayConfirmationAsync)", LogEventLevel.Information),
				new("Sent BankToCustomerDebitCreditNotificationV09 to RTGS (SendPayawayConfirmationAsync)", LogEventLevel.Information),
				new("Received BankToCustomerDebitCreditNotificationV09 acknowledgement (acknowledged) from RTGS (SendPayawayConfirmationAsync)", LogEventLevel.Information)
			});

	public override IPublisherAction<Admi00200101> PayawayRejection =>
		new PublisherActionWithLogs<Admi00200101>(
			PublisherActions.PayawayRejection,
			new List<LogEntry>
			{
				new("No message signer found for Admi00200101 message, skipping signing", LogEventLevel.Debug),
				new("Sending Admi00200101 to RTGS (SendPayawayRejectionAsync)", LogEventLevel.Information),
				new("Sent Admi00200101 to RTGS (SendPayawayRejectionAsync)", LogEventLevel.Information),
				new("Received Admi00200101 acknowledgement (acknowledged) from RTGS (SendPayawayRejectionAsync)", LogEventLevel.Information)
			});

	public override IPublisherAction<BankPartnersRequestV1> BankPartnersRequest =>
		new PublisherActionWithLogs<BankPartnersRequestV1>(
			PublisherActions.BankPartnersRequest,
			new List<LogEntry>
			{
				new("No message signer found for BankPartnersRequestV1 message, skipping signing", LogEventLevel.Debug),
				new("Sending BankPartnersRequestV1 to RTGS (SendBankPartnersRequestAsync)", LogEventLevel.Information),
				new("Sent BankPartnersRequestV1 to RTGS (SendBankPartnersRequestAsync)", LogEventLevel.Information),
				new("Received BankPartnersRequestV1 acknowledgement (acknowledged) from RTGS (SendBankPartnersRequestAsync)", LogEventLevel.Information)
			});
}
