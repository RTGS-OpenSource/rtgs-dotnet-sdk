using Microsoft.Extensions.Logging;
using RTGS.DotNetSDK.IdCrypt;
using RTGS.DotNetSDK.Subscriber.Exceptions;
using RTGS.Public.Messages.Subscriber;

namespace RTGS.DotNetSDK.Subscriber.IdCrypt.Verification;

internal class PayawayFundsV1MessageVerifier : IVerifyMessage<PayawayFundsV1>
{
	private readonly IIdCryptServiceClient _idCryptServiceClient;
	private readonly ILogger<PayawayFundsV1MessageVerifier> _logger;

	public PayawayFundsV1MessageVerifier(IIdCryptServiceClient idCryptServiceClient, ILogger<PayawayFundsV1MessageVerifier> logger)
	{
		_idCryptServiceClient = idCryptServiceClient;
		_logger = logger;
	}

	public async Task<bool> VerifyMessageAsync(
		PayawayFundsV1 message,
		string privateSignature,
		string alias,
		string fromRtgsGlobalId,
		CancellationToken cancellationToken = default)
	{
		var messageToVerify = message?.FIToFICstmrCdtTrf;

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
