namespace RTGS.DotNetSDK.IntegrationTests.Publisher.TestData;

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

	public static readonly PublisherAction<PayawayCreationV1> PayawayCreate = new(
		ValidMessages.PayawayCreation,
		(publisher, request, cancellationToken) => publisher.SendPayawayCreateAsync(request, cancellationToken),
		ValidMessages.SignedDocuments.PayawayCreateDocument);

	public static readonly PublisherAction<PayawayConfirmationV1> PayawayConfirmation = new(
		ValidMessages.PayawayConfirmation,
		(publisher, request, cancellationToken) => publisher.SendPayawayConfirmationAsync(request, cancellationToken),
		ValidMessages.SignedDocuments.PayawayConfirmationDocument);

	public static readonly PublisherAction<PayawayRejectionV1> PayawayRejection = new(
		ValidMessages.PayawayRejection,
		(publisher, request, cancellationToken) =>
			publisher.SendPayawayRejectionAsync(request, cancellationToken),
		ValidMessages.SignedDocuments.PayawayRejectionDocument);

	public static readonly PublisherAction<BankPartnersRequestV1> BankPartnersRequest = new(
		ValidMessages.BankPartnersRequest,
		(publisher, request, cancellationToken) => publisher.SendBankPartnersRequestAsync(request, cancellationToken));
}
