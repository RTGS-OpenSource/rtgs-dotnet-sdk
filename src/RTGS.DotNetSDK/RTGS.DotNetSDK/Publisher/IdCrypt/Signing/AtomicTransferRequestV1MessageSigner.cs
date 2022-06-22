using RTGS.DotNetSDK.IdCrypt;
using RTGS.IDCrypt.Service.Contracts.Message.Sign;
using RTGS.Public.Messages.Publisher;

namespace RTGS.DotNetSDK.Publisher.IdCrypt.Signing;

internal class AtomicTransferRequestV1MessageSigner : ISignMessage<AtomicTransferRequestV1>
{
	private readonly IIdCryptServiceClient _idCryptServiceClient;

	public AtomicTransferRequestV1MessageSigner(IIdCryptServiceClient idCryptServiceClient)
	{
		_idCryptServiceClient = idCryptServiceClient;
	}

	public async Task<SignMessageResponse> SignAsync(
		AtomicTransferRequestV1 message,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(message);

		var documentToSign = new Dictionary<string, object>
		{
			{ "creditTransfer", message.FIToFICstmrCdtTrf },
			{ "lockId", message.LckId},
		};

		var response = await _idCryptServiceClient.SignMessageForBankAsync(message.ToRtgsGlobalId, documentToSign, cancellationToken);

		return response;
	}
}
