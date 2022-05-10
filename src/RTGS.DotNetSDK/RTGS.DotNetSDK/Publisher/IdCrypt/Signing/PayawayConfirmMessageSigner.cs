using RTGS.IDCryptSDK.JsonSignatures;
using RTGS.IDCryptSDK.JsonSignatures.Models;
using RTGS.Public.Messages.Publisher;

namespace RTGS.DotNetSDK.Publisher.IdCrypt.Signing;

internal class PayawayConfirmMessageSigner : ISignMessage<PayawayConfirmationV1>
{
	private readonly IJsonSignaturesClient _client;

	public PayawayConfirmMessageSigner(IJsonSignaturesClient client)
	{
		_client = client;
	}

	public async Task<SignDocumentResponse> SignAsync(
		PayawayConfirmationV1 message,
		string alias,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(message);

		var documentToSign = new Dictionary<string, object>
		{
			{ "iban", message.BkToCstmrDbtCdtNtfctn?.Ntfctn[0]?.Acct?.Id?.IBAN },
			{ "amount", message.BkToCstmrDbtCdtNtfctn?.Ntfctn[0]?.Ntry[0]?.Amt?.Value }
		};

		var response = await _client.SignDocumentAsync(documentToSign, alias, cancellationToken);

		return response;
	}
}
