using RTGS.ISO20022.Messages.Admi_002_001.V01;
using RTGS.ISO20022.Messages.Camt_054_001.V09;
using RTGS.ISO20022.Messages.Pacs_008_001.V10;

namespace RTGS.DotNetSDK.IntegrationTests.Publisher.TestData;

public static class PublisherActionsWithNullMessages
{
	public static readonly PublisherAction<AtomicLockRequestV1> AtomicLock = new(
		null,
		(publisher, request, cancellationToken) => publisher.SendAtomicLockRequestAsync(request, cancellationToken));

	public static readonly PublisherAction<AtomicTransferRequestV1> AtomicTransfer = new(
		null,
		(publisher, request, cancellationToken) => publisher.SendAtomicTransferRequestAsync(request, cancellationToken));

	public static readonly PublisherAction<EarmarkConfirmationV1> EarmarkConfirmation = new(
		null,
		(publisher, request, cancellationToken) => publisher.SendEarmarkConfirmationAsync(request, cancellationToken));

	public static readonly PublisherAction<AtomicTransferConfirmationV1> AtomicTransferConfirmation = new(
		null,
		(publisher, request, cancellationToken) => publisher.SendAtomicTransferConfirmationAsync(request, cancellationToken));

	public static readonly PublisherAction<UpdateLedgerRequestV1> UpdateLedger = new(
		null,
		(publisher, request, cancellationToken) => publisher.SendUpdateLedgerRequestAsync(request, cancellationToken));

	public static readonly PublisherAction<FIToFICustomerCreditTransferV10> PayawayCreate = new(
		null,
		(publisher, request, cancellationToken) => publisher.SendPayawayCreateAsync(request, "id-crypt-alias", cancellationToken));

	public static readonly PublisherAction<BankToCustomerDebitCreditNotificationV09> PayawayConfirmation = new(
		null,
		(publisher, request, cancellationToken) => publisher.SendPayawayConfirmationAsync(request, cancellationToken));

	public static readonly PublisherAction<Admi00200101> PayawayRejection = new(
		null,
		(publisher, request, cancellationToken) =>
			publisher.SendPayawayRejectionAsync(request, null, "id-crypt-alias", cancellationToken));

	public static readonly PublisherAction<BankPartnersRequestV1> BankPartnersRequest = new(
		null,
		(publisher, request, cancellationToken) => publisher.SendBankPartnersRequestAsync(request, cancellationToken));
}
