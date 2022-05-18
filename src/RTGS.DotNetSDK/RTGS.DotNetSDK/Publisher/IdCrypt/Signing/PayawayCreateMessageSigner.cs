using RTGS.DotNetSDK.IdCrypt;
using RTGS.IDCrypt.Service.Contracts.SignMessage;
using RTGS.Public.Messages.Publisher;

namespace RTGS.DotNetSDK.Publisher.IdCrypt.Signing;

internal class PayawayCreateMessageSigner : ISignMessage<PayawayCreationV1>
{
	private readonly IIdCryptServiceClient _idCryptServiceClient;

	public PayawayCreateMessageSigner(IIdCryptServiceClient idCryptServiceClient)
	{
		_idCryptServiceClient = idCryptServiceClient;
	}

	public async Task<SignMessageResponse> SignAsync(
		PayawayCreationV1 message,
		string partnerRtgsGlobalId,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(message);

		var response = await _idCryptServiceClient.SignMessageAsync(partnerRtgsGlobalId, message.FIToFICstmrCdtTrf, cancellationToken);

		return response;
	}
}
