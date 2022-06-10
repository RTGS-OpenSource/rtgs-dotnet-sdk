using Microsoft.Extensions.Logging;
using RTGS.DotNetSDK.IdCrypt;
using RTGS.DotNetSDK.Subscriber.Exceptions;
using RTGS.Public.Messages.Subscriber;

namespace RTGS.DotNetSDK.Subscriber.IdCrypt.Verification;

internal class PayawayCompleteV1MessageVerifier : IVerifyMessage<PayawayCompleteV1>
{
	private readonly IIdCryptServiceClient _idCryptServiceClient;
	private readonly ILogger<PayawayCompleteV1MessageVerifier> _logger;

	public PayawayCompleteV1MessageVerifier(IIdCryptServiceClient idCryptServiceClient, ILogger<PayawayCompleteV1MessageVerifier> logger)
	{
		_idCryptServiceClient = idCryptServiceClient;
		_logger = logger;
	}

	public async Task<bool> VerifyMessageAsync(
		PayawayCompleteV1 message,
		string privateSignature,
		string publicSignature,
		string alias,
		string fromRtgsGlobalId,
		CancellationToken cancellationToken = default)
	{
		var messageToVerify = new Dictionary<string, object>
		{
			{ "payawayId", message.BkToCstmrDbtCdtNtfctn?.Ntfctn[0]?.Ntry[0]?.NtryDtls[0]?.TxDtls[0]?.Refs?.EndToEndId},
			{ "iban", message.BkToCstmrDbtCdtNtfctn?.Ntfctn[0]?.Acct?.Id?.IBAN },
			{ "amount", message.BkToCstmrDbtCdtNtfctn?.Ntfctn[0]?.Ntry[0]?.Amt?.Value }
		};

		try
		{
			var response = await _idCryptServiceClient.VerifyMessageAsync(fromRtgsGlobalId, messageToVerify, privateSignature, alias, cancellationToken);
			return response.Verified;
		}
		catch (Exception innerException)
		{
			const string errorMessage = "Error occurred when sending VerifyMessage request to ID Crypt Service";

			var exception = new RtgsSubscriberException(errorMessage, innerException);

			_logger.LogError(exception, errorMessage);

			throw exception;
		}
	}
}
