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

	public static readonly PublisherAction<PayawayCreationV1> PayawayCreate = new(
		null,
		(publisher, request, cancellationToken) => publisher.SendPayawayCreateAsync(request, cancellationToken));

	public static readonly PublisherAction<PayawayConfirmationV1> PayawayConfirmation = new(
		null,
		(publisher, request, cancellationToken) => publisher.SendPayawayConfirmationAsync(request, cancellationToken));

	public static readonly PublisherAction<PayawayRejectionV1> PayawayRejection = new(
		null,
		(publisher, request, cancellationToken) => publisher.SendPayawayRejectionAsync(request, cancellationToken));

	public static readonly PublisherAction<BankPartnersRequestV1> BankPartnersRequest = new(
		null,
		(publisher, request, cancellationToken) => publisher.SendBankPartnersRequestAsync(request, cancellationToken));
	
	public static readonly PublisherAction<AtomicLockRequestV2> AtomicLockV2 = new(
		null,
		(publisher, request, cancellationToken) => publisher.SendAtomicLockRequestAsync(request, cancellationToken));
}
