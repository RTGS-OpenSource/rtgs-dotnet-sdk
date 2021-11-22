using System;
using System.Threading;
using System.Threading.Tasks;
using RTGS.DotNetSDK.Publisher.Messages;
using RTGS.ISO20022.Messages.Camt_054_001.V09;
using RTGS.ISO20022.Messages.Pacs_008_001.V10;

namespace RTGS.DotNetSDK.Publisher
{
	public interface IRtgsPublisher : IAsyncDisposable
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="message"><see cref="AtomicLockRequest"/></param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/></param>
		/// <returns><see cref="Task&lt;SendResult&gt;"/></returns>
		/// <example>
		/// <code>
		/// int c = Math.Add(4, 5);
		/// if (c > 10)
		/// {
		///     Console.WriteLine(c);
		/// }
		/// </code>
		/// </example>
		Task<SendResult> SendAtomicLockRequestAsync(AtomicLockRequest message, CancellationToken cancellationToken = default);
		Task<SendResult> SendAtomicTransferRequestAsync(AtomicTransferRequest message, CancellationToken cancellationToken = default);
		Task<SendResult> SendEarmarkConfirmationAsync(EarmarkConfirmation message, CancellationToken cancellationToken = default);
		Task<SendResult> SendTransferConfirmationAsync(TransferConfirmation message, CancellationToken cancellationToken = default);
		Task<SendResult> SendUpdateLedgerRequestAsync(UpdateLedgerRequest message, CancellationToken cancellationToken = default);
		Task<SendResult> SendPayawayCreateAsync(FIToFICustomerCreditTransferV10 message, CancellationToken cancellationToken = default);
		Task<SendResult> SendPayawayConfirmationAsync(BankToCustomerDebitCreditNotificationV09 message, CancellationToken cancellationToken = default);
	}
}
