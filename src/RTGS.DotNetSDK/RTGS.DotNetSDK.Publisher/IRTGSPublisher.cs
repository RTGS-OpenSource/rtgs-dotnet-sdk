using System;
using System.Threading;
using System.Threading.Tasks;
using RTGS.DotNetSDK.Publisher.Messages;
using RTGS.ISO20022.Messages.Camt_054_001.V09;
using RTGS.ISO20022.Messages.Pacs_008_001.V10;

namespace RTGS.DotNetSDK.Publisher
{
	/// <summary>
	/// The IRtgsPublisher interface, implementations of this interface are responsible for publishing messages to the RTGS platform
	/// </summary>
	public interface IRtgsPublisher : IAsyncDisposable
	{
		/// <summary>
		/// Sends an <see cref="AtomicLockRequest"/> to initiate a transaction.
		/// </summary>
		/// <param name="message">The AtomicLockRequest message</param>
		/// <param name="cancellationToken">A cancellation token</param>
		/// <returns>The result of the operation</returns>
		Task<SendResult> SendAtomicLockRequestAsync(AtomicLockRequest message, CancellationToken cancellationToken = default);

		/// <summary>
		/// Sends an <see cref="AtomicTransferRequest"/> to invoke transfer of funds.
		/// </summary>
		/// <param name="message">The AtomicTransferRequest message</param>
		/// <param name="cancellationToken">A cancellation token</param>
		/// <returns>The result of the operation</returns>
		Task<SendResult> SendAtomicTransferRequestAsync(AtomicTransferRequest message, CancellationToken cancellationToken = default);

		/// <summary>
		/// Sends an <see cref="EarmarkConfirmation"/> to confirm funds have been earmarked.
		/// </summary>
		/// <param name="message">The EarmarkConfirmation message</param>
		/// <param name="cancellationToken">A cancellation token</param>
		/// <returns>The result of the operation</returns>
		Task<SendResult> SendEarmarkConfirmationAsync(EarmarkConfirmation message, CancellationToken cancellationToken = default);

		/// <summary>
		/// Sends a <see cref="TransferConfirmation"/> request to confirm fund transfer.
		/// </summary>
		/// <param name="message">The TransferConfirmation message</param>
		/// <param name="cancellationToken">A cancellation token</param>
		/// <returns>The result of the operation</returns>
		Task<SendResult> SendTransferConfirmationAsync(TransferConfirmation message, CancellationToken cancellationToken = default);

		/// <summary>
		/// Sends an <see cref="UpdateLedgerRequest"/> to notify RTGS of a change to available funds.
		/// </summary>
		/// <param name="message">The UpdateLedgerRequest message</param>
		/// <param name="cancellationToken">A cancellation token</param>
		/// <returns>The result of the operation</returns>
		Task<SendResult> SendUpdateLedgerRequestAsync(UpdateLedgerRequest message, CancellationToken cancellationToken = default);

		/// <summary>
		/// Sends a FIToFICustomerCreditTransferV10 (payaway) transaction request.
		/// </summary>
		/// <param name="message">The FIToFICustomerCreditTransferV10 message</param>
		/// <param name="cancellationToken">A cancellation token</param>
		/// <returns>The result of the operation</returns>
		/// <remarks>
		/// The <c>FIToFICustomerCreditTransferV10</c> type is from nuget package RTGS.ISO20022.Messages <see href="https://www.nuget.org/packages/RTGS.ISO20022.Messages/"/>
		/// </remarks>
		Task<SendResult> SendPayawayCreateAsync(FIToFICustomerCreditTransferV10 message, CancellationToken cancellationToken = default);

		/// <summary>
		/// Sends a BankToCustomerDebitCreditNotificationV09 (payaway) confirmation request
		/// </summary>
		/// <param name="message">The BankToCustomerDebitCreditNotificationV09 message</param>
		/// <param name="cancellationToken">A cancellation token</param>
		/// <returns>The result of the operation</returns>
		/// <remarks>
		/// The <c>BankToCustomerDebitCreditNotificationV09</c> type is from nuget package RTGS.ISO20022.Messages <see href="https://www.nuget.org/packages/RTGS.ISO20022.Messages/"/>
		/// </remarks>
		Task<SendResult> SendPayawayConfirmationAsync(BankToCustomerDebitCreditNotificationV09 message, CancellationToken cancellationToken = default);
	}
}
