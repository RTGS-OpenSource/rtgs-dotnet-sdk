using RTGS.Public.Messages.Publisher;

namespace RTGS.DotNetSDK.Publisher;

internal sealed class RtgsPublisher : IRtgsPublisher
{
	private readonly IInternalPublisher _internalPublisher;

	public RtgsPublisher(IInternalPublisher internalPublisher)
	{
		_internalPublisher = internalPublisher;
	}

	public Task<SendResult> SendAtomicLockRequestAsync(AtomicLockRequestV1 message, CancellationToken cancellationToken = default) =>
		_internalPublisher.SendMessageAsync(message, cancellationToken);

	public Task<SendResult> SendAtomicTransferRequestAsync(AtomicTransferRequestV1 message, CancellationToken cancellationToken = default) =>
		_internalPublisher.SendMessageAsync(message, cancellationToken);

	public Task<SendResult> SendEarmarkConfirmationAsync(EarmarkConfirmationV1 message, CancellationToken cancellationToken = default) =>
		_internalPublisher.SendMessageAsync(message, cancellationToken);

	public Task<SendResult> SendAtomicTransferConfirmationAsync(AtomicTransferConfirmationV1 message, CancellationToken cancellationToken = default) =>
		_internalPublisher.SendMessageAsync(message, cancellationToken);

	public Task<SendResult> SendUpdateLedgerRequestAsync(UpdateLedgerRequestV1 message, CancellationToken cancellationToken = default) =>
		_internalPublisher.SendMessageAsync(message, cancellationToken);

	public Task<SendResult> SendPayawayCreateAsync(PayawayCreationV1 message, string idCryptAlias, CancellationToken cancellationToken = default) =>
		_internalPublisher.SendMessageAsync(message, cancellationToken, idCryptAlias: idCryptAlias);

	public Task<SendResult> SendPayawayConfirmationAsync(PayawayConfirmationV1 message, string idCryptAlias, CancellationToken cancellationToken = default) =>
		_internalPublisher.SendMessageAsync(message, cancellationToken, idCryptAlias: idCryptAlias);

	public Task<SendResult> SendPayawayRejectionAsync(PayawayRejectionV1 message, string idCryptAlias, CancellationToken cancellationToken = default) =>
		_internalPublisher.SendMessageAsync(message, cancellationToken, idCryptAlias: idCryptAlias);

	public Task<SendResult> SendBankPartnersRequestAsync(BankPartnersRequestV1 message, CancellationToken cancellationToken = default) =>
		_internalPublisher.SendMessageAsync(message, cancellationToken);
}
