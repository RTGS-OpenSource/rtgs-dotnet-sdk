using System;
using System.Threading;
using System.Threading.Tasks;
using RTGS.DotNetSDK.Publisher.Messages;
using RTGS.ISO20022.Messages.Camt_054_001.V09;
using RTGS.ISO20022.Messages.Pacs_008_001.V10;

namespace RTGS.DotNetSDK.Publisher
{
	/// <summary>
	/// The IRtgsPublisher interface
	/// </summary>
	public interface IRtgsPublisher : IAsyncDisposable
	{
		/// <summary>
		/// Sends an AtomicLockRequest
		/// </summary>
		/// <param name="message">The AtomicLockRequest message</param>
		/// <param name="cancellationToken">A cancellation token</param>
		/// <returns>The result of the operation</returns>
		Task<SendResult> SendAtomicLockRequestAsync(AtomicLockRequest message, CancellationToken cancellationToken = default);

		/// <summary>
		/// Sends an AtomicTransferRequest
		/// </summary>
		/// <param name="message">The AtomicTransferRequest message</param>
		/// <param name="cancellationToken">A cancellation token</param>
		/// <returns>The result of the operation</returns>
		Task<SendResult> SendAtomicTransferRequestAsync(AtomicTransferRequest message, CancellationToken cancellationToken = default);

		/// <summary>
		/// Sends an EarmarkConfirmation request
		/// </summary>
		/// <param name="message">The EarmarkConfirmation message</param>
		/// <param name="cancellationToken">A cancellation token</param>
		/// <returns>The result of the operation</returns>
		Task<SendResult> SendEarmarkConfirmationAsync(EarmarkConfirmation message, CancellationToken cancellationToken = default);

		/// <summary>
		/// Sends a TransferConfirmation request
		/// </summary>
		/// <param name="message">The TransferConfirmation message</param>
		/// <param name="cancellationToken">A cancellation token</param>
		/// <returns>The result of the operation</returns>
		Task<SendResult> SendTransferConfirmationAsync(TransferConfirmation message, CancellationToken cancellationToken = default);

		/// <summary>
		/// Sends an UpdateLedgerRequest
		/// </summary>
		/// <param name="message">The UpdateLedgerRequest message</param>
		/// <param name="cancellationToken">A cancellation token</param>
		/// <returns>The result of the operation</returns>
		Task<SendResult> SendUpdateLedgerRequestAsync(UpdateLedgerRequest message, CancellationToken cancellationToken = default);

		/// <summary>
		/// Sends a FIToFICustomerCreditTransferV10 request
		/// </summary>
		/// <param name="message">The FIToFICustomerCreditTransferV10 message</param>
		/// <param name="cancellationToken">A cancellation token</param>
		/// <returns>The result of the operation</returns>
		Task<SendResult> SendPayawayCreateAsync(FIToFICustomerCreditTransferV10 message, CancellationToken cancellationToken = default);

		/// <summary>
		/// Sends a BankToCustomerDebitCreditNotificationV09 request
		/// </summary>
		/// <param name="message">The BankToCustomerDebitCreditNotificationV09 message</param>
		/// <param name="cancellationToken">A cancellation token</param>
		/// <returns>The result of the operation</returns>
		Task<SendResult> SendPayawayConfirmationAsync(BankToCustomerDebitCreditNotificationV09 message, CancellationToken cancellationToken = default);
	}
}
