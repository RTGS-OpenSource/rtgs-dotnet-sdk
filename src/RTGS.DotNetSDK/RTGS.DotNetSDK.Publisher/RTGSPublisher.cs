using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using RTGS.DotNetSDK.Publisher.Messages;
using RTGS.ISO20022.Messages.Admi_002_001.V01;
using RTGS.ISO20022.Messages.Camt_054_001.V09;
using RTGS.ISO20022.Messages.Pacs_008_001.V10;
using RTGS.Public.Payment.V3;

namespace RTGS.DotNetSDK.Publisher;

internal sealed class RtgsPublisher : RtgsPublisherBase, IRtgsPublisher
{
	public RtgsPublisher(ILogger<RtgsPublisher> logger, Payment.PaymentClient paymentClient, RtgsPublisherOptions options)
		: base(logger, paymentClient, options)
	{
	}

	public Task<SendResult> SendAtomicLockRequestAsync(AtomicLockRequestV1 message, string bankPartnerDid, CancellationToken cancellationToken)
	{
		var headers = new Dictionary<string, string> { { "bankpartnerdid", bankPartnerDid } };
		return SendMessage(message, "payment.lock.v2", cancellationToken, headers);
	}

	public Task<SendResult> SendAtomicTransferRequestAsync(AtomicTransferRequestV1 message, CancellationToken cancellationToken) =>
		SendMessage(message, "payment.block.v2", cancellationToken);

	public Task<SendResult> SendEarmarkConfirmationAsync(EarmarkConfirmationV1 message, CancellationToken cancellationToken) =>
		SendMessage(message, "payment.earmarkconfirmation.v1", cancellationToken);

	public Task<SendResult> SendAtomicTransferConfirmationAsync(AtomicTransferConfirmationV1 message, CancellationToken cancellationToken) =>
		SendMessage(message, "payment.blockconfirmation.v1", cancellationToken);

	public Task<SendResult> SendUpdateLedgerRequestAsync(UpdateLedgerRequestV1 message, CancellationToken cancellationToken) =>
		SendMessage(message, "payment.update.ledger.v2", cancellationToken);

	public Task<SendResult> SendPayawayCreateAsync(FIToFICustomerCreditTransferV10 message, CancellationToken cancellationToken) =>
		SendMessage(message, "payaway.create.v1", cancellationToken);

	public Task<SendResult> SendPayawayConfirmationAsync(BankToCustomerDebitCreditNotificationV09 message, CancellationToken cancellationToken) =>
		SendMessage(message, "payaway.confirmation.v1", cancellationToken);

	public Task<SendResult> SendPayawayRejectionAsync(Admi00200101 message, string toBankDid, CancellationToken cancellationToken)
	{
		var headers = new Dictionary<string, string> { { "tobankdid", toBankDid } };
		return SendMessage(message, "payaway.rejection.v1", cancellationToken, headers);
	}

	public Task<SendResult> SendBankPartnersRequestAsync(BankPartnersRequestV1 message, CancellationToken cancellationToken) =>
		SendMessage(message, "bank.partners.v1", cancellationToken);
}
