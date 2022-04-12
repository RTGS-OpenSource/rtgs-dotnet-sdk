using Microsoft.Extensions.Logging;
using RTGS.DotNetSDK.Publisher.Exceptions;
using RTGS.DotNetSDK.Publisher.IdCrypt.Messages;
using RTGS.IDCryptSDK.Connections;
using RTGS.IDCryptSDK.Connections.Models;
using RTGS.IDCryptSDK.Wallet;

namespace RTGS.DotNetSDK.Publisher.IdCrypt;

internal class RtgsConnectionBroker : IRtgsConnectionBroker
{
	private readonly ILogger<RtgsConnectionBroker> _logger;
	private readonly IIdCryptPublisher _idCryptPublisher;
	private readonly IWalletClient _walletClient;
	private readonly IConnectionsClient _connectionsClient;

	public RtgsConnectionBroker(
		ILogger<RtgsConnectionBroker> logger,
		IIdCryptPublisher idCryptPublisher,
		IWalletClient walletClient,
		IConnectionsClient connectionsClient)
	{
		_logger = logger;
		_idCryptPublisher = idCryptPublisher;
		_walletClient = walletClient;
		_connectionsClient = connectionsClient;
	}

	public async Task<SendInvitationResult> SendInvitationAsync(CancellationToken cancellationToken = default)
	{
		var alias = Guid.NewGuid().ToString();

		var idCryptResponse = await CreateIdCryptInvitationAsync(alias, cancellationToken);
		var agentPublicDid = await GetIdCryptAgentPublicDidAsync(cancellationToken);
		var sendToRtgsResult = await SendInvitationToRtgsAsync(alias, idCryptResponse.Invitation, agentPublicDid, cancellationToken);

		var sendInvitationResult = new SendInvitationResult
		{
			Alias = sendToRtgsResult is SendResult.Success ? alias : null,
			ConnectionId = sendToRtgsResult is SendResult.Success ? idCryptResponse.ConnectionId : null,
			SendResult = sendToRtgsResult
		};

		return sendInvitationResult;
	}

	private async Task<string> GetIdCryptAgentPublicDidAsync(CancellationToken cancellationToken)
	{
		try
		{
			_logger.LogDebug("Sending GetPublicDid request to ID Crypt Cloud Agent");

			var response = await _walletClient.GetPublicDidAsync(cancellationToken);

			_logger.LogDebug("Sent GetPublicDid request to ID Crypt Cloud Agent");

			return response;
		}
		catch (Exception innerException)
		{
			const string errorMessage = "Error occurred when sending GetPublicDid request to ID Crypt Cloud Agent";

			var exception = new RtgsPublisherException(errorMessage, innerException);

			_logger.LogError(exception, errorMessage);

			throw exception;
		}
	}

	private async Task<CreateInvitationResponse> CreateIdCryptInvitationAsync(string alias, CancellationToken cancellationToken)
	{
		const bool autoAccept = true;
		const bool multiUse = false;
		const bool usePublicDid = false;

		try
		{
			_logger.LogDebug("Sending CreateInvitation request with alias {Alias} to ID Crypt Cloud Agent", alias);

			var response = await _connectionsClient.CreateInvitationAsync(
				alias,
				autoAccept,
				multiUse,
				usePublicDid,
				cancellationToken);

			_logger.LogDebug("Sent CreateInvitation request with alias {Alias} to ID Crypt Cloud Agent", alias);

			return response;
		}
		catch (Exception innerException)
		{
			var exception = new RtgsPublisherException(
				$"Error occurred when sending CreateInvitation request with alias {alias} to ID Crypt Cloud Agent",
				innerException);

			_logger.LogError(
				exception,
				"Error occurred when sending CreateInvitation request with alias {Alias} to ID Crypt Cloud Agent",
				alias);

			throw exception;
		}
	}

	private async Task<SendResult> SendInvitationToRtgsAsync(
		string alias,
		ConnectionInvitation invitation,
		string agentPublicDid,
		CancellationToken cancellationToken)
	{
		var invitationMessage = new IdCryptInvitationV1
		{
			Alias = alias,
			Id = invitation.Id,
			Label = invitation.Label,
			RecipientKeys = invitation.RecipientKeys,
			ServiceEndPoint = invitation.ServiceEndpoint,
			Type = invitation.Type,
			AgentPublicDid = agentPublicDid
		};

		var sendResult = await _idCryptPublisher
			.SendIdCryptInvitationToRtgsAsync(invitationMessage, cancellationToken);

		return sendResult;
	}
}
