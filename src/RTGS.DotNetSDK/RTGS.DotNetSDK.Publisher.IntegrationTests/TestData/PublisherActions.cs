using RTGS.ISO20022.Messages.Admi_002_001.V01;
using RTGS.ISO20022.Messages.Camt_054_001.V09;
using RTGS.ISO20022.Messages.Pacs_008_001.V10;

namespace RTGS.DotNetSDK.Publisher.IntegrationTests.TestData;

public static class PublisherActions
{
	public static readonly PublisherAction<AtomicLockRequestV1> AtomicLock = new(
		ValidMessages.AtomicLockRequest,
		(publisher, request, cancellationToken) => publisher.SendAtomicLockRequestAsync(request, cancellationToken));

	public static readonly PublisherAction<AtomicTransferRequestV1> AtomicTransfer = new(
		ValidMessages.AtomicTransferRequest,
		(publisher, request, cancellationToken) => publisher.SendAtomicTransferRequestAsync(request, cancellationToken));

	public static readonly PublisherAction<EarmarkConfirmationV1> EarmarkConfirmation = new(
		ValidMessages.EarmarkConfirmation,
		(publisher, request, cancellationToken) => publisher.SendEarmarkConfirmationAsync(request, cancellationToken));

	public static readonly PublisherAction<AtomicTransferConfirmationV1> AtomicTransferConfirmation = new(
		ValidMessages.AtomicTransferConfirmation,
		(publisher, request, cancellationToken) => publisher.SendAtomicTransferConfirmationAsync(request, cancellationToken));

	public static readonly PublisherAction<UpdateLedgerRequestV1> UpdateLedger = new(
		ValidMessages.UpdateLedgerRequest,
		(publisher, request, cancellationToken) => publisher.SendUpdateLedgerRequestAsync(request, cancellationToken));

	public static readonly PublisherAction<FIToFICustomerCreditTransferV10> PayawayCreate = new(
		ValidMessages.PayawayCreate,
		(publisher, request, cancellationToken) => publisher.SendPayawayCreateAsync(request, cancellationToken));

	public static readonly PublisherAction<BankToCustomerDebitCreditNotificationV09> PayawayConfirmation = new(
		ValidMessages.PayawayConfirmation,
		(publisher, request, cancellationToken) => publisher.SendPayawayConfirmationAsync(request, cancellationToken));

	public static readonly PublisherAction<Admi00200101> PayawayRejection = new(
		ValidMessages.PayawayRejection,
		new Dictionary<string, string> { { "ToBankDid", "partner-bank-did" } },
		(publisher, request, cancellationToken) => publisher.SendPayawayRejectionAsync(request, "partner-bank-did", cancellationToken));

	public static readonly PublisherAction<BankPartnersRequestV1> BankPartnersRequest = new(
		ValidMessages.BankPartnersRequest,
		(publisher, request, cancellationToken) => publisher.SendBankPartnersRequestAsync(request, cancellationToken));

}
