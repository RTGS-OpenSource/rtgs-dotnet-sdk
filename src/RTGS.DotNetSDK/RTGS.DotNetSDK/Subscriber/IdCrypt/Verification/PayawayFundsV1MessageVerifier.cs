using System.Text.Json;
using Microsoft.Extensions.Logging;
using RTGS.DotNetSDK.Subscriber.Exceptions;
using RTGS.IDCryptSDK.JsonSignatures;
using RTGS.Public.Messages.Subscriber;
using RTGS.Public.Payment.V3;

namespace RTGS.DotNetSDK.Subscriber.IdCrypt.Verification;

internal class PayawayFundsV1MessageVerifier : IVerifyMessage
{
	private readonly IJsonSignaturesClient _jsonSignaturesClient;
	private readonly ILogger<PayawayFundsV1MessageVerifier> _logger;

	public PayawayFundsV1MessageVerifier(IJsonSignaturesClient jsonSignaturesClient, ILogger<PayawayFundsV1MessageVerifier> logger)
	{
		_jsonSignaturesClient = jsonSignaturesClient;
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

		if (string.IsNullOrEmpty(privateSignature) || string.IsNullOrEmpty(alias))
		{
			throw new RtgsSubscriberException($"Unable to verify {MessageIdentifier} message due to missing headers.");
		}

		var message = JsonSerializer.Deserialize<PayawayFundsV1>(rtgsMessage.Data);

		var privateSignatureIsValid = await _jsonSignaturesClient.VerifyPrivateSignatureAsync(message?.FIToFICstmrCdtTrf, privateSignature, alias, cancellationToken);

		if (!privateSignatureIsValid)
		{
			var exception = new RtgsSubscriberException($"Verification of {MessageIdentifier} message failed.");

			_logger.LogError(exception, "Verification of {MessageIdentifier} message private signature failed", MessageIdentifier);

			throw exception;
		}
	}
}
