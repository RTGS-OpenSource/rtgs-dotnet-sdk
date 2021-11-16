using RTGS.DotNetSDK.Publisher.Messages;
using RTGS.ISO20022.Messages.Camt_054_001.V09;
using RTGS.ISO20022.Messages.Pacs_008_001.V10;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RTGS.DotNetSDK.Publisher
{
	public interface IRtgsPublisher : IAsyncDisposable
	{
		Task<SendResult> SendAtomicLockRequestAsync(AtomicLockRequest message, CancellationToken cancellationToken = default);
		Task<SendResult> SendAtomicTransferRequestAsync(AtomicTransferRequest message, CancellationToken cancellationToken = default);
		Task<SendResult> SendEarmarkConfirmationAsync(EarmarkConfirmation message, CancellationToken cancellationToken = default);
		Task<SendResult> SendTransferConfirmationAsync(TransferConfirmation message, CancellationToken cancellationToken = default);
		Task<SendResult> SendUpdateLedgerRequestAsync(UpdateLedgerRequest message, CancellationToken cancellationToken = default);
		Task<SendResult> SendPayawayCreateAsync(FIToFICustomerCreditTransferV10 message, CancellationToken cancellationToken = default);
		Task<SendResult> SendPayawayConfirmationAsync(BankToCustomerDebitCreditNotificationV09 message, CancellationToken cancellationToken = default);
	}
}
