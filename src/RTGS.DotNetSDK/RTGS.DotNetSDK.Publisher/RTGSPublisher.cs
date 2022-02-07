using System.Collections.Generic;
using RTGS.DotNetSDK.Publisher.Messages;
using RTGS.ISO20022.Messages.Admi_002_001.V01;
using RTGS.ISO20022.Messages.Camt_054_001.V09;
using RTGS.ISO20022.Messages.Pacs_008_001.V10;

namespace RTGS.DotNetSDK.Publisher;

internal sealed class RtgsPublisher : IRtgsPublisher
{
	private readonly IMessagePublisher _messagePublisher;

	public RtgsPublisher(IMessagePublisher messagePublisher)
	{
		_messagePublisher = messagePublisher;
	}

	public Task<SendResult> SendAtomicLockRequestAsync(AtomicLockRequestV1 message, string bankPartnerDid, CancellationToken cancellationToken)
	{
		var headers = new Dictionary<string, string> { { "bankpartnerdid", bankPartnerDid } };
		return _messagePublisher.SendMessage(message, "payment.lock.v2", cancellationToken, headers);
	}

	public Task<SendResult> SendAtomicTransferRequestAsync(AtomicTransferRequestV1 message, CancellationToken cancellationToken) =>
		_messagePublisher.SendMessage(message, "payment.block.v2", cancellationToken);

	public Task<SendResult> SendEarmarkConfirmationAsync(EarmarkConfirmationV1 message, CancellationToken cancellationToken) =>
		_messagePublisher.SendMessage(message, "payment.earmarkconfirmation.v1", cancellationToken);

	public Task<SendResult> SendAtomicTransferConfirmationAsync(AtomicTransferConfirmationV1 message, CancellationToken cancellationToken) =>
		_messagePublisher.SendMessage(message, "payment.blockconfirmation.v1", cancellationToken);

	public Task<SendResult> SendUpdateLedgerRequestAsync(UpdateLedgerRequestV1 message, CancellationToken cancellationToken) =>
		_messagePublisher.SendMessage(message, "payment.update.ledger.v2", cancellationToken);

	public Task<SendResult> SendPayawayCreateAsync(FIToFICustomerCreditTransferV10 message, CancellationToken cancellationToken) =>
		_messagePublisher.SendMessage(message, "payaway.create.v1", cancellationToken);

	public Task<SendResult> SendPayawayConfirmationAsync(BankToCustomerDebitCreditNotificationV09 message, CancellationToken cancellationToken) =>
		_messagePublisher.SendMessage(message, "payaway.confirmation.v1", cancellationToken);

	public Task<SendResult> SendPayawayRejectionAsync(Admi00200101 message, string toBankDid, CancellationToken cancellationToken)
	{
		var headers = new Dictionary<string, string> { { "tobankdid", toBankDid } };
		return _messagePublisher.SendMessage(message, "payaway.rejection.v1", cancellationToken, headers);
	}

	public Task<SendResult> SendBankPartnersRequestAsync(BankPartnersRequestV1 message, CancellationToken cancellationToken) =>
		_messagePublisher.SendMessage(message, "bank.partners.v1", cancellationToken);
}
