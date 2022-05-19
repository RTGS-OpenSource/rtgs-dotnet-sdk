using RTGS.DotNetSDK.IdCrypt;
using RTGS.IDCrypt.Service.Contracts.SignMessage;
using RTGS.Public.Messages.Publisher;

namespace RTGS.DotNetSDK.Publisher.IdCrypt.Signing;

internal class PayawayConfirmMessageSigner : ISignMessage<PayawayConfirmationV1>
{
	private readonly IIdCryptServiceClient _idCryptServiceClient;

	public PayawayConfirmMessageSigner(IIdCryptServiceClient idCryptServiceClient)
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
			{ "iban", message.BkToCstmrDbtCdtNtfctn?.Ntfctn[0]?.Acct?.Id?.IBAN },
			{ "amount", message.BkToCstmrDbtCdtNtfctn?.Ntfctn[0]?.Ntry[0]?.Amt?.Value }
		};

		var response = await _idCryptServiceClient.SignMessageAsync(documentToSign, cancellationToken);

		return response;
	}
}
