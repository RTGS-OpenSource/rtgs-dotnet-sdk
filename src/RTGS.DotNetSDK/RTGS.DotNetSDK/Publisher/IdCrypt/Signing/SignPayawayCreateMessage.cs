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

		var documentRequest = new SignDocumentRequest<FIToFICustomerCreditTransferV10>()
		{
			Alias = alias,
			Document = message
		};

		var response = await _client.SignDocumentAsync(documentRequest);

		return response;
	}
}
