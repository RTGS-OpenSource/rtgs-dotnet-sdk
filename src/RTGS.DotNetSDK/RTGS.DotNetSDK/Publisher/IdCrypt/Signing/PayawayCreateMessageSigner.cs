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
		string toRtgsGlobalId,
		PayawayCreationV1 message,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(message);

		var response = await _idCryptServiceClient.SignMessageAsync(toRtgsGlobalId, message.FIToFICstmrCdtTrf, cancellationToken);

		return response;
	}
}
