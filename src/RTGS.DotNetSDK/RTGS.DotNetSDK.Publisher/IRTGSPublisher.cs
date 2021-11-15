using System;
using System.Threading.Tasks;
using RTGS.DotNetSDK.Publisher.Messages;
using RTGS.ISO20022.Messages.Camt_054_001.V09;
using RTGS.ISO20022.Messages.Pacs_008_001.V10;

namespace RTGS.DotNetSDK.Publisher
{
	public interface IRtgsPublisher : IAsyncDisposable
	{
		Task<SendResult> SendAtomicLockRequestAsync(AtomicLockRequest message);
		Task<SendResult> SendAtomicTransferRequestAsync(AtomicTransferRequest message);
		Task<SendResult> SendEarmarkConfirmationAsync(EarmarkConfirmation message);
		Task<SendResult> SendTransferConfirmationAsync(TransferConfirmation message);
		Task<SendResult> SendUpdateLedgerRequestAsync(UpdateLedgerRequest message);
		Task<SendResult> SendPayawayCreateAsync(FIToFICustomerCreditTransferV10 message);
		Task<SendResult> SendPayawayConfirmationAsync(BankToCustomerDebitCreditNotificationV09 message);
	}
}
