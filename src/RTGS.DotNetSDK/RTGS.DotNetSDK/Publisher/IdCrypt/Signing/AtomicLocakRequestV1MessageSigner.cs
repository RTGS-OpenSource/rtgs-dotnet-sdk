using RTGS.DotNetSDK.IdCrypt;
using RTGS.IDCrypt.Service.Contracts.Message.Sign;
using RTGS.Public.Messages.Publisher;

namespace RTGS.DotNetSDK.Publisher.IdCrypt.Signing;

internal class AtomicLocakRequestV1MessageSigner : ISignMessage<AtomicLockRequestV1>
{
	private readonly IIdCryptServiceClient _idCryptServiceClient;

	public AtomicLocakRequestV1MessageSigner(IIdCryptServiceClient idCryptServiceClient)
	{
		_idCryptServiceClient = idCryptServiceClient;
	}

	public async Task<SignMessageResponse> SignAsync(
		string toRtgsGlobalId,
		AtomicLockRequestV1 message,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(message);

		var documentToSign = new Dictionary<string, object>
		{
			{ "creditorAmount", message.CdtrAmt },
			{ "creditorAgentAccount", message.CdtrAgntAcct },
			{ "debtorAccount", message.DbtrAcct }
		};

		var response = await _idCryptServiceClient.SignMessageAsync(toRtgsGlobalId, documentToSign, cancellationToken);

		return response;
	}
}
