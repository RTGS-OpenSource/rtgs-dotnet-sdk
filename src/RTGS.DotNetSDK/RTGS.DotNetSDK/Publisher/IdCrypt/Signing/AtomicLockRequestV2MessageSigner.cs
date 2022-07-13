using RTGS.DotNetSDK.IdCrypt;
using RTGS.IDCrypt.Service.Contracts.Message.Sign;
using RTGS.Public.Messages.Publisher;

namespace RTGS.DotNetSDK.Publisher.IdCrypt.Signing;

internal class AtomicLockRequestV2MessageSigner : ISignMessage<AtomicLockRequestV2>
{
	private readonly IIdCryptServiceClient _idCryptServiceClient;

	public AtomicLockRequestV2MessageSigner(IIdCryptServiceClient idCryptServiceClient)
	{
		_idCryptServiceClient = idCryptServiceClient;
	}

	public async Task<SignMessageResponse> SignAsync(
		AtomicLockRequestV2 message,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(message);

		var documentToSign = new Dictionary<string, object>
		{
			{ "creditorAmount", message.CdtrAmt },
			{ "debtorAgentAccountIban", message.DbtrAgntAcct?.Id?.IBAN },
			{ "debtorAccountIban", message.DbtrAcct?.Id?.IBAN },
			{ "creditorAccountIban", message.CdtrAcct?.Id?.IBAN },
			{ "creditorAgentAccountIban", message.CdtrAgntAcct?.Id?.IBAN }
		};

		var response = await _idCryptServiceClient.SignMessageForBankAsync(message.BkPrtnrRtgsGlobalId, documentToSign, cancellationToken);

		return response;
	}
}
