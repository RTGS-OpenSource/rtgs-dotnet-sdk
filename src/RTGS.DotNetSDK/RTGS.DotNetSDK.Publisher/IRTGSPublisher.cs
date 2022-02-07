using RTGS.DotNetSDK.Publisher.Messages;
using RTGS.ISO20022.Messages.Admi_002_001.V01;
using RTGS.ISO20022.Messages.Camt_054_001.V09;
using RTGS.ISO20022.Messages.Pacs_008_001.V10;

namespace RTGS.DotNetSDK.Publisher;

/// <summary>
/// The IRtgsPublisher interface, implementations of this interface are responsible for publishing messages to the RTGS platform
/// </summary>
public interface IRtgsPublisher 
{
	/// <summary>
	/// Sends an <see cref="AtomicLockRequestV1"/> to initiate a transaction.
	/// </summary>
	/// <param name="message">The <see cref="AtomicLockRequestV1"/> message</param>
	/// <param name="bankPartnerDid">The Bank Did of the Bank Partner</param>
	/// <param name="cancellationToken">A cancellation token</param>
	/// <returns>The result of the operation</returns>
	Task<SendResult> SendAtomicLockRequestAsync(AtomicLockRequestV1 message, string bankPartnerDid, CancellationToken cancellationToken = default);

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
	/// Sends a <see cref="FIToFICustomerCreditTransferV10"/> (payaway) transaction request.
	/// </summary>
	/// <param name="message">The <see cref="FIToFICustomerCreditTransferV10"/> message</param>
	/// <param name="cancellationToken">A cancellation token</param>
	/// <returns>The result of the operation</returns>
	/// <remarks>
	/// The <see cref="FIToFICustomerCreditTransferV10"/> type is from nuget package RTGS.ISO20022.Messages <see href="https://www.nuget.org/packages/RTGS.ISO20022.Messages/"/>
	/// </remarks>
	Task<SendResult> SendPayawayCreateAsync(FIToFICustomerCreditTransferV10 message, CancellationToken cancellationToken = default);

	/// <summary>
	/// Sends a <see cref="BankToCustomerDebitCreditNotificationV09"/> (payaway) confirmation request.
	/// </summary>
	/// <param name="message">The <see cref="BankToCustomerDebitCreditNotificationV09"/>  message</param>
	/// <param name="cancellationToken">A cancellation token</param>
	/// <returns>The result of the operation</returns>
	/// <remarks>
	/// The <see cref="BankToCustomerDebitCreditNotificationV09"/> type is from nuget package RTGS.ISO20022.Messages <see href="https://www.nuget.org/packages/RTGS.ISO20022.Messages/"/>
	/// </remarks>
	Task<SendResult> SendPayawayConfirmationAsync(BankToCustomerDebitCreditNotificationV09 message, CancellationToken cancellationToken = default);

	/// <summary>
	/// Sends a <see cref="Admi00200101"/> (payaway) rejection request.
	/// </summary>
	/// <param name="message">The <see cref="Admi00200101"/> rejection message</param>
	/// <param name="toBankDid">The BankDid for the bank to which this rejection should be sent</param>
	/// <param name="cancellationToken">A cancellation token</param>
	/// <returns>The result of the operation</returns>
	/// <remarks>
	/// The <see cref="Admi00200101"/> type is from nuget package RTGS.ISO20022.Messages <see href="https://www.nuget.org/packages/RTGS.ISO20022.Messages/"/>
	/// </remarks>
	Task<SendResult> SendPayawayRejectionAsync(Admi00200101 message, string toBankDid, CancellationToken cancellationToken);

	/// <summary>
	/// Sends a <see cref="BankPartnersRequestV1"/> bank partners request.
	/// </summary>
	/// <param name="message">The <see cref="BankPartnersRequestV1"/> message</param>
	/// <param name="cancellationToken">A cancellation token</param>
	/// <returns>The result of the operation</returns>
	Task<SendResult> SendBankPartnersRequestAsync(BankPartnersRequestV1 message, CancellationToken cancellationToken = default);
}
