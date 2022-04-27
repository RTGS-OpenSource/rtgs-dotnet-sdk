using RTGS.IDCryptSDK.JsonSignatures;
using RTGS.IDCryptSDK.JsonSignatures.Models;
using RTGS.ISO20022.Messages.Admi_002_001.V01;

namespace RTGS.DotNetSDK.Publisher.IdCrypt.Signing;

internal class PayawayRejectMessageSigner : ISignMessage<Admi00200101>
{
	private readonly IJsonSignaturesClient _client;

	public PayawayRejectMessageSigner(IJsonSignaturesClient client)
	{
		_client = client;
	}

	public async Task<SignDocumentResponse> SignAsync(
		Admi00200101 message,
		string alias,
		CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(message);

		var documentToSign = new Dictionary<string, object> { { "reason", message.Rsn.RsnDesc } };

		var response = await _client.SignDocumentAsync(documentToSign, alias, cancellationToken);

		return response;
	}
}
