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
			{"creditorAmount", message.CdtrAmt},
			{"debtorAgentAccountIban", message.DbtrAgntAcct?.Id?.IBAN},
			{"debtorAgentAccountOtherId", message.DbtrAgntAcct?.Id?.Othr?.Id},
			{"debtorAccountIban", message.DbtrAcct?.Id?.IBAN},
			{"debtorAccountOtherId", message.DbtrAcct?.Id?.Othr?.Id},
			{"creditorAccountIban", message.CdtrAcct?.Id?.IBAN},
			{"creditorAccountOtherId", message.CdtrAcct?.Id?.Othr?.Id},
			{"creditorAgentAccountIban", message.CdtrAgntAcct?.Id?.IBAN},
			{"creditorAgentAccountOtherId", message.CdtrAgntAcct?.Id?.Othr?.Id}
		};

		var response =
			await _idCryptServiceClient.SignMessageForBankAsync(message.BkPrtnrRtgsGlobalId, documentToSign,
				cancellationToken);

		return response;
	}
}
