﻿using RTGS.DotNetSDK.Publisher.Messages;
using RTGS.ISO20022.Messages.Camt_054_001.V09;
using RTGS.ISO20022.Messages.Pacs_008_001.V10;

namespace RTGS.DotNetSDK.Publisher.IntegrationTests.TestData
{
	public static class PublisherActions
	{
		public static readonly PublisherAction<AtomicLockRequest> AtomicLock = new(
			ValidRequests.AtomicLockRequest,
			(publisher, request, cancellationToken) => publisher.SendAtomicLockRequestAsync(request, cancellationToken));

		public static readonly PublisherAction<AtomicTransferRequest> AtomicTransfer = new(
			ValidRequests.AtomicTransferRequest,
			(publisher, request, cancellationToken) => publisher.SendAtomicTransferRequestAsync(request, cancellationToken));

		public static readonly PublisherAction<EarmarkConfirmation> EarmarkConfirmation = new(
			ValidRequests.EarmarkConfirmation,
			(publisher, request, cancellationToken) => publisher.SendEarmarkConfirmationAsync(request, cancellationToken));

		public static readonly PublisherAction<TransferConfirmation> TransferConfirmation = new(
			ValidRequests.TransferConfirmation,
			(publisher, request, cancellationToken) => publisher.SendTransferConfirmationAsync(request, cancellationToken));

		public static readonly PublisherAction<UpdateLedgerRequest> UpdateLedger = new(
			ValidRequests.UpdateLedgerRequest,
			(publisher, request, cancellationToken) => publisher.SendUpdateLedgerRequestAsync(request, cancellationToken));

		public static readonly PublisherAction<FIToFICustomerCreditTransferV10> PayawayCreate = new(
			ValidRequests.PayawayCreate,
			(publisher, request, cancellationToken) => publisher.SendPayawayCreateAsync(request, cancellationToken));

		public static readonly PublisherAction<BankToCustomerDebitCreditNotificationV09> PayawayConfirmation = new(
			ValidRequests.PayawayConfirmation,
			(publisher, request, cancellationToken) => publisher.SendPayawayConfirmationAsync(request, cancellationToken));
	}
}
