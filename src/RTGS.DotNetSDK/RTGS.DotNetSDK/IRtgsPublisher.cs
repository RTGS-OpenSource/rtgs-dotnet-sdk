using RTGS.Public.Messages.Publisher;

namespace RTGS.DotNetSDK;

/// <summary>
/// The IRtgsPublisher interface, implementations of this interface are responsible for publishing messages to the RTGS platform
/// </summary>
public interface IRtgsPublisher
{
	/// <summary>
	/// Sends an <see cref="AtomicLockRequestV1"/> to initiate a transaction.
	/// </summary>
	/// <param name="message">The <see cref="AtomicLockRequestV1"/> message</param>
	/// <param name="cancellationToken">A cancellation token</param>
	/// <returns>The result of the operation</returns>
	Task<SendResult> SendAtomicLockRequestAsync(AtomicLockRequestV1 message, CancellationToken cancellationToken = default);

	/// <summary>
	/// Sends an <see cref="AtomicTransferRequestV1"/> to invoke transfer of funds.
	/// </summary>
	/// <param name="message">The <see cref="AtomicTransferRequestV1"/> message</param>
	/// <param name="cancellationToken">A cancellation token</param>
	/// <returns>The result of the operation</returns>
	Task<SendResult> SendAtomicTransferRequestAsync(AtomicTransferRequestV1 message, CancellationToken cancellationToken = default);

	/// <summary>
	/// Sends an <see cref="EarmarkConfirmationV1"/> to confirm funds have been earmarked.
	/// </summary>
	/// <param name="message">The <see cref="EarmarkConfirmationV1"/> message</param>
	/// <param name="cancellationToken">A cancellation token</param>
	/// <returns>The result of the operation</returns>
	Task<SendResult> SendEarmarkConfirmationAsync(EarmarkConfirmationV1 message, CancellationToken cancellationToken = default);

	/// <summary>
	/// Sends a <see cref="AtomicTransferConfirmationV1"/> request to confirm atomic fund transfer.
	/// </summary>
	/// <param name="message">The <see cref="AtomicTransferConfirmationV1"/> message</param>
	/// <param name="cancellationToken">A cancellation token</param>
	/// <returns>The result of the operation</returns>
	Task<SendResult> SendAtomicTransferConfirmationAsync(AtomicTransferConfirmationV1 message, CancellationToken cancellationToken = default);

	/// <summary>
	/// Sends an <see cref="UpdateLedgerRequestV1"/> to notify RTGS of a change to available funds.
	/// </summary>
	/// <param name="message">The <see cref="UpdateLedgerRequestV1"/> message</param>
	/// <param name="cancellationToken">A cancellation token</param>
	/// <returns>The result of the operation</returns>
	Task<SendResult> SendUpdateLedgerRequestAsync(UpdateLedgerRequestV1 message, CancellationToken cancellationToken = default);

	/// <summary>
	/// Sends a <see cref="PayawayCreationV1"/> request.
	/// </summary>
	/// <param name="message">The <see cref="PayawayCreationV1"/> message</param>
	/// <param name="partnerRtgsGlobalId">The RTGS Global ID of the recipient partner bank</param>
	/// <param name="cancellationToken">A cancellation token</param>
	/// <returns>The result of the operation</returns>
	Task<SendResult> SendPayawayCreateAsync(PayawayCreationV1 message, string partnerRtgsGlobalId, CancellationToken cancellationToken = default);

	/// <summary>
	/// Sends a <see cref="PayawayConfirmationV1"/> request.
	/// </summary>
	/// <param name="message">The <see cref="PayawayConfirmationV1"/>  message</param>
	/// <param name="partnerRtgsGlobalId">The RTGS Global ID of the recipient partner bank</param>
	/// <param name="cancellationToken">A cancellation token</param>
	/// <returns>The result of the operation</returns>
	Task<SendResult> SendPayawayConfirmationAsync(PayawayConfirmationV1 message, string partnerRtgsGlobalId, CancellationToken cancellationToken = default);

	/// <summary>
	/// Sends a <see cref="PayawayRejectionV1"/> request.
	/// </summary>
	/// <param name="message">The <see cref="PayawayRejectionV1"/> rejection message</param>
	/// <param name="partnerRtgsGlobalId">The RTGS Global ID of the recipient partner bank</param>
	/// <param name="cancellationToken">A cancellation token</param>
	/// <returns>The result of the operation</returns>
	Task<SendResult> SendPayawayRejectionAsync(PayawayRejectionV1 message, string partnerRtgsGlobalId, CancellationToken cancellationToken = default);

	/// <summary>
	/// Sends a <see cref="BankPartnersRequestV1"/> request.
	/// </summary>
	/// <param name="message">The <see cref="BankPartnersRequestV1"/> message</param>
	/// <param name="cancellationToken">A cancellation token</param>
	/// <returns>The result of the operation</returns>
	Task<SendResult> SendBankPartnersRequestAsync(BankPartnersRequestV1 message, CancellationToken cancellationToken = default);
}
