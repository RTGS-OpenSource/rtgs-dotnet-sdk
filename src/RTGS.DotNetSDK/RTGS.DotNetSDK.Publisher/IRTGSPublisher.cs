using System;
using System.Threading.Tasks;
using RTGS.DotNetSDK.Publisher.Messages;
using RTGS.Public.Payment.V1.Pacs;

namespace RTGS.DotNetSDK.Publisher
{
	public interface IRtgsPublisher : IAsyncDisposable
	{
		Task<SendResult> SendAtomicLockRequestAsync(AtomicLockRequest message);
		Task<SendResult> SendAtomicTransferRequestAsync(AtomicTransferRequest message);
		Task<SendResult> SendEarmarkConfirmationAsync(EarmarkConfirmation message);
		Task<SendResult> SendTransferConfirmationAsync(TransferConfirmation request);
		Task<SendResult> SendUpdateLedgerRequestAsync(UpdateLedgerRequest request);
		Task<SendResult> SendPayawayCreateAsync(FinancialInstitutionToFinancialInstitutionCustomerCreditTransfer request);
		Task<SendResult> SendPayawayConfirmationAsync(BankToCustomerDebitCreditNotification request);
		Task<SendResult> SendRequestAsync<T>(T message, string instructionType);
	}
}
