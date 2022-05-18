using System.Text.Json;
using Microsoft.Extensions.Logging;
using RTGS.DotNetSDK.IdCrypt;
using RTGS.DotNetSDK.Subscriber.Exceptions;
using RTGS.IDCrypt.Service.Contracts.VerifyMessage;
using RTGS.Public.Messages.Subscriber;
using RTGS.Public.Payment.V4;

namespace RTGS.DotNetSDK.Subscriber.IdCrypt.Verification;

internal class PayawayFundsV1MessageVerifier : IVerifyMessage
{
	private readonly IIdCryptServiceClient _idCryptServiceClient;
	private readonly ILogger<PayawayFundsV1MessageVerifier> _logger;

	public PayawayFundsV1MessageVerifier(IIdCryptServiceClient idCryptServiceClient, ILogger<PayawayFundsV1MessageVerifier> logger)
	{
		_idCryptServiceClient = idCryptServiceClient;
		_logger = logger;
	}

	public string MessageIdentifier => nameof(PayawayFundsV1);

	public async Task VerifyMessageAsync(
		RtgsMessage rtgsMessage,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(rtgsMessage);

		if (!rtgsMessage.Headers.TryGetValue("pairwise-did-signature", out var privateSignature)
			|| string.IsNullOrEmpty(privateSignature))
		{
			_logger.LogError("Private signature not found on {MessageIdentifier} message, yet was expected", MessageIdentifier);
		}

		if (!rtgsMessage.Headers.TryGetValue("alias", out var alias)
			|| string.IsNullOrEmpty(alias))
		{
			_logger.LogError("Alias not found on {MessageIdentifier} message, yet was expected", MessageIdentifier);
		}

		if (!rtgsMessage.Headers.TryGetValue("partner-rtgs-global-id", out var partnerRtgsGlobalId)
			|| string.IsNullOrEmpty(partnerRtgsGlobalId))
		{
			_logger.LogError("Partner RTGS Global ID not found on {MessageIdentifier} message, yet was expected", MessageIdentifier);
		}

		if (string.IsNullOrEmpty(privateSignature) || string.IsNullOrEmpty(alias) || string.IsNullOrEmpty(partnerRtgsGlobalId))
		{
			throw new RtgsSubscriberException($"Unable to verify {MessageIdentifier} message due to missing headers.");
		}

		var payawayFundsMessage = JsonSerializer.Deserialize<PayawayFundsV1>(rtgsMessage.Data.Span);

		var message = payawayFundsMessage?.FIToFICstmrCdtTrf;

		VerifyPrivateSignatureResponse response;
		try
		{
			response = await _idCryptServiceClient.VerifyMessageAsync(partnerRtgsGlobalId, message, privateSignature, alias, cancellationToken);
		}
		catch (Exception innerException)
		{
			const string errorMessage = "Error occurred when sending VerifyMessage request to ID Crypt Service";

			var exception = new RtgsSubscriberException(errorMessage, innerException);

			_logger.LogError(exception, errorMessage);

			throw exception;
		}

		if (!response.Verified)
		{
			var exception = new RtgsSubscriberException($"Verification of {MessageIdentifier} message failed.");

			_logger.LogError(exception, "Verification of {MessageIdentifier} message private signature failed", MessageIdentifier);

			throw exception;
		}
	}
}
