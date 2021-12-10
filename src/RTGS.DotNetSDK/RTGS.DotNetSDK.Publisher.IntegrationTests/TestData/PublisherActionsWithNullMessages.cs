using RTGS.DotNetSDK.Publisher.Messages;
using RTGS.ISO20022.Messages.Camt_054_001.V09;
using RTGS.ISO20022.Messages.Pacs_008_001.V10;

namespace RTGS.DotNetSDK.Publisher.IntegrationTests.TestData
{
	public static class PublisherActionsWithNullMessages
	{
		public static readonly PublisherAction<AtomicLockRequest> AtomicLock = new(
			null,
			(publisher, request, cancellationToken) => publisher.SendAtomicLockRequestAsync(request, cancellationToken));

		public static readonly PublisherAction<AtomicTransferRequest> AtomicTransfer = new(
			null,
			(publisher, request, cancellationToken) => publisher.SendAtomicTransferRequestAsync(request, cancellationToken));

		public static readonly PublisherAction<EarmarkConfirmation> EarmarkConfirmation = new(
			null,
			(publisher, request, cancellationToken) => publisher.SendEarmarkConfirmationAsync(request, cancellationToken));

		public static readonly PublisherAction<AtomicTransferConfirmation> AtomicTransferConfirmation = new(
			null,
			(publisher, request, cancellationToken) => publisher.SendAtomicTransferConfirmationAsync(request, cancellationToken));

		public static readonly PublisherAction<UpdateLedgerRequest> UpdateLedger = new(
			null,
			(publisher, request, cancellationToken) => publisher.SendUpdateLedgerRequestAsync(request, cancellationToken));

		public static readonly PublisherAction<FIToFICustomerCreditTransferV10> PayawayCreate = new(
			null,
			(publisher, request, cancellationToken) => publisher.SendPayawayCreateAsync(request, cancellationToken));

		public static readonly PublisherAction<BankToCustomerDebitCreditNotificationV09> PayawayConfirmation = new(
			null,
			(publisher, request, cancellationToken) => publisher.SendPayawayConfirmationAsync(request, cancellationToken));
	}
}
