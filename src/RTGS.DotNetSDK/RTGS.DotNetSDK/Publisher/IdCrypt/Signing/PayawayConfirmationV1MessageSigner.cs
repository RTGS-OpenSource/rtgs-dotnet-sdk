using RTGS.DotNetSDK.IdCrypt;
using RTGS.IDCrypt.Service.Contracts.Message.Sign;
using RTGS.Public.Messages.Publisher;

namespace RTGS.DotNetSDK.Publisher.IdCrypt.Signing;

internal class PayawayConfirmationV1MessageSigner : ISignMessage<PayawayConfirmationV1>
{
	private readonly IIdCryptServiceClient _idCryptServiceClient;

	public PayawayConfirmationV1MessageSigner(IIdCryptServiceClient idCryptServiceClient)
	{
		_idCryptServiceClient = idCryptServiceClient;
	}

	public async Task<SignMessageResponse> SignAsync(
		PayawayConfirmationV1 message,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(message);

		var documentToSign = new Dictionary<string, object>
		{
			{ "payawayId", message.BkToCstmrDbtCdtNtfctn?.Ntfctn[0]?.Ntry[0]?.NtryDtls[0]?.TxDtls[0]?.Refs?.EndToEndId},
			{ "iban", message.BkToCstmrDbtCdtNtfctn?.Ntfctn[0]?.Acct?.Id?.IBAN },
			{ "amount", message.BkToCstmrDbtCdtNtfctn?.Ntfctn[0]?.Ntry[0]?.Amt?.Value }
		};

		var response = await _idCryptServiceClient.SignMessageAsync(message.ToRtgsGlobalId, documentToSign, cancellationToken);

		return response;
	}
}
