using RTGS.IDCryptSDK.JsonSignatures;
using RTGS.IDCryptSDK.JsonSignatures.Models;
using RTGS.ISO20022.Messages.Pacs_008_001.V10;

namespace RTGS.DotNetSDK.Publisher.IdCrypt.Signing;

internal class PayawayCreateMessageSigner : ISignMessage<FIToFICustomerCreditTransferV10>
{
	private readonly IJsonSignaturesClient _client;

	public PayawayCreateMessageSigner(IJsonSignaturesClient client)
	{
		_client = client;
	}

	public async Task<SignDocumentResponse> SignAsync(
		FIToFICustomerCreditTransferV10 message,
		string alias,
		CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(message);

		var response = await _client.SignDocumentAsync(message, alias, cancellationToken);

		return response;
	}
}
