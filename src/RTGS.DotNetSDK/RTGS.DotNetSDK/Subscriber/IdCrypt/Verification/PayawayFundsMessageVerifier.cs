using System.Text.Json;
using Microsoft.Extensions.Logging;
using RTGS.DotNetSDK.Subscriber.Exceptions;
using RTGS.IDCryptSDK.JsonSignatures;
using RTGS.ISO20022.Messages.Pacs_008_001.V10;
using RTGS.Public.Payment.V3;

namespace RTGS.DotNetSDK.Subscriber.IdCrypt.Verification;

internal class PayawayFundsMessageVerifier : IVerifyMessage
{
	private readonly IJsonSignaturesClient _jsonSignaturesClient;
	private readonly ILogger<PayawayFundsMessageVerifier> _logger;

	public PayawayFundsMessageVerifier(IJsonSignaturesClient jsonSignaturesClient, ILogger<PayawayFundsMessageVerifier> logger)
	{
		_jsonSignaturesClient = jsonSignaturesClient;
		_logger = logger;
	}

	public string MessageIdentifier => "PayawayFunds";

	public async Task VerifyMessageAsync(
		RtgsMessage rtgsMessage,
		CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(rtgsMessage);

		if (!rtgsMessage.Headers.TryGetValue("public-did-signature", out var publicSignature))
		{
			_logger.LogError("Public signature not found on {MessageIdentifier} message, yet was expected", MessageIdentifier);
		}

		if (!rtgsMessage.Headers.TryGetValue("pairwise-did-signature", out var privateSignature))
		{
			_logger.LogError("Private signature not found on {MessageIdentifier} message, yet was expected", MessageIdentifier);
		}

		if (!rtgsMessage.Headers.TryGetValue("alias", out var alias))
		{
			_logger.LogError("Alias not found on {MessageIdentifier} message, yet was expected", MessageIdentifier);
		}

		if (publicSignature is null || privateSignature is null || alias is null)
		{
			throw new RtgsSubscriberException($"Unable to verify {MessageIdentifier} message due to missing headers.");
		}

		var message = JsonSerializer.Deserialize<FIToFICustomerCreditTransferV10>(rtgsMessage.Data);

		var publicSignatureIsValid = await _jsonSignaturesClient.VerifyPublicSignatureAsync(message, publicSignature, alias, cancellationToken);
		
		var privateSignatureIsValid = await _jsonSignaturesClient.VerifyPrivateSignatureAsync(rtgsMessage.Data, privateSignature, alias, cancellationToken);

		if (!publicSignatureIsValid)
		{
			_logger.LogError("Verification of {MessageIdentifier} message public signature failed", MessageIdentifier);
		}

		if (!privateSignatureIsValid)
		{
			_logger.LogError("Verification of {MessageIdentifier} message private signature failed", MessageIdentifier);
		}

		if (!publicSignatureIsValid || !privateSignatureIsValid)
		{
			throw new RtgsSubscriberException($"Verification of {MessageIdentifier} message failed.");
		}
	}
}
