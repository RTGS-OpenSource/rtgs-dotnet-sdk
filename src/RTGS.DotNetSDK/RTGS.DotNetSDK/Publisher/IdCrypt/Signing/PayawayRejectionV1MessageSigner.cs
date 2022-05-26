using RTGS.DotNetSDK.IdCrypt;
using RTGS.IDCrypt.Service.Contracts.Message.Sign;
using RTGS.Public.Messages.Publisher;

namespace RTGS.DotNetSDK.Publisher.IdCrypt.Signing;

internal class PayawayRejectionV1MessageSigner : ISignMessage<PayawayRejectionV1>
{
	private readonly IIdCryptServiceClient _idCryptServiceClient;

	public PayawayRejectionV1MessageSigner(IIdCryptServiceClient idCryptServiceClient)
	{
		_idCryptServiceClient = idCryptServiceClient;
	}

	public async Task<SignMessageResponse> SignAsync(
		string toRtgsGlobalId,
		PayawayRejectionV1 message,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(message);

		var documentToSign = new Dictionary<string, object> { { "reason", message.MsgRjctn?.Rsn?.RsnDesc } };

		var response = await _idCryptServiceClient.SignMessageAsync(toRtgsGlobalId, documentToSign, cancellationToken);

		return response;
	}
}
