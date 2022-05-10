using RTGS.IDCryptSDK.JsonSignatures;
using RTGS.IDCryptSDK.JsonSignatures.Models;
using RTGS.Public.Messages.Publisher;

namespace RTGS.DotNetSDK.Publisher.IdCrypt.Signing;

internal class PayawayCreateMessageSigner : ISignMessage<PayawayCreationV1>
{
	private readonly IJsonSignaturesClient _client;

	public PayawayCreateMessageSigner(IJsonSignaturesClient client)
	{
		_client = client;
	}

	public async Task<SignDocumentResponse> SignAsync(
		PayawayCreationV1 message,
		string alias,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(message);

		var response = await _client.SignDocumentAsync(message.FIToFICstmrCdtTrf, alias, cancellationToken);

		return response;
	}
}
