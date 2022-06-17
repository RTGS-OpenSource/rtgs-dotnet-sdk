using RTGS.DotNetSDK.IdCrypt;
using RTGS.IDCrypt.Service.Contracts.Message.Sign;
using RTGS.Public.Messages.Publisher;

namespace RTGS.DotNetSDK.Publisher.IdCrypt.Signing;

internal class AtomicLockRequestV1MessageSigner : ISignMessage<AtomicLockRequestV1>
{
	private readonly IIdCryptServiceClient _idCryptServiceClient;

	public AtomicLockRequestV1MessageSigner(IIdCryptServiceClient idCryptServiceClient)
	{
		_idCryptServiceClient = idCryptServiceClient;
	}

	public async Task<SignMessageResponse> SignAsync(
		AtomicLockRequestV1 message,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(message);

		var documentToSign = new Dictionary<string, object>
		{
			{ "creditorAmount", message.CdtrAmt },
			{ "debtorAgentAccountIban", message.DbtrAgntAcct?.Id?.IBAN },
			{ "debtorAccountIban", message.DbtrAcct?.Id?.IBAN }
		};

		var response = await _idCryptServiceClient.SignMessageAsync(message.BkPrtnrRtgsGlobalId, documentToSign, cancellationToken);

		return response;
	}
}
