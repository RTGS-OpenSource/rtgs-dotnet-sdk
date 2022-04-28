using RTGS.IDCryptSDK.JsonSignatures;
using RTGS.IDCryptSDK.JsonSignatures.Models;
using RTGS.ISO20022.Messages.Camt_054_001.V09;

namespace RTGS.DotNetSDK.Publisher.IdCrypt.Signing;

internal class PayawayConfirmMessageSigner : ISignMessage<BankToCustomerDebitCreditNotificationV09>
{
	private readonly IJsonSignaturesClient _client;

	public PayawayConfirmMessageSigner(IJsonSignaturesClient client)
	{
		_client = client;
	}

	public async Task<SignDocumentResponse> SignAsync(
		BankToCustomerDebitCreditNotificationV09 message,
		string alias,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(message);

		var documentToSign = new Dictionary<string, object>
		{
			{ "iban", message.Ntfctn[0]?.Acct?.Id?.IBAN },
			{ "amount", message.Ntfctn[0]?.Ntry[0]?.Amt?.Value }
		};

		var response = await _client.SignDocumentAsync(documentToSign, alias, cancellationToken);

		return response;
	}
}
