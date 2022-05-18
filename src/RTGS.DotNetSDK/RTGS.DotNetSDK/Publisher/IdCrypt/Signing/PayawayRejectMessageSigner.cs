using RTGS.DotNetSDK.IdCrypt;
using RTGS.IDCrypt.Service.Contracts.SignMessage;
using RTGS.Public.Messages.Publisher;

namespace RTGS.DotNetSDK.Publisher.IdCrypt.Signing;

internal class PayawayRejectMessageSigner : ISignMessage<PayawayRejectionV1>
{
	private readonly IIdCryptServiceClient _idCryptServiceClient;

	public PayawayRejectMessageSigner(IIdCryptServiceClient idCryptServiceClient)
	{
		_idCryptServiceClient = idCryptServiceClient;
	}

	public async Task<SignMessageResponse> SignAsync(
		PayawayRejectionV1 message,
		string partnerRtgsGlobalId,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(message);

		var documentToSign = new Dictionary<string, object> { { "reason", message.MsgRjctn?.Rsn?.RsnDesc } };

		// TODO - Wrap in try?
		var response = await _idCryptServiceClient.SignMessageAsync(partnerRtgsGlobalId, documentToSign, cancellationToken);

		return response;
	}
}
