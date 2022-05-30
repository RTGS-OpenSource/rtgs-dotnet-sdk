using System.Text.Json;
using Microsoft.Extensions.Logging;
using RTGS.DotNetSDK.Subscriber.Exceptions;
using RTGS.DotNetSDK.Subscriber.Handlers;
using RTGS.DotNetSDK.Subscriber.IdCrypt.Verification;
using RTGS.Public.Messages.Subscriber;
using RTGS.Public.Payment.V4;

namespace RTGS.DotNetSDK.Subscriber.Adapters;

internal class DataVerifyingMessageAdapter<TMessage> : IMessageAdapter<TMessage>
{
	private readonly ILogger<DataVerifyingMessageAdapter<TMessage>> _logger;
	private readonly IVerifyMessage<TMessage> _verifier;

	public DataVerifyingMessageAdapter(ILogger<DataVerifyingMessageAdapter<TMessage>> logger, IVerifyMessage<TMessage> verifier)
	{
		_logger = logger;
		_verifier = verifier;
	}

	public string MessageIdentifier => typeof(TMessage).Name;

	public async Task HandleMessageAsync(RtgsMessage rtgsMessage, IHandler<TMessage> handler)
	{
		ArgumentNullException.ThrowIfNull(rtgsMessage);

		var deserializedMessage = JsonSerializer.Deserialize<TMessage>(rtgsMessage.Data.Span);

		// Intermediary solution until we have decided how to handle unsigned MessageRejectV1 messages from RTGS.Global
		if (rtgsMessage.MessageIdentifier != nameof(MessageRejectV1) ||
			rtgsMessage.Headers.ContainsKey("pairwise-did-signature"))
		{
			await VerifyMessageAsync(rtgsMessage, deserializedMessage);
		}

		await handler.HandleMessageAsync(deserializedMessage);
	}

	private async Task VerifyMessageAsync(RtgsMessage rtgsMessage, TMessage deserializedMessage)
	{
		_logger.LogInformation("Verifying {MessageIdentifier} message", rtgsMessage.MessageIdentifier);

		if (!rtgsMessage.Headers.TryGetValue("pairwise-did-signature", out var privateSignature) ||
			string.IsNullOrEmpty(privateSignature))
		{
			_logger.LogError("Private signature not found on {MessageIdentifier} message, yet was expected",
				MessageIdentifier);
		}

		if (!rtgsMessage.Headers.TryGetValue("alias", out var alias) || string.IsNullOrEmpty(alias))
		{
			_logger.LogError("Alias not found on {MessageIdentifier} message, yet was expected", MessageIdentifier);
		}

		if (!rtgsMessage.Headers.TryGetValue("from-rtgs-global-id", out var fromRtgsGlobalId) ||
			string.IsNullOrEmpty(fromRtgsGlobalId))
		{
			_logger.LogError("From RTGS Global ID not found on {MessageIdentifier} message, yet was expected",
				MessageIdentifier);
		}

		if (string.IsNullOrEmpty(privateSignature) || string.IsNullOrEmpty(alias) || string.IsNullOrEmpty(fromRtgsGlobalId))
		{
			throw new VerificationFailedException(
				$"Unable to verify {MessageIdentifier} message due to missing headers.");
		}

		var verified = await _verifier.VerifyMessageAsync(deserializedMessage, privateSignature, alias, fromRtgsGlobalId,
			cancellationToken: default);

		if (!verified)
		{
			var exception =
				new VerificationFailedException($"Verification of {MessageIdentifier} message failed.", MessageIdentifier);
			throw exception;
		}

		_logger.LogInformation("Verified {MessageIdentifier} message", rtgsMessage.MessageIdentifier);
	}
}
