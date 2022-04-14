using RTGS.IDCryptSDK.JsonSignatures;
using RTGS.IDCryptSDK.JsonSignatures.Models;
using RTGS.ISO20022.Messages.Pacs_008_001.V10;

namespace RTGS.DotNetSDK.Publisher.IdCrypt.Signing;

internal class SignPayawayCreateMessage : ISignMessage<FIToFICustomerCreditTransferV10>
{
	private readonly IJsonSignaturesClient _client;

	public SignPayawayCreateMessage(IJsonSignaturesClient client)
	{
		_client = client;
	}

	public async Task<SignDocumentResponse> SignAsync(FIToFICustomerCreditTransferV10 message, string alias)
	{
		ArgumentNullException.ThrowIfNull(message);

		var typedMessage = message;

		var document = new Dictionary<string, object>
		{
			{ "iban", typedMessage!.CdtTrfTxInf[0].CdtrAcct.Id.IBAN },
			{ "currency", typedMessage!.CdtTrfTxInf[0].CdtrAcct.Id.IBAN },
			{ "value", typedMessage!.CdtTrfTxInf[0].CdtrAcct.Id.IBAN }
		};

		var documentRequest = new SignDocumentRequest<Dictionary<string, object>>()
		{
			ConnectionId = alias,
			Document = document
		};

		var response = await _client.SignDocumentAsync(documentRequest);

		return response;
	}
}
