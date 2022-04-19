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

	public Task<SendResult> SendAtomicLockRequestAsync(AtomicLockRequestV1 message, string bankPartnerRtgsGlobalId, CancellationToken cancellationToken)
	{
		var headers = new Dictionary<string, string> { { "bankpartnerdid", bankPartnerRtgsGlobalId }, { "bank-partner-rtgs-global-id", bankPartnerRtgsGlobalId } };
		return _internalPublisher.SendMessageAsync(message, "payment.lock.v2", cancellationToken, headers);
	}

	public Task<SendResult> SendAtomicTransferRequestAsync(AtomicTransferRequestV1 message, CancellationToken cancellationToken) =>
		_internalPublisher.SendMessageAsync(message, "payment.block.v2", cancellationToken);

	public Task<SendResult> SendEarmarkConfirmationAsync(EarmarkConfirmationV1 message, CancellationToken cancellationToken) =>
		_internalPublisher.SendMessageAsync(message, "payment.earmarkconfirmation.v1", cancellationToken);

	public Task<SendResult> SendAtomicTransferConfirmationAsync(AtomicTransferConfirmationV1 message, CancellationToken cancellationToken) =>
		_internalPublisher.SendMessageAsync(message, "payment.blockconfirmation.v1", cancellationToken);

	public Task<SendResult> SendUpdateLedgerRequestAsync(UpdateLedgerRequestV1 message, CancellationToken cancellationToken) =>
		_internalPublisher.SendMessageAsync(message, "payment.update.ledger.v2", cancellationToken);

	public Task<SendResult> SendPayawayCreateAsync(FIToFICustomerCreditTransferV10 message, string idCryptAlias, CancellationToken cancellationToken) =>
		_internalPublisher.SendMessageAsync(message, "payaway.create.v1", idCryptAlias: idCryptAlias, cancellationToken: cancellationToken);

	public Task<SendResult> SendPayawayConfirmationAsync(BankToCustomerDebitCreditNotificationV09 message, CancellationToken cancellationToken) =>
		_internalPublisher.SendMessageAsync(message, "payaway.confirmation.v1", cancellationToken);

	public Task<SendResult> SendPayawayRejectionAsync(Admi00200101 message, string toRtgsGlobalId, CancellationToken cancellationToken)
	{
		var headers = new Dictionary<string, string> { { "tobankdid", toRtgsGlobalId }, { "to-rtgs-global-id", toRtgsGlobalId } };
		return _internalPublisher.SendMessageAsync(message, "payaway.rejection.v1", cancellationToken, headers);
	}

	public Task<SendResult> SendBankPartnersRequestAsync(BankPartnersRequestV1 message, CancellationToken cancellationToken) =>
		_internalPublisher.SendMessageAsync(message, "bank.partners.v1", cancellationToken);
}
