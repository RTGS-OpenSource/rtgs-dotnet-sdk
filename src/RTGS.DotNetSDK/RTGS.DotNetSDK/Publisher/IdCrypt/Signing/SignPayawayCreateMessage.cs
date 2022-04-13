using RTGS.IDCryptSDK.JsonSignatures;
using RTGS.IDCryptSDK.JsonSignatures.Models;
using RTGS.ISO20022.Messages.Pacs_008_001.V10;

namespace RTGS.DotNetSDK.Publisher.IdCrypt.Signing;

internal class SignPayawayCreateMessage : ISignMessage
{
	private readonly IJsonSignaturesClient _client;

	public SignPayawayCreateMessage(IJsonSignaturesClient client)
	{
		_client = client;
	}
	public Type MessageType => typeof(FIToFICustomerCreditTransferV10);

	public async Task<SignDocumentResponse> SignAsync<TMessageType>(TMessageType message, string alias)
	{
		ArgumentNullException.ThrowIfNull(message);
		var typedMessage = message as FIToFICustomerCreditTransferV10;

		var document = new Dictionary<string, object>
		{
			{ "iban", typedMessage!.CdtTrfTxInf[0].CdtrAcct.Id }
		};

		var documentRequest = new SignDocumentRequest<Dictionary<string, object>>()
		{
			ConnectionId = alias,
			Document = document
		};

		return await _client.SignDocumentAsync(documentRequest);
	}
}
