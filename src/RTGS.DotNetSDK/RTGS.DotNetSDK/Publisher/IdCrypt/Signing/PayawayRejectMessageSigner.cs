using RTGS.IDCryptSDK.JsonSignatures;
using RTGS.IDCryptSDK.JsonSignatures.Models;
using RTGS.Public.Messages.Publisher;

namespace RTGS.DotNetSDK.Publisher.IdCrypt.Signing;

internal class PayawayRejectMessageSigner : ISignMessage<PayawayRejectionV1>
{
	private readonly IJsonSignaturesClient _client;

	public PayawayRejectMessageSigner(IJsonSignaturesClient client)
	{
		_client = client;
	}

	public async Task<SignDocumentResponse> SignAsync(
		PayawayRejectionV1 message,
		string alias,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(message);

		var documentToSign = new Dictionary<string, object> { { "reason", message.MsgRjctn?.Rsn?.RsnDesc } };

		var response = await _client.SignDocumentAsync(documentToSign, alias, cancellationToken);

		return response;
	}
}
