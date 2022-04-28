using RTGS.DotNetSDK.Publisher.Messages;
using RTGS.ISO20022.Messages.Admi_002_001.V01;
using RTGS.ISO20022.Messages.Camt_054_001.V09;
using RTGS.ISO20022.Messages.Pacs_008_001.V10;

namespace RTGS.DotNetSDK.Publisher;

internal sealed class RtgsPublisher : IRtgsPublisher
{
	private readonly IInternalPublisher _internalPublisher;

	public RtgsPublisher(IInternalPublisher internalPublisher)
	{
		_internalPublisher = internalPublisher;
	}

	public Task<SendResult> SendAtomicLockRequestAsync(AtomicLockRequestV1 message, CancellationToken cancellationToken = default) =>
		_internalPublisher.SendMessageAsync(message, "payment.lock.v2", cancellationToken);

	public Task<SendResult> SendAtomicTransferRequestAsync(AtomicTransferRequestV1 message, CancellationToken cancellationToken = default) =>
		_internalPublisher.SendMessageAsync(message, "payment.block.v2", cancellationToken);

	public Task<SendResult> SendEarmarkConfirmationAsync(EarmarkConfirmationV1 message, CancellationToken cancellationToken = default) =>
		_internalPublisher.SendMessageAsync(message, "payment.earmarkconfirmation.v1", cancellationToken);

	public Task<SendResult> SendAtomicTransferConfirmationAsync(AtomicTransferConfirmationV1 message, CancellationToken cancellationToken = default) =>
		_internalPublisher.SendMessageAsync(message, "payment.blockconfirmation.v1", cancellationToken);

	public Task<SendResult> SendUpdateLedgerRequestAsync(UpdateLedgerRequestV1 message, CancellationToken cancellationToken = default) =>
		_internalPublisher.SendMessageAsync(message, "payment.update.ledger.v2", cancellationToken);

	public Task<SendResult> SendPayawayCreateAsync(FIToFICustomerCreditTransferV10 message, string idCryptAlias, CancellationToken cancellationToken = default) =>
		_internalPublisher.SendMessageAsync(message, "payaway.create.v1", idCryptAlias: idCryptAlias, cancellationToken: cancellationToken);

	public Task<SendResult> SendPayawayConfirmationAsync(BankToCustomerDebitCreditNotificationV09 message, string idCryptAlias, CancellationToken cancellationToken = default) =>
		_internalPublisher.SendMessageAsync(message, "payaway.confirmation.v1", cancellationToken, idCryptAlias: idCryptAlias);

	public Task<SendResult> SendPayawayRejectionAsync(Admi00200101 message, string toRtgsGlobalId, string idCryptAlias, CancellationToken cancellationToken = default)
	{
		var headers = new Dictionary<string, string> { { "to-rtgs-global-id", toRtgsGlobalId } };
		return _internalPublisher.SendMessageAsync(message, "payaway.rejection.v1", cancellationToken, headers, idCryptAlias);
	}

	public Task<SendResult> SendBankPartnersRequestAsync(BankPartnersRequestV1 message, CancellationToken cancellationToken = default) =>
		_internalPublisher.SendMessageAsync(message, "bank.partners.v1", cancellationToken);
}
