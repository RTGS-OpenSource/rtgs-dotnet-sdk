using RTGS.DotNetSDK.IdCrypt;
using RTGS.IDCrypt.Service.Contracts.Message.Sign;
using RTGS.Public.Messages.Publisher;

namespace RTGS.DotNetSDK.Publisher.IdCrypt.Signing;

internal class PayawayCreationV1MessageSigner : ISignMessage<PayawayCreationV1>
{
	private readonly IIdCryptServiceClient _idCryptServiceClient;

	public PayawayCreationV1MessageSigner(IIdCryptServiceClient idCryptServiceClient)
	{
		_idCryptServiceClient = idCryptServiceClient;
	}

	public async Task<SignMessageResponse> SignAsync(
		PayawayCreationV1 message,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(message);

		var response = await _idCryptServiceClient.SignMessageForBankAsync(message.ToRtgsGlobalId, message.FIToFICstmrCdtTrf, cancellationToken);

		return response;
	}
}
